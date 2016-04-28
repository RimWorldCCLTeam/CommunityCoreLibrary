using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_LoadGame : MainMenu
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
			return ( anyMapFiles );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Load() );
		}

	}

}
