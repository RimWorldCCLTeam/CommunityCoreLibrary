using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class MainMenu_ReviewScenario : MainMenu
    {

        public override bool RenderNow( bool anyMapFiles )
        {
            return( Current.ProgramState == ProgramState.Playing );
        }

        public override void ClickAction()
        {
            Find.WindowStack.Add( new Dialog_MessageBox( Find.Scenario.GetFullInformationText(), Find.Scenario.name ) );
        }

    }

}
