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

    public class PlaceWorker_OnlyOnThing : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            ThingDef def = checkingDef as ThingDef;

            var Restrictions = def.GetCompProperties( typeof( RestrictedPlacement_Comp ) ) as RestrictedPlacement_Properties;
            if( Restrictions == null ){
                Log.Error( "Could not get restrictions!" );
                return (AcceptanceReport)false;
            }

            // Override steam-geyser restriction
            if( ( Restrictions.RestrictedThing.FindIndex( r => r == ThingDefOf.SteamGeyser ) >= 0 )&&
                ( ThingDefOf.GeothermalGenerator != ( checkingDef as ThingDef ) ) ){
                var newGeo = checkingDef as ThingDef;
                ThingDefOf.GeothermalGenerator = newGeo;
            }

            foreach( Thing t in loc.GetThingList() ){
                if( ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                    ( t.Position == loc ) )
                    return (AcceptanceReport)true;
            }

            return "MessagePlacementNotHere".Translate();
        }
    }
}

