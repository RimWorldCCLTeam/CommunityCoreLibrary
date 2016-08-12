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

        #region Reflected Methods

        internal static SimpleCurve         FoodOptimalityEffectFromMoodCurve()
        {
            if( _FoodOptimalityEffectFromMoodCurve == null )
            {
                _FoodOptimalityEffectFromMoodCurve = typeof( FoodUtility ).GetField( "FoodOptimalityEffectFromMoodCurve", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (SimpleCurve)_FoodOptimalityEffectFromMoodCurve.GetValue( null );
        }

        internal static List<ThoughtDef>    IngestThoughts()
        {
            if( _ingestThoughts == null )
            {
                _ingestThoughts = typeof( FoodUtility ).GetField( "ingestThoughts", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (List<ThoughtDef>)_ingestThoughts.GetValue( null );
        }

        internal static Thing               SpawnedFoodSearchInnerScan( Pawn eater, IntVec3 root, List<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999, Predicate<Thing> validator = null )
        {
            if( _SpawnedFoodSearchInnerScan == null )
            {
                _SpawnedFoodSearchInnerScan = typeof( FoodUtility ).GetMethod( "SpawnedFoodSearchInnerScan", BindingFlags.Static | BindingFlags.NonPublic );
            }
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

        internal static bool                _GetFoodDefAlcohol;
        internal static ThingDef            _GetFoodDef( Thing foodSource )
        {
            var lookForAlcohol = _GetFoodDefAlcohol;
            _GetFoodDefAlcohol = false;

            //CCL_Log.Message( string.Format( "GetFoodDef( {0} )", foodSource.ThingID ) );

            var nutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
            if( nutrientPasteDispenser != null )
            {
                //CCL_Log.Message( string.Format( "GetFoodDef( {0} ) - {1}", foodSource.ThingID, nutrientPasteDispenser.DispensableDef.defName ) );
                return nutrientPasteDispenser.DispensableDef;
            }

            var factory = foodSource as Building_AutomatedFactory;
            if( factory != null )
            {
                if( lookForAlcohol )
                {
                    var product = factory.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol );
                    //CCL_Log.Message( string.Format( "GetFoodDef( {0} ) - {1}", foodSource.ThingID, product.defName ) );
                    return product;
                }
                else
                {
                    var product = factory.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                    //CCL_Log.Message( string.Format( "GetFoodDef( {0} ) - {1}", foodSource.ThingID, product.defName ) );
                    return product;
                }
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

        internal static float               _FoodSourceOptimality( Pawn eater, Thing t, float dist )
        {
            var def = t.def;
            float num = 300f - dist;
            if( t is Building_AutomatedFactory )
            {
                def = ((Building_AutomatedFactory)t).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                if( def == null )
                {
                    return FoodOptimalityUnusable;
                }
            }
            else if( t is Building_NutrientPasteDispenser )
            {
                def = ((Building_NutrientPasteDispenser)t).DispensableDef;
            }
            //CCL_Log.Message( string.Format( "FoodSourceOptimality for {0} eating {1} from {2}", eater.LabelShort, def.defName, t.ThingID ) );
            switch( def.ingestible.preferability )
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
                    num += 40f;
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
            return num;
        }

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
                mealDef = ((Building_AutomatedFactory)t).BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
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

        internal static Thing               _BestFoodSourceOnMap( Pawn getter, Pawn eater, bool desperate, FoodPreferability maxPref = FoodPreferability.MealLavish, bool allowPlant = true, bool allowLiquor = true, bool allowCorpse = true, bool allowDispenserFull = true, bool allowDispenserEmpty = true, bool allowForbidden = false )
        {
            Profiler.BeginSample( "BestFoodInWorldFor getter=" + getter.LabelCap + " eater=" + eater.LabelCap );

            var getterCanManipulate = (
                ( getter.RaceProps.ToolUser )&&
                ( getter.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )
            );
            if(
                ( !getterCanManipulate )&&
                ( getter != eater )
            )
            {
                Log.Error( string.Format( "{0} tried to find food to bring to {1} but {0} is incapable of Manipulation.", getter.LabelCap, eater.LabelCap ) );
                Profiler.EndSample();
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
            validator.allowLiquor = allowLiquor;
            validator.desperate = desperate;
            validator.eater = eater;
            validator.allowLiquor &= !desperate;
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

            var thingsRequested = Find.ListerThings.ThingsMatching( thingRequest );
            //DumpThingsRequestedForGroup( thingRequest, thingsRequested );

            var potentialFoodSource = (Thing)null;

            if( getter.RaceProps.Humanlike )
            {
                //CCL_Log.Message( "Humanlike inner scan..." );
                potentialFoodSource = SpawnedFoodSearchInnerScan(
                    eater,
                    getter.Position,
                    thingsRequested,
                    PathEndMode.ClosestTouch,
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
                //CCL_Log.Message( "Non-humanlike closest reachable..." );
                int searchRegionsMax = 30;
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
                    //CCL_Log.Message( "Non-humanlike closest reachable desperate..." );
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
            Profiler.EndSample();
            //CCL_Log.Message( string.Format( "{0} picked {1} for {2}", getter.LabelShort, potentialFoodSource == null ? "nothing" : potentialFoodSource.ThingID, eater.LabelShort ) );
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
                    ( !allowForbidden )&&
                    ( t.IsForbidden( getter ) )
                )
                {
                    Profiler.EndSample();
                    //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is forbidden", getter.LabelShort, t.ThingID ) );
                    return false;
                }
                if(
                    ( t.Faction != null )&&
                    ( t.Faction != getter.Faction )&&
                    ( t.Faction != getter.HostFaction )
                )
                {
                    Profiler.EndSample();
                    //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is the wrong faction - Faction for {1} is {2} - Faction for {0} is {3}, host is {4}", getter.LabelShort, t.ThingID, t.Faction?.Name, getter.Faction?.Name, getter.HostFaction?.Name ) );
                    return false;
                }
                if( !t.IsSociallyProper( getter ) )
                {
                    Profiler.EndSample();
                    //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is socially improper", getter.LabelShort, t.ThingID ) );
                    return false;
                }

                if( t is Building )
                {
                    // Common checks for all machines
                    if( !allowDispenserFull )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because cannot use full dispensers", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !getterCanManipulate )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because {0} cannot manipulate", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    var compPower = t.TryGetComp<CompPowerTrader>();
                    if(
                        ( compPower != null )&&
                        ( !compPower.PowerOn )
                    )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is unpowered", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !t.InteractionCell.Standable() )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because the interaction cell is unstandable", getter.LabelShort, t.ThingID ) );
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
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is unreachable", getter.LabelShort, t.ThingID ) );
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
                            Profiler.EndSample();
                            //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not preferable", getter.LabelShort, t.ThingID ) );
                            return false;
                        }
                        if(
                            ( !allowDispenserEmpty )&&
                            ( !NPD.HasEnoughFeedstockInHoppers() )
                        )
                        {
                            Profiler.EndSample();
                            //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is empty", getter.LabelShort, t.ThingID ) );
                            return false;
                        }
                    }
                    // AF checks
                    var FS = t as Building_AutomatedFactory;
                    if( FS != null )
                    {
                        var mealDef = FS.BestProduct( FoodSynthesis.IsMeal, FoodSynthesis.SortMeal );
                        if( mealDef == null )
                        {
                            Profiler.EndSample();
                            //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is empty", getter.LabelShort, t.ThingID ) );
                            return false;
                        }
                        if(
                            ( mealDef.ingestible.preferability < minPref )||
                            ( mealDef.ingestible.preferability > maxPref )
                        )
                        {
                            Profiler.EndSample();
                            //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not preferable", getter.LabelShort, t.ThingID ) );
                            return false;
                        }
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
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not preferable", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !t.IngestibleNow )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not ingestible now", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if(
                        ( !allowCorpse )&&
                        ( t is Corpse )
                    )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is a corpse", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if(
                        ( !allowLiquor )&&
                        ( t.def.ingestible.isPleasureDrug )
                    )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is liquor", getter.LabelShort, t.ThingID ) );
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
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not fresh or it's dessicated", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !eater.RaceProps.WillAutomaticallyEat( t ) )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it will not automatically eat it", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !getter.AnimalAwareOf( t ) )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it is not aware of it", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                    if( !getter.CanReserve( t, 1 ) )
                    {
                        Profiler.EndSample();
                        //CCL_Log.Message( string.Format( "{0} cannot use {1} because it cannot reserve it", getter.LabelShort, t.ThingID ) );
                        return false;
                    }
                }
                Profiler.EndSample();
                //CCL_Log.Message( string.Format( "{0} can use {1}", getter.LabelShort, t.ThingID ) );
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
