using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_LoadGame : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return ( anyMapFiles );
        }

        public void                         ClickAcion()
        {
            Find.WindowStack.Add( (Window) new Dialog_MapList_Load() );
        }

    }

}
