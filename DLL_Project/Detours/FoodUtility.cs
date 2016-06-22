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

        internal const float FoodOptimalityUnusable = -9999999f;

        internal static MethodInfo  _BestPawnToHuntForPredator;

        internal static Pawn        BestPawnToHuntForPredator( Pawn predator )
        {
            if( _BestPawnToHuntForPredator == null )
            {
                _BestPawnToHuntForPredator = typeof( FoodUtility ).GetMethod( "BestPawnToHuntForPredator", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (Pawn) _BestPawnToHuntForPredator.Invoke( null, new System.Object[] { predator } );
        }

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

            switch ( def.ingestible.preferability )
            {
                case FoodPreferability.NeverForNutrition:
                    return FoodOptimalityUnusable;
                case FoodPreferability.DesperateOnly:
                    num += 250f;
                    break;
                case FoodPreferability.RawBad:
                    num += 80f;
                    break;
                case FoodPreferability.RawTasty:
                    // A14 - new category, check me!
                    num += 60f;
                    break;
                case FoodPreferability.MealAwful:
                    num += 40f;
                    break;
                case FoodPreferability.MealSimple:
                    break;
                case FoodPreferability.MealFine:
                    num -= 25f;
                    break;
                case FoodPreferability.MealLavish:
                    num -= 40f;
                    break;
                default:
                    Log.Warning( "Food preferability for " + def.LabelCap + " not set." );
                    return FoodOptimalityUnusable;
            }
            return -num;
        }

        internal static Thing _BestFoodSourceFor( Pawn getter, Pawn eater, bool fullDispensersOnly, out ThingDef foodDef )
        {
            var dispenserValidator = new DispenserValidator();
            dispenserValidator.getter = getter;
            dispenserValidator.fullDispensersOnly = fullDispensersOnly;

            // A14 - method signature is drastically changed - desperate is now a required parameter, where before it was inferred.
            bool desperate = eater.needs.food.CurCategory == HungerCategory.Starving;
            Thing bestFoodSpawnedFor = FoodUtility.BestFoodSourceOnMap( getter, eater, desperate, allowPlant: getter == eater );

            if(
                ( getter == eater )&&
                ( getter.RaceProps.predator )&&
                ( bestFoodSpawnedFor == null )
            )
            {
                Pawn prey = BestPawnToHuntForPredator( getter );
                if( prey != null )
                {
                    foodDef = prey.RaceProps.corpseDef;
                    return (Thing) prey;
                }
            }

            if( getter.RaceProps.ToolUser )
            {
                // Try to find a working nutrient paste dispenser or food sythesizer
                var validatorPredicate = new Predicate<Thing>( dispenserValidator.Validate );
                var dispensers = Find.ListerThings.AllThings.Where( t => (
                    ( t is Building_NutrientPasteDispenser )||
                    (
                        ( t is Building_AutomatedFactory )&&
                        ( ((Building_AutomatedFactory)t).CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.DirectToPawn )
                    )
                ) );
                if( dispensers.Any() )
                {
                    // Check dispenses and synthesizers (automated factories)
                    if( bestFoodSpawnedFor != null )
                    {
                        // Compare with best spawned meal
                        float dist = ( getter.Position - bestFoodSpawnedFor.Position ).LengthManhattan;
                        dispenserValidator.meal.thing = bestFoodSpawnedFor;
                        dispenserValidator.meal.def = bestFoodSpawnedFor.def;
                        dispenserValidator.meal.score = FoodOptimality( bestFoodSpawnedFor, dist );
                    }
                    else
                    {
                        // Nothing to compare to
                        dispenserValidator.meal.thing = null;
                        dispenserValidator.meal.def = null;
                        dispenserValidator.meal.score = FoodOptimalityUnusable;
                    }

                    // Now find the best/closest dispenser
                    var dispenser = GenClosest.ClosestThingReachable(
                        getter.Position,
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
                        // Found a dispenser/synthesizer and it's better than the spawned meal
                        foodDef = dispenserValidator.meal.def;
                        return dispenser;
                    }
                }
            }
            foodDef = bestFoodSpawnedFor == null ? null : bestFoodSpawnedFor.def;
            return bestFoodSpawnedFor;
        }

        internal static float _NutritionAvailableFromFor( Thing t, Pawn p )
        {
            if(
                ( t.def.IsNutritionGivingIngestible )&&
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
                if( t is Building_AutomatedFactory )
                {
                    var FS = t as Building_AutomatedFactory;
                    var meal = FS.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
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
                    ( !t.IsSociallyProper( this.getter ) )
                )
                {
                    return false;
                }
                var interactionCell = IntVec3.Invalid;
                var checkDef = (ThingDef) null;
                var building = (Building) t;
                var powerComp = building.TryGetComp<CompPowerTrader>();
                if(
                    ( !building.InteractionCell.Standable() )||
                    ( t.IsForbidden( this.getter ) )||
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
                if( t is Building_AutomatedFactory )
                {
                    var FS = t as Building_AutomatedFactory;
                    checkDef = FS.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
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
