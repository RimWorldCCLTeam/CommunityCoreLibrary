using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_NewColony : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return ( Game.Mode == GameMode.Entry )&&( anyWorldFiles );
        }

        public void                         ClickAcion()
        {
            MapInitData.Reset();
            Find.WindowStack.Add( (Window) new Page_SelectStoryteller() );
        }

    }

}
