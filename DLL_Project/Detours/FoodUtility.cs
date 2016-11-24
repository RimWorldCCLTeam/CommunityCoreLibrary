using System;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _FoodUtility
    {

        internal const float                FoodOptimalityUnusable = -9999999f;

        internal static FieldInfo           _FoodOptimalityEffectFromMoodCurve;
        internal static FieldInfo           _ingestThoughts;
        internal static MethodInfo          _SpawnedFoodSearchInnerScan;

        static                              _FoodUtility()
        {
            _FoodOptimalityEffectFromMoodCurve = typeof( FoodUtility ).GetField( "FoodOptimalityEffectFromMoodCurve", Controller.Data.UniversalBindingFlags );
            if( _FoodOptimalityEffectFromMoodCurve == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'FoodOptimalityEffectFromMoodCurve' in 'FoodUtility'",
                    "Detour.FoodUtility" );
            }
            _ingestThoughts = typeof( FoodUtility ).GetField( "ingestThoughts", Controller.Data.UniversalBindingFlags );
            if( _ingestThoughts == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ingestThoughts' in 'FoodUtility'",
                    "Detour.FoodUtility" );
            }
            _SpawnedFoodSearchInnerScan = typeof( FoodUtility ).GetMethod( "SpawnedFoodSearchInnerScan", Controller.Data.UniversalBindingFlags );
            if( _SpawnedFoodSearchInnerScan == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'SpawnedFoodSearchInnerScan' in 'FoodUtility'",
                    "Detour.FoodUtility" );
            }
        }

        #region Reflected Methods

        internal static SimpleCurve         FoodOptimalityEffectFromMoodCurve()
        {
            return (SimpleCurve)_FoodOptimalityEffectFromMoodCurve.GetValue( null );
        }

        internal static List<ThoughtDef>    IngestThoughts()
        {
            return (List<ThoughtDef>)_ingestThoughts.GetValue( null );
        }

        internal static Thing               SpawnedFoodSearchInnerScan( Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999, Predicate<Thing> validator = null )
        {
            return (Thing)_SpawnedFoodSearchInnerScan.Invoke( null, new object[] { eater, root, searchSet, peMode, traverseParams, maxDistance, validator } );
        }

        #endregion

#if DEVELOPER
        internal static void                DumpThingsRequestedForGroup( ThingRequest thingRequest, List<Thing> thingsRequested )
        {
            var str = string.Format( "ListerThings.ThingsMatching( {0} ) ::\n", thingRequest );
            foreach( var thing in thingsRequested )
            {
                str += string.Format( "\t{0} - {1}\n", thing.ThingID, thing.def.defName );
            }
            CCL_Log.Message( str );
        }
