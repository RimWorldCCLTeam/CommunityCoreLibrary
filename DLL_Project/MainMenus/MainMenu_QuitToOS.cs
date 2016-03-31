using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_QuitToOS : MainMenu
	{

		public override void ClickAction()
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
