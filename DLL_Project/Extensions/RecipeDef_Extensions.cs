using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class RecipeDef_Extensions
    {

        #region Availability

        public static bool                  IsLockedOut( this RecipeDef recipeDef )
        {
            // Advanced research unlocking it?
            if( ResearchController.AdvancedResearch.Any( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ) )
            {
                return false;
            }
            // Is the research parent locked out?
            if(
                ( recipeDef.researchPrerequisite != null )&&
                ( recipeDef.researchPrerequisite.IsLockedOut() )
            )
            {
                return true;
            }
            // Assigned to things which are all locked out?
            if( !DefDatabase< ThingDef >.AllDefsListForReading.Any( t => (
                ( t.AllRecipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.AllRecipes.Contains( recipeDef ) )
            ) ) )
            {
                return true;
            }
            // Something has it at some point
            return false;
        }

        public static bool                  HasResearchRequirement( this RecipeDef recipeDef )
        {
            // Can't entirely rely on this one check as it's state may change mid-game
            if( recipeDef.researchPrerequisite != null )
            {
                // Easiest check, do it first
                return true;
            }

            // Check for an advanced research unlock
            if( ResearchController.AdvancedResearch.Any( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ) )
            {
                return true;
            }

            // Get list of things referencing
            var thingsOn = DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( t.recipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.recipes.Contains( recipeDef ) )
            ) ).ToList();

            if( thingsOn == null )
            {
                thingsOn = new List<ThingDef>();
            }
            else
            {
                thingsOn.AddRange( recipeDef.recipeUsers );
            }

            var advancedResearchDefs = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ).ToList();
            if( !advancedResearchDefs.NullOrEmpty() )
            {
                foreach( var a in advancedResearchDefs )
                {
                    thingsOn.AddRange( a.thingDefs );
                }
            }
            // Now check for an absolute requirement
            return ( thingsOn.All( t => t.HasResearchRequirement() ) );
        }

        #endregion

        #region Lists of affected data

        public static List< Def >           GetResearchRequirements( this RecipeDef recipeDef )
        {
            var researchDefs = new List< Def >();

            if( recipeDef.researchPrerequisite != null )
            {
                // Basic requirement
                researchDefs.Add( recipeDef.researchPrerequisite );

                // Advanced requirement
                var advancedResearchDefs = ResearchController.AdvancedResearch.Where( a => (
                    ( a.IsRecipeToggle )&&
                    ( !a.HideDefs )&&
                    ( a.recipeDefs.Contains( recipeDef ) )
                ) ).ToList();

                if( !advancedResearchDefs.NullOrEmpty() )
                {
                    foreach( var a in advancedResearchDefs )
                    {
                        researchDefs.Add( a );
                    }
                }

            }

            // Get list of things recipe is used on
            var thingsOn = new List< ThingDef >();
            var recipeThings = DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                ( t.recipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.recipes.Contains( recipeDef ) )
            ) ).ToList();

            if( !recipeThings.NullOrEmpty() )
            {
                thingsOn.AddRange( recipeThings );
            }

            // Add those linked via the recipe
            if( !recipeDef.recipeUsers.NullOrEmpty() )
            {
                thingsOn.AddRange( recipeDef.recipeUsers );
            }

            // Make sure they all have hard requirements
            if(
                ( !thingsOn.NullOrEmpty() )&&
                ( thingsOn.All( t => t.HasResearchRequirement() ) )
            )
            {
                foreach( var t in thingsOn )
                {
                    researchDefs.AddRange( t.GetResearchRequirements() );
                }
            }

            // Return the list of research required
            return researchDefs;
        }

        public static List< ThingDef >      GetThingsCurrent( this RecipeDef recipeDef )
        {
            // Things it is currently on
            var thingsOn = new List<ThingDef>();
            var recipeThings = DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( t.AllRecipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.AllRecipes.Contains( recipeDef ) )
            ) ).ToList();

            if( !recipeThings.NullOrEmpty() )
            {
                thingsOn.AddRange( recipeThings );
            }

            return thingsOn;
        }

        public static List< ThingDef >      GetThingsUnlocked( this RecipeDef recipeDef, ref List< Def > researchDefs )
        {
            // Things it is unlocked on with research
            var thingDefs = new List<ThingDef>();
            if( researchDefs != null )
            {
                researchDefs.Clear();
            }

            if( recipeDef.researchPrerequisite != null )
            {
                thingDefs.AddRange( recipeDef.recipeUsers );
                if( researchDefs != null )
                {
                    researchDefs.Add( recipeDef.researchPrerequisite );
                }
            }

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ).ToList();

            // Aggregate advanced research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    thingDefs.AddRange( a.thingDefs );

                    if( researchDefs != null )
                    {
                        if( a.researchDefs.Count == 1 )
                        {
                            // If it's a single research project, add that
                            researchDefs.Add( a.researchDefs[ 0 ] );
                        }
                        else
                        {
                            // Add the advanced project instead
                            researchDefs.Add( a );
                        }
                    }
                }
            }

            return thingDefs;
        }

        public static List< ThingDef >      GetThingsLocked( this RecipeDef recipeDef, ref List< Def > researchDefs )
        {
            // Things it is locked on with research
            var thingDefs = new List<ThingDef>();
            if( researchDefs != null )
            {
                researchDefs.Clear();
            }

            // Look in advanced research
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ).ToList();

            // Aggregate advanced research
            foreach( var a in advancedResearch )
            {
                thingDefs.AddRange( a.thingDefs );

                if( researchDefs != null )
                {
                    if( a.researchDefs.Count == 1 )
                    {
                        // If it's a single research project, add that
                        researchDefs.Add( a.researchDefs[ 0 ] );
                    }
                    else if( a.ResearchConsolidator != null )
                    {
                        // Add the advanced project instead
                        researchDefs.Add( a.ResearchConsolidator );
                    }
                }
            }

            return thingDefs;
        }

        #endregion

    }

}
