using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_LoadGame : MainMenu
	{

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return ( anyMapFiles );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Load() );
		}

	}

}
