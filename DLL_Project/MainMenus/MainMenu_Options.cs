using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class MainMenu_Options : MainMenu
    {

        public override void ClickAction()
        {
            MainMenuDrawer_Extensions.CloseMainTab();
            Find.WindowStack.Add( (Window)new Dialog_Options() );
        }

    }

}
