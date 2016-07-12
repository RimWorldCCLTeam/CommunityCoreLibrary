using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_Mods : MainMenu
	{

		public override bool RenderNow( bool anyMapFiles )
		{
			return ( Current.ProgramState == ProgramState.Entry );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( (Window)new Page_ModsConfig() );
		}

	}

}
