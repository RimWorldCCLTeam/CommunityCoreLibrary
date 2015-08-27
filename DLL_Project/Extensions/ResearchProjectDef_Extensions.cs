using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ResearchProjectDef_Extensions
    {

        #region Availability

        public static bool                  IsLockedOut( this ResearchProjectDef researchProjectDef )
        {
            // Check for possible unlock
            if( researchProjectDef != null )
            {
                // Check each prerequisite
                foreach( var p in researchProjectDef.prerequisites )
                {
                    if( p.defName == researchProjectDef.defName )
                    {
                        // Self-prerequisite means potential lock-out

                        // Check for possible unlock
                        if( !ResearchController.AdvancedResearch.Exists( a => (
                            ( a.IsResearchToggle )&&
                            ( !a.HideDefs )&&
                            ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                        ) ) )
                        {
                            // No unlockers, always locked out
                            return true;
                        }
                    }
                    else if( p.IsLockedOut() )
                    {
                        // Any of the research parents locked out?
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool                  HasResearchRequirement( this ResearchProjectDef researchProjectDef )
        {
            // Can't entirely rely on this one check as it's state may change mid-game
            if( researchProjectDef.prerequisites != null )
            {
                // Fast and easy
                return true;
            }

            // Check for an advanced research unlock
            return
                ResearchController.AdvancedResearch.Exists( a => (
                    ( a.IsResearchToggle )&&
                    ( !a.HideDefs )||
                    ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                ) );
        }

        #endregion
        
        #region Lists of affected data

        public static List< Def >           GetResearchRequirements( this ResearchProjectDef researchProjectDef )
        {
            var researchDefs = new List< Def >();

            if( researchProjectDef.prerequisites != null )
            {
                if( !researchProjectDef.prerequisites.Contains( Research.Locker ) )
                {
                    researchDefs.AddRange( researchProjectDef.prerequisites.ConvertAll<Def>( def =>(Def)def ) );
                }
                else
                {
                    var advancedResearchDefs = ResearchController.AdvancedResearch.Where( a => (
                        ( a.IsResearchToggle )&&
                        ( !a.HideDefs )||
                        ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                    ) ).ToList();
                    foreach( var advancedResearchDef in advancedResearchDefs )
                    {
                        researchDefs.AddRange( advancedResearchDef.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                    }
                }
            }

            // Return the list of research required
            return researchDefs;
        }

        public static List< Def >           GetResearchedLockedBy( this ResearchProjectDef researchProjectDef )
        {
            // Advanced Research that locks it
            var researchDefs = new List<Def>();

            // Look in advanced research
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsResearchToggle )&&
                ( a.HideDefs )&&
                ( a.effectedResearchDefs.Contains( researchProjectDef ) )
            ) ).ToList();
            // Aggregate research
            foreach( var a in advancedResearch )
            {
                researchDefs.AddRange( a.researchDefs.ConvertAll<Def>( def => (Def)def ) );
            }
            return researchDefs;
        }

        public static List< ThingDef >      GetBuildingsUnlocked( this ResearchProjectDef researchProjectDef )
        {
            // Buildings it unlocks
            var thingsOn = new List<ThingDef>();
            thingsOn.AddRange( DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.researchPrerequisite != null )&&
                ( t.researchPrerequisite == researchProjectDef )
            ) ).ToList() );
            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsBuildingToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();
            // Aggregate research
            foreach( var a in advancedResearch )
            {
                thingsOn.AddRange( a.thingDefs );
            }
            return thingsOn;
        }

        public static void                  GetRecipesOnBuildingsUnlocked( this ResearchProjectDef researchProjectDef, ref List< RecipeDef > recipes, ref List< ThingDef > buildings )
        {
            // Recipes on buildings it unlocks
            recipes = new List<RecipeDef>();
            buildings = new List<ThingDef>();

            // Add all recipes using this research projects
            recipes.AddRange( DefDatabase<RecipeDef>.AllDefsListForReading.Where( d => (
                ( d.researchPrerequisite == researchProjectDef )
            ) ).ToList() );

            // Add buildings for those recipes
            foreach( var r in recipes )
            {
                buildings.AddRange( r.recipeUsers );
            }

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            foreach( var a in advancedResearch )
            {
                recipes.AddRange( a.recipeDefs );
                buildings.AddRange( a.thingDefs );
            }

        }

        public static void                  GetSowTagsOnPlantsUnlocked( this ResearchProjectDef researchProjectDef, ref List< string > sowTags, ref List< ThingDef > plants )
        {
            sowTags = new List< string >();
            plants = new List< ThingDef >();

            // Look in advanced research to add plants and sow tags it unlocks
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsPlantToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate advanced research
            foreach( var a in advancedResearch )
            {
                sowTags.AddRange( a.sowTags );
                plants.AddRange( a.thingDefs );
            }

        }

        #endregion

    }

}
