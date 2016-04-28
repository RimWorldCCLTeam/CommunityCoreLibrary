using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_HelpMenu : MainMenu
	{

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
            if(
                ( Controller.Data.RequireRestart )&&
                ( !Controller.Data.ContinueWithoutRestart )
            )
            {
                return false;
            }
			return ( Game.Mode == GameMode.Entry );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new MainTabWindow_ModHelp() );
		}

	}

}
