using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

    // When all research is complete, recipes on buildings or buildings in the architect
    // menu will become available or hidden from the players use.
    // 
    // If no recipes are associated with the advanced research, it is assumed that
    // the associated tables are to be unlocked by multiple research projects and
    // they are hidden from architects menu until the research is complete.
    //
    // If the HideDefs flag is true, recipes are removed from tables or buildings are
    // hidden in the architects menu as appropriate.

    public struct ResearchCompletePair
    {
        // Research projects and last completion flag
        public ResearchProjectDef researchProject;
        public bool wasComplete;

        public ResearchCompletePair( ResearchProjectDef r )
        {
            researchProject = r;
            wasComplete = false;
        }

    }

    public struct ThingResearchPair
    {
        // Maintains the original buildings research prerequisite
        public ThingDef thingDef;
        public ResearchProjectDef researchProject;

        public ThingResearchPair( ThingDef t, ResearchProjectDef r )
        {
            thingDef = t;
            researchProject = r;
        }

    }

    public class ResearchControl : MapComponent
    {

        List< AdvancedResearchDef >         advancedResearch;
        int                                 tickCount = 0;

        const int                           UpdateTicks = 250;
        bool                                okToProcess = false;
        bool                                wasGodMode = false;

        bool                                firstRun = true;

        // These are used to optimize the process so the same table isn't constantly
        // recached and the designation category defs are resolved multiple times
        private List< ResearchCompletePair > researchComplete = new List< ResearchCompletePair >();
        private List< ThingResearchPair >   thingDefResearch = new List< ThingResearchPair >();
        private List< ThingDef >            buildingRecipeRecache = new List< ThingDef >();
        private List< ResearchMod >         actionCache = new List< ResearchMod >();

        public ResearchControl()
        {
            firstRun = true;
        }

        private void InitComponent()
        {
            firstRun = false;

            // Make sure the hidden research exists
            if( Research.Locker == null )
            {
                Log.Message( "Community Core Library :: Advanced Research :: Unable to locate hidden research!" );
                okToProcess = false;
                return;
            }

            // Get the [advanced] research defs
            advancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();

            if( ( advancedResearch == null )||
                ( advancedResearch.Count < 1 ) )
            {
                Log.Message( "Community Core Library :: Advanced Research :: No advanced research defined, hybernating..." );
                okToProcess = false;
                return;
            }

            // Build research quick-reference
            researchComplete.Clear();
            List< ResearchProjectDef > research = DefDatabase< ResearchProjectDef >.AllDefs.ToList();
            for( int cIndex = 0, maxCount = research.Count; cIndex < maxCount; cIndex++ )
                researchComplete.Add( new ResearchCompletePair( research[ cIndex ] ) );

            // Do sanity checks and lock everything for now
            LockEverything( true );

            // Set for an immediate research check
            tickCount = 0;

            Log.Message( "Community Core Library :: Advanced Research :: Initialized" );
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if( firstRun )
                InitComponent();

            if( !okToProcess )
                return;

            tickCount--;
            if( tickCount > 0 )
                return;
            tickCount = UpdateTicks;

            if( Game.GodMode )
                wasGodMode = true;
            
            CheckAdvancedResearch();
        }

        private void PrepareCache()
        {
            // Prepare the caches
            buildingRecipeRecache.Clear();
            actionCache.Clear();
        }

        private void ProcessCache()
        {
            // Process the caches

            // Recache the buildings recipes
            for( int cIndex = 0, cCountTo = buildingRecipeRecache.Count; cIndex < cCountTo; cIndex++ )
                Common.ClearBuildingRecipeCache( buildingRecipeRecache[ cIndex ] );

            // Apply all the research mods
            for( int cIndex = 0, cCountTo = actionCache.Count; cIndex < cCountTo; cIndex++ )
                actionCache[ cIndex ].Apply();

        }

        private bool IsRecipeToggle( AdvancedResearchDef Advanced )
        {
            // Determine if this def toggles recipes
            return ( Advanced.recipeDefs != null )&&( Advanced.recipeDefs.Count > 0 )&&
                ( Advanced.buildingDefs != null )&&( Advanced.buildingDefs.Count > 0 );
        }

        private bool IsBuildingToggle( AdvancedResearchDef Advanced )
        {
            // Determine if this def toggles buildings
            return ( Advanced.recipeDefs == null )||( Advanced.recipeDefs.Count == 0 )&&
                ( Advanced.buildingDefs != null )&&( Advanced.buildingDefs.Count > 0 );
        }

        private bool HasCallbacks( AdvancedResearchDef Advanced )
        {
            // Determine if this def has callbacks
            return ( Advanced.researchMods != null )&&( Advanced.researchMods.Count > 0 );
        }

        private void LockEverything( bool sanityChecks = false )
        {   // Lock everything by breaking the links and do sanity checks of all the defs
            
            // Until all sanity checks are done...
            okToProcess = false;

            // Prepare (clear) the processing caches
            PrepareCache();

            // Get each advanced research def
            for( int aIndex = 0, aCountTo = advancedResearch.Count; aIndex < aCountTo; aIndex++ ){
                AdvancedResearchDef Advanced = advancedResearch[ aIndex ];

                Advanced.toggleRecipes = IsRecipeToggle( Advanced );
                Advanced.toggleBuildings = IsBuildingToggle( Advanced );
                Advanced.doCallbacks = HasCallbacks( Advanced );

                // Get each building associated with the advanced research
                for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
                    ThingDef building = Advanced.buildingDefs[ bIndex ];
                    
                    if( ( sanityChecks )&&
                        ( building == null ) ){
                        Log.Message( "Community Core Library :: Advanced Research :: buildingDefs( index = " + bIndex + " ) resolved to null in AdvancedResearchDef( " + Advanced.defName + " )" );
                        return;
                    }

                    if( Advanced.toggleRecipes ){
                        // Recipe unlock on buildings
                        // Get each recipe associated with the advanced research
                        for( int rIndex = 0, rCountTo = Advanced.recipeDefs.Count; rIndex < rCountTo; rIndex++ ){
                            RecipeDef recipe = Advanced.recipeDefs[ rIndex ];
                            
                            if( ( sanityChecks )&&
                                ( recipe == null ) ){
                                // Sanity check
                                Log.Message( "Community Core Library :: Advanced Research :: recipeDefs( index = " + rIndex + " ) resolved to null in AdvancedResearchDef( " + Advanced.defName + " )" );
                                return;
                            }

                            // Only deal with defs which unlock
                            if( !Advanced.HideDefs )
                            {
                                // Remove building from recipe
                                if( ( recipe.recipeUsers != null ) && ( recipe.recipeUsers.IndexOf( building ) >= 0 ) ){
                                    recipe.recipeUsers.Remove( building );
                                }
                                
                                // Remove recipe from building
                                if( ( building.recipes != null ) && ( building.recipes.IndexOf( recipe ) >= 0 ) ){
                                    building.recipes.Remove( recipe );
                                }
                            }
                        }
                        
                        // Add this building to the list to recache
                        buildingRecipeRecache.Add( building );

                    }
                    if( Advanced.toggleBuildings ){
                        // Make sure to know how this building was originally set
                        if( thingDefResearch.FindIndex( pair => ( pair.thingDef == building ) ) < 0 )
                            thingDefResearch.Add( new ThingResearchPair( building, building.researchPrerequisite ) );

                        if( Advanced.HideDefs )
                        {   // Reset building to original research
                            building.researchPrerequisite = thingDefResearch.Find( pair => ( pair.thingDef == building ) ).researchProject;
                        } else {
                            // Hide the building
                            building.researchPrerequisite = Research.Locker;
                        }
                    }
                }
                // Validate callbacks
                if( ( sanityChecks )&&
                    ( Advanced.doCallbacks ) )
                    for( int mIndex = 0, mCountTo = Advanced.researchMods.Count; mIndex < mCountTo; mIndex++ )
                        if( Advanced.researchMods[ mIndex ] == null ){
                            // Sanity check
                            Log.Message( "Community Core Library :: Advanced Research :: researchMods( index = " + mIndex + " ) resolved to null in AdvancedResearchDef( " + Advanced.defName + " )" );
                            return;
                        }


                // Flag it as disabled to trigger research completion checks
                Advanced.isEnabled = false;
            }

            // Everything is ok!
            okToProcess = true;

            // Now do the work!
            ProcessCache();

        }

        private void CheckAdvancedResearch()
        {
            //Log.Message( "Checking - " + researchComplete.Count + " - defs" );
            // Quick scan to see if anything changed and early out if nothing is complete
            if( Game.GodMode == false )
            {
                // Reset everything
                if( wasGodMode )
                    LockEverything();

                // Not anymore
                wasGodMode = false;

                // God mode is off, do a real check
                bool noChange = true;
                for( int rcIndex = 0, rcCountTo = researchComplete.Count; rcIndex < rcCountTo; rcIndex++ ){
                    ResearchCompletePair rcPair = researchComplete[ rcIndex ];
                    //Log.Message( "Checking - " + rcPair.researchProject.LabelCap );
                    if( ( rcPair.researchProject.IsFinished ) && ( rcPair.wasComplete == false ) ){
                        rcPair.wasComplete = true;
                        noChange = false;
                        //Log.Message( rcPair.researchProject.LabelCap + " - Is now done" );
                    }
                }

                // GTFO!
                if( noChange ) return;
            }
            //Log.Message( "updating" );

            // Prepare for some work
            PrepareCache();

            // Start determining work
            for( int aIndex = 0, aCountTo = advancedResearch.Count; aIndex < aCountTo; aIndex++ ){
                AdvancedResearchDef Advanced = advancedResearch[ aIndex ];
                // Is it done?
                if( ( Advanced.isEnabled == false )&&
                    ( Advanced.ResearchGroupComplete() ) ){
                    // If all the research is done, process the def
                    ProcessResearch( Advanced );
                }
            }

            ProcessCache();
        }

        private void ProcessResearch( AdvancedResearchDef Advanced )
        {
            if( Advanced.toggleRecipes ){
                // Recipe toggle on buildings

                // Get each building associated with the advanced research
                for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
                    ThingDef building = Advanced.buildingDefs[ bIndex ];
                    
                    // Get each recipe associated with the advanced research
                    for( int rIndex = 0, rCountTo = Advanced.recipeDefs.Count; rIndex < rCountTo; rIndex++ ){
                        RecipeDef recipe = Advanced.recipeDefs[ rIndex ];

                        // Show or hide recipes?
                        if( !Advanced.HideDefs )
                        {
                            // Make sure recipe has user list
                            if( recipe.recipeUsers == null )
                                recipe.recipeUsers = new List<ThingDef>();
                            
                            // Add building to recipe
                            recipe.recipeUsers.Add( building );
                        } else {
                            // Remove building from recipe
                            if( ( recipe.recipeUsers != null ) && ( recipe.recipeUsers.IndexOf( building ) >= 0 ) ){
                                recipe.recipeUsers.Remove( building );
                            }

                            // Remove recipe from building
                            if( ( building.recipes != null ) && ( building.recipes.IndexOf( recipe ) >= 0 ) ){
                                building.recipes.Remove( recipe );
                            }
                        }
                    }

                    // Add this building to the list to recache
                    buildingRecipeRecache.Add( building );
                }
            }
            if( Advanced.toggleBuildings ) {
                // Designator toggle on buildings
                for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
                    ThingDef building = Advanced.buildingDefs[ bIndex ];

                    // Show/Hide as appropriate
                    if( !Advanced.HideDefs ){
                        // Show building
                        building.researchPrerequisite = Research.Unlocker;
                    } else {
                        building.researchPrerequisite = Research.Locker;
                    }
                }
            }
            // Cache any callbacks
            if( Advanced.doCallbacks )
                for( int mIndex = 0, mCountTo = Advanced.researchMods.Count; mIndex < mCountTo; mIndex++ )
                    actionCache.Add( Advanced.researchMods[ mIndex ] );

            // Flag it as enabled to skip it in later checks
            Advanced.isEnabled = true;
        }

    }
}