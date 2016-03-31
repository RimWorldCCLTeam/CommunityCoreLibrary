using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_SaveGame : MainMenu
	{

		public override bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return ( Game.Mode == GameMode.MapPlaying );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Dialog_MapList_Save() );
		}

	}

}
