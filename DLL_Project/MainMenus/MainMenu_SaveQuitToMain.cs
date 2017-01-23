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

        public override bool RenderNow( bool anyMapFiles )
        {
            return(
                ( Current.ProgramState == ProgramState.Playing )&&
                ( Current.Game.Info.permadeathMode )
            );
        }

        public override void ClickAction()
        {
            LongEventHandler.QueueLongEvent(
                () =>
                    {
                        GameDataSaveLoader.SaveGame( Current.Game.Info.permadeathModeUniqueName );
                        Verse.Profile.MemoryUtility.ClearAllMapsAndWorld();

                        // A14 <= (A13) GameDataSaver.SaveGame( Find.Map, Find.Map.info.permadeathModeUniqueName );
                    },
                "Entry",
                "SavingLongEvent",
                false,
                null
            );
        }

    }

}
