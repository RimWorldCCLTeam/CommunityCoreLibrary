using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_Options : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return true;
        }

        public void                         ClickAcion()
        {
            Find.WindowStack.Add( (Window) new Dialog_Options() );
        }

    }

}
