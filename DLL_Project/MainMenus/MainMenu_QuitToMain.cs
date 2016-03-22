using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class                            MainMenu_QuitToMain : IMainMenu
    {

        public bool                         RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return ( Game.Mode == GameMode.MapPlaying );
        }

        public void                         ClickAcion()
        {
            Find.WindowStack.Add( (Window) new Dialog_Confirm(
                "ConfirmQuit".Translate(),
                () =>
            {
                Application.LoadLevel( "Entry" );
            },
                false
            ) );
        }

    }

}
