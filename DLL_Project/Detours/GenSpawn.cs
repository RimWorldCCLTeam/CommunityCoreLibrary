using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _GenSpawn
    {
        
        // This method is to remove the hard-coded references allowing more flexibility in
        // building placements.  Specifically, it removes the steam geyser/geothermal generator
        // lock.
        internal static bool                _CanPlaceBlueprintOver( BuildableDef newDef, ThingDef oldDef )
        {
            if( oldDef.EverHaulable )
            {
                return true;
            }

            // Handle steam geysers in a mod friendly way (not geothermal exclusive)
            // By default, nothing can be placed on steam geysers without a place worker which allows it
            if( oldDef == ThingDefOf.SteamGeyser )
            {
                if( newDef.placeWorkers.NullOrEmpty() )
                {
                    // No place workers means nothing to allow it
                    return false;
                }
                if( newDef.placeWorkers.Contains( typeof( PlaceWorker_OnSteamGeyser ) ) )
                {
                    return true;
                }
                if( newDef.placeWorkers.Contains( typeof( PlaceWorker_OnlyOnThing ) ) )
                {
                    var Restrictions = newDef.RestrictedPlacement_Properties();
#if DEBUG
                    if( Restrictions == null )
                    {
                        CCL_Log.Error( "PlaceWorker_OnlyOnThing unable to get properties!", newDef.defName );
                        return false;
                    }
#endif
                    if( Restrictions.RestrictedThing.Contains( ThingDefOf.SteamGeyser ) )
                    {
                        return true;
                    }
                }
                return false;
            }

            ThingDef newThingDef = newDef as ThingDef;
            ThingDef oldThingDef = oldDef;
            BuildableDef buildableDef = GenSpawn.BuiltDefOf( oldDef );
            ThingDef resultThingDef = buildableDef as ThingDef;

            if(
                ( oldDef.category == ThingCategory.Plant )&&
                ( oldDef.passability == Traversability.Impassable )&&
                (
                    ( newThingDef != null )&&
                    ( newThingDef.category == ThingCategory.Building )
                )&&
                ( !newThingDef.building.canPlaceOverImpassablePlant )
            )
            {
                return false;
            }

            if(
                ( oldDef.category != ThingCategory.Building )&&
                ( !oldDef.IsBlueprint )&&
                ( !oldDef.IsFrame )
            )
            {
                return true;
            }


            if( newThingDef != null )
            {
                if( !EdificeUtility.IsEdifice( (BuildableDef) newThingDef ) )
                {
                    return
                        (
                            ( oldDef.building == null )||
                            ( oldDef.building.canBuildNonEdificesUnder )
                        )&&
                        (
                            ( !newThingDef.EverTransmitsPower )||
                            ( !oldDef.EverTransmitsPower )
                        );
                }
                if(
                    ( EdificeUtility.IsEdifice( (BuildableDef) newThingDef ) )&&
                    ( oldThingDef != null )&&
                    (
                        ( oldThingDef.category == ThingCategory.Building )&&
                        ( !EdificeUtility.IsEdifice( (BuildableDef) oldThingDef ) )
                    )
                )
                {
                    return
                        ( newThingDef.building == null )||
                        ( newThingDef.building.canBuildNonEdificesUnder );
                }
                if(
                    ( resultThingDef != null )&&
                    ( resultThingDef == ThingDefOf.Wall )&&
                    (
                        ( newThingDef.building != null )&&
                        ( newThingDef.building.canPlaceOverWall )
                    )||
                    ( newDef != ThingDefOf.PowerConduit )&&
                    ( buildableDef == ThingDefOf.PowerConduit )
                )
                {
                    return true;
                }
            }

            return
                ( newDef is TerrainDef )&&
                ( buildableDef is ThingDef )&&
                ( ( (ThingDef) buildableDef ).CoexistsWithFloors )||
                ( buildableDef is TerrainDef )&&
                ( !( newDef is TerrainDef ) );
        }

    }

}
