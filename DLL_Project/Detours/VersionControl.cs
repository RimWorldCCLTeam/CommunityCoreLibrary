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

        internal static FieldInfo           _versionStringShort;
        internal static FieldInfo           _buildDate;

        #region Reflected Methods

        internal static string              GetVersionStringShort()
        {
            if( _versionStringShort == null )
            {
                _versionStringShort = typeof( VersionControl ).GetField( "versionStringShort", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (string)_versionStringShort.GetValue( null );
        }

        internal static DateTime            GetBuildDate()
        {
            if( _buildDate == null )
            {
                _buildDate = typeof( VersionControl ).GetField( "buildDate", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (DateTime)_buildDate.GetValue( null );
        }

        #endregion

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
            Rect rect = new Rect( 10f, 10f, 330f, Text.CalcHeight( str2, 330f ) );
            Widgets.Label( rect, str2 );
            GUI.color = Color.white;
            var versionRect = new Rect( 10f, rect.yMax - 5f, 330f, 999f );
            Current.Root.gameObject.GetComponent<LatestVersionGetter>().DrawAt( versionRect );
            Version.DrawAt( versionRect );
        }

    }

}
