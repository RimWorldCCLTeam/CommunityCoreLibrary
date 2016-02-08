using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _GenPlant
    {

        internal static bool _CanEverPlantAt( this ThingDef plantDef, IntVec3 c )
        {
            if( plantDef.category != ThingCategory.Plant )
            {
                Verse.Log.Error( "Checking CanGrowAt with " + (object)plantDef + " which is not a plant." );
            }
            if(
                ( !GenGrid.InBounds( c ) )||
                ( (double) Find.FertilityGrid.FertilityAt( c ) < (double) plantDef.plant.fertilityMin )
            )
            {
                return false;
            }
            List<Thing> list = Find.ThingGrid.ThingsListAt( c );
            for( int index = 0; index < list.Count; ++index )
            {
                Thing thing = list[ index ];
                if(
                    ( thing.def.BlockPlanting )||
                    ( plantDef.passability == Traversability.Impassable )&&
                    (
                        ( thing.def.category == ThingCategory.Pawn )||
                        ( thing.def.category == ThingCategory.Item )||
                        (
                            ( thing.def.category == ThingCategory.Building )||
                            ( thing.def.category == ThingCategory.Plant )
                        )
                    )
                )
                {
                    return false;
                }
            }
            if( plantDef.passability == Traversability.Impassable )
            {
                for( int index = 0; index < 4; ++index )
                {
                    IntVec3 c1 = c + GenAdj.CardinalDirections[ index ];
                    if( GenGrid.InBounds( c1 ) )
                    {
                        Building edifice = GridsUtility.GetEdifice( c1 );
                        if(
                            ( edifice != null )&&
                            (
                                ( edifice.def.thingClass == typeof( Building_Door ) )||
                                ( edifice.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                            )
                        )
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }

}
