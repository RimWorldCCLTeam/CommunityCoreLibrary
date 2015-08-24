using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class ResearchController : MapComponent
    {

        const int                           TicksPerSecond = 60;
        const int                           UpdatesPerSecond = 2;
        const int                           BaseUpdateTicks = TicksPerSecond / UpdatesPerSecond;
        int                                 UpdateTicks;

        List< AdvancedResearchDef >         _advancedResearch;

        bool                                wasGodMode;

        bool                                firstRun = true;
        bool                                okToProcess;

        // These are used to optimize the process so the same data
        // isn't constantly reprocessed with every itteration.
        readonly List< ResearchCompletePair > researchCache = new List< ResearchCompletePair >();
        public static readonly List< ThingDef >    buildingCache = new List< ThingDef >();
        public static readonly List< AdvancedResearchMod > researchModCache = new List< AdvancedResearchMod >();
        public static readonly List< HelpCategoryDef > helpCategoryCache = new List<HelpCategoryDef>();

        #region State Management

        List< AdvancedResearchDef >         AdvancedResearch
        {
            get
            {
                if( _advancedResearch == null )
                {
                    _advancedResearch = ModController.AdvancedResearch;
                }
                return _advancedResearch;
            }
        }

        public                              ResearchController()
        {
            firstRun = true;
        }

        void                                InitComponent()
        {
            firstRun = false;
            okToProcess = false;

            if( ( AdvancedResearch == null )||
                ( AdvancedResearch.Count == 0 ) )
            {
                // No advanced research, hybernate
                Log.Message( "Community Core Library :: Advanced Research :: No advanced research defined, hybernating..." );
                return;
            }

            // Build research quick-reference
            var researchProjects = DefDatabase< ResearchProjectDef >.AllDefs.ToList();
            foreach( var researchProject in researchProjects )
            {
                researchCache.Add( new ResearchCompletePair( researchProject ) );
            }

            // Set the initial state
            SetInitialState( true );

            // Set for an immediate research check
            UpdateTicks = 0;
            okToProcess = true;

            Log.Message( "Community Core Library :: Advanced Research :: Ready" );
        }

        void                                UpdateComponent()
        {
            if( firstRun )
            {
                InitComponent();
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

        void                                PrepareCaches()
        {
            // Prepare the caches
            buildingCache.Clear();
            researchModCache.Clear();
            helpCategoryCache.Clear();
        }

        void                                ProcessCaches( bool setInitialState = false )
        {
            // Process the caches

            // Recache the buildings recipes
            foreach( var building in buildingCache )
            {
                building.RecacheRecipes( !setInitialState );
            }

            // Apply all the research mods
            foreach( var researchMod in researchModCache )
            {
                researchMod.Invoke( !setInitialState );
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
        
        void                                SetInitialState( bool firstTimeRun = false )
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

        void                                CheckAdvancedResearch()
        {
            if( !Game.GodMode )
            {
                // Quick scan to see if anything changed and early out if nothing new is complete

                if( wasGodMode )
                {
                    // Reset everything
                    SetInitialState();
                    wasGodMode = false;
                }

                // God mode is off, do a real check
                bool Changed = false;
                for( int i = 0; i < researchCache.Count; i++ ){
                    var rcPair = researchCache[ i ];
                    if( ( rcPair.researchProject.IsFinished ) && ( !rcPair.wasComplete ) ){
                        rcPair.wasComplete = true;
                        Changed = true;
                    }
                }

                if( !Changed )
                {
                    // No new research complete
                    return;
                }
            }

            // Prepare for some work
            PrepareCaches();

            // Scan advanced research for newly completed projects
            foreach( var Advanced in AdvancedResearch )
            {
                if( Advanced.CanEnable )
                {
                    // Enable this project
                    Advanced.Enable();
                }
            }

            // Process caches
            ProcessCaches();
        }

        #endregion

    }

}
