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
            if( ResearchController.AdvancedResearch.Exists( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ) )
            {
                return false;
            }
            // Is the research parent locked out?
            if( recipeDef.researchPrerequisite.IsLockedOut() )
            {
                return true;
            }
            // Assigned to things which are all locked out?
            if( !DefDatabase< ThingDef >.AllDefsListForReading.Exists( t => (
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
            if( ResearchController.AdvancedResearch.Exists( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )||
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
            thingsOn.AddRange( recipeDef.recipeUsers );
            foreach( var a in ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ).ToList() )
            {
                thingsOn.AddRange( a.thingDefs );
            }
            // Now check for an absolute requirement
            return
                ( thingsOn.Exists( t => t.HasResearchRequirement() ) )&&
                ( !thingsOn.Exists( t => !t.HasResearchRequirement() ) );
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
                foreach( var a in ResearchController.AdvancedResearch.Where( a => (
                    ( a.IsRecipeToggle )&&
                    ( !a.HideDefs )&&
                    ( a.ConsolidateHelp )&&
                    ( a.recipeDefs.Contains( recipeDef ) )
                ) ).ToList() )
                {
                    researchDefs.Add( a );
                }

            }

            // Get list of things recipe is used on
            var thingsOn = new List< ThingDef >();

            thingsOn.AddRange( DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                ( t.recipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.recipes.Contains( recipeDef ) )
            ) ) );

            // Add those linked via the recipe
            if( ( recipeDef.recipeUsers != null )&&
                ( recipeDef.recipeUsers.Count > 0 ) )
            {
                thingsOn.AddRange( recipeDef.recipeUsers );
            }

            // Make sure they all have hard requirements
            if( thingsOn.All( t => t.HasResearchRequirement() ) )
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

            thingsOn.AddRange( DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( t.AllRecipes != null )&&
                ( !t.IsLockedOut() )&&
                ( t.AllRecipes.Contains( recipeDef ) )
            ) ).ToList() );

            return thingsOn;
        }

        public static void                  GetThingsByResearchUnlocked( this RecipeDef recipeDef, ref List< ThingDef > thingDefs, ref List< Def > researchDefs )
        {
            // Things it is unlocked on with research
            thingDefs = new List<ThingDef>();
            researchDefs = new List<Def>();

            if( recipeDef.researchPrerequisite != null )
            {
                thingDefs.AddRange( recipeDef.recipeUsers );
                researchDefs.Add( recipeDef.researchPrerequisite );
            }

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.recipeDefs.Contains( recipeDef ) )
            ) ).ToList();

            // Aggregate advanced research
            foreach( var a in advancedResearch )
            {
                thingDefs.AddRange( a.thingDefs );
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

        public static void                  GetThingsByResearchLocked( this RecipeDef recipeDef, ref List< ThingDef > thingDefs, ref List< Def > researchDefs )
        {
            // Things it is locked on with research
            thingDefs = new List<ThingDef>();
            researchDefs = new List<Def>();

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
                if( a.researchDefs.Count == 1 )
                {
                    // If it's a single research project, add that
                    researchDefs.Add( a.researchDefs[ 0 ] );
                }
                else if( a.HelpConsolidator != null )
                {
                    // Add the advanced project instead
                    researchDefs.Add( a.HelpConsolidator );
                }
            }
        }

        #endregion

    }

}
