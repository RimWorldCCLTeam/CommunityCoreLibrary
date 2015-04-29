using System;
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
	public static class Toils_Common
	{
		public static Toil SpawnThingOfCountAt( ThingDef of, int count, IntVec3 at )
		{
			return new Toil
			{
				initAction = delegate
				{
					Common.SpawnThingOfCountAt( of, count, at );
				}
			};
		}

		public static Toil ReplaceThingWithThingOfCount( Thing oldThing, ThingDef of, int count )
		{
			return new Toil
			{
				initAction = delegate
				{
					Common.ReplaceThingWithThingOfCount( oldThing, of, count );
				}
			};
		}

		public static Toil RemoveDesignationOfAt( DesignationDef of, IntVec3 at )
		{
			return new Toil
			{
				initAction = delegate
				{
					Common.RemoveDesignationOfAt( of, at );
				}
			};
		}

		public static Toil RemoveDesignationOfOn( DesignationDef of, Thing on )
		{
			return new Toil
			{
				initAction = delegate
				{
					Common.RemoveDesignationOfOn( of, on );
				}
			};
		}
	}
}
