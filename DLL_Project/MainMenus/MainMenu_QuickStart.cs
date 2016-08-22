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
            _quickStarted = QuickStarter.GetField( "quickStarted", BindingFlags.Static | BindingFlags.NonPublic );
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
