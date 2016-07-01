using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _FoodUtility
    {

        internal const float FoodOptimalityUnusable = -9999999f;

        internal static MethodInfo _BestPawnToHuntForPredator;
        internal static MethodInfo _FoodSourceOptimality;
        internal static MethodInfo _SpawnedFoodSearchInnerScan;

        #region Reflected Methods

        internal static Pawn BestPawnToHuntForPredator( Pawn predator )
        {
            if( _BestPawnToHuntForPredator == null )
            {
                _BestPawnToHuntForPredator = typeof( FoodUtility ).GetMethod( "BestPawnToHuntForPredator", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (Pawn)_BestPawnToHuntForPredator.Invoke( null, new System.Object[] { predator } );
        }

        internal static float FoodSourceOptimality( Pawn eater, Thing t, float dist )
        {
            if( _FoodSourceOptimality == null )
            {
                _FoodSourceOptimality = typeof( FoodUtility ).GetMethod( "FoodSourceOptimality", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (float)_FoodSourceOptimality.Invoke( null, new System.Object[] { eater, t, dist } );
        }

        internal static Thing SpawnedFoodSearchInnerScan( Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999, Predicate<Thing> validator = null )
        {
            if( _SpawnedFoodSearchInnerScan == null )
            {
                _SpawnedFoodSearchInnerScan = typeof( FoodUtility ).GetMethod( "SpawnedFoodSearchInnerScan", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (Thing)_SpawnedFoodSearchInnerScan.Invoke( null, new System.Object[] { eater, root, searchSet, peMode, traverseParams, maxDistance, validator } );
        }

        #endregion

        internal static Thing _BestFoodSourceOnMap( Pawn getter, Pawn eater, bool desperate, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowLiquor = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false )
        {
            Profiler.BeginSample( "BestFoodInWorldFor getter=" + getter.LabelCap + " eater=" + eater.LabelCap );
            var dispenserValidator = new DispenserValidator();
            dispenserValidator.allowDispenserFull = allowDispenserFull;
            dispenserValidator.maxPref = maxPref;
            dispenserValidator.allowForbidden = allowForbidden;
            dispenserValidator.getter = getter;
            dispenserValidator.allowDispenserEmpty = allowDispenserEmpty;
            dispenserValidator.allowCorpse = allowCorpse;
            dispenserValidator.allowLiquor = allowLiquor;
            dispenserValidator.desperate = desperate;
            dispenserValidator.eater = eater;
            var getterCanManipulate = (
                ( getter.RaceProps.ToolUser ) &&
                ( getter.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )
            );
            if(
                ( !getterCanManipulate ) &&
                ( getter != eater )
            )
            {
                Log.Error( String.Format( "{0} tried to find food to bring to {1} but {0} is incapable of Manipulation.", getter.LabelCap, eater.LabelCap ) );
                Profiler.EndSample();
                return (Thing)null;
            }
            if( desperate )
            {
                allowLiquor = false;
            }

            var minPref =
                desperate
                ? FoodPreferability.DesperateOnly
                :
                    ( !dispenserValidator.eater.RaceProps.Humanlike
                      ? FoodPreferability.NeverForNutrition
                      :
                        ( dispenserValidator.eater.needs.food.CurCategory <= HungerCategory.UrgentlyHungry
                          ? FoodPreferability.RawBad
                          : FoodPreferability.MealAwful
                        )
                    );

            var thingRequest =
                (
                    ( ( eater.RaceProps.foodType & ( FoodTypeFlags.Plant | FoodTypeFlags.Tree ) ) == FoodTypeFlags.None ) ||
                    ( !allowPlant )
                )
                ? ThingRequest.ForGroup( ThingRequestGroup.FoodSourceNotPlantOrTree )
                : ThingRequest.ForGroup( ThingRequestGroup.FoodSource );

            var potentialFoodSource = (Thing)null;

            var foodValidator = new Predicate<Thing>( dispenserValidator.Validate );
            var foodValidatorFast = new Predicate<Thing>( dispenserValidator.ValidateFast );

            if( getter.RaceProps.Humanlike )
            {
                potentialFoodSource = SpawnedFoodSearchInnerScan( dispenserValidator.eater, dispenserValidator.getter.Position, Find.ListerThings.ThingsMatching( thingRequest ), PathEndMode.ClosestTouch, TraverseParms.For( getter, Danger.Deadly, TraverseMode.ByPawn, false ), 9999f, foodValidatorFast );
            }
            else
            {
                int searchRegionsMax = 30;
                if( dispenserValidator.getter.Faction == Faction.OfPlayer )
                {
                    searchRegionsMax = 60;
                }
                potentialFoodSource = GenClosest.ClosestThingReachable( getter.Position, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For( getter, Danger.Deadly, TraverseMode.ByPawn, false ), 9999f, foodValidator, (IEnumerable<Thing>)null, searchRegionsMax, false );
                if( potentialFoodSource == null )
                {
                    dispenserValidator.desperate = true;
                    potentialFoodSource = GenClosest.ClosestThingReachable( getter.Position, thingRequest, PathEndMode.ClosestTouch, TraverseParms.For( getter, Danger.Deadly, TraverseMode.ByPawn, false ), 9999f, foodValidatorFast, (IEnumerable<Thing>)null, searchRegionsMax, false );
                }
            }
            Profiler.EndSample();
            return potentialFoodSource;
        }

        internal class DispenserValidator
        {
            internal bool allowDispenserFull;
            internal FoodPreferability minPref;
            internal FoodPreferability maxPref;
            internal bool getterCanManipulate;
            internal bool allowForbidden;
            internal Pawn getter;
            internal bool allowDispenserEmpty;
            internal bool allowCorpse;
            internal bool allowLiquor;
            internal bool desperate;
            internal Pawn eater;

            internal bool ValidateFast( Thing t )
            {
                Profiler.BeginSample( "foodValidator" );
                if(
                    (
                        ( !allowForbidden ) &&
                        ( t.IsForbidden( getter ) )
                    ) ||
                    (
                        ( t.Faction != getter.Faction ) &&
                        ( t.Faction != getter.HostFaction )
                    ) ||
                    ( !t.IsSociallyProper( getter ) )
                )
                {
                    Profiler.EndSample();
                    return false;
                }

                var nutrientPasteDispenser = t as Building_NutrientPasteDispenser;
                var foodSynthesizer = t as Building_AutomatedFactory;
                if(
                    ( nutrientPasteDispenser != null ) ||
                    ( foodSynthesizer != null )
                )
                {
                    // Common checks for all machines
                    if(
                        ( !allowDispenserFull ) ||
                        ( !getterCanManipulate ) ||
                        ( !t.InteractionCell.Standable() ) ||
                        ( !getter.Position.CanReach( (TargetInfo)t.InteractionCell, PathEndMode.OnCell, TraverseParms.For( getter, Danger.Some, TraverseMode.ByPawn, false ) ) )
                    )
                    {
                        Profiler.EndSample();
                        return false;
                    }
                    // NPD checks
                    if(
                        ( nutrientPasteDispenser != null ) &&
                        (
                            ( ThingDefOf.MealNutrientPaste.ingestible.preferability < minPref ) ||
                            ( ThingDefOf.MealNutrientPaste.ingestible.preferability > maxPref ) ||
                            ( !nutrientPasteDispenser.powerComp.PowerOn ) ||
                            (
                                ( !allowDispenserEmpty ) &&
                                ( !nutrientPasteDispenser.HasEnoughFeedstockInHoppers() )
                            )
                        )
                    )
                    {
                        Profiler.EndSample();
                        return false;
                    }
                    // AF checks
                    if( foodSynthesizer != null )
                    {
                        var mealDef = foodSynthesizer.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                        if(
                            ( mealDef == null ) ||
                            ( mealDef.ingestible.preferability < minPref ) ||
                            ( mealDef.ingestible.preferability > maxPref )
                        )
                        {
                            Profiler.EndSample();
                            return false;
                        }
                    }
                }
                else
                {
                    // Non-machine checks
                    if(
                        ( t.def.ingestible.preferability < minPref ) ||
                        ( t.def.ingestible.preferability > maxPref ) ||
                        ( !t.IngestibleNow )
                    )
                    {
                        Profiler.EndSample();
                        return false;
                    }

                    if(
                        (
                            ( !allowCorpse )&&
                            ( t is Corpse )
                        )||
                        (
                            ( !allowLiquor )&&
                            ( t.def.ingestible.isPleasureDrug )
                        )||
                        (
                            ( !desperate )&&
                            ( t.IsNotFresh() )||
                            ( t.IsDessicated() )
                        )||
                        ( !eater.RaceProps.WillAutomaticallyEat( t ) )||
                        ( !getter.AnimalAwareOf( t ) )||
                        ( !getter.CanReserve( (TargetInfo)t, 1 ) )
                    )
                    {
                        Profiler.EndSample();
                        return false;
                    }
                }
                Profiler.EndSample();
                return true;
            }

            internal bool Validate( Thing t )
            {
                return (
                    ( ValidateFast( t ) ) &&
                    ( t.def.ingestible.preferability > FoodPreferability.DesperateOnly ) &&
                    ( !t.IsNotFresh() )
                );
            }

        }

    }

}
