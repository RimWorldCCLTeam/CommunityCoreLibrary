using System;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class DetourInjector : SpecialInjector
    {

        public override bool                Inject()
        {
#if DEVELOPER
            DumpAllTypesFieldsPropertiesAndMethods();
#endif

            // Make sure custom doors are region barriers
            FixDoors();

            // Change CompGlower into CompGlowerToggleable
            FixGlowers();

            // Detour Verse.GenSpawn.CanPlaceBlueprintOver
            MethodInfo Verse_GenSpawn_CanPlaceBlueprintOver = typeof( GenSpawn ).GetMethod( "CanPlaceBlueprintOver", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_GenSpawn_CanPlaceBlueprintOver = typeof( Detour._GenSpawn ).GetMethod( "_CanPlaceBlueprintOver", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_GenSpawn_CanPlaceBlueprintOver, CCL_GenSpawn_CanPlaceBlueprintOver ) )
                return false;

            // Detour Verse.RegionMaker.CheckRegionableAndProcessNeighbor
            Detour._RegionMaker._TryMakePortalSpan = typeof( RegionMaker ).GetMethod( "TryMakePortalSpan", BindingFlags.Static | BindingFlags.NonPublic );
#if DEBUG
            if( Detour._RegionMaker._TryMakePortalSpan == null )
            {
                CCL_Log.Error( "Unable to find 'Verse.RegionMaker.TryMakePortalScan'" );
                return false;
            }
#endif
            MethodInfo Verse_RegionMaker_CheckRegionableAndProcessNeighbor = typeof( RegionMaker ).GetMethod( "CheckRegionableAndProcessNeighbor", BindingFlags.Static | BindingFlags.NonPublic, null, new [] { typeof( IntVec3 ), typeof( Rot4 ) }, null );
            MethodInfo CCL_RegionMaker_CheckRegionableAndProcessNeighbor = typeof( Detour._RegionMaker ).GetMethod( "_CheckRegionableAndProcessNeighbor", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_RegionMaker_CheckRegionableAndProcessNeighbor, CCL_RegionMaker_CheckRegionableAndProcessNeighbor ) )
                return false;

            // Detour RimWorld.TargetingParameters.CanTarget
            MethodInfo RimWorld_TargetingParameters_CanTarget = typeof( TargetingParameters ).GetMethod( "CanTarget", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_TargetingParameters_CanTarget = typeof( Detour._TargetingParameters ).GetMethod( "_CanTarget", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_TargetingParameters_CanTarget, CCL_TargetingParameters_CanTarget ) )
                return false;

            // Detour RimWorld.GenPlant.CanEverPlantAt
            MethodInfo RimWorld_GenPlant_CanEverPlantAt = typeof( GenPlant ).GetMethod( "CanEverPlantAt", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_GenPlant_CanEverPlantAt = typeof( Detour._GenPlant ).GetMethod( "_CanEverPlantAt", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_GenPlant_CanEverPlantAt, CCL_GenPlant_CanEverPlantAt ) )
                return false;

            // Detour RimWorld.StatWorker.ShouldShowFor
            MethodInfo RimWorld_StatWorker_ShouldShowFor = typeof( StatWorker ).GetMethod( "ShouldShowFor", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_StatWorker_ShouldShowFor = typeof( Detour._StatWorker ).GetMethod( "_ShouldShowFor", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_StatWorker_ShouldShowFor, CCL_StatWorker_ShouldShowFor ) )
                return false;

            // Detour Verse.SectionLayer_IndoorMask.Regenerate
            Type Verse_Sectionlayer_IndoorMask = Controller.Data.Assembly_CSharp.GetType( "Verse.SectionLayer_IndoorMask" );
#if DEBUG
            if( Verse_Sectionlayer_IndoorMask == null )
            {
                CCL_Log.Error( "Unable to find 'Verse.Sectionlayer_IndoorMask'" );
                return false;
            }
#endif
            Detour._SectionLayer_IndoorMask._HideRainPrimary = Verse_Sectionlayer_IndoorMask.GetMethod( "HideRainPrimary", BindingFlags.Instance | BindingFlags.NonPublic );
#if DEBUG
            if( Detour._SectionLayer_IndoorMask._HideRainPrimary == null )
            {
                CCL_Log.Error( "Unable to find 'Verse.Sectionlayer_IndoorMask.HideRainPrimary'" );
                return false;
            }
#endif
            MethodInfo Verse_SectionLayer_IndoorMask_Regenerate = Verse_Sectionlayer_IndoorMask.GetMethod( "Regenerate", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_SectionLayer_IndoorMask_Regenerate = typeof( Detour._SectionLayer_IndoorMask ).GetMethod( "_Regenerate", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_SectionLayer_IndoorMask_Regenerate, CCL_SectionLayer_IndoorMask_Regenerate ) )
                return false;

            // Detour RimWorld.Building_Door.AlignQualityAgainst
            MethodInfo RimWorld_Building_Door_AlignQualityAgainst = typeof( Building_Door ).GetMethod( "AlignQualityAgainst", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_Building_Door_AlignQualityAgainst = typeof( Detour._Building_Door ).GetMethod( "_AlignQualityAgainst", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_Building_Door_AlignQualityAgainst, CCL_Building_Door_AlignQualityAgainst ) )
                return false;

            // Detour Verse.GhostDrawer.GhostGraphicFor
            MethodInfo Verse_GhostDrawer_GhostGraphicFor = typeof( GhostDrawer ).GetMethod( "GhostGraphicFor", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_GhostDrawer_GhostGraphicFor = typeof( Detour._GhostDrawer ).GetMethod( "_GhostGraphicFor", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_GhostDrawer_GhostGraphicFor, CCL_GhostDrawer_GhostGraphicFor ) )
                return false;

            // Detour Verse.GenPlace.PlaceSpotQualityAt
            MethodInfo Verse_GenPlace_PlaceSpotQualityAt = typeof( GenPlace ).GetMethod( "PlaceSpotQualityAt", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_GenPlace_PlaceSpotQualityAt = typeof( Detour._GenPlace ).GetMethod( "_PlaceSpotQualityAt", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_GenPlace_PlaceSpotQualityAt, CCL_GenPlace_PlaceSpotQualityAt ) )
                return false;

            // Detour RimWorld.JobGiver_Binge.DrinkAlcoholJob
            MethodInfo RimWorld_JobGiver_Binge_DrinkAlcoholJob = typeof( JobGiver_Binge ).GetMethod( "DrinkAlcoholJob", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_JobGiver_Binge_DrinkAlcoholJob = typeof( Detour._JobGiver_Binge ).GetMethod( "_DrinkAlcoholJob", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobGiver_Binge_DrinkAlcoholJob, CCL_JobGiver_Binge_DrinkAlcoholJob ) )
                return false;

            // Detour Verse.BrokenStateWorker_Binging.StateCanOccur
            MethodInfo Verse_BrokenStateWorker_Binging_StateCanOccur = typeof( MentalStateWorker_Binging ).GetMethod( "StateCanOccur", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_BrokenStateWorker_Binging_StateCanOccur = typeof( Detour._MentalStateWorker_Binging).GetMethod( "_StateCanOccur", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( Verse_BrokenStateWorker_Binging_StateCanOccur, CCL_BrokenStateWorker_Binging_StateCanOccur ) )
                return false;

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

            // Detour RimWorld.DropCellFinder.TradeDropSpot
            MethodInfo RimWorld_DropCellFinder_TradeDropSpot = typeof( DropCellFinder ).GetMethod( "TradeDropSpot", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_DropCellFinder_TradeDropSpot = typeof( Detour._DropCellFinder ).GetMethod( "_TradeDropSpot", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_DropCellFinder_TradeDropSpot, CCL_DropCellFinder_TradeDropSpot ) )
                return false;

            // Detour RimWorld.PassingShip.Depart
            MethodInfo RimWorld_PassingShop_Depart = typeof( PassingShip ).GetMethod( "Depart", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_PassingShop_Depart = typeof( Detour._PassingShip ).GetMethod( "_Depart", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_PassingShop_Depart, CCL_PassingShop_Depart ) )
                return false;

            // Detour RimWorld.FoodUtility.BestFoodSourceFor
            MethodInfo RimWorld_FoodUtility_BestFoodSourceFor = typeof( FoodUtility ).GetMethod( "BestFoodSourceFor", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_FoodUtility_BestFoodSourceFor = typeof( Detour._FoodUtility ).GetMethod( "_BestFoodSourceFor", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_BestFoodSourceFor, CCL_FoodUtility_BestFoodSourceFor ) )
                return false;

            // Detour RimWorld.FoodUtility.NutritionAvailableFromFor
            MethodInfo RimWorld_FoodUtility_NutritionAvailableFromFor = typeof( FoodUtility ).GetMethod( "NutritionAvailableFromFor", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_FoodUtility_NutritionAvailableFromFor = typeof( Detour._FoodUtility ).GetMethod( "_NutritionAvailableFromFor", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_FoodUtility_NutritionAvailableFromFor, CCL_FoodUtility_NutritionAvailableFromFor ) )
                return false;

            // Detour RimWorld.Building_NutrientPasteDispenser.AdjacentReachableHopper
            MethodInfo RimWorld_Building_NutrientPasteDispenser_AdjacentReachableHopper = typeof( Building_NutrientPasteDispenser ).GetMethod( "AdjacentReachableHopper", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_Building_NutrientPasteDispenser_AdjacentReachableHopper = typeof( Detour._Building_NutrientPasteDispenser ).GetMethod( "_AdjacentReachableHopper", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_Building_NutrientPasteDispenser_AdjacentReachableHopper, CCL_Building_NutrientPasteDispenser_AdjacentReachableHopper ) )
                return false;

            // Detour RimWorld.Building_NutrientPasteDispenser.FindFeedInAnyHopper
            MethodInfo RimWorld_Building_NutrientPasteDispenser_FindFeedInAnyHopper = typeof( Building_NutrientPasteDispenser ).GetMethod( "FindFeedInAnyHopper", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_Building_NutrientPasteDispenser_FindFeedInAnyHopper = typeof( Detour._Building_NutrientPasteDispenser ).GetMethod( "_FindFeedInAnyHopper", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_Building_NutrientPasteDispenser_FindFeedInAnyHopper, CCL_Building_NutrientPasteDispenser_FindFeedInAnyHopper ) )
                return false;

            // Detour RimWorld.Building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers
            MethodInfo RimWorld_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers = typeof( Building_NutrientPasteDispenser ).GetMethod( "HasEnoughFeedstockInHoppers", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers = typeof( Detour._Building_NutrientPasteDispenser ).GetMethod( "_HasEnoughFeedstockInHoppers", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers, CCL_Building_NutrientPasteDispenser_HasEnoughFeedstockInHoppers ) )
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

            // Detour RimWorld.JobDriver_Ingest.MakeNewToils
            MethodInfo RimWorld_JobDriver_Ingest_MakeNewToils = typeof( JobDriver_Ingest ).GetMethod( "MakeNewToils", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_Ingest_MakeNewToils = typeof( Detour._JobDriver_Ingest ).GetMethod( "_MakeNewToils", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_Ingest_MakeNewToils, CCL_JobDriver_Ingest_MakeNewToils ) )
                return false;

            // Detour RimWorld.JobGiver_GetFood.TryGiveTerminalJob
            MethodInfo RimWorld_JobGiver_GetFood_TryGiveTerminalJob = typeof( JobGiver_GetFood ).GetMethod( "TryGiveTerminalJob", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobGiver_GetFood_TryGiveTerminalJob = typeof( Detour._JobGiver_GetFood ).GetMethod( "_TryGiveTerminalJob", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobGiver_GetFood_TryGiveTerminalJob, CCL_JobGiver_GetFood_TryGiveTerminalJob ) )
                return false;

            // Detour RimWorld.JobDriver_SocialRelax.MakeNewToils
            MethodInfo RimWorld_JobDriver_SocialRelax_MakeNewToils = typeof( JobDriver_SocialRelax ).GetMethod( "MakeNewToils", BindingFlags.Instance | BindingFlags.NonPublic );
            MethodInfo CCL_JobDriver_SocialRelax_MakeNewToils = typeof( Detour._JobDriver_SocialRelax ).GetMethod( "_MakeNewToils", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_JobDriver_SocialRelax_MakeNewToils, CCL_JobDriver_SocialRelax_MakeNewToils ) )
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
            case 1135:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__3E9";
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
            
            /*
            // Detour 
            MethodInfo foo = typeof( foo_class ).GetMethod( "foo_method", BindingFlags.Static | BindingFlags.NonPublic );
            MethodInfo CCL_bar = typeof( Detour._bar ).GetMethod( "_bar_method", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( foo, CCL_bar ) )
                return false;

            */

            return true;
        }

        private void                        FixDoors()
        {
            foreach( var doorDef in DefDatabase<ThingDef>.AllDefs.Where( def => (
                ( def.thingClass != null )&&
                (
                    ( def.thingClass == typeof( Building_Door ) )||
                    ( def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                )
            ) ) )
            {
                doorDef.regionBarrier = true;
            }
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

#if DEVELOPER
        private void                        DumpAllTypesFieldsPropertiesAndMethods()
        {
            CCL_Log.Write( "All Types:" );
            foreach( var type in Controller.Data.Assembly_CSharp.GetTypes() )
            {
                var str = "\n\t" + type.FullName;
                str += "\n\t\tFields:";
                foreach( var entity in type.GetFields( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                {
                    str += "\n\t\t\t" + entity.Name;
                    if( entity.IsStatic )
                        str += " (Static)";
                    else
                        str += " (Instance)";
                    if( entity.IsPrivate ) str += " (NonPublic)";
                    if( entity.IsPublic ) str += " (Public)";
                }
                str += "\n\t\tProperties:";
                foreach( var entity in type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                {
                    str += "\n\t\t\t" + entity.Name;
                    var method = entity.GetGetMethod();
                    if( method != null )
                    {
                        str += " (Public Get)";
                    }
                    else
                    {
                        method = entity.GetGetMethod( true );
                        if( method != null ) str += " (NonPublic Get)";
                    }
                    method = entity.GetSetMethod();
                    if( method != null )
                    {
                        str += " (Public Set)";
                    }
                    else
                    {
                        method = entity.GetSetMethod( true );
                        if( method != null ) str += " (NonPublic Set)";
                    }
                }
                str += "\n\t\tMethods:";
                foreach( var entity in type.GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                {
                    str += "\n\t\t\t" + entity.Name;
                    if( entity.IsStatic )
                        str += " (Static)";
                    else
                        str += " (Instance)";
                    if( entity.IsPrivate ) str += " (NonPublic)";
                    if( entity.IsPublic ) str += " (Public)";
                    if( entity.GetParameters() != null )
                    {
                        str += " Parameters:";
                        foreach( var pi in entity.GetParameters() )
                        {
                            str += " " + pi.ParameterType.ToString();
                            if( pi.IsOut ) str += " (out)";
                            if( pi.IsRetval ) str += " (ret)";
                        }
                    }
                }
                CCL_Log.Write( str );
            }
        }
#endif

    }

}
