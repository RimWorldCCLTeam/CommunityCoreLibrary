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
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            IntVec3 c = loc + rot.FacingCell * -1;
            if( !c.InBounds() )
            {
                return false;
            }
            Building support = c.GetEdifice();
            if( ( support == null )||
                ( ( support.def.graphicData.linkFlags & ( LinkFlags.Rock | LinkFlags.Wall ) ) == 0 ) )
            {
                return "MessagePlacementAgainstSupport".Translate();
            }
            return true;
        }
    }
}

