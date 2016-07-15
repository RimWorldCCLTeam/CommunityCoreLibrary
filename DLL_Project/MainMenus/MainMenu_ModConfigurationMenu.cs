using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_ModConfigurationMenu : MainMenu
	{

		public override Color Color
		{
			get
			{
				return Controller.Data.RequireRestart ? Color.yellow : Color.white;
			}
		}

		public override bool RenderNow( bool anyMapFiles )
		{
            return ( !Controller.Data.MCMHosts.NullOrEmpty() );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Window_ModConfigurationMenu() );
		}

	}

}
