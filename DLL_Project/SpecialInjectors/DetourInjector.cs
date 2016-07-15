using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class DetourInjector : SpecialInjector
    {

        public override bool                Inject()
        {
            // Change CompGlower into CompGlowerToggleable
            FixGlowers();

            // Change Building_NutrientPasteDispenser into Building_AdvancedPasteDispenser
            UpgradeNutrientPasteDispensers();

            // Detour RimWorld.JoyGiver_SocialRelax.TryGiveJobInt
            MethodInfo RimWorld_JoyGiver_SocialRelax_TryGiveJobInt = typeof( JoyGiver_SocialRelax ).GetMethod( "TryGiveJobInt", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JoyGiver_SocialRelax_TryGiveJobInt = typeof( Detour._JoyGiver_SocialRelax ).GetMethod( "_TryGiveJobInt", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JoyGiver_SocialRelax_TryGiveJobInt, CCL_JoyGiver_SocialRelax_TryGiveJobInt ) )
                return false;

            // Detour RimWorld.ThingSelectionUtility.SelectableNow
            MethodInfo RimWorld_ThingSelectionUtility_SelectableNow = typeof( ThingSelectionUtility ).GetMethod( "SelectableNow", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_ThingSelectionUtility_SelectableNow = typeof( Detour._ThingSelectionUtility ).GetMethod( "_SelectableNow", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_ThingSelectionUtility_SelectableNow, CCL_ThingSelectionUtility_SelectableNow ) )
                return false;

            // Detour RimWorld.FoodUtility.GetFoodDef
            MethodInfo RimWorld_FoodUtility_GetFoodDef = typeof( FoodUtility ).GetMethod( "GetFoodDef", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_FoodUtility_GetFoodDef = typeof( Detour._FoodUtility ).GetMethod( "_GetFoodDef", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_GetFoodDef, CCL_FoodUtility_GetFoodDef ) )
                return false;

            // Detour RimWorld.FoodUtility.FoodSourceOptimality
            MethodInfo RimWorld_FoodUtility_FoodSourceOptimality = typeof( FoodUtility ).GetMethod( "FoodSourceOptimality", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_FoodUtility_FoodSourceOptimality = typeof( Detour._FoodUtility ).GetMethod( "_FoodSourceOptimality", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_FoodSourceOptimality, CCL_FoodUtility_FoodSourceOptimality ) )
                return false;

            // Detour RimWorld.FoodUtility.ThoughtsFromIngesting
            MethodInfo RimWorld_FoodUtility_ThoughtsFromIngesting = typeof( FoodUtility ).GetMethod( "ThoughtsFromIngesting", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_FoodUtility_ThoughtsFromIngesting = typeof( Detour._FoodUtility ).GetMethod( "_ThoughtsFromIngesting", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_ThoughtsFromIngesting, CCL_FoodUtility_ThoughtsFromIngesting ) )
                return false;

            // Detour RimWorld.FoodUtility.BestFoodSourceOnMap
            MethodInfo RimWorld_FoodUtility_BestFoodSourceOnMap = typeof( FoodUtility ).GetMethod( "BestFoodSourceOnMap", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_FoodUtility_BestFoodSourceOnMap = typeof( Detour._FoodUtility ).GetMethod( "_BestFoodSourceOnMap", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_BestFoodSourceOnMap, CCL_FoodUtility_BestFoodSourceOnMap ) )
                return false;

            // Detour RimWorld.JobDriver_FoodDeliver.MakeNewToils
            MethodInfo RimWorld_JobDriver_FoodDeliver_MakeNewToils = typeof( JobDriver_FoodDeliver ).GetMethod( "MakeNewToils", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_FoodDeliver_MakeNewToils = typeof( Detour._JobDriver_FoodDeliver ).GetMethod( "_MakeNewToils", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_FoodDeliver_MakeNewToils, CCL_JobDriver_FoodDeliver_MakeNewToils ) )
                return false;

            // Detour RimWorld.JobDriver_FoodFeedPatient.MakeNewToils
            MethodInfo RimWorld_JobDriver_FoodFeedPatient_MakeNewToils = typeof( JobDriver_FoodFeedPatient ).GetMethod( "MakeNewToils", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_FoodFeedPatient_MakeNewToils = typeof( Detour._JobDriver_FoodFeedPatient ).GetMethod( "_MakeNewToils", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_FoodFeedPatient_MakeNewToils, CCL_JobDriver_FoodFeedPatient_MakeNewToils ) )
                return false;

            // Detour RimWorld.JobDriver_Ingest.UsingNutrientPasteDispenser
            PropertyInfo RimWorld_JobDriver_Ingest_UsingNutrientPasteDispenser = typeof( JobDriver_Ingest ).GetProperty( "UsingNutrientPasteDispenser", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo RimWorld_JobDriver_Ingest_UsingNutrientPasteDispenser_get = RimWorld_JobDriver_Ingest_UsingNutrientPasteDispenser.GetGetMethod( true );
            MethodInfo CCL_JobDriver_Ingest_UsingNutrientPasteDispenser = typeof( Detour._JobDriver_Ingest ).GetMethod( "_UsingNutrientPasteDispenser", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_Ingest_UsingNutrientPasteDispenser_get, CCL_JobDriver_Ingest_UsingNutrientPasteDispenser ) )
                return false;

            // Detour RimWorld.JobDriver_Ingest.GetReport
            MethodInfo RimWorld_JobDriver_Ingest_GetReport = typeof( JobDriver_Ingest ).GetMethod( "GetReport", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_JobDriver_Ingest_GetReport = typeof( Detour._JobDriver_Ingest ).GetMethod( "_GetReport", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_Ingest_GetReport, CCL_JobDriver_Ingest_GetReport ) )
                return false;

            // Detour RimWorld.JobDriver_Ingest.PrepareToEatToils_Dispenser
            MethodInfo RimWorld_JobDriver_Ingest_PrepareToEatToils_Dispenser = typeof( JobDriver_Ingest ).GetMethod( "PrepareToEatToils_Dispenser", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_Ingest_PrepareToEatToils_Dispenser = typeof( Detour._JobDriver_Ingest ).GetMethod( "_PrepareToEatToils_Dispenser", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_Ingest_PrepareToEatToils_Dispenser, CCL_JobDriver_Ingest_PrepareToEatToils_Dispenser ) )
                return false;

            // Detour RimWorld.JobGiver_GetFood.TryGiveJob
            MethodInfo RimWorld_JobGiver_GetFood_TryGiveJob = typeof( JobGiver_GetFood ).GetMethod( "TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobGiver_GetFood_TryGiveJob = typeof( Detour._JobGiver_GetFood ).GetMethod( "_TryGiveJob", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobGiver_GetFood_TryGiveJob, CCL_JobGiver_GetFood_TryGiveJob ) )
                return false;

            // Detour RimWorld.JobDriver_SocialRelax.MakeNewToils
            MethodInfo RimWorld_JobDriver_SocialRelax_MakeNewToils = typeof( JobDriver_SocialRelax ).GetMethod( "MakeNewToils", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_SocialRelax_MakeNewToils = typeof( Detour._JobDriver_SocialRelax ).GetMethod( "_MakeNewToils", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_SocialRelax_MakeNewToils, CCL_JobDriver_SocialRelax_MakeNewToils ) )
                return false;

            // Detour Verse.MentalStateWorker_BingingAlcohol.StateCanOccur
            MethodInfo Verse_MentalStateWorker_BingingAlcohol_StateCanOccur = typeof( MentalStateWorker_BingingAlcohol ).GetMethod( "StateCanOccur", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_MentalStateWorker_BingingAlcohol_StateCanOccur = typeof( Detour._MentalStateWorker_BingingAlcohol ).GetMethod( "_StateCanOccur", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_MentalStateWorker_BingingAlcohol_StateCanOccur, CCL_MentalStateWorker_BingingAlcohol_StateCanOccur ) )
                return false;

            // Detour RimWorld.CompRottable.CompTickRare
            MethodInfo RimWorld_CompRottable_CompTickRare = typeof( CompRottable ).GetMethod( "CompTickRare", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_CompRottable_CompTickRare = typeof( Detour._CompRottable ).GetMethod( "_CompTickRare", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_CompRottable_CompTickRare, CCL_CompRottable_CompTickRare ) )
                return false;

            // Detour RimWorld.CompRottable.CompInspectStringExtra
            MethodInfo RimWorld_CompRottable_CompInspectStringExtra = typeof( CompRottable ).GetMethod( "CompInspectStringExtra", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_CompRottable_CompInspectStringExtra = typeof( Detour._CompRottable ).GetMethod( "_CompInspectStringExtra", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_CompRottable_CompInspectStringExtra, CCL_CompRottable_CompInspectStringExtra ) )
                return false;

            // Detour Verse.CompHeatPusherPowered.ShouldPushHeatNow
            PropertyInfo Verse_CompHeatPusherPowered_ShouldPushHeatNow = typeof( CompHeatPusherPowered ).GetProperty( "ShouldPushHeatNow", BindingFlags.Instance | BindingFlags.NonPublic );
#if DEBUG
            if( Verse_CompHeatPusherPowered_ShouldPushHeatNow == null )
            {
                CCL_Log.Error( "Unable to find 'Verse.CompHeatPusherPowered.ShouldPushHeatNow'" );
                return false;
            }
#endif
            MethodInfo Verse_CompHeatPusherPowered_ShouldPushHeatNow_Getter = Verse_CompHeatPusherPowered_ShouldPushHeatNow.GetGetMethod( true );
            MethodInfo CCL_CompHeatPusherPowered_ShouldPushHeatNow = typeof( Detour._CompHeatPusherPowered ).GetMethod( "_ShouldPushHeatNow", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_CompHeatPusherPowered_ShouldPushHeatNow_Getter, CCL_CompHeatPusherPowered_ShouldPushHeatNow ) )
                return false;

            // Detour Verse.CompGlower.ShouldBeLitNow
            PropertyInfo Verse_CompGlower_ShouldBeLitNow = typeof( CompGlower ).GetProperty( "ShouldBeLitNow", BindingFlags.Instance | BindingFlags.NonPublic );
#if DEBUG
            if( Verse_CompGlower_ShouldBeLitNow == null )
            {
                CCL_Log.Error( "Unable to find 'Verse.CompGlower.ShouldBeLitNow'" );
                return false;
            }
#endif
            MethodInfo Verse_CompGlower_ShouldBeLitNow_Getter = Verse_CompGlower_ShouldBeLitNow.GetGetMethod( true );
            MethodInfo CCL_CompGlower_ShouldBeLitNow = typeof( Detour._CompGlower ).GetMethod( "_ShouldBeLitNow", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_CompGlower_ShouldBeLitNow_Getter, CCL_CompGlower_ShouldBeLitNow ) )
                return false;

            // Detour RimWorld.MainTabWindow_Research.DrawLeftRect "NotFinished" predicate function
            // Use build number to get the correct predicate function
            var RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = string.Empty;
            var RimWorld_Build = RimWorld.VersionControl.CurrentBuild;
            switch( RimWorld_Build )
            {
            case 1220:
            case 1230:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__460";
                break;
            case 1232:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__45E";
                break;
            case 1234:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__45F";
                break;
            default:
                CCL_Log.Trace(
                    Verbosity.Warnings,
                    "CCL needs updating for RimWorld build " + RimWorld_Build.ToString() );
                break;
            }
            if( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name != string.Empty )
            {
                MethodInfo RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished = typeof( RimWorld.MainTabWindow_Research ).GetMethod( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name, BindingFlags.Static | BindingFlags.NonPublic );
                MethodInfo CCL_MainTabWindow_Research_DrawLeftRect_NotFinishedNotLockedOut = typeof( Detour._MainTabWindow_Research ).GetMethod( "_NotFinishedNotLockedOut", BindingFlags.Static | BindingFlags.NonPublic );
                if( !Detours.TryDetourFromTo( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished, CCL_MainTabWindow_Research_DrawLeftRect_NotFinishedNotLockedOut ) )
                    return false;
            }
            
            // Detour RimWorld.SocialProperness.IsSociallyProper
            MethodInfo RimWorld_SocialProperness_IsSociallyProper = typeof( SocialProperness ).GetMethods().First<MethodInfo>( ( arg ) => (
                ( arg.Name == "IsSociallyProper" ) &&
                ( arg.GetParameters().Count() == 4 )
            ) );
            MethodInfo CCL_SocialProperness_IsSociallyProper = typeof( Detour._SocialProperness ).GetMethod( "_IsSociallyProper", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_SocialProperness_IsSociallyProper, CCL_SocialProperness_IsSociallyProper ) )
                return false;

            // Detour Verse.ThingListGroupHelper.Includes
            MethodInfo Verse_ThingListGroupHelper_Includes = typeof( ThingListGroupHelper ).GetMethod( "Includes", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_ThingListGroupHelper_Includes = typeof( Detour._ThingListGroupHelper ).GetMethod( "_Includes", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_ThingListGroupHelper_Includes, CCL_ThingListGroupHelper_Includes ) )
                return false;

            // Detour RimWorld.GenConstruct.CanBuildOnTerrain
            MethodInfo RimWorld_GenConstruct_CanBuildOnTerrain = typeof( GenConstruct ).GetMethod( "CanBuildOnTerrain", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_GenConstruct_CanBuildOnTerrain = typeof( Detour._GenConstruct ).GetMethod( "_CanBuildOnTerrain", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_GenConstruct_CanBuildOnTerrain, CCL_GenConstruct_CanBuildOnTerrain ) )
                return false;

            /*
            // Detour 
            MethodInfo foo = typeof( foo_class ).GetMethod( "foo_method", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_bar = typeof( Detour._bar ).GetMethod( "_bar_method", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( foo, CCL_bar ) )
                return false;

            */

            return true;
        }

        private void                        FixGlowers()
        {
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => (
                ( def != null )&&
                ( def.comps != null )&&
                ( def.HasComp( typeof( CompGlower ) ) )
            ) ) )
            {
                var compGlower = def.GetCompProperties<CompProperties_Glower>();
                compGlower.compClass = typeof( CompGlowerToggleable );
            }
        }

        private void                        UpgradeNutrientPasteDispensers()
        {
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => def.thingClass == typeof( Building_NutrientPasteDispenser ) ) )
            {
                def.thingClass = typeof( Building_AdvancedPasteDispenser );
            }
        }

    }

}
