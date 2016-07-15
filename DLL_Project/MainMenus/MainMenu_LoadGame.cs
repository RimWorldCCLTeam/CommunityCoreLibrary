using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_LoadGame : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.red : Color.white;
            }
        }

		public override bool RenderNow( bool anyMapFiles )
		{
			return ( anyMapFiles );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Load() );
		}

	}

}
