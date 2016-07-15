using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_QuitToOS : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.green : Color.white;
            }
        }

        public override bool RenderNow( bool anyMapFiles )
        {
            return(
                ( Current.ProgramState == ProgramState.Entry )||
                (
                    ( !Current.Game.Info.permadeathMode )||
                    ( Controller.Data.RequireRestart )
                )
            );
        }

		public override void ClickAction()
		{
			if( Current.ProgramState == ProgramState.MapPlaying )
			{
				Find.WindowStack.Add( (Window)new Dialog_Confirm(
					"ConfirmQuit".Translate(),
					Root.Shutdown,
					false
				) );
			}
			else
			{
				Root.Shutdown();
			}
		}

	}

}
