using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_QuitToMain : MainMenu
	{

		public override bool RenderNow( bool anyMapFiles )
		{
            return(
                ( Current.ProgramState == ProgramState.MapPlaying )&&
                ( !Current.Game.Info.permadeathMode )
            );
		}

		public override void ClickAction()
		{
            if( GameDataSaveLoader.CurrentMapStateIsValuable )
            {
    			Find.WindowStack.Add( (Window)new Dialog_Confirm(
    				"ConfirmQuit".Translate(),
    				RootMap.GoToMainMenu,
    				true,
                    null,
                    true
    			) );
            }
            else
            {
                RootMap.GoToMainMenu();
            }
		}

	}

}
