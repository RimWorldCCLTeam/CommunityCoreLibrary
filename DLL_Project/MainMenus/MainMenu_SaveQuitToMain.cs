using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_SaveQuitToMain : MainMenu
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
			return(
                ( Game.Mode == GameMode.MapPlaying )&&
                ( Find.Map.info.permadeathMode )
            );
		}

		public override void ClickAction()
		{
            LongEventHandler.QueueLongEvent(
                () =>
                    {
                        GameDataSaver.SaveGame( Find.Map, Find.Map.info.permadeathModeUniqueName );
                    },
                "Entry",
                "SavingLongEvent",
                false,
                null
            );
		}

	}

}
