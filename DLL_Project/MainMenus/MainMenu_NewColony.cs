using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_NewColony : MainMenu
	{

        public override Color               Color
        {
            get
            {
                return Controller.Data.RequireRestart ? Color.red : Color.white;
            }
        }

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return (
				( Game.Mode == GameMode.Entry ) &&
				( anyWorldFiles )
			);
		}

		public override void ClickAction()
		{
			MapInitData.Reset();
			Find.WindowStack.Add( (Window)new Page_SelectStoryteller() );
		}

	}

}
