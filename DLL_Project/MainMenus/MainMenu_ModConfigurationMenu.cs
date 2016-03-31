using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_ModConfigurationMenu : MainMenu
	{

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return ( Window_ModConfigurationMenu.AnyMenus );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Window_ModConfigurationMenu() );
		}

	}

}
