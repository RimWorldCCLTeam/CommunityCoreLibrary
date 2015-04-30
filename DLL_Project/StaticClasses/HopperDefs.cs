using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

	public static class HopperDefs
	{
		private static List<ThingDef>			cachedDefs;
		private static int						lastIndex = 0;

		private static void FillDefs()
		{
			List<ThingDef> allThings = DefDatabase< ThingDef >.AllDefsListForReading;
			cachedDefs = new List<ThingDef>();

			// Make sure the vanilla hopper is in the list
			cachedDefs.Add( ThingDefOf.Hopper );

			// Look for any ThingDef with the common hopper comp property
			foreach( ThingDef t in allThings )
			{
				if( t.GetCompProperties( typeof( CompProperties_Hopper ) ) != null )
					cachedDefs.Add( t );
			}
		}

		public static ThingDef					NextDef
		{
			// Cycle through the list in a loop without end
			get
			{
				if( cachedDefs == null )
					FillDefs();

				if( lastIndex >= cachedDefs.Count )
					lastIndex = 0;

				return cachedDefs[ lastIndex++ ];
			}
		}

		public static List<ThingDef>			AllDefs
		{
			get
			{
				if( cachedDefs == null )
					FillDefs();
				return cachedDefs;
			}
		}
	}
}

