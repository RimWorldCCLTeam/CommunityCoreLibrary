using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller handles advanced research (validation, initialization, updates)
    /// </summary>
    internal class ResearchSubController : SubController
    {

        private class OriginalDefData
        {
            public Dictionary<ThingDef,List<string>>                        ThingDef_SowTags                    = new Dictionary<ThingDef, List<string>>();
            public Dictionary<ThingDef,List<RecipeDef>>                     ThingDef_Recipes                    = new Dictionary<ThingDef, List<RecipeDef>>();
            public Dictionary<ThingDef,List<ResearchProjectDef>>            ThingDef_ResearchPrerequisites      = new Dictionary<ThingDef, List<ResearchProjectDef>>();
            public Dictionary<RecipeDef,List<ThingDef>>                     RecipeDef_RecipeUsers               = new Dictionary<RecipeDef, List<ThingDef>>();
            public Dictionary<ResearchProjectDef,List<ResearchProjectDef>>  ResearchDef_ResearchPrerequisites   = new Dictionary<ResearchProjectDef, List<ResearchProjectDef>>();
        }

        // These are used to optimize the process so the same data
        // isn't constantly reprocessed with every itteration.
        private static List< ThingDef >     buildingCache;
        private static List< HelpCategoryDef > helpCategoryCache;
        private static List< AdvancedResearchMod > researchModCache;

        // This is to store the effected defs original data in
        private static OriginalDefData      OriginalData;

        // Was god-mode on the last update?
        private bool                        wasGodMode;

        public override string              Name
        {
            get
            {
                return "Advanced Research";
            }
        }

        // Override sequence priorities
        public override int                 ValidationPriority      => 50;
        public override int                 InitializationPriority  => 90;
        public override int                 UpdatePriority          => 50;
        public override int                 GameLoadPriority        => 90;

        // Validate ...research...?
        public override bool                Validate ()
        {
            // Hopefully...
            var stringBuilder = new StringBuilder();
            var rVal = true;

            CCL_Log.CaptureBegin( stringBuilder );

            var AdvancedResearchDefs = Controller.Data.AdvancedResearchDefs;

            // Make sure the hidden research exists
            if( CommunityCoreLibrary.Research.Locker == null )
            {
                CCL_Log.Trace( Verbosity.FatalErrors, "Missing research locker!" );
                rVal = false;
            }

            // Validate each advanced research def
            for( int index = AdvancedResearchDefs.Count - 1; index >= 0; index-- )
            {
                var advancedResearchDef = AdvancedResearchDefs[ index ];

                if( !advancedResearchDef.IsValid() )
                {
                    // Remove projects with errors from list of usable projects
                    AdvancedResearchDefs.Remove( advancedResearchDef );
                    rVal = false;
                    continue;
                }

                if( advancedResearchDef.IsLockedOut() )
                {
                    // Remove locked out projects
                    CCL_Log.TraceMod(
                        advancedResearchDef,
                        Verbosity.Warnings,
                        "Def is locked out by one or more research prerequisites" );
                    AdvancedResearchDefs.Remove( advancedResearchDef );
                    continue;
                }

            }

#if DEBUG
            if( rVal == true )
            {
                var allMods = Controller.Data.Mods;
                foreach( var mod in allMods )
                {
                    if( !Find_Extensions.DefListOfTypeForMod<AdvancedResearchDef>( mod ).NullOrEmpty() )
                    {
                        CCL_Log.TraceMod(
                            mod,
                            Verbosity.Validation,
                            "Passed validation"
                        );
                    }
                }
            }
#endif

            // Should be empty or a laundry list
            CCL_Log.CaptureEnd(
                stringBuilder,
                rVal ? "Validated" : "Errors during validation"
            );
            strReturn = stringBuilder.ToString();

            // Return true if all mods OK, false if any failed validation
            State = rVal ? SubControllerState.Validated : SubControllerState.ValidationError;
            return rVal;
        }

        public override bool                Initialize ()
        {
            // Create caches
            InitializeCaches();

            if ( Data.AdvancedResearchDefs.NullOrEmpty() )
            {
                // No advanced research, hybernate
                strReturn = "No advanced research defined, hybernating...";
                State = Controller.SubControllerState.Hybernating;
                return true;
            }

            // Get the initial state
            GetInitialState();

            // Upgrade system state
            strReturn = "Initialized";
            State = Controller.SubControllerState.Ok;
            return true;
        }

        public override bool                PreLoad()
        {
            if( Data.AdvancedResearchDefs.NullOrEmpty() )
            {
                // No advanced research, hybernate
                strReturn = "No advanced research defined, hybernating...";
                State = Controller.SubControllerState.Hybernating;
                return true;
            }

            // Reset the initial state
            SetInitialState( true );

            // Upgrade system state
            strReturn = "Initialized";
            State = Controller.SubControllerState.Ok;
            return true;
        }

        public override bool                PreThingLoad()
        {
            // Don't display an update message
            strReturn = string.Empty;

            // Everything is good, do some work
            wasGodMode = false;

            CheckAdvancedResearch();
            return true;
        }

        public override bool                Update()
        {
            // Don't display an update message
            strReturn = string.Empty;

            // Everything is good, do some work
            wasGodMode |= DebugSettings.godMode;

            CheckAdvancedResearch();
            return true;
        }

        #region Cache Processing

        private void                        InitializeCaches()
        {
            buildingCache = new List< ThingDef >();
            helpCategoryCache = new List<HelpCategoryDef>();
            researchModCache = new List< AdvancedResearchMod >();
            OriginalData = new OriginalDefData();
        }

        private void                        PrepareCaches()
        {
            // Prepare the caches
            buildingCache.Clear();
            researchModCache.Clear();
            helpCategoryCache.Clear();
        }

        private void                        ProcessCaches( bool setInitialState = false )
        {
            // Process the caches

            // Recache the buildings recipes
            if ( buildingCache.Count > 0 )
            {
                foreach ( var building in buildingCache )
                {
                    building.RecacheRecipes( !setInitialState );
                }
            }

            // Apply all the research mods
            if ( researchModCache.Count > 0 )
            {
                foreach ( var researchMod in researchModCache )
                {
                    researchMod.Invoke( !setInitialState );
                }
            }

            // Recache the help system
            if ( helpCategoryCache.Count > 0 )
            {
                foreach ( var helpCategory in helpCategoryCache )
                {
                    helpCategory.Recache();
                }
                MainTabWindow_ModHelp.Recache();
            }
        }

        #endregion Cache Processing

        #region Research Processing

        public void                         CheckAdvancedResearch()
        {
            if(
                ( !DebugSettings.godMode )&&
                ( wasGodMode )
            )
            {
                // Reset everything
                SetInitialState();
                wasGodMode = false;
            }

            // Prepare for some work
            PrepareCaches();

            // Scan advanced research for newly completed projects
            foreach( var Advanced in Data.AdvancedResearchDefs )
            {
                if( Advanced.ResearchState != ResearchEnableMode.Complete )
                {
                    var enableMode = Advanced.EnableMode;
                    if( enableMode != Advanced.ResearchState )
                    {
                        // Enable this project
                        Advanced.Enable( enableMode );
                    }
                }
            }

            // Process caches
            ProcessCaches();
        }

        private void                        GetInitialState()
        {
            // Process each advanced research def
            foreach( var Advanced in Data.AdvancedResearchDefs )
            {
                if( Advanced.IsPlantToggle )
                {   // Handle plants sow tags
                    foreach( var plantDef in Advanced.thingDefs )
                    {
                        if( !OriginalData.ThingDef_SowTags.ContainsKey( plantDef ) )
                        {
                            // Add plant to dictionary with their original list of sowTags
                            var sowTags = new List<string>();
                            if( !plantDef.plant.sowTags.NullOrEmpty() )
                            {
                                sowTags.AddRange( plantDef.plant.sowTags );
                            }
                            OriginalData.ThingDef_SowTags[ plantDef ] = sowTags;
                        }
                    }
                }
                if( Advanced.IsRecipeToggle )
                {   // Handle recipe toggles
                    foreach( var thingDef in Advanced.thingDefs )
                    {
                        if( !OriginalData.ThingDef_Recipes.ContainsKey( thingDef ) )
                        {
                            // Add ThingDef to dictionary with it's original list of recipes
                            var recipeDefs = new List<RecipeDef>();
                            if( !thingDef.recipes.NullOrEmpty() )
                            {
                                recipeDefs.AddRange( thingDef.recipes );
                                foreach( var recipeDef in recipeDefs )
                                {
                                    if( !OriginalData.RecipeDef_RecipeUsers.ContainsKey( recipeDef ) )
                                    {
                                        // Add RecipeDef to dictionary with it's original list of recipe users
                                        var recipeUsers = new List<ThingDef>();
                                        if( !recipeDef.recipeUsers.NullOrEmpty() )
                                        {
                                            recipeUsers.AddRange( recipeDef.recipeUsers );
                                        }
                                        OriginalData.RecipeDef_RecipeUsers[ recipeDef ] = recipeUsers;
                                    }
                                }
                            }
                            OriginalData.ThingDef_Recipes[ thingDef ] = recipeDefs;
                        }
                    }
                }
                if( Advanced.IsBuildingToggle )
                {   // Handle building research prerequisites
                    foreach( var thingDef in Advanced.thingDefs )
                    {
                        if( !OriginalData.ThingDef_ResearchPrerequisites.ContainsKey( thingDef ) )
                        {
                            // Add ThingDef to dictionary with it's original list of prerequisites
                            var researchDefs = new List<ResearchProjectDef>();
                            if( !thingDef.researchPrerequisites.NullOrEmpty() )
                            {
                                researchDefs.AddRange( thingDef.researchPrerequisites );
                            }
                            OriginalData.ThingDef_ResearchPrerequisites[ thingDef ] = researchDefs;
                        }
                    }
                }
                if( Advanced.IsResearchToggle )
                {   // Handle research project prerequisites
                    foreach( var researchDef in Advanced.effectedResearchDefs )
                    {
                        if( !OriginalData.ResearchDef_ResearchPrerequisites.ContainsKey( researchDef ) )
                        {
                            // Add research to dictionary with it's original list of prerequisites
                            var researchDefs = new List<ResearchProjectDef>();
                            if( !researchDef.prerequisites.NullOrEmpty() )
                            {
                                researchDefs.AddRange( researchDef.prerequisites );
                            }
                            OriginalData.ResearchDef_ResearchPrerequisites[ researchDef ] = researchDefs;
                        }
                    }
                }
            }
        }

        private void                        SetInitialState( bool firstTimeRun = false )
        {
            // Set the initial state of the advanced research
            PrepareCaches();

            // Reset to the XML data state
            ResetOriginalData();

            // Now update the states for things which will be enabled later
            foreach( var advanced in Data.AdvancedResearchDefs )
            {
                advanced.ResetOnGameLoad();
            }

            // Now do the work!
            ProcessCaches( firstTimeRun );
        }

        private void                        ResetOriginalData()
        {
            foreach( var pair in OriginalData.RecipeDef_RecipeUsers )
            {   // Restore RecipeDef.recipeUsers
                if( pair.Key.recipeUsers == null )
                {
                    pair.Key.recipeUsers = new List<ThingDef>();
                }
                pair.Key.recipeUsers.Clear();
                pair.Key.recipeUsers.AddRange( pair.Value );
                RecacheBuildingRecipes( pair.Key.recipeUsers );
            }

            foreach( var pair in OriginalData.ThingDef_Recipes )
            {   // Restore ThingDef.recipes
                if( pair.Key.recipes == null )
                {
                    pair.Key.recipes = new List<RecipeDef>();
                }
                pair.Key.recipes.Clear();
                pair.Key.recipes.AddRange( pair.Value );
                RecacheBuildingRecipes( pair.Key );
            }

            foreach( var pair in OriginalData.ThingDef_SowTags )
            {   // Restore ThingDef.sowTags
                if( pair.Key.plant.sowTags == null )
                {
                    pair.Key.plant.sowTags = new List<string>();
                }
                pair.Key.plant.sowTags.Clear();
                pair.Key.plant.sowTags.AddRange( pair.Value );
            }

            foreach( var pair in OriginalData.ThingDef_ResearchPrerequisites )
            {   // Restore ThingDef.researchPrerequisites
                if( pair.Key.researchPrerequisites == null )
                {
                    pair.Key.researchPrerequisites = new List<ResearchProjectDef>();
                }
                pair.Key.researchPrerequisites.Clear();
                pair.Key.researchPrerequisites.AddRange( pair.Value );
            }

            foreach( var pair in OriginalData.ResearchDef_ResearchPrerequisites )
            {   // Restore ResearchProjectDef.prerequisites
                if( pair.Key.prerequisites == null )
                {
                    pair.Key.prerequisites = new List<ResearchProjectDef>();
                }
                pair.Key.prerequisites.Clear();
                pair.Key.prerequisites.AddRange( pair.Value );
            }
        }

        #endregion Research Processing

        #region Recache/Process Interface

        public static void                  RecacheBuildingRecipes( ThingDef def )
        {
            buildingCache.AddUnique( def );
        }

        public static void                  RecacheBuildingRecipes( List<ThingDef> defs )
        {
            buildingCache.AddRangeUnique( defs );
        }

        public static void                  RecacheHelpCategory( HelpCategoryDef def )
        {
            helpCategoryCache.AddUnique( def );
        }

        public static void                  RecacheHelpCategories( List<HelpCategoryDef> defs )
        {
            helpCategoryCache.AddRangeUnique( defs );
        }

        public static void                  ProcessResearchMod( AdvancedResearchMod mod )
        {
            researchModCache.AddUnique( mod );
        }

        public static void                  ProcessResearchMods( List<AdvancedResearchMod> mods )
        {
            researchModCache.AddRangeUnique( mods );
        }

        #endregion

    }

}
