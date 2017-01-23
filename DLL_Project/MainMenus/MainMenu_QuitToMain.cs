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
                ( Current.ProgramState == ProgramState.Playing )&&
                ( !Current.Game.Info.permadeathMode )
            );
        }

        public override void ClickAction()
        {
            if( GameDataSaveLoader.CurrentGameStateIsValuable )
            {
                Find.WindowStack.Add( Dialog_MessageBox.CreateConfirmation(
                    "ConfirmQuit".Translate(),
                    GenScene.GoToMainMenu,
                    true,
                    null
                ) );
            }
            else
            {
                GenScene.GoToMainMenu();
            }
        }

    }

}
