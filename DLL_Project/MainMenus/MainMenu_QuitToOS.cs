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

        public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return(
                ( Game.Mode == GameMode.Entry )||
                (
                    ( !Find.Map.info.permadeathMode )||
                    ( Controller.Data.RequireRestart )
                )
            );
        }

		public override void ClickAction()
		{
			if( Game.Mode == GameMode.MapPlaying )
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
