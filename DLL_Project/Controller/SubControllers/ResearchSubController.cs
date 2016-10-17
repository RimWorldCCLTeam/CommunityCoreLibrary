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

        // These are used to optimize the process so the same data
        // isn't constantly reprocessed with every itteration.
        private static List< ThingDef >     buildingCache;
        private static List< HelpCategoryDef > helpCategoryCache;
        private static List< AdvancedResearchMod > researchModCache;

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
        public override int                 ValidationPriority
        {
            get
            {
                return 50;
            }
        }
        public override int                 InitializationPriority
        {
            get
            {
                return 90;
            }
        }
        public override int                 UpdatePriority
        {
            get
            {
                return 50;
            }
        }

        public override bool                ReinitializeOnGameLoad
        {
            get
            {
                return true;
            }
        }

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
            if( Data.AdvancedResearchDefs.NullOrEmpty() )
            {
                // No advanced research, hybernate
                strReturn = "No advanced research defined, hybernating...";
                State = Controller.SubControllerState.Hybernating;
                return true;
            }

            // Create caches
            InitializeCaches();

            // Set the initial state
            SetInitialState( true );

            // Upgrade system state
            strReturn = "Initialized";
            State = Controller.SubControllerState.Ok;
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
            if (
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
            foreach ( var Advanced in Data.AdvancedResearchDefs )
            {
                if ( Advanced.ResearchState != ResearchEnableMode.Complete )
                {
                    var enableMode = Advanced.EnableMode;
                    if ( enableMode != Advanced.ResearchState )
                    {
                        // Enable this project
                        Advanced.Enable( enableMode );
                    }
                }
            }

            // Process caches
            ProcessCaches();
        }

        private void                        SetInitialState( bool firstTimeRun = false )
        {
            // Set the initial state of the advanced research
            PrepareCaches();

            // Process each advanced research def in reverse order
            for ( int i = Data.AdvancedResearchDefs.Count - 1; i >= 0; i-- )
            {
                var Advanced = Data.AdvancedResearchDefs[i];

                // Reset the project
                Advanced.Disable( firstTimeRun );
            }

            // Now do the work!
            ProcessCaches( firstTimeRun );
        }

        #endregion Research Processing

        #region Recache/Process Interface

        public static void                  RecacheBuildingRecipes( ThingDef def )
        {
            buildingCache.AddUnique( def );
        }

        public static void                  RecacheHelpCategory( HelpCategoryDef def )
        {
            helpCategoryCache.AddUnique( def );
        }

        public static void                  ProcessResearchMod( AdvancedResearchMod mod )
        {
            researchModCache.AddUnique( mod );
        }

        #endregion

	}

}
