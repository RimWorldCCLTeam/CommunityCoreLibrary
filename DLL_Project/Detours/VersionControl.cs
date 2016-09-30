using System;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.Steam;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _VersionControl
    {

        internal static FieldInfo           _versionString;
        internal static FieldInfo           _buildDate;

        static                              _VersionControl()
        {
            _versionString = typeof( VersionControl ).GetField( "versionString", Controller.Data.UniversalBindingFlags );
            if( _versionString == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'versionString' in 'VersionControl'",
                    "Detour.VersionControl" );
            }
            _buildDate = typeof( VersionControl ).GetField( "buildDate", Controller.Data.UniversalBindingFlags );
            if( _buildDate == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'buildDate' in 'VersionControl'",
                    "Detour.VersionControl" );
            }
        }

        #region Reflected Methods

        internal static string              GetVersionStringShort()
        {
            return (string)_versionString.GetValue( null );
        }

        internal static DateTime            GetBuildDate()
        {
            return (DateTime)_buildDate.GetValue( null );
        }

        #endregion

        [DetourClassMethod( typeof( VersionControl ), "DrawInfoInCorner" )]
        internal static void                _DrawInfoInCorner()
        {
            if( Current.ProgramState != ProgramState.Entry )
            {
                // Only display it when in the main menu
                return;
            }
            var versionStringShort = GetVersionStringShort();
            var buildDate = GetBuildDate();

            Text.Font = GameFont.Small;
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            string str1 = "VersionIndicator".Translate( versionStringShort );
            if( UnityData.isDebugBuild )
            {
                str1 = str1 + " (" + "DevelopmentBuildLower".Translate() + ")";
            }
            string str2 = str1 + "\n" + "CompiledOn".Translate( buildDate.ToString( "MMM d yyyy" ) );
            if( SteamManager.Initialized )
            {
                str2 = str2 + "\n" + "LoggedIntoSteamAs".Translate( SteamUtility.SteamPersonaName );
            }
            var str2Height = Text.CalcHeight( str2, 330f );
            Rect rect = new Rect( 10f, 10f, 330f, str2Height );
            Widgets.Label( rect, str2 );
            GUI.color = Color.white;
            var versionRect = new Rect( 10f, rect.yMax - 5f, 330f, 999f );
            Current.Root.gameObject.GetComponent<LatestVersionGetter>().DrawAt( versionRect );
            // Now draw CCL version compared to the remote
            Version.DrawAt( versionRect, str2Height );
        }

    }

}
