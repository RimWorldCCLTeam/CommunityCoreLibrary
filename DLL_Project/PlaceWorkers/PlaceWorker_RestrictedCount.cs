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

    public class PlaceWorker_RestrictedCount : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            ThingDef def = checkingDef as ThingDef;

            var Restrictions = def.GetCompProperties( typeof( RestrictedPlacement_Comp ) ) as RestrictedPlacement_Properties;
            if( Restrictions == null ){
                Log.Error( "Could not get restrictions!" );
                return false;
            }

            // Get the current count of instances and blueprints of
            int count = Enumerable.Count<Building>(
                Enumerable.Where<Building>(
                    (IEnumerable<Building>) Find.ListerBuildings.allBuildingsColonist, 
                    (Func<Building, bool>) (b => b.def == def) ) )
                + Find.ListerThings.ThingsOfDef( def.blueprintDef ).Count;

            if( count < Restrictions.MaxCount )
                return true;

            return "MessagePlacementCountRestricted".Translate() + Restrictions.MaxCount;
        }
    }
}

