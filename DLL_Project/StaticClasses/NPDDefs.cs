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

	public static class NPDDefs
	{
		private static List<ThingDef>			cachedDefs;

		public static List<ThingDef>			AllDefs
		{
			get
			{
				if( cachedDefs == null )
				{
					List<ThingDef> allThings = DefDatabase< ThingDef >.AllDefsListForReading;
					cachedDefs = new List<ThingDef>();

					// Make sure the vanilla NPD is in the list
					cachedDefs.Add( ThingDefOf.NutrientPasteDispenser );

					// Look for any ThingDef in the common NPD class
					foreach( ThingDef t in allThings )
					{
						if( t.thingClass.GetType() == typeof( CommunityCoreLibrary.Building_NutrientPasteDispenser ) )
							cachedDefs.Add( t );
					}
				}
				return cachedDefs;
			}
		}
	}

}
