using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _GenConstruct
    {

        [DetourClassMethod( typeof( GenConstruct ), "CanBuildOnTerrain" )]
        internal static bool _CanBuildOnTerrain( BuildableDef entDef, IntVec3 c, Rot4 rot, Thing thingToIgnore = null )
        {

            CompProperties_RestrictedPlacement Restrictions = null;
            if( entDef is TerrainWithComps )
            {
                var terrainWithComps = entDef as TerrainWithComps;
                if(
                    ( !terrainWithComps.placeWorkers.NullOrEmpty() ) &&
                    ( terrainWithComps.placeWorkers.Contains( typeof( PlaceWorker_OnlyOnTerrain ) ) )
                )
                {
                    Restrictions = terrainWithComps.GetCompProperties( typeof( CompRestrictedPlacement ) ) as CompProperties_RestrictedPlacement;
                }
            }
            else if( entDef is ThingDef )
            {
                var thingDef = entDef as ThingDef;
                if(
                    ( !thingDef.placeWorkers.NullOrEmpty() ) &&
                    ( thingDef.placeWorkers.Contains( typeof( PlaceWorker_OnlyOnTerrain ) ) )
                )
                {
                    Restrictions = thingDef.GetCompProperties<CompProperties_RestrictedPlacement>();
                }
            }
            if( Restrictions != null )
            {
                var cellRect = GenAdj.OccupiedRect( c, rot, entDef.Size );
                cellRect.ClipInsideMap();
                foreach( var cell in cellRect )
                {
                    if( !Restrictions.RestrictedTerrain.Contains( cell.GetTerrain() ) )
                    {
                        return false;
                    }
                    var thingList = cell.GetThingList();
                    for( int index = 0; index < thingList.Count; ++index )
                    {
                        if( thingList[ index ] != thingToIgnore )
                        {
                            var terrainDef = thingList[ index ].def.entityDefToBuild as TerrainDef;
                            if(
                                ( terrainDef != null )&&
                                ( !Restrictions.RestrictedTerrain.Contains( terrainDef ) )
                            )
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                // Use the vanilla method to check
                if(
                    ( entDef is TerrainDef ) &&
                    ( !c.GetTerrain().changeable )
                )
                {
                    return false;
                }
                var cellRect = GenAdj.OccupiedRect( c, rot, entDef.Size );
                cellRect.ClipInsideMap();
                foreach( var cell in cellRect )
                {
                    if( !cell.GetTerrain().affordances.Contains( entDef.terrainAffordanceNeeded ) )
                    {
                        return false;
                    }
                    var thingList = cell.GetThingList();
                    for( int index = 0; index < thingList.Count; ++index )
                    {
                        if( thingList[ index ] != thingToIgnore )
                        {
                            var terrainDef = thingList[ index ].def.entityDefToBuild as TerrainDef;
                            if(
                                ( terrainDef != null )&&
                                ( !terrainDef.affordances.Contains( entDef.terrainAffordanceNeeded ) )
                            )
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            // Allow placement
            return true;
        }

    }

}
