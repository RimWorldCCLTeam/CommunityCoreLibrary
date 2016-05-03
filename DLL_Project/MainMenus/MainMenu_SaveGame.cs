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

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return (
                ( Game.Mode == GameMode.MapPlaying )&&
                ( !Find.Map.info.permadeathMode )
            );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Save() );
		}

	}

}
