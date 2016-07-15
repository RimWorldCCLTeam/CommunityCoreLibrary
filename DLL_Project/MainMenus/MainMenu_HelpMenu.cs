using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_HelpMenu : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.yellow : Color.white;
            }
        }

		public override bool RenderNow( bool anyMapFiles )
		{
			return ( Current.ProgramState == ProgramState.Entry );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new MainTabWindow_ModHelp() );
		}

	}

}
