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

	// Note: All research must be complete for the recipes and tables to be unlocked.
	//   When unlocked, all the tables are added to all the recipes.  This allows the
	//   setup of groups of recipes using multiple research trees.  For recipes to be
	//   unlocked, a minimum of one recipe, one research project and one table is required.
	//   If no recipes are associated with the advanced research, it is assumed that
	//   the associated tables are to be unlocked by multiple research projects and
	//   they are hidden from designation manager until the research is complete.

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

    public class BuildingResearchPair
    {
        // building research pair for research unlock
        public ThingDef building;
        public ResearchProjectDef originalResearch;
        public AdvancedResearchDef locker;

        public BuildingResearchPair( ThingDef b, AdvancedResearchDef l )
        {
            building = b;
            originalResearch = b.researchPrerequisite;
            locker = l;
        }

    }

	public class ResearchControl : MapComponent
	{

		List< AdvancedResearchDef >			advancedResearch;
		int									tickCount = 0;

		const int							UpdateTicks = 250;
		bool								okToUnlock = false;
        bool                                wasGodMode = false;

        bool                                firstRun = true;

		// These are used to optimize the process so the same table isn't constantly
		// recached and the designation category defs are resolved multiple times
		private List< ResearchCompletePair > researchComplete = new List< ResearchCompletePair >();
		private List< ThingDef >			buildingRecipeRecache = new List< ThingDef >();

        // This stores buildings research for those that are unlocked by multiple trees
        public List< BuildingResearchPair > originalBuildingResearch = new List< BuildingResearchPair >();

		public ResearchControl()
		{
            firstRun = true;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if( firstRun )
            {
                firstRun = false;

                // Make sure the hidden research exists
                if( Research.Locker == null )
                {
                    Log.Message( "Community Core Library :: Advanced Research :: Unable to locate hidden research!" );
                    okToUnlock = false;
                    return;
                }

    			// Get the [advanced] research defs
    			advancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.ToList();

    			// Build research quick-reference
    			researchComplete.Clear();
    			List< ResearchProjectDef > research = DefDatabase< ResearchProjectDef >.AllDefs.ToList();
    			for( int cIndex = 0, maxCount = research.Count; cIndex < maxCount; cIndex++ )
    				researchComplete.Add( new ResearchCompletePair( research[ cIndex ] ) );
    			
    			// Do sanity checks and lock everything for now
                LockEverything( true );

    			// Set for an immediate research check
    			tickCount = UpdateTicks;

    			if( okToUnlock ){
    				Log.Message( "Community Core Library :: Advanced Research :: Initialized" );
    			} else{
    				Log.Message( "Community Core Library :: Advanced Research :: Unable to start" );
    			}
    		}

			if( !okToUnlock )
				return;

			tickCount += 1;
			if( tickCount < UpdateTicks )
				return;
			tickCount = 0;

            if( Game.GodMode )
                wasGodMode = true;
            
			CheckAdvancedResearch();
		}

		private void PrepareCache()
		{
			// Prepare the itteration cache
			buildingRecipeRecache.Clear();
		}

		private void ProcessCache()
		{
			// Process the itteration cache

			// Recache the buildings recipes
			for( int cIndex = 0, cCountTo = buildingRecipeRecache.Count; cIndex < cCountTo; cIndex++ )
				Common.ClearBuildingRecipeCache( buildingRecipeRecache[ cIndex ] );

		}

		private bool IsRecipeUnlock( AdvancedResearchDef Advanced )
		{
			// Determine if this def unlocks recipes or tables
			return ( Advanced.recipeDefs != null )&&( Advanced.recipeDefs.Count > 0 );
		}

        private BuildingResearchPair FindOriginalBuildingResearch( ThingDef b )
        {
            foreach( BuildingResearchPair p in originalBuildingResearch )
            {
                if( p.building == b )
                    return p;
            }
            return (BuildingResearchPair)null;
        }

        private void LockEverything( bool sanityChecks = false )
		{
			// Until all sanity checks are done...
			okToUnlock = false;

			// Lock everything by breaking the links and do sanity checks of all the defs
			PrepareCache();

			// Get each advanced research def
			for( int aIndex = 0, aCountTo = advancedResearch.Count; aIndex < aCountTo; aIndex++ ){
				AdvancedResearchDef Advanced = advancedResearch[ aIndex ];

                if( ( sanityChecks )&&
                    ( ( Advanced.buildingDefs == null ) || ( Advanced.buildingDefs.Count < 1 ) ) ){
					Log.Message( "Community Core Library :: Advanced Research :: Missing buildingDefs in AdvancedResearchDef( " + Advanced.defName + " )" );
					return;
                }
				
                bool recipeUnlock = IsRecipeUnlock( Advanced );
				
                // Get each building associated with the advanced research
				for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
					ThingDef building = Advanced.buildingDefs[ bIndex ];
					
                    if( ( sanityChecks )&&
                        ( building == null ) ){
						Log.Message( "Community Core Library :: Advanced Research :: buildingDefs( index = " + bIndex + " ) resolved to null in AdvancedResearchDef( " + Advanced.defName + " )" );
						return;
					}

					if( recipeUnlock ){
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

							// Remove building from recipe
							if( ( recipe.recipeUsers != null ) && ( recipe.recipeUsers.IndexOf( building ) >= 0 ) ){
								recipe.recipeUsers.Remove( building );
							}
							
                            // Remove recipe from building
							if( ( building.recipes != null ) && ( building.recipes.IndexOf( recipe ) >= 0 ) ){
								building.recipes.Remove( recipe );
							}
						}
						
                        // Add this building to the list to recache
						buildingRecipeRecache.Add( building );

					} else{
						// Designator lock on buildings
                        BuildingResearchPair locked = FindOriginalBuildingResearch( building );

                        if( ( sanityChecks )&&
                            ( locked != null ) ){
							// Sanity check
                            Log.Message( "Community Core Library :: Advanced Research :: AdvancedResearchDef( " + Advanced.defName + " ) is locking buildingDef( " + building.defName + " ) which is already locked by AdvancedResearchDef( " + locked.locker.defName + " )" );
							return;
						}

                        // Add original data to list
                        if( locked == null )
                            originalBuildingResearch.Add( new BuildingResearchPair( building, Advanced ) );

                        //Log.Message( "Removing building research prereq: " + building.defName + " -> " + building.researchPrerequisite.defName );

						// Hide the building
                        building.researchPrerequisite = Research.Locker;
					}
				}
				// Flag it as disabled to trigger research completion checks
				Advanced.isEnabled = false;
			}

			// Everything is ok!
			okToUnlock = true;

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
				if( Advanced.isEnabled == false ){
					// Nope, can it be done?
					int rDone = 0;

                    // God mode, allow it
                    if( Game.GodMode == true )
                        rDone = Advanced.researchDefs.Count();
                    
                    else
                        // No god mode, check it
    					for( int rIndex = 0, rCountTo = Advanced.researchDefs.Count; rIndex < rCountTo; rIndex++ )
    						if( Advanced.researchDefs[ rIndex ].IsFinished )
    							rDone += 1;
                    
					// If all the research is done, unlock the recipes
					if( rDone == Advanced.researchDefs.Count() )
						UnlockResearch( Advanced );
					
				}
			}
			ProcessCache();
		}

		private void UnlockResearch( AdvancedResearchDef Advanced )
		{
			bool recipeUnlock = IsRecipeUnlock( Advanced );

			if( recipeUnlock ){
				// Recipe unlock on buildings

				// Get each building associated with the advanced research
				for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
					ThingDef building = Advanced.buildingDefs[ bIndex ];
					
                    // Get each recipe associated with the advanced research
					for( int rIndex = 0, rCountTo = Advanced.recipeDefs.Count; rIndex < rCountTo; rIndex++ ){
						RecipeDef recipe = Advanced.recipeDefs[ rIndex ];
						
                        // Make sure recipe has user list
                        if( recipe.recipeUsers == null )
							recipe.recipeUsers = new List<ThingDef>();
						
                        // Add building to recipe
						recipe.recipeUsers.Add( building );
					}

					// Add this building to the list to recache
					buildingRecipeRecache.Add( building );
				}
			} else{
				// Designator unlock on buildings
                for( int bIndex = 0, bCountTo = Advanced.buildingDefs.Count; bIndex < bCountTo; bIndex++ ){
					ThingDef building = Advanced.buildingDefs[ bIndex ];

                    // Get original pair
                    BuildingResearchPair p = FindOriginalBuildingResearch( building );

                    //Log.Message( "Restoring building research prereq: " + building.defName + " -> " + p.originalResearch.defName );

                    // Set the original pair
                    building.researchPrerequisite = p.originalResearch;
				}
			}
			// Flag it as enabled to skip it in later checks
		    Advanced.isEnabled = true;
		}

	}
}