#endif


        [DetourMember( typeof( FoodUtility ) )]
        internal static ThingDef            _GetFinalIngestibleDef( Thing foodSource )
        {
            //CCL_Log.Message( string.Format( "GetFinalIngestibleDef( {0} )", foodSource.ThingID ) );

            var nutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
            if( nutrientPasteDispenser != null )
            {
                //CCL_Log.Message( string.Format( "GetFinalIngestibleDef( {0} ) - {1}", foodSource.ThingID, nutrientPasteDispenser.DispensableDef.defName ) );
                return nutrientPasteDispenser.DispensableDef;
            }

            var synthesizer = foodSource as Building_AutomatedFactory;
            if( synthesizer != null )
            {
                var synthesizedDef = synthesizer.ConsideredProduct;
#if DEVELOPER
                if( synthesizedDef == null )
                {
                    CCL_Log.Trace(
                        Verbosity.NonFatalErrors,
                        string.Format( "{0} - Has not been reserved for use", foodSource.ThingID ),
                        "Detour.FoodUtility.GetFinalIngestibleDef"
                    );
                }
#endif
                return synthesizedDef;
            }

            var prey = foodSource as Pawn;
            if( prey != null )
            {
                //CCL_Log.Message( string.Format( "GetFoodDef( {0} ) - {1}", foodSource.ThingID, prey.RaceProps.corpseDef.defName ) );
                return prey.RaceProps.corpseDef;
            }
            //CCL_Log.Message( string.Format( "GetFoodDef( {0} ) - {1}", foodSource.ThingID, foodSource.def.defName ) );
            return foodSource.def;
        }

        [DetourMember( typeof( FoodUtility ) )]
        internal static float               _FoodSourceOptimality( Pawn eater, Thing t, float dist )
        {
            var mealDef = t.def;
            float num = 300f - dist;
            var synthesizer = t as Building_AutomatedFactory;
            if( synthesizer != null )
            {
                mealDef = null;
                if( synthesizer.IsConsidering( eater ) )
                {
                    mealDef = synthesizer.ConsideredProduct;
                }
                else if( synthesizer.IsReserved )
                {
#if DEVELOPER
                    CCL_Log.Message(
                        string.Format( "{0} cannot be used by {1} as it has been reserved by {2}", t.ThingID, eater.LabelShort, synthesizer.ConsideredPawn.LabelShort ),
                        "Detour.FoodUtility.FoodSourceOptimality"
                    );
#endif
                    return FoodOptimalityUnusable;
                }
                else
                {
                    mealDef = synthesizer.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                    if( mealDef == null )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} does not have enough resources to produce anything for {1}", t.ThingID, eater.LabelShort ),
                            "Detour.FoodUtility.FoodSourceOptimality"
                        );
#endif
                        return FoodOptimalityUnusable;
                    }
                    if( !synthesizer.ConsiderFor( mealDef, eater ) )
                    {
#if DEVELOPER
                        CCL_Log.Trace(
                            Verbosity.NonFatalErrors,
                            string.Format( "{0} cannot be considered by {1} for {2}", t.ThingID, eater.LabelShort, mealDef.defName ),
                            "Detour.FoodUtility.FoodSourceOptimality"
                        );
#endif
                        return FoodOptimalityUnusable;
                    }
                }
                if( mealDef == null )
                {   // This should never happen, why is it?
#if DEVELOPER
                    CCL_Log.Trace(
                        Verbosity.NonFatalErrors,
                        string.Format( "{0} has not or cannot been considered by {1}", t.ThingID, eater.LabelShort ),
                        "Detour.FoodUtility.FoodSourceOptimality"
                    );
#endif
                    return FoodOptimalityUnusable;
                }
            }
            else if( t is Building_NutrientPasteDispenser )
            {
                mealDef = ((Building_NutrientPasteDispenser)t).DispensableDef;
            }
            //CCL_Log.Message( string.Format( "FoodSourceOptimality for {0} eating {1} from {2}", eater.LabelShort, def.defName, t.ThingID ) );
            switch( mealDef.ingestible.preferability )
            {
            case FoodPreferability.NeverForNutrition:
                return FoodOptimalityUnusable;
            case FoodPreferability.DesperateOnly:
                num -= 150f;
                break;
            }
            var comp = t.TryGetComp<CompRottable>();
            if( comp != null )
            {
                if( comp.Stage == RotStage.Dessicated )
                {
                    return FoodOptimalityUnusable;
                }
                if(
                    ( comp.Stage == RotStage.Fresh )&&
                    ( comp.TicksUntilRotAtCurrentTemp < 30000 )
                )
                {
                    num += 12f;
                }
            }
            //CCL_Log.Message( string.Format( "FoodSourceOptimality for {0} eating {1} from {2} = {3}", eater.LabelShort, def.defName, t.ThingID, num ) );
            if(
                ( eater.needs != null )&&
                ( eater.needs.mood != null )
            )
            {
                var curve = FoodOptimalityEffectFromMoodCurve();
                List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting( eater, t );
                for( int index = 0; index < list.Count; ++index )
                {
                    num += curve.Evaluate( list[ index ].stages[ 0 ].baseMoodEffect );
                }
            }
            if( t.def.ingestible != null )
            {
                num += t.def.ingestible.optimalityOffset;
            }
            return num;
        }

        [DetourMember( typeof( FoodUtility ) )]
        internal static List<ThoughtDef>    _ThoughtsFromIngesting( Pawn ingester, Thing t )
        {
            var ingestThoughts = IngestThoughts();
            ingestThoughts.Clear();

            if(
                ( ingester.needs == null )||
                ( ingester.needs.mood == null )
            )
            {
                return ingestThoughts;
            }

            var mealDef = t.def;
            if( t is Building_AutomatedFactory )
            {
                mealDef = ((Building_AutomatedFactory)t).ConsideredProduct;
                /*
                if( mealDef == null )
                {
                    mealDef = ((Building_AutomatedFactory)t).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                }
                */
            }
            else if( t is Building_NutrientPasteDispenser )
            {
                mealDef = ((Building_NutrientPasteDispenser)t).DispensableDef;
            }

            var corpse = t as Corpse;
            if( !ingester.story.traits.HasTrait( TraitDefOf.Ascetic ) )
            {
                if( mealDef.ingestible.preferability == FoodPreferability.MealLavish )
                {
                    ingestThoughts.Add( ThoughtDefOf.AteLavishMeal );
                }
                else if( mealDef.ingestible.preferability == FoodPreferability.MealFine )
                {
                    ingestThoughts.Add( ThoughtDefOf.AteFineMeal );
                }
                else if( mealDef.ingestible.preferability == FoodPreferability.MealAwful )
                {
                    ingestThoughts.Add( ThoughtDefOf.AteAwfulMeal );
                }
                else if( mealDef.ingestible.tastesRaw )
                {
                    ingestThoughts.Add( ThoughtDefOf.AteRawFood );
                }
                else if( corpse != null )
                {
                    ingestThoughts.Add( ThoughtDefOf.AteCorpse );
                }
            }

            var isCannibal = ingester.story.traits.HasTrait( TraitDefOf.Cannibal );
            var comp = t.TryGetComp<CompIngredients>();
            if(
                ( FoodUtility.IsHumanlikeMeat( mealDef ) )&&
                ( ingester.RaceProps.Humanlike )
            )
            {
                ingestThoughts.Add( !isCannibal ? ThoughtDefOf.AteHumanlikeMeatDirect : ThoughtDefOf.AteHumanlikeMeatDirectCannibal );
            }
            else if( comp != null )
            {
                for( int index = 0; index < comp.ingredients.Count; ++index )
                {
                    var ingredientDef = comp.ingredients[ index ];
                    if( ingredientDef.ingestible != null )
                    {
                        if(
                            ( ingester.RaceProps.Humanlike )&&
                            ( FoodUtility.IsHumanlikeMeat( ingredientDef ) )
                        )
                        {
                            ingestThoughts.Add( !isCannibal ? ThoughtDefOf.AteHumanlikeMeatAsIngredient : ThoughtDefOf.AteHumanlikeMeatAsIngredientCannibal );
                        }
                        else if( ingredientDef.ingestible.specialThoughtAsIngredient != null )
                        {
                            ingestThoughts.Add( ingredientDef.ingestible.specialThoughtAsIngredient );
                        }
                    }
                }
            }
            else if( mealDef.ingestible.specialThoughtDirect != null )
            {
                ingestThoughts.Add( mealDef.ingestible.specialThoughtDirect );
            }
            if( t.IsNotFresh() )
            {
                ingestThoughts.Add( ThoughtDefOf.AteRottenFood );
            }
            return ingestThoughts;
        }

        [DetourMember( typeof( FoodUtility ) )]
        internal static Thing               _BestFoodSourceOnMap( Pawn getter, Pawn eater, bool desperate, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowDrug = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false )
        {
#if DEVELOPER
            CCL_Log.Message(
                string.Format( "{0} fetching for {1} ", getter.LabelShort, eater.LabelShort ),
                "Detour.FoodUtility.BestFoodSourceOnMap"
            );
#endif
            var getterCanManipulate = (
                ( getter.RaceProps.ToolUser )&&
                ( getter.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )
            );
            if(
                ( !getterCanManipulate )&&
                ( getter != eater )
            )
            {
#if DEVELOPER
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "{0} tried to find food to bring to {1} but {0} is incapable of Manipulation.", getter.LabelShort, eater.LabelShort ),
                    "Detour.FoodUtility.BestFoodSourceOnMap"
                );
