using System;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{

    public static class ResearchProjectDef_Extensions
    {

        #region Static Data

        static Dictionary<ResearchProjectDef,bool> isLockedOut = new Dictionary<ResearchProjectDef, bool>();

        #endregion

        #region Availability

        public static bool                  IsLockedOut( this ResearchProjectDef researchProjectDef, ResearchProjectDef initialDef = null )
        {
            bool rVal = false;
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "IsLockedOut()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            if( !isLockedOut.TryGetValue( researchProjectDef, out rVal ) )
            {
                if( initialDef == null )
                {
                    // Stop cyclical recursion before it starts
                    initialDef = researchProjectDef;
                }
                // Check for possible unlock
                if( !researchProjectDef.prerequisites.NullOrEmpty() )
                {
                    // Check each prerequisite
                    foreach( var p in researchProjectDef.prerequisites )
                    {
                        if(
                            ( p.defName == initialDef.defName )||
                            ( p.IsLockedOut( initialDef ) )
                        )
                        {
                            // Cyclical-prerequisite or parent locked means potential lock-out

                            // Check for possible unlock
                            if( !ResearchController.AdvancedResearch.Any( a => (
                                ( a.IsResearchToggle )&&
                                ( !a.HideDefs )&&
                                ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                            ) ) )
                            {
                                // No unlockers, always locked out
                                rVal = true;
                            }
                        }
                    }
                }
                isLockedOut.Add( researchProjectDef, rVal );
            }
            return rVal;
        }

        public static bool                  HasResearchRequirement( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "HasResearchRequirement()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Can't entirely rely on this one check as it's state may change mid-game
            if( researchProjectDef.prerequisites != null )
            {
                // Fast and easy
                return true;
            }

            // Check for an advanced research unlock
            return
                ResearchController.AdvancedResearch.Any( a => (
                    ( a.IsResearchToggle )&&
                    ( !a.HideDefs )&&
                    ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                ) );
        }

        #endregion

        #region Lists of affected data

        public static List< Def >           GetResearchRequirements( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetResearchRequirements()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
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
                        ( !a.HideDefs )&&
                        ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                    ) ).ToList();

                    if( !advancedResearchDefs.NullOrEmpty() )
                    {
                        foreach( var advancedResearchDef in advancedResearchDefs )
                        {
                            researchDefs.AddRange( advancedResearchDef.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                        }
                    }
                }
            }

            // Return the list of research required
            return researchDefs;
        }

        public static List<Def>             GetResearchUnlocked(this ResearchProjectDef researchProjectDef)
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetResearchUnlocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            var researchDefs = new List<Def>();

            //CCL_Log.Message("Normal");
            researchDefs.AddRange(DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where(rd => rd.prerequisites.Contains(researchProjectDef)).ToList().ConvertAll<Def>(def => (Def) def));
            
            //CCL_Log.Message("Advanced");
            // same as prerequisites, but with effectedResearchDefs and researchDefs switched.
            var advancedResearchDefs = ResearchController.AdvancedResearch.Where(a => (
                (a.IsResearchToggle) &&
                (!a.HideDefs) &&
                (a.researchDefs.Contains(researchProjectDef))
            )).ToList();

            researchDefs.AddRange( advancedResearchDefs.SelectMany(ar => ar.effectedResearchDefs ).ToList().ConvertAll<Def>(Def => ( Def )Def ) );

            return researchDefs;
        } 

        public static List< Def >           GetResearchedLockedBy( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetResearchLockedBy()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Advanced Research that locks it
            var researchDefs = new List<Def>();

            // Look in advanced research
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsResearchToggle )&&
                ( a.HideDefs )&&
                ( a.effectedResearchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    researchDefs.AddRange( a.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }
            }

            return researchDefs;
        }

        public static List< ThingDef >      GetThingsUnlocked( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetThingsUnlocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Buildings it unlocks
            var thingsOn = new List<ThingDef>();
            var researchThings = DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.researchPrerequisite != null )&&
                ( t.researchPrerequisite == researchProjectDef )
            ) ).ToList();

            if( !researchThings.NullOrEmpty() )
            {
                thingsOn.AddRange( researchThings );
            }

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsBuildingToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    thingsOn.AddRange( a.thingDefs );
                }
            }

            return thingsOn;
        }

        public static List<TerrainDef> GetTerrainUnlocked( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetTerrainUnlocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Buildings it unlocks
            var thingsOn = new List<TerrainDef>();
            var researchThings = DefDatabase<TerrainDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.researchPrerequisite != null )&&
                ( t.researchPrerequisite == researchProjectDef )
            ) ).ToList();

            if( !researchThings.NullOrEmpty() )
            {
                thingsOn.AddRange( researchThings );
            }

            return thingsOn;
        } 

        public static List< RecipeDef >     GetRecipesUnlocked( this ResearchProjectDef researchProjectDef, ref List< ThingDef > thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetRecipesUnlocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Recipes on buildings it unlocks
            var recipes = new List<RecipeDef>();
            if( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Add all recipes using this research projects
            var researchRecipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where( d => (
                ( d.researchPrerequisite == researchProjectDef )
            ) ).ToList();

            if( !researchRecipes.NullOrEmpty() )
            {
                recipes.AddRange( researchRecipes );
            }

            if( thingDefs != null )
            {
                // Add buildings for those recipes
                foreach( var r in recipes )
                {
                    thingDefs.AddRange( r.recipeUsers );
                }
            }

            // Look in advanced research too
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    recipes.AddRange( a.recipeDefs );
                    if( thingDefs != null )
                    {
                        thingDefs.AddRange( a.thingDefs );
                    }
                }
            }

            return recipes;
        }

        public static List< RecipeDef >     GetRecipesLocked( this ResearchProjectDef researchProjectDef, ref List< ThingDef > thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetRecipesLocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            // Recipes on buildings it locks
            var recipes = new List<RecipeDef>();
            if( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Look in advanced research
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsRecipeToggle )&&
                ( a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    recipes.AddRange( a.recipeDefs );
                    if( thingDefs != null )
                    {
                        thingDefs.AddRange( a.thingDefs );
                    }
                }
            }

            return recipes;
        }

        public static List< string >        GetSowTagsUnlocked( this ResearchProjectDef researchProjectDef, ref List< ThingDef > thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetSowTagsUnlocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
            var sowTags = new List< string >();
            if( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsPlantToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate advanced research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    sowTags.AddRange( a.sowTags );
                    if( thingDefs != null )
                    {
                        thingDefs.AddRange( a.thingDefs );
                    }
                }
            }

            return sowTags;
        }

        public static List< string >        GetSowTagsLocked( this ResearchProjectDef researchProjectDef, ref List< ThingDef > thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                Find_Extensions.ModByDefOfType<ResearchProjectDef>( researchProjectDef.defName ),
                Verbosity.Stack,
                "GetSowTagsLocked()",
                "ResearchProjectDef",
                researchProjectDef
            );
#endif
                       var sowTags = new List< string >();
            if( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.IsPlantToggle )&&
                ( a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate advanced research
            if( !advancedResearch.NullOrEmpty() )
            {
                foreach( var a in advancedResearch )
                {
                    sowTags.AddRange( a.sowTags );
                    if( thingDefs != null )
                    {
                        thingDefs.AddRange( a.thingDefs );
                    }
                }
            }

            return sowTags;
        }

        #endregion

    }

}
