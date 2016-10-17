using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_LearnToPlay : MainMenu
	{

        static MethodInfo                   _MainMenuDrawer_InitLearnToPlay;

        static                              MainMenu_LearnToPlay()
        {
            _MainMenuDrawer_InitLearnToPlay = typeof( MainMenuDrawer ).GetMethod( "InitLearnToPlay", Controller.Data.UniversalBindingFlags );
            if( _MainMenuDrawer_InitLearnToPlay == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'InitLearnToPlay' in 'MainMenuDrawer'",
                    "MainMenu_LearnToPlay" );
            }
        }

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.red : Color.white;
            }
        }

		public override bool                RenderNow( bool anyMapFiles )
		{
			return ( Current.ProgramState == ProgramState.Entry );
		}

		public override void                ClickAction()
		{
            _MainMenuDrawer_InitLearnToPlay.Invoke( null, null );
		}

	}

}
