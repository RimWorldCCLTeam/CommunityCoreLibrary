using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class MainMenu_ReviewScenario : MainMenu
    {

        public override bool RenderNow( bool anyMapFiles )
        {
            return( Current.ProgramState == ProgramState.MapPlaying );
        }

        public override void ClickAction()
        {
            Find.WindowStack.Add( (Window)new Dialog_Message( Find.Scenario.GetFullInformationText(), Find.Scenario.name ) );
        }

    }

}
