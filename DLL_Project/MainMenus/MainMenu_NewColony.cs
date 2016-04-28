using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_NewColony : MainMenu
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
			return(
                ( Game.Mode == GameMode.Entry )&&
                ( anyWorldFiles )
            );
		}

		public override void ClickAction()
		{
			MapInitData.Reset();
			Find.WindowStack.Add( (Window)new Page_SelectStoryteller() );
		}

	}

}