#endif
                return null;
            }

            var validator = new DispenserValidator();
            validator.getterCanManipulate = getterCanManipulate;
            validator.allowDispenserFull = allowDispenserFull;
            validator.maxPref = maxPref;
            validator.allowForbidden = allowForbidden;
            validator.getter = getter;
            validator.allowDispenserEmpty = allowDispenserEmpty;
            validator.allowCorpse = allowCorpse;
            validator.allowDrug = allowDrug;
            validator.desperate = desperate;
            validator.eater = eater;
            validator.minPref =
                desperate
                ? FoodPreferability.DesperateOnly
                :
                    !eater.RaceProps.Humanlike
                    ? FoodPreferability.NeverForNutrition
                    :
                        eater.needs.food.CurCategory >= HungerCategory.UrgentlyHungry
                        ? FoodPreferability.RawBad
                        : FoodPreferability.MealAwful;

            var thingRequest =
                (
                    ( ( eater.RaceProps.foodType & ( FoodTypeFlags.Plant | FoodTypeFlags.Tree ) ) == FoodTypeFlags.None )||
                    ( !allowPlant )
                )
                ? ThingRequest.ForGroup( ThingRequestGroup.FoodSourceNotPlantOrTree )
                : ThingRequest.ForGroup( ThingRequestGroup.FoodSource );

            var potentialFoodSource = (Thing)null;

            if( getter.RaceProps.Humanlike )
            {
                var thingsRequested = Find.ListerThings.ThingsMatching( thingRequest );

#if DEVELOPER
                DumpThingsRequestedForGroup( thingRequest, thingsRequested );


                CCL_Log.Message(
                    "Humanlike inner scan...",
                    "Detour.FoodUtility.BestFoodSourceOnMap"
                );
#endif
                potentialFoodSource = SpawnedFoodSearchInnerScan(
                    eater,
                    getter.Position,
                    thingsRequested,
                    PathEndMode.InteractionCell,
                    TraverseParms.For(
                        getter,
                        Danger.Deadly,
                        TraverseMode.ByPawn,
                        false ),
                    9999f,
                    validator.ValidateFast );
            }
            else
            {
#if DEVELOPER
                CCL_Log.Message(
                    "Non-humanlike closest reachable...",
                    "Detour.FoodUtility.BestFoodSourceOnMap"
                );
#endif
                var searchRegionsMax = 30;
                if( getter.Faction == Faction.OfPlayer )
                {
                    searchRegionsMax = 60;
                }
                potentialFoodSource = GenClosest.ClosestThingReachable(
                    getter.Position,
                    thingRequest,
                    PathEndMode.ClosestTouch,
                    TraverseParms.For(
                        getter,
                        Danger.Deadly,
                        TraverseMode.ByPawn,
                        false ),
                    9999f,
                    validator.Validate,
                    null,
                    searchRegionsMax,
                    false );
                if( potentialFoodSource == null )
                {
#if DEVELOPER
                    CCL_Log.Message(
                        "Non-humanlike closest reachable desperate...",
                        "Detour.FoodUtility.BestFoodSourceOnMap"
                    );
#endif
                    validator.desperate = true;
                    potentialFoodSource = GenClosest.ClosestThingReachable(
                        getter.Position,
                        thingRequest,
                        PathEndMode.ClosestTouch,
                        TraverseParms.For(
                            getter,
                            Danger.Deadly,
                            TraverseMode.ByPawn,
                            false ),
                        9999f,
                        validator.ValidateFast,
                        null,
                        searchRegionsMax,
                        false );
                }
            }
            var mealDef = potentialFoodSource?.def;
            var synthesizer = potentialFoodSource as Building_AutomatedFactory;
            if( synthesizer != null )
            {
                mealDef = synthesizer.ConsideredProduct;
            }
