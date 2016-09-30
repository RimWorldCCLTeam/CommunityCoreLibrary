using System;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public static class Version
    {

        #region Instance Data

        private static System.Version       versionMin = new System.Version( "0.15.0" );
        private const string                versionCurrentInt = "0.15.0";

        private static System.Version       versionCurrent;

        private static string               remoteVersionURL;
        internal static WWW                 remoteVersionWWW;

        #endregion

        #region Constructor

        static                              Version()
        {
#if DEVELOPER
            remoteVersionURL = "https://raw.githubusercontent.com/RimWorldCCLTeam/CommunityCoreLibrary/development/version.txt";
#else
            remoteVersionURL = "https://raw.githubusercontent.com/RimWorldCCLTeam/CommunityCoreLibrary/master/version.txt";
#endif
            remoteVersionWWW = new WWW( remoteVersionURL );
        }

        #endregion

        #region Static Properties

        private static bool                 Errored
        {
            get
            {
                return (
                    remoteVersionWWW.error == null
                    ? 1
                    :
                    remoteVersionWWW.error == string.Empty
                    ? 1
                    : 0
                ) == 0;
            }
        }

        public static System.Version        Minimum
        {
            get
            {
                return versionMin;
            }
        }

        public static System.Version        Current
        {
            get
            {
                if( versionCurrent == null )
                {
                    // TODO:  Fix issue #30 so we can use proper assembly versioning
                    //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    //CCLVersion = assembly.GetName().Version;
                    versionCurrent = new System.Version( versionCurrentInt );
                }
                return versionCurrent;
            }
        }

        #endregion

        #region Comparison

        public enum VersionCompare
        {
            LessThanMin,
            GreaterThanMax,
            ExactMatch,
            WithinRange,
            Invalid
        }

        public static VersionCompare        Compare( string v, bool includeRevision = false )
        {
            try
            {
                var modVersion = new System.Version( v );
                return CompareInt( modVersion, includeRevision );
            }
            catch
            {
                return VersionCompare.Invalid;
            }
        }

        public static VersionCompare        Compare( System.Version v, bool includeRevision = false )
        {
            return CompareInt( v, includeRevision );
        }

        private static VersionCompare       CompareInt( System.Version v, bool includeRevision )
        {
            if( includeRevision )
            {
                // Compare versions including revision number (hotfixes)
                if(
                    ( v.Major < Version.Minimum.Major )||
                    ( v.Minor < Version.Minimum.Minor )||
                    ( v.Build < Version.Minimum.Build )
                )
                {
                    return VersionCompare.LessThanMin;
                }
                else if(
                    ( v.Major > Version.Current.Major )||
                    ( v.Minor > Version.Current.Minor )||
                    ( v.Build > Version.Current.Build )
                )
                {
                    return VersionCompare.GreaterThanMax;
                }
                else if(
                    ( v.Major == Version.Current.Major )&&
                    ( v.Minor == Version.Current.Minor )&&
                    ( v.Build == Version.Current.Build )
                )
                {
                    if( v.Revision < Version.Current.Revision )
                    {
                        return VersionCompare.WithinRange;
                    }
                    else if( v.Revision > Version.Current.Revision )
                    {
                        return VersionCompare.GreaterThanMax;
                    }
                    return VersionCompare.ExactMatch;
                }
            }
            else
            {
                // Compare versions ignoring revision number (hotfixes)
                if(
                    ( v.Major < Version.Minimum.Major )||
                    ( v.Minor < Version.Minimum.Minor )||
                    ( v.Build < Version.Minimum.Build )
                )
                {
                    return VersionCompare.LessThanMin;
                }
                else if(
                    ( v.Major > Version.Current.Major )||
                    ( v.Minor > Version.Current.Minor )||
                    ( v.Build > Version.Current.Build )
                )
                {
                    return VersionCompare.GreaterThanMax;
                }
                else if(
                    ( v.Major == Version.Current.Major )&&
                    ( v.Minor == Version.Current.Minor )&&
                    ( v.Build == Version.Current.Build )
                )
                {
                    return VersionCompare.ExactMatch;
                }
            }
            return VersionCompare.WithinRange;
        }

        #endregion

        public static void                  DrawAt( Rect rect, float offsetFromTop )
        {
            Color color = Color.white;
            string str = string.Empty;
            if( Errored )
            {
                color = new Color( 1f, 1f, 1f, 0.5f );
                str = "ErrorGettingCCLVersionInfo".Translate( remoteVersionWWW.error );
            }
            else if( !remoteVersionWWW.isDone )
            {
                color = new Color( 1f, 1f, 1f, 0.5f );
                str = "LoadingCCLVersionInfo".Translate();
            }
            else
            {
                var remoteCompare = Compare( remoteVersionWWW.text, true );
                if( remoteCompare == VersionCompare.GreaterThanMax )
                {
                    color = Color.yellow;
                    str = "CCLBuildNowAvailable".Translate( remoteVersionWWW.text );
                }
                else if( remoteCompare == VersionCompare.ExactMatch )
                {
                    color = new Color( 1f, 1f, 1f, 0.5f );
                    str = "CCLBuildUpToDate".Translate();
                }
                else if( remoteCompare == VersionCompare.WithinRange )
                {
                    color = new Color( 1f, 1f, 1f, 0.5f );
                    str = "CCLBuildUpToDate".Translate();
                }
            }
            rect.y += offsetFromTop;
            rect.y -= 5f;
            rect.height = Text.CalcHeight( str, rect.width );
            GUI.color = color;
            Widgets.Label( rect, str );
            GUI.color = Color.white;
        }

        public static void                  Log()
        {
#if DEBUG
            CCL_Log.Message( "v" + Current + " (debug)" );
#else
            CCL_Log.Message( "v" + Current );
#endif
        }

    }

}
