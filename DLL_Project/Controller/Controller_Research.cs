using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

/*
    TODO:  Alpha 13 API change
    
    Can't change yet otherwise existing saves will get null errors for missing class
    
namespace CommunityCoreLibrary.Controller
{
    
    public class Research : MapComponent
*/

namespace CommunityCoreLibrary
{

    public class ResearchController : MapComponent
    {

        const int                           TicksPerSecond = 60;
        const int                           UpdatesPerSecond = 2;
        const int                           BaseUpdateTicks = TicksPerSecond / UpdatesPerSecond;
        static int                          UpdateTicks;

        static List< AdvancedResearchDef >  _advancedResearch;

        static bool                         wasGodMode;

        static bool                         firstRun = true;
        static bool                         okToProcess;

        // These are used to optimize the process so the same data
        // isn't constantly reprocessed with every itteration.
        public static readonly List< ThingDef > buildingCache = new List< ThingDef >();
        public static readonly List< AdvancedResearchMod > researchModCache = new List< AdvancedResearchMod >();
        public static readonly List< HelpCategoryDef > helpCategoryCache = new List<HelpCategoryDef>();

        #region State Management

        public static List< AdvancedResearchDef > AdvancedResearch
        {
            get
            {
                if( _advancedResearch == null )
                {
                    _advancedResearch = Controller.Data.AdvancedResearchDefs;
                }
                return _advancedResearch;
            }
        }

        public                              ResearchController()
        {
            firstRun = true;
        }

        public static bool                  Initialize()
        {
            firstRun = false;
            okToProcess = false;

            if( AdvancedResearch.NullOrEmpty() )
            {
                // No advanced research, hybernate
                CCL_Log.Message( "No advanced research defined, hybernating...", "Advanced Research" );
                return true;
            }

            // Set the initial state
            SetInitialState( true );

            // Set for an immediate research check
            UpdateTicks = 0;
            okToProcess = true;

            CCL_Log.Message( "Initialized", "Advanced Research" );
            return true;
        }

        void                                UpdateComponent()
        {
            if( firstRun )
            {
                Initialize();
            }

            if( !okToProcess )
            {
                return;
            }

            UpdateTicks--;
            if( UpdateTicks > 0 )
            {
                return;
            }

            UpdateTicks = BaseUpdateTicks;

            wasGodMode |= Game.GodMode;

            CheckAdvancedResearch();
        }

        #endregion

        #region Core Callbacks

        public override void                MapComponentOnGUI()
        {
            base.MapComponentOnGUI();
            UpdateComponent();
        }

        public override void                MapComponentTick()
        {
            base.MapComponentTick();
            UpdateComponent();
        }

        #endregion

        #region Cache Processing

        static void                         PrepareCaches()
        {
            // Prepare the caches
            buildingCache.Clear();
            researchModCache.Clear();
            helpCategoryCache.Clear();
        }

        static void                         ProcessCaches( bool setInitialState = false )
        {
            // Process the caches

            // Recache the buildings recipes
            if( buildingCache.Count > 0 )
            {
                foreach( var building in buildingCache )
                {
                    building.RecacheRecipes( !setInitialState );
                }
            }

            // Apply all the research mods
            if( researchModCache.Count > 0 )
            {
                foreach( var researchMod in researchModCache )
                {
                    researchMod.Invoke( !setInitialState );
                }
            }

            // Recache the help system
            if( helpCategoryCache.Count > 0 )
            {
                foreach( var helpCategory in helpCategoryCache )
                {
                    helpCategory.Recache();
                }
                MainTabWindow_ModHelp.Recache();
            }

        }

        #endregion

        #region Research Processing

        static void                         SetInitialState( bool firstTimeRun = false )
        {   
            // Set the initial state of the advanced research
            PrepareCaches();

            // Process each advanced research def in reverse order
            for( int i = AdvancedResearch.Count - 1; i >= 0; i-- )
            {
                var Advanced = AdvancedResearch[ i ];

                // Reset the project
                Advanced.Disable( firstTimeRun );
            }

            // Now do the work!
            ProcessCaches( true );

        }

        static void                         CheckAdvancedResearch()
        {
            if(
                ( !Game.GodMode )&&
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
            foreach( var Advanced in AdvancedResearch )
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

        #endregion

    }

}
