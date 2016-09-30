using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_RestartRimWorld : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.green : Color.white;
            }
        }

        public override bool RenderNow( bool anyMapFiles )
        {
#if DEBUG
            if( Prefs.DevMode )
            {
                return true;
            }
#endif
            return Controller.Data.RequireRestart;
        }

		public override void ClickAction()
		{
            Controller.RimWorld.Restart();
		}

	}

}
