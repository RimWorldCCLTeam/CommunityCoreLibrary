using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_SaveGame : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.red : Color.white;
            }
        }

		public override bool RenderNow( bool anyMapFiles )
		{
			return (
                ( Current.ProgramState == ProgramState.MapPlaying )&&
                ( !Current.Game.Info.permadeathMode )
            );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Save() );
		}

	}

}
