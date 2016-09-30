using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _WorkGiver_Warden_DeliverFood : WorkGiver_Warden_DeliverFood
    {

        #region Helper Methods

        internal static float               NutritionAvailableForFrom( Pawn p, Thing foodSource )
        {
            if(
                ( foodSource.def.IsNutritionGivingIngestible )&&
                ( p.RaceProps.WillAutomaticallyEat( foodSource ) )
            )
            {
                return foodSource.def.ingestible.nutrition * (float) foodSource.stackCount;
            }
            if(
                ( p.RaceProps.ToolUser )&&
                ( p.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                ( foodSource.def.IsFoodDispenser )
            )
            {
                if( foodSource is Building_NutrientPasteDispenser )
                {
                    var NPD = foodSource as Building_NutrientPasteDispenser;
                    if( NPD.CanDispenseNow )
                    {
                        return 99999f;
                    }
                }
                if( foodSource is Building_AutomatedFactory )
                {
                    var FS = foodSource as Building_AutomatedFactory;
                    var foodDef = FS.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                    if( foodDef != null )
                    {
                        return 99999f;
                    }
                }
            }
            return 0.0f;
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( WorkGiver_Warden_DeliverFood ), "FoodAvailableInRoomTo" )]
        internal static bool                _FoodAvailableInRoomTo( Pawn prisoner )
        {
            if(
                ( prisoner.carrier.CarriedThing != null )&&
                ( NutritionAvailableForFrom( prisoner, prisoner.carrier.CarriedThing ) > 0.0f )
            )
            {
                //Log.Message( "Prisoner is carrying food" );
                return true;
            }
            var neededNutrition = 0.0f;
            var foodNutrition = 0.0f;
            var room = prisoner.Position.GetRoom();
            if( room == null )
            {   // This should never actually happen...
                //Log.Message( "Prisoner is not in a room!" );
                return false;
            }
            for( int regionIndex = 0; regionIndex < room.RegionCount; ++regionIndex )
            {
                var region = room.Regions[ regionIndex ];
                var foodSources = region.ListerThings.ThingsInGroup( ThingRequestGroup.FoodSourceNotPlantOrTree );
                if(
                    ( prisoner.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                    ( foodSources.Any( (source) =>
                {
                    if( source.def.IsFoodDispenser )
                    {   // Properly handle Nutrient Paste Dispensers and Food Synthesizers (Automated Factories)
                        if(
                            ( source is Building_NutrientPasteDispenser )&&
                            ( ((Building_NutrientPasteDispenser)source).CanDispenseNow )
                        )
                        {
                            return true;
                        }
                        if(
                            ( source is Building_AutomatedFactory )&&
                            ( ((Building_AutomatedFactory)source).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal ) != null )
                        )
                        {
                            return true;
                        }
                    }
                    return false;
                } ) )
                )
                {
                    //Log.Message( "Prisoner has access to a stocked food machine" );
                    return true;
                }
                for( int foodIndex = 0; foodIndex < foodSources.Count; ++foodIndex )
                {
                    var foodSource = foodSources[ foodIndex ];
                    if(
                        ( foodSource.def.IsFoodDispenser )||
                        (
                            ( foodSource.def.IsNutritionGivingIngestible )&&
                            ( foodSource.def.ingestible.preferability > FoodPreferability.NeverForNutrition )
                        )
                    )
                    {
                        foodNutrition += NutritionAvailableForFrom( prisoner, foodSource );
                    }
                }
                var pawns = region.ListerThings.ThingsInGroup( ThingRequestGroup.Pawn );
                for( int pawnIndex = 0; pawnIndex < pawns.Count; ++pawnIndex )
                {
                    var pawn = pawns[ pawnIndex ] as Pawn;
                    if(
                        ( pawn.IsPrisonerOfColony )&&
                        ( pawn.needs.food.CurLevelPercentage < ( pawn.needs.food.PercentageThreshHungry + 0.0199999995529652 ) )&&
                        (
                            ( pawn.carrier.CarriedThing == null )||
                            ( !pawn.RaceProps.WillAutomaticallyEat( pawn.carrier.CarriedThing ) )
                        )
                    )
                    {
                        neededNutrition += pawn.needs.food.NutritionWanted;
                    }
                }
            }
            //Log.Message( string.Format( "return {0} + 0.5f >= {1};", foodNutrition, neededNutrition ) );
            return foodNutrition + 0.5f >= neededNutrition;
        }

        #endregion

    }

}
