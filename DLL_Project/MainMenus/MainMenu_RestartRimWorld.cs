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
                return Color.green;
            }
        }

        public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
        {
            return Controller.Data.RequireRestart;
        }

		public override void ClickAction()
		{
            Controller.MainMonoBehaviour.RestartRimWorld();
		}

	}

}
