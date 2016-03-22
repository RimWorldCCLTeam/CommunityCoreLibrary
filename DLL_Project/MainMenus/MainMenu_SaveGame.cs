using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_SaveGame : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return ( Game.Mode == GameMode.MapPlaying );
        }

        public void                         ClickAcion()
        {
            Find.WindowStack.Add( (Window) new Dialog_MapList_Save() );
        }

    }

}
