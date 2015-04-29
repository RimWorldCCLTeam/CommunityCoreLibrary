using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
	public class PlaceWorker_WallAttachment : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing( EntityDef checkingDef, IntVec3 loc, Rot4 rot )
		{
			IntVec3 c = loc + rot.FacingCell * -1;
			if( !c.InBounds() )
			{
				return false;
			}
			if( c.GetEdifice() == null )
			{
				return "MessagePlacementAgainstSupport".Translate();
			}
			return true;
		}
	}
}

