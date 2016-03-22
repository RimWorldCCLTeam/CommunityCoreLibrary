using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_ModConfigurationMenu : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return ( Window_ModConfigurationMenu.AnyMenus );
        }

        public void                         ClickAcion()
        {
            Find.WindowStack.Add( (Window) new Window_ModConfigurationMenu() );
        }

    }

}
