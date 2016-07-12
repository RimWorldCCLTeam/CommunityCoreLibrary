using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

	public class MainMenu_Credits : MainMenu
	{

		public override bool RenderNow( bool anyMapFiles )
		{
			return ( Current.ProgramState == ProgramState.Entry );
		}

		public override void ClickAction()
		{
			Find.WindowStack.Add( new Screen_Credits() );
		}

	}

}
