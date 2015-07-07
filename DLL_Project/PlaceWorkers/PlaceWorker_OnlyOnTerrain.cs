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

    public class PlaceWorker_OnlyOnTerrain : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            ThingDef def = checkingDef as ThingDef;

            var Restrictions = def.GetCompProperties( typeof( RestrictedPlacement_Comp ) ) as RestrictedPlacement_Properties;
            if( Restrictions == null ){
                Log.Error( "Could not get restrictions!" );
                return false;
            }

            TerrainDef terrainDef = loc.GetTerrain();
            for( int i = 0; i < Restrictions.RestrictedTerrain.Count; i++ )
                if( Restrictions.RestrictedTerrain[ i ] == terrainDef )
                    return true;

            return "MessagePlacementNotOn".Translate() + terrainDef.label;
        }
    }
}