#if DEVELOPER
            CCL_Log.Message(
                string.Format(
                    "{0} picked {1} ({3}) for {2}",
                    getter.LabelShort,
                    potentialFoodSource == null ? "nothing" : potentialFoodSource.ThingID,
                    eater.LabelShort,
                    mealDef == null ? "nothing" : mealDef.defName
                ),
                "Detour.FoodUtility.BestFoodSourceOnMap"
            );
#endif
            if(
                ( potentialFoodSource == null )||
                ( mealDef == null )
            )
            {
                return null;
            }
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
            internal bool allowDrug;
            internal bool desperate;
            internal Pawn eater;

            internal bool ValidateFast( Thing t )
            {
                if(
                    ( !allowForbidden )&&
                    ( t.IsForbidden( getter ) )
                )
                {
#if DEVELOPER
                    CCL_Log.Message(
                        string.Format( "{0} cannot use {1} because it is forbidden", getter.LabelShort, t.ThingID ),
                        "Detour.FoodUtility.DispenserValidator"
                    );
#endif
                    return false;
                }
                if(
                    ( t.Faction != null )&&
                    ( t.Faction != getter.Faction )&&
                    ( t.Faction != getter.HostFaction )
                )
                {
#if DEVELOPER
                    CCL_Log.Message(
                        string.Format( "{0} cannot use {1} because it is the wrong faction - Faction for {1} is {2} - Faction for {0} is {3}, host is {4}", getter.LabelShort, t.ThingID, t.Faction?.Name, getter.Faction?.Name, getter.HostFaction?.Name ),
                        "Detour.FoodUtility.DispenserValidator"
                    );
#endif
                    return false;
                }
                if( !t.IsSociallyProper( getter ) )
                {
#if DEVELOPER
                    CCL_Log.Message(
                        string.Format( "{0} cannot use {1} because it is socially improper", getter.LabelShort, t.ThingID ),
                        "Detour.FoodUtility.DispenserValidator"
                    );
#endif
                    return false;
                }

                if( t is Building )
                {
                    // Common checks for all machines
                    if( !allowDispenserFull )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because cannot use full dispensers", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !getterCanManipulate )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because {0} cannot manipulate", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    var compPower = t.TryGetComp<CompPowerTrader>();
                    if(
                        ( compPower != null )&&
                        ( !compPower.PowerOn )
                    )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is unpowered", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !t.InteractionCell.Standable() )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because the interaction cell is unstandable", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif

                        return false;
                    }
                    if( !getter.Position.CanReach(
                        t.InteractionCell,
                        PathEndMode.OnCell,
                        TraverseParms.For(
                            getter,
                            Danger.Some,
                            TraverseMode.ByPawn,
                            false )
                    ) )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is unreachable", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !getter.CanReserve( t, 1 ) )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is unreservable", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    // NPD checks
                    var NPD = t as Building_NutrientPasteDispenser;
                    if( NPD != null )
                    {
                        if(
                            ( NPD.DispensableDef.ingestible.preferability < minPref )||
                            ( NPD.DispensableDef.ingestible.preferability > maxPref )
                        )
                        {
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot use {1} because it is not preferable", getter.LabelShort, t.ThingID ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
                        if(
                            ( !allowDispenserEmpty )&&
                            ( !NPD.HasEnoughFeedstockInHoppers() )
                        )
                        {
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot use {1} because it is empty", getter.LabelShort, t.ThingID ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
                    }
                    // AF checks
                    var synthesizer = t as Building_AutomatedFactory;
                    if( synthesizer != null )
                    {
                        if( synthesizer.IsReserved )
                        {   // Already in use
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot use {1} because it has been reserved by {2}", getter.LabelShort, t.ThingID, synthesizer.ConsideredPawn.LabelShort ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} might use {1}", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        var mealDef = synthesizer.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                        if( mealDef == null )
                        {
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot use {1} because it is empty", getter.LabelShort, t.ThingID ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
                        if(
                            ( mealDef.ingestible.preferability < minPref )||
                            ( mealDef.ingestible.preferability > maxPref )
                        )
                        {
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot use {1} for {2} because it is not preferable", getter.LabelShort, t.ThingID, mealDef.defName ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
                        if( !synthesizer.ConsiderFor( mealDef, getter ) )
                        {
#if DEVELOPER
                            CCL_Log.Message(
                                string.Format( "{0} cannot consider {1} for {2}", getter.LabelShort, t.ThingID, mealDef.defName ),
                                "Detour.FoodUtility.DispenserValidator"
                            );
#endif
                            return false;
                        }
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} considered {1} for {2}", getter.LabelShort, t.ThingID, mealDef.defName ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                    }
                }
                else
                {
                    // Non-machine checks
                    if(
                        ( t.def.ingestible.preferability < minPref )||
                        ( t.def.ingestible.preferability > maxPref )
                    )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is not preferable", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !t.IngestibleNow )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is not ingestible now", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if(
                        ( !allowCorpse )&&
                        ( t is Corpse )
                    )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is a corpse", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if(
                        ( !allowDrug )&&
                        ( t.def.IsDrug )
                    )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is liquor", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if(
                        ( !desperate )&&
                        (
                            ( t.IsNotFresh() )||
                            ( t.IsDessicated() )
                        )
                    )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is not fresh or it's dessicated", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !eater.RaceProps.WillAutomaticallyEat( t ) )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it will not automatically eat it", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !getter.AnimalAwareOf( t ) )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it is not aware of it", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                    if( !getter.CanReserve( t, 1 ) )
                    {
#if DEVELOPER
                        CCL_Log.Message(
                            string.Format( "{0} cannot use {1} because it cannot reserve it", getter.LabelShort, t.ThingID ),
                            "Detour.FoodUtility.DispenserValidator"
                        );
#endif
                        return false;
                    }
                }
#if DEVELOPER
                CCL_Log.Message(
                    string.Format( "{0} can use {1}", getter.LabelShort, t.ThingID ),
                    "Detour.FoodUtility.DispenserValidator"
                );
#endif
                return true;
            }

            internal bool Validate( Thing t )
            {
                return (
                    ( ValidateFast( t ) )&&
                    ( t.def.ingestible.preferability > FoodPreferability.DesperateOnly )&&
                    ( !t.IsNotFresh() )
                );
            }

        }

    }

}
