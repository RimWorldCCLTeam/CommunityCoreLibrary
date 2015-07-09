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
    public class PlaceWorker_OnlyOnSurface : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            bool placementOk = false;
            foreach( Thing curThing in loc.GetThingList() )
            {
                if( curThing.def.surfaceType != SurfaceType.None )
                {
                    placementOk = true;
                    break;
                }
            }
            AcceptanceReport result;
            if( placementOk )
            {
                result = true;
            }
            else
            {
                result = "MessagePlacementItemSurface".Translate();
            }
            return result;
        }
    }
}

