using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_QuitToOS : IMainMenu
	{

		public bool RenderNow( bool anyWorldFiles, bool anyMapFiles )
		{
			return true;
		}

		public void ClickAction()
		{
			if( Game.Mode == GameMode.MapPlaying )
			{
				Find.WindowStack.Add( (Window)new Dialog_Confirm(
					"ConfirmQuit".Translate(),
					Root.Shutdown,
					false
				) );
			}
			else
			{
				Root.Shutdown();
			}
		}

	}

}
