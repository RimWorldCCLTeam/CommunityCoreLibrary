using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class ResearchControl : MapComponent
    {

        const int                           TicksPerSecond = 60;
        const int                           UpdatesPerSecond = 2;
        const int                           BaseUpdateTicks = TicksPerSecond / UpdatesPerSecond;
        int                                 UpdateTicks;

        List< AdvancedResearchDef >         advancedResearch;

        bool                                wasGodMode;

        bool                                firstRun = true;
        bool                                okToProcess;

        // These are used to optimize the process so the same data
        // isn't constantly reprocessed with every itteration.
        readonly List< ResearchCompletePair > researchCache = new List< ResearchCompletePair >();
        readonly List< ThingDef >           buildingCache = new List< ThingDef >();
        readonly List< ResearchMod >        researchModCache = new List< ResearchMod >();

        #region State Management

        public                              ResearchControl()
        {
            firstRun = true;
        }

        void                                InitComponent()
        {
            firstRun = false;
            okToProcess = false;

            // Get the [advanced] research defs
            advancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();

            if( ( advancedResearch == null )||
                ( advancedResearch.Count == 0 ) )
            {
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
            SetInitialState();

            // Set for an immediate research check
            UpdateTicks = 0;
            okToProcess = true;
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
        }

        void                                ProcessCaches()
        {
            // Process the caches

            // Recache the buildings recipes
            foreach( var building in buildingCache )
            {
                building.RecacheRecipes();
            }

            // Apply all the research mods
            foreach( var researchMod in researchModCache )
            {
                researchMod.Apply();
            }

        }

        #endregion

        #region Research Processing
        
        void                                SetInitialState()
        {   
            // Set the initial state of the advanced research
            PrepareCaches();

            // Process each advanced research def in reverse order
            for( int i = advancedResearch.Count - 1; i >= 0; i-- )
            {
                var Advanced = advancedResearch[ i ];

                if( Advanced.IsRecipeToggle() ){
                    // Recipe toggle
                    Advanced.ToggleRecipes( buildingCache, true );
                }
                if( Advanced.IsPlantToggle() ){
                    // Plant toggle
                    Advanced.TogglePlants( true );
                }
                if( Advanced.IsBuildingToggle() ){
                    // Building toggle
                    Advanced.ToggleBuildings( true );
                }
                if( Advanced.IsResearchToggle() ){
                    // Research toggle
                    Advanced.ToggleResearch( true );
                }
                // Flag it as disabled to trigger research completion checks
                Advanced.isEnabled = false;
            }

            // Now do the work!
            ProcessCaches();

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
            foreach( var Advanced in advancedResearch )
            {
                if( ( !Advanced.isEnabled )&&
                    ( Advanced.ResearchComplete() ) )
                {
                    // Process this project
                    ProcessResearch( Advanced );
                }
            }

            // Process caches
            ProcessCaches();
        }

        void                                ProcessResearch( AdvancedResearchDef Advanced )
        {
            if( Advanced.IsRecipeToggle() )
            {
                Advanced.ToggleRecipes( buildingCache );
            }
            if( Advanced.IsPlantToggle() )
            {
                Advanced.TogglePlants();
            }
            if( Advanced.IsBuildingToggle() )
            {
                Advanced.ToggleBuildings();
            }
            if( Advanced.IsResearchToggle() )
            {
                Advanced.ToggleResearch();
            }
            if( Advanced.HasCallbacks() )
            {
                // Cache callbacks
                foreach( var researchMod in Advanced.researchMods )
                {
                    researchModCache.Add( researchMod );
                }
            }

            // Flag it as enabled to skip it in later checks
            Advanced.isEnabled = true;
        }

        #endregion

    }

}
