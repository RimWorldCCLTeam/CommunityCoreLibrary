using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _FoodUtility
    {

        internal static float FoodOptimalityUnusable = -9999999f;

        internal static float FoodOptimality( Thing thing, float dist )
        {
            if( thing == null )
            {
                return FoodOptimalityUnusable;
            }
            return FoodOptimalityByDef( thing.def, dist );
        }

        internal static float FoodOptimalityByDef( ThingDef def, float dist )
        {
            float num = dist;
            switch( def.ingestible.preferability )
            {
            case FoodPreferability.Plant:
                num += 500f;
                break;
            case FoodPreferability.NeverForNutrition:
                return FoodOptimalityUnusable;
            case FoodPreferability.DesperateOnly:
                num += 250f;
                break;
            case FoodPreferability.Raw:
                num += 80f;
                break;
            case FoodPreferability.Awful:
                num += 40f;
                break;
            case FoodPreferability.Simple:
                //num = num;
                break;
            case FoodPreferability.Fine:
                num -= 25f;
                break;
            case FoodPreferability.Lavish:
                num -= 40f;
                break;
            default:
                Log.Error("Unknown food taste.");
                break;
            }
            return -num;
        }

        internal static Thing _BestFoodSourceFor( Pawn getter, Pawn eater, bool fullDispensersOnly, out ThingDef foodDef )
        {
            var dispenserValidator = new DispenserValidator();
            dispenserValidator.getter = getter;
            dispenserValidator.fullDispensersOnly = fullDispensersOnly;
            Thing spawnedMeal = FoodUtility.BestFoodSpawnedFor( dispenserValidator.getter, eater, dispenserValidator.getter == eater );
            if( dispenserValidator.getter.RaceProps.ToolUser )
            {
                if( spawnedMeal != null )
                {
                    // Compare dispensers and synthesizers with best spawned meal
                    float dist = ( getter.Position - spawnedMeal.Position ).LengthManhattan;
                    dispenserValidator.meal.thing = spawnedMeal;
                    dispenserValidator.meal.def = spawnedMeal.def;
                    dispenserValidator.meal.score = FoodOptimality( spawnedMeal, dist );
                }
                else
                {
                    dispenserValidator.meal.thing = null;
                    dispenserValidator.meal.def = null;
                    dispenserValidator.meal.score = FoodOptimalityUnusable;
                }
                Predicate<Thing> validatorPredicate = new Predicate<Thing>( dispenserValidator.Validate );
                // Try to find a working nutrient paste dispenser or food sythesizer
                var dispensers = Find.ListerThings.AllThings.Where( t => (
                    ( t is Building_NutrientPasteDispenser )||
                    ( t is Building_FoodSynthesizer )
                ) );
                var dispenser = GenClosest.ClosestThingReachable(
                    dispenserValidator.getter.Position,
                    ThingRequest.ForUndefined(),
                    PathEndMode.InteractionCell,
                    TraverseParms.For(
                        dispenserValidator.getter,
                        dispenserValidator.getter.NormalMaxDanger() ),
                    9999f,
                    validatorPredicate,
                    dispensers,
                    -1,
                    true );
                if( dispenser != null )
                {
                    foodDef = dispenserValidator.meal.def;
                    return dispenser;
                }
            }
            foodDef = spawnedMeal == null ? (ThingDef) null : spawnedMeal.def;
            return spawnedMeal;
        }

        internal static float _NutritionAvailableFromFor( Thing t, Pawn p )
        {
            if(
                ( t.def.IsNutritionSource )&&
                ( p.RaceProps.WillAutomaticallyEat( t ) )
            )
            {
                return ( t.def.ingestible.nutrition * (float) t.stackCount );
            }
            if( p.RaceProps.ToolUser )
            {
                if( t is Building_NutrientPasteDispenser )
                {
                    var NPD = t as Building_NutrientPasteDispenser;
                    if( NPD.CanDispenseNow )
                    {
                        return ThingDefOf.MealNutrientPaste.ingestible.nutrition;
                    }
                }
                if( t is Building_FoodSynthesizer )
                {
                    var FS = t as Building_FoodSynthesizer;
                    var meal = FS.BestMealFrom();
                    if(
                        ( meal != null )&&
                        ( FS.CanDispenseNow( meal ) )
                    )
                    {
                        return meal.ingestible.nutrition;
                    }
                }
            }
            return 0.0f;
        }

        internal struct MealValidator
        {
            internal Thing              thing;
            internal ThingDef           def;
            internal float              score;
        }

        internal sealed class DispenserValidator
        {
            internal Pawn               getter;
            internal bool               fullDispensersOnly;
            internal MealValidator      meal;

            internal bool Validate( Thing t )
            {
                if(
                    ( t.Faction != this.getter.Faction )&&
                    ( t.Faction != this.getter.HostFaction )||
                    ( !SocialProperness.IsSociallyProper( t, this.getter ) )
                )
                {
                    return false;
                }
                var interactionCell = IntVec3.Invalid;
                var checkDef = (ThingDef) null;
                var building = (Building) t;
                var powerComp = building.TryGetComp<CompPowerTrader>();
                if(
                    ( !GenGrid.Standable( building.InteractionCell ) )||
                    ( ForbidUtility.IsForbidden( t, this.getter ) )||
                    ( !powerComp.PowerOn )
                )
                {
                    return false;
                }
                if( t is Building_NutrientPasteDispenser )
                {
                    var NPD = t as Building_NutrientPasteDispenser;
                    if(
                        ( this.fullDispensersOnly )&&
                        ( !NPD.HasEnoughFeedstockInHoppers() )
                    )
                    {
                        return false;
                    }
                    checkDef = ThingDefOf.MealNutrientPaste;
                    interactionCell = NPD.InteractionCell;
                }
                if( t is Building_FoodSynthesizer )
                {
                    var FS = t as Building_FoodSynthesizer;
                    checkDef = FS.BestMealFrom();
                    if(
                        ( this.fullDispensersOnly )&&
                        ( checkDef == null )
                    )
                    {
                        return false;
                    }
                    interactionCell = FS.InteractionCell;
                }
                if( checkDef != null )
                {
                    var dist = ( getter.Position - interactionCell ).LengthManhattan;
                    var checkScore = FoodOptimalityByDef( checkDef, dist );
                    if( checkScore > meal.score )
                    {
                        meal.thing = t;
                        meal.def = checkDef;
                        meal.score = checkScore;
                        return true;
                    }
                }
                return false;
            }

        }

    }

}
