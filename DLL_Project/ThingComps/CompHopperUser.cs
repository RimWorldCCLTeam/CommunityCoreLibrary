using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class CompHopperUser : ThingComp
    {

        private class HopperSettingsAmount
        {

            public List<CompHopper>         hoppers = new List<CompHopper>();
            public StorageSettings          settings = new StorageSettings();
            public ThingCategoryDef         categoryDef;
            public ThingDef                 thingDef;
            public int                      count;
            private int                     hoppersRequired = -1;

            public                          HopperSettingsAmount( ThingDef thingDef, float count )
            {
                this.categoryDef = null;
                this.thingDef = thingDef;
                this.count = Mathf.CeilToInt( count );
            }

            public                          HopperSettingsAmount( ThingCategoryDef categoryDef, float count )
            {
                this.categoryDef = categoryDef;
                this.thingDef = null;
                this.count = Mathf.CeilToInt( count );
            }

            public bool                     ShouldBeRefrigerated
            {
                get
                {
                    return settings.filter.AllowedThingDefs.Any( def => def.HasComp( typeof( CompRottable ) ) );
                }
            }

            public int                      HoppersRequired
            {
                get
                {
                    if( hoppersRequired < 0 )
                    {
                        hoppersRequired = 0;
                        if( this.categoryDef != null )
                        {
                            int largest = 0;
                            foreach( var thingDef in categoryDef.childThingDefs )
                            {
                                if( thingDef.stackLimit > largest )
                                {
                                    largest = thingDef.stackLimit;
                                }
                            }
                            hoppersRequired = Mathf.Max( 1, Mathf.CeilToInt( (float) count / largest ) );
                        }
                        if( this.thingDef != null )
                        {
                            hoppersRequired = Mathf.Max( 1, Mathf.CeilToInt( (float) count / thingDef.stackLimit ) );
                        }
                    }
                    return hoppersRequired;
                }
            }

            public static void              AddToList( List<HopperSettingsAmount> list, ThingDef thingDef, float baseCount, RecipeDef recipe )
            {
                int countNeeded = CountForThingDef( thingDef, baseCount, recipe );
                for( int index = 0; index < list.Count; ++index )
                {
                    if( list[ index ].thingDef == thingDef )
                    {
                        if( countNeeded > list[ index ].count )
                        {
                            list[ index ] = new HopperSettingsAmount( list[ index ].thingDef, countNeeded );
                        }
                        return;
                    }
                }
                list.Add( new HopperSettingsAmount( thingDef, countNeeded ) );
            }

            public static void              AddToList( List<HopperSettingsAmount> list, ThingCategoryDef categoryDef, float baseCount, RecipeDef recipe )
            {
                int countNeeded = CountForCategoryDef( categoryDef, baseCount, recipe );
                for( int index = 0; index < list.Count; ++index )
                {
                    if( list[ index ].categoryDef == categoryDef )
                    {
                        if( countNeeded > list[ index ].count )
                        {
                            list[ index ] = new HopperSettingsAmount( list[ index ].categoryDef, countNeeded );
                        }
                        return;
                    }
                }
                list.Add( new HopperSettingsAmount( categoryDef, countNeeded ) );
            }

            public static int               CountForThingDef( ThingDef thingDef, float baseCount, RecipeDef recipe )
            {
                if( !recipe.IsIngredient( thingDef ) )
                {
                    return 0;
                }
                if( baseCount < 0 )
                {
                    foreach( var ingredientCount in recipe.ingredients )
                    {
                        if( ingredientCount.filter.AllowedThingDefs.Contains( thingDef ) )
                        {
                            baseCount = ingredientCount.GetBaseCount();
                            break;
                        }
                    }
                }
                float ingredientValue = recipe.IngredientValueGetter.ValuePerUnitOf( thingDef );
                return Mathf.Max( 1, Mathf.CeilToInt( baseCount / ingredientValue ) );
            }

            public static int               CountForCategoryDef( ThingCategoryDef categoryDef, float baseCount, RecipeDef recipe )
            {
                int largest = 0;
                foreach( var thingDef in categoryDef.DescendantThingDefs )
                {
                    int thisThingCount = CountForThingDef( thingDef, baseCount, recipe );
                    if( thisThingCount > largest )
                    {
                        largest = thisThingCount;
                    }
                }

                foreach( var childCategoryDef in categoryDef.childCategories )
                {
                    int thisCategoryCount = CountForCategoryDef( childCategoryDef, baseCount, recipe );
                    if( thisCategoryCount > largest )
                    {
                        largest = thisCategoryCount;
                    }
                }

                return largest;
            }

        }

        private StorageSettings             resourceSettings;
        private ThingFilter                 xmlResources;

        private List<RecipeDef>             recipeFilter = new List<RecipeDef>();

        private List<HopperSettingsAmount>  hopperSettings = new List<HopperSettingsAmount>();

        private bool                        settingsBuilt = false;

        #region Neighbouring Cell Enumeration

        private List<IntVec3>               cachedAdjCellsCardinal;
        private List<IntVec3>               AdjCellsCardinalInBounds
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.AdjCellsCardinalInBounds", this.parent.ThingID ) );
                if( cachedAdjCellsCardinal == null )
                {
                    cachedAdjCellsCardinal = GenAdj.CellsAdjacentCardinal( parent ).Where( c => c.InBounds() ).ToList();
                }
                return cachedAdjCellsCardinal;
            }
        }

        #endregion

        #region Comps & Properties

        private CompProperties_HopperUser   HopperProperties
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.HopperProperties", this.parent.ThingID ) );
                return ( props as CompProperties_HopperUser );
            }
        }

        public Building                     Building
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.Building", this.parent.ThingID ) );
                return parent as Building;
            }
        }

        #endregion

        #region Core Comp Overrides

        public override void                PostSpawnSetup()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.PostSpawnSetup()", this.parent.ThingID ) );
            base.PostSpawnSetup();

            FindAndProgramHoppers();
        }

        public override void                PostDeSpawn()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.PostDeSpawn()", this.parent.ThingID ) );
            base.PostDeSpawn();

            // Scan for hoppers and deprogram each one
            var hoppers = FindHoppers();
            if( !hoppers.NullOrEmpty() )
            {
                foreach( var hopper in hoppers )
                {
                    hopper.DeprogramHopper();
                }
            }
        }

        #endregion

        #region Storage Settings

        public void                         ResetResourceSettings()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.ResetResourceSettings()", this.parent.ThingID ) );
            recipeFilter.Clear();
            hopperSettings.Clear();
            resourceSettings = null;
            settingsBuilt = false;
        }

        public StorageSettings              ResourceSettings
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.ResourceSettings", this.parent.ThingID ) );
                if( resourceSettings == null )
                {
                    var iHopperUser = parent as IHopperUser;
                    if(
                        ( Resources == null )&&
                        (
                            ( iHopperUser == null )||
                            ( iHopperUser.ResourceFilter == null )
                        )
                    )
                    {
                        // Does not contain xml resource filter
                        // or (properly) implement IHopperUser
                        CCL_Log.TraceMod(
                            parent.def,
                            Verbosity.FatalErrors,
                            "Configuration error (missing xml definition for 'resources' in CompProperties_HopperUser or ThingClass does not implement IHopperUser)"
                        );
                        return null;
                    }

                    // Create storage settings
                    resourceSettings = new StorageSettings();

                    // Set priority
                    resourceSettings.Priority = StoragePriority.Important;

                    // Set the filter from the hopper user
                    if( iHopperUser != null )
                    {
                        // Copy a filter from a building implementing IHopperUser
                        resourceSettings.filter.CopyFrom( iHopperUser.ResourceFilter );
                    }
                    else
                    {
                        // Copy a filter from xml flag
                        resourceSettings.filter.CopyFrom( Resources );
                    }

                    // Block default special filters
                    //resourceSettings.filter.BlockDefaultAcceptanceFilters();

                    // Resolve references again
                    resourceSettings.filter.ResolveReferences();

                    // Disallow quality
                    resourceSettings.filter.allowedQualitiesConfigurable = false;

                }
                return resourceSettings;
            }
        }

        public ThingFilter                  Resources
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.Resources", this.parent.ThingID ) );
                if( xmlResources == null )
                {
                    if( HopperProperties != null )
                    {
                        xmlResources = HopperProperties.resources;
                        if( xmlResources != null )
                        {
                            xmlResources.ResolveReferences();
                        }
                    }
                }
                return xmlResources;
            }
        }

        #endregion

        #region Filter Builders

        private void                        MergeIngredientIntoFilter( ThingFilter filter, IngredientCount ingredient )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.MergeIngredientIntoFilter( ThingFilter, IngredientCount )", this.parent.ThingID ) );
            if( ingredient.filter != null )
            {
                if( !ingredient.filter.Categories().NullOrEmpty() )
                {
                    foreach( var category in ingredient.filter.Categories() )
                    {
                        var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                        filter.SetAllow( categoryDef, true );
                    }
                }
                if( !ingredient.filter.ThingDefs().NullOrEmpty() )
                {
                    foreach( var thingDef in ingredient.filter.ThingDefs() )
                    {
                        filter.SetAllow( thingDef, true );
                    }
                }
            }
        }

        private void                        MergeExceptionsIntoFilter( ThingFilter filter, ThingFilter exceptionFilter )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.MergeExceptionsIntoFilter( ThingFilter, ThingFilter )", this.parent.ThingID ) );
            if( !exceptionFilter.ExceptedCategories().NullOrEmpty() )
            {
                foreach( var category in exceptionFilter.ExceptedCategories() )
                {
                    var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                    filter.SetAllow( categoryDef, false );
                }
            }
            if( !exceptionFilter.ExceptedThingDefs().NullOrEmpty() )
            {
                foreach( var thingDef in exceptionFilter.ExceptedThingDefs() )
                {
                    filter.SetAllow( thingDef, false );
                }
            }
        }

        public bool                         IsRecipeInFilter( RecipeDef recipe )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.IsRecipeInFilter( {1} )", this.parent.ThingID, recipe == null ? "null" : recipe.defName ) );
            return recipeFilter.Contains( recipe );
        }

        public void                         MergeRecipeIntoFilter( ThingFilter filter, RecipeDef recipe )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.MergeRecipeInfoFilter( ThingFilter, {1} )", this.parent.ThingID, recipe == null ? "null" : recipe.defName ) );
            if( recipeFilter.Contains( recipe ) )
            {
                return;
            }
            recipeFilter.Add( recipe );
            if( !recipe.ingredients.NullOrEmpty() )
            {
                foreach( var ingredient in recipe.ingredients )
                {
                    MergeIngredientIntoFilter( filter, ingredient );
                }
            }
            if( recipe.defaultIngredientFilter != null )
            {
                MergeExceptionsIntoFilter( filter, recipe.defaultIngredientFilter );
            }
            if( recipe.fixedIngredientFilter != null )
            {
                MergeExceptionsIntoFilter( filter, recipe.fixedIngredientFilter );
            }
        }

        private void                        MergeIngredientIntoHopperSettings( IngredientCount ingredient, RecipeDef recipe )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.MergeIngredientIntoHopperSettings( IngredientCount, {1} )", this.parent.ThingID, recipe == null ? "null" : recipe.defName ) );
            if( ingredient.filter != null )
            {
                if( !ingredient.filter.Categories().NullOrEmpty() )
                {
                    foreach( var category in ingredient.filter.Categories() )
                    {
                        var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                        HopperSettingsAmount.AddToList( hopperSettings, categoryDef, ingredient.GetBaseCount(), recipe );
                    }
                }
                if( !ingredient.filter.ThingDefs().NullOrEmpty() )
                {
                    foreach( var thingDef in ingredient.filter.ThingDefs() )
                    {
                        HopperSettingsAmount.AddToList( hopperSettings, thingDef, ingredient.GetBaseCount(), recipe );
                    }
                }
            }
        }

        private void                        MergeExceptionsIntoHopperSettings( ThingFilter exceptionFilter )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.MergeExceptionsIntoHopperSettings( ThingFilter )", this.parent.ThingID ) );
            if( !exceptionFilter.ExceptedCategories().NullOrEmpty() )
            {
                foreach( var category in exceptionFilter.ExceptedCategories() )
                {
                    var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                    foreach( var hopperSetting in hopperSettings )
                    {
                        hopperSetting.settings.filter.SetAllow( categoryDef, false );
                    }
                }
            }
            if( !exceptionFilter.ExceptedThingDefs().NullOrEmpty() )
            {
                foreach( var thingDef in exceptionFilter.ExceptedThingDefs() )
                {
                    foreach( var hopperSetting in hopperSettings )
                    {
                        hopperSetting.settings.filter.SetAllow( thingDef, false );
                    }
                }
            }
        }

        private void                        BuildHopperSettings()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.BuildHopperSettings()", this.parent.ThingID ) );
            // Create initial list of hopper settings from recipe ingredients
            foreach( var recipe in recipeFilter )
            {
                if( !recipe.ingredients.NullOrEmpty() )
                {
                    foreach( var ingredient in recipe.ingredients )
                    {
                        MergeIngredientIntoHopperSettings( ingredient, recipe );
                    }
                }
            }

            // Assign hopper settings filters from ingredients
            foreach( var hopperSetting in hopperSettings )
            {
                if( hopperSetting.categoryDef != null )
                {
                    hopperSetting.settings.filter.SetAllow( hopperSetting.categoryDef, true );
                }
                if( hopperSetting.thingDef != null )
                {
                    hopperSetting.settings.filter.SetAllow( hopperSetting.thingDef, true );
                }
            }

            // Add exceptions to hopper settings filters
            foreach( var recipe in recipeFilter )
            {
                if( recipe.defaultIngredientFilter != null )
                {
                    MergeExceptionsIntoHopperSettings( recipe.defaultIngredientFilter );
                }
                if( recipe.fixedIngredientFilter != null )
                {
                    MergeExceptionsIntoHopperSettings( recipe.fixedIngredientFilter );
                }
            }

            // Exclude categories and things from other categories
            for( int index = 0; index < hopperSettings.Count; index++ )
            {
                var settingA = hopperSettings[ index ];
                if(
                    ( settingA.categoryDef != null )&&
                    ( settingA.settings.filter.AllowedDefCount > 1 )
                )
                {
                    for( int index2 = 0; index2 < hopperSettings.Count; index2++ )
                    {
                        if( index != index2 )
                        {
                            var settingB = hopperSettings[ index2 ];
                            if(
                                (
                                    ( settingB.categoryDef != null )&&
                                    ( settingA.categoryDef.ThisAndChildCategoryDefs.Contains( settingB.categoryDef ) )
                                )||
                                (
                                    ( settingB.thingDef != null )&&
                                    ( settingA.categoryDef.DescendantThingDefs.Contains( settingB.thingDef ) )
                                )
                            )
                            {
                                foreach( var thingDef in settingB.settings.filter.AllowedThingDefs )
                                {
                                    settingA.settings.filter.SetAllow( thingDef, false );
                                }
                            }
                        }
                    }
                }
            }

            // Recheck hopper settings count requirements for the recipes
            foreach( var hopperSetting in hopperSettings )
            {
                int largest = 0;
                foreach( var recipe in recipeFilter )
                {
                    foreach( var thingDef in hopperSetting.settings.filter.AllowedThingDefs )
                    {
                        int thisThingDefCount = HopperSettingsAmount.CountForThingDef( thingDef, -1, recipe );
                        if( thisThingDefCount > largest )
                        {
                            largest = thisThingDefCount;
                        }
                    }
                }
                hopperSetting.count = largest;
            }

            // Remove empty hopper settings from the list
            for( int index = 0; index < hopperSettings.Count; index++ )
            {
                var hopperSetting = hopperSettings[ index ];
                if( hopperSetting.settings.filter.AllowedDefCount < 1 )
                {
                    hopperSettings.Remove( hopperSetting );
                }
            }

            // Sort the hopper settings from most required ingredients to least
            hopperSettings.Sort( ( x, y ) => ( x.count > y.count ? -1 : 1 ) );

            // Finalize hopper settings
            for( int index = 0; index < hopperSettings.Count; ++index )
            {
                var hopperSetting = hopperSettings[ index ];
                hopperSetting.settings.Priority = StoragePriority.Important;
                hopperSetting.settings.filter.ResolveReferences();
                //hopperSetting.settings.filter.BlockDefaultAcceptanceFilters();
                hopperSetting.settings.filter.allowedQualitiesConfigurable = false;
            }

            settingsBuilt = true;
        }

        #endregion

        #region Hopper Programming

        private void                        ProgramHoppersSimple( List<CompHopper> hoppers )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.ProgramHoppersSimple( List<CompHopper> )", this.parent.ThingID ) );
            // Blanket all hoppers with the main filter
            foreach( var hopper in hoppers )
            {
                hopper.ProgramHopper( ResourceSettings );
            }
        }

        private void                        ProgramHoppersIndividual( List<CompHopper> hoppers )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.ProgramHoppersIndividual( List<CompHopper> )", this.parent.ThingID ) );
            // Try to find hoppers which match already
            var freeHoppers = new List<CompHopper>();
            foreach( var hopper in hoppers )
            {
                if( !hopper.WasProgrammed )
                {
                    freeHoppers.AddUnique( hopper );
                }
                else
                {
                    var hopperStoreSettings = hopper.GetStoreSettings();
                    bool found = false;
                    foreach( var hopperSetting in hopperSettings )
                    {
                        if( hopperStoreSettings.filter.Matches( hopperSetting.settings.filter ) )
                        {
                            if(
                                ( hopperSetting.HoppersRequired > hopperSetting.hoppers.Count )&&
                                ( hopperSetting.ShouldBeRefrigerated == hopper.IsRefrigerated )
                            )
                            {
                                // Filters match, refrigeration requirements match, now check existing resources
                                var currentResources = hopper.GetAllResources( resourceSettings.filter );
                                if( currentResources.NullOrEmpty() )
                                {
                                    // None here so we can use it for this
                                    found = true;
                                }
                                else
                                {
                                    // Something here, prefer hoppers with resources which are allowed already
                                    foreach( var thing in currentResources )
                                    {
                                        if( hopperSetting.settings.filter.Allows( thing ) )
                                        {
                                            found = true;
                                            break;
                                        }
                                    }
                                }
                                if( found )
                                {
                                    hopperSetting.hoppers.Add( hopper );
                                    break;
                                }
                            }
                        }
                        if( found == true )
                        {
                            break;
                        }
                    }
                    if( !found )
                    {
                        // No matching hopper for this setting
                        hopper.DeprogramHopper();
                        freeHoppers.Add( hopper );
                    }
                }
            }

            // Assign any resources which need hoppers matching refrigeration requirements
            foreach( var hopperSetting in hopperSettings )
            {
                int amountToAssign = hopperSetting.HoppersRequired - hopperSetting.hoppers.Count;
                if( amountToAssign > 0 )
                {
                    for( int index = 0; index < amountToAssign; index++ )
                    {
                        for( int index2 = 0; index2 < freeHoppers.Count; index2++ )
                        {
                            var hopper = freeHoppers[ index2 ];
                            if( hopperSetting.ShouldBeRefrigerated == hopper.IsRefrigerated )
                            {
                                hopperSetting.hoppers.Add( hopper );
                                freeHoppers.Remove( hopper );
                                break;
                            }
                        }
                    }
                }
            }

            // Assign any resources which need hoppers to any hopper regardless of refrigeration requirements
            foreach( var hopperSetting in hopperSettings )
            {
                int amountToAssign = hopperSetting.HoppersRequired - hopperSetting.hoppers.Count;
                if( amountToAssign > 0 )
                {
                    for( int index = 0; index < amountToAssign; index++ )
                    {
                        var hopper = freeHoppers[ 0 ];
                        hopperSetting.hoppers.Add( hopper );
                        freeHoppers.Remove( hopper );
                    }
                }
            }

            // Assign any remaining hoppers to resource settings from most required ingredients to least
            if( !freeHoppers.NullOrEmpty() )
            {
                int nextSetting = 0;
                do
                {
                    var hopper = freeHoppers[ 0 ];
                    bool found = false;
                    bool matchRefrigeration = true;
                    int settingIndex = nextSetting;
                    do
                    {
                        var hopperSetting = hopperSettings[ settingIndex ];
                        if(
                            (
                                ( matchRefrigeration )&&
                                ( hopper.IsRefrigerated == hopperSetting.ShouldBeRefrigerated )
                            )||
                            ( !matchRefrigeration )
                        )
                        {
                            hopperSetting.hoppers.Add( hopper );
                            freeHoppers.Remove( hopper );
                            found = true;
                        }
                        else
                        {
                            settingIndex++;
                            settingIndex %= hopperSettings.Count;
                            if( settingIndex == nextSetting )
                            {
                                matchRefrigeration = false;
                            }
                        }
                    } while ( !found );
                    nextSetting++;
                    nextSetting %= hopperSettings.Count;
                } while ( freeHoppers.Count > 0 );
            }

            // Finally, program the hoppers
            foreach( var hopperSetting in hopperSettings )
            {
                foreach( var hopper in hopperSetting.hoppers )
                {
                    hopper.ProgramHopper( hopperSetting.settings );
                }
            }
        }

        public void                         FindAndProgramHoppers()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.FindAndProgramHoppers()", this.parent.ThingID ) );
            if( ResourceSettings == null )
            {
                // No xml or IHopperUser settings
                return;
            }
            if( !settingsBuilt )
            {
                // Rebuild the hopper settings
                BuildHopperSettings();
            }
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                // No hoppers to program
                return;
            }
            if(
                ( recipeFilter.NullOrEmpty() )||
                ( hoppers.Count < HoppersRequired )
            )
            {
                // No recipe filter or not enough connected hoppers to individually program, do simple programming
                ProgramHoppersSimple( hoppers );
            }
            else
            {
                // Program individual hoppers with individual settings
                ProgramHoppersIndividual( hoppers );
            }
        }

        public void                         FindAndDeprogramHoppers()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.FindAndDeprogramHoppers()", this.parent.ThingID ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                // No hoppers to deprogram
                return;
            }
            foreach( var hopper in hoppers )
            {
                hopper.DeprogramHopper();
            }
        }

        public void                         NotifyHopperAttached()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.NotifyHopperAttached()", this.parent.ThingID ) );
            FindAndDeprogramHoppers();
            FindAndProgramHoppers();
        }

        public void                         NotifyHopperDetached()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.NotifyHopperDetached()", this.parent.ThingID ) );
            FindAndDeprogramHoppers();
            FindAndProgramHoppers();
        }

        #endregion

        #region Hopper Enumeration

        public int                          HoppersRequired
        {
            get
            {
                //Log.Message( string.Format( "{0}.CompHopperUser.HoppersRequired", this.parent.ThingID ) );
                if( hopperSettings.NullOrEmpty() )
                {
                    return 1;
                }
                int count = 0;
                foreach( var resourceAmount in hopperSettings )
                {
                    count += resourceAmount.HoppersRequired;
                }
                return count;
            }
        }

        public List<CompHopper>             FindHoppers()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.FindHoppers()", this.parent.ThingID ) );
            // Find hoppers for building
            var hoppers = new List<CompHopper>();
            var occupiedCells = parent.OccupiedRect();
            foreach( var cell in AdjCellsCardinalInBounds )
            {
                var hopper = FindHopper( cell );
                if (
                    ( hopper != null )&&
                    ( occupiedCells.Cells.Contains( hopper.parent.Position + hopper.parent.Rotation.FacingCell ) )
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add( hopper );
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

        public static List<CompHopper>      FindHoppers( IntVec3 thingCenter, Rot4 thingRot, IntVec2 thingSize )
        {
            //Log.Message( string.Format( "CompHopperUser.FindHoppers( {0}, {1}, {2} )", thingCenter.ToString(), thingRot.ToString(), thingSize.ToString() ) );
            // Find hoppers for near cell
            var hoppers = new List<CompHopper>();
            var occupiedCells = GenAdj.OccupiedRect( thingCenter, thingRot, thingSize );
            foreach( var cell in GenAdj.CellsAdjacentCardinal( thingCenter, thingRot, thingSize ).
                Where( c => c.InBounds() ).ToList() )
            {
                var hopper = FindHopper( cell );
                if (
                    ( hopper != null ) &&
                    ( occupiedCells.Cells.Contains( hopper.Building.Position + hopper.Building.Rotation.FacingCell ) )
                )
                {
                    // Hopper is adjacent and rotated correctly
                    hoppers.Add( hopper );
                }
            }
            // Return list of hoppers connected to this building
            return hoppers;
        }

        public static CompHopper            FindHopper( IntVec3 cell )
        {
            //var str = string.Format( "CompHopperUser.FindHopper( {0} )", cell.ToString() );
            if( !cell.InBounds() )
            {
                //Log.Message( str );
                return null;
            }
            List<Thing> thingList = null;
            if( Scribe.mode != LoadSaveMode.Inactive )
            {   // Find hopper in world matching cell
                if(
                    ( Find.ThingGrid == null )||
                    ( Find.ThingGrid.ThingsAt( cell ).Count() == 0 )
                )
                {
                    //Log.Message( str );
                    return null;
                }
                thingList = Find.ThingGrid.ThingsAt( cell ).ToList();
            }
            else
            {   // Find hopper in cell
                thingList = cell.GetThingList();
            }
            if( !thingList.NullOrEmpty() )
            {
                var hopper = thingList.FirstOrDefault( (thing) =>
                {
                    var thingDef = GenSpawn.BuiltDefOf( thing.def ) as ThingDef;
                    return ( thingDef != null )&&( thingDef.IsHopper() );
                } );
                if( hopper != null )
                {   // Found a hopper
                    //str += " = " + hopper.ThingID;
                    //Log.Message( str );
                    return hopper.TryGetComp<CompHopper>();
                }
            }
            // No hopper found
            //Log.Message( str );
            return null;
        }

        public CompHopper                   FindBestHopperForResources()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.FindBestHopperForResources()", this.parent.ThingID ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return null;
            }
            var bestHopper = (CompHopper)null;
            var bestResource = (Thing)null;
            foreach( var hopper in hoppers )
            {
                // Find best in hopper
                var hopperResources = hopper.GetAllResources( ResourceSettings.filter );
                foreach( var resource in hopperResources )
                {
                    if( resource != null )
                    {
                        if(
                            ( bestHopper == null )||
                            ( resource.stackCount > bestResource.stackCount )
                        )
                        {
                            // First resource or this hopper holds more
                            bestHopper = hopper;
                            bestResource = resource;
                        }
                    }
                }
            }
            // Return the best hopper
            return bestHopper;
        }

        #endregion

        #region Remove Resource(s) From Hoppers

        public bool                         RemoveResourceFromHoppers( ThingDef resourceDef, int resourceCount )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.RemoveResourceFromHoppers( {1}, {2} )", this.parent.ThingID, resourceDef == null ? "null" : resourceDef.defName, resourceCount ) );
            if( !EnoughResourceInHoppers( resourceDef, resourceCount ) )
            {
                return false;
            }
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return false;
            }

            foreach( var hopper in hoppers )
            {
                var resource = hopper.GetResource( resourceDef );
                if( resource!= null )
                {
                    if( resource.stackCount >= resourceCount )
                    {
                        resource.SplitOff( resourceCount );
                        return true;
                    }
                    else
                    {
                        resourceCount -= resource.stackCount;
                        resource.SplitOff( resource.stackCount );
                    }
                }
            }
            // Should always be true...
            return ( resourceCount <= 0 );
        }

        public bool                         RemoveResourcesFromHoppers( int resourceCount )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.RemoveResourcesFromHoppers( {1} )", this.parent.ThingID, resourceCount ) );
            if( !EnoughResourcesInHoppers( resourceCount ) )
            {
                return false;
            }
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return false;
            }

            foreach( var hopper in hoppers )
            {
                var resources = hopper.GetAllResources( ResourceSettings.filter );
                if( !resources.NullOrEmpty() )
                {
                    foreach( var resource in resources )
                    {
                        if( resource.stackCount >= resourceCount )
                        {
                            resource.SplitOff( resourceCount );
                            return true;
                        }
                        else
                        {
                            resourceCount -= resource.stackCount;
                            resource.SplitOff( resource.stackCount );
                        }
                    }
                }
            }
            // Should always be true...
            return ( resourceCount <= 0 );
        }

        public bool                         RemoveResourcesFromHoppers( RecipeDef recipe, List<ThingAmount> chosen )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.RemoveResourcesFromHoppers( {0}, List<ThingAmount> )", this.parent.ThingID, recipe == null ? "null" : recipe.defName ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return false;
            }

            var allResources = new List<Thing>();

            foreach( var hopper in hoppers )
            {
                var hopperResources = hopper.GetAllResources( ResourceSettings.filter );
                if( hopperResources != null )
                {
                    allResources.AddRangeUnique( hopperResources );
                }
            }

            bool removeThings = false;
            if( recipe.allowMixingIngredients )
            {
                removeThings = recipe.TryFindBestRecipeIngredientsInSet_AllowMix( allResources, chosen );
            }
            else
            {
                removeThings = recipe.TryFindBestRecipeIngredientsInSet_NoMix( allResources, chosen );
            }
            if( !removeThings )
            {
                return false;
            }

            foreach( var chosenThing in chosen )
            {
                if( chosenThing.count >= chosenThing.thing.stackCount )
                {
                    chosenThing.thing.Destroy();
                }
                else
                {
                    chosenThing.thing.stackCount -= chosenThing.count;
                }
            }

            return true;
        }

        #endregion

        #region Enough Resource(s) In Hoppers

        public bool                         EnoughResourceInHoppers( ThingDef resourceDef, int resourceCount )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.EnoughResourceInHoppers( {1}, {2} )", this.parent.ThingID, resourceDef == null ? "null" : resourceDef.defName, resourceCount ) );
            return ( CountResourceInHoppers( resourceDef ) >= resourceCount );
        }

        public bool                         EnoughResourcesInHoppers( int resourceCount )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.EnoughResourcesInHoppers( {1} )", this.parent.ThingID, resourceCount ) );
            return ( CountResourcesInHoppers() >= resourceCount );
        }

        public bool                         EnoughResourcesInHoppers( RecipeDef recipe )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.EnoughResourcesInHoppers()", this.parent.ThingID, recipe == null ? "null" : recipe.defName ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return false;
            }

            var allResources = new List<Thing>();

            foreach( var hopper in hoppers )
            {
                var hopperResources = hopper.GetAllResources( ResourceSettings.filter );
                if( hopperResources != null )
                {
                    allResources.AddRangeUnique( hopperResources );
                }
            }

            List<ThingAmount> chosen = new List<ThingAmount>();
            if( recipe.allowMixingIngredients )
            {
                if( recipe.TryFindBestRecipeIngredientsInSet_AllowMix( allResources, chosen ) )
                {
                    return true;
                }
            }
            else
            {
                if( recipe.TryFindBestRecipeIngredientsInSet_NoMix( allResources, chosen ) )
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Count Resource(s) In Hoppers

        public int                          CountResourceInHoppers( ThingDef resourceDef )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.CountResourceInHoppers( {1} )", this.parent.ThingID, resourceDef == null ? "null" : resourceDef.defName ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return 0;
            }

            int availableResources = 0;
            foreach( var hopper in hoppers )
            {
                var resources = hopper.GetAllResources( resourceDef );
                if( !resources.NullOrEmpty() )
                {
                    foreach( var resource in resources )
                    {
                        availableResources += resource.stackCount;
                    }
                }
            }
            return availableResources;
        }

        public int                          CountResourcesInHoppers()
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.CountResourcesInHoppers()", this.parent.ThingID ) );
            var hoppers = FindHoppers();
            if( hoppers.NullOrEmpty() )
            {
                return 0;
            }

            int availableResources = 0;
            foreach( var hopper in hoppers )
            {
                var resources = hopper.GetAllResources( ResourceSettings.filter );
                if( !resources.NullOrEmpty() )
                {
                    foreach( var resource in resources )
                    {
                        availableResources += resource.stackCount;
                    }
                }
            }
            return availableResources;
        }

        public bool                         CountResourcesInHoppers( List<ResourceAmount> resources )
        {
            //Log.Message( string.Format( "{0}.CompHopperUser.CountResourcesInHoppers( List<ResourceAmount> )", this.parent.ThingID ) );
            if( resources == null )
            {
                return false;
            }
            resources.Clear();

            foreach( var hopper in FindHoppers() )
            {
                foreach( var thing in hopper.GetAllResources( Resources ) )
                {
                    ResourceAmount.AddToList( resources, thing.def, thing.stackCount );
                }
            }
            return !resources.NullOrEmpty();
        }

        #endregion

    }

}
