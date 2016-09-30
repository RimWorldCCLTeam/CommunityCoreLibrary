using System;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommunityCoreLibrary
{

	public class MainMenu_QuickStart : MainMenu
	{

#if DEBUG
        private static FieldInfo            _quickStarted;

        private static bool                 QuickStarted
        {
            get
            {
                return (bool)_quickStarted.GetValue( null );
            }
            set
            {
                _quickStarted.SetValue( null, value );
            }
        }

        static MainMenu_QuickStart()
        {
            Type QuickStarter = Controller.Data.Assembly_CSharp.GetType( "Verse.QuickStarter" );
            if( QuickStarter == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get class 'Verse.QuickStarter' in 'Assembly_CSharp'",
                    "MainMenu_QuickStart" );
            }
            _quickStarted = QuickStarter.GetField( "quickStarted", Controller.Data.UniversalBindingFlags );
            if( _quickStarted == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'quickStarted' in 'QuickStarter'",
                    "MainMenu_QuickStart" );
            }
        }

#endif

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.red : Color.white;
            }
        }

		public override bool RenderNow( bool anyMapFiles )
		{
#if DEBUG
            if( Prefs.DevMode == false )
            {
                return false;
            }
			return ( Current.ProgramState == ProgramState.Entry );
#else
            return false;
#endif
		}

		public override void ClickAction()
		{
#if DEBUG
            QuickStarted = true;
            SceneManager.LoadScene("Map");
#endif
		}

	}

}
