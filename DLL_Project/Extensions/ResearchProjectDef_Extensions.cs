using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CommunityCoreLibrary
{
    [StaticConstructorOnStartup]
    public static class ResearchProjectDef_Extensions
    {
        #region Static Data

        private static Dictionary<ushort, bool> isLockedOut;

        private static Dictionary<ushort, List<Pair<Def, string>>> _unlocksCache;

        static ResearchProjectDef_Extensions()
        {
            isLockedOut = new Dictionary<ushort, bool>();
            _unlocksCache = new Dictionary<ushort, List<Pair<Def, string>>>();
        }

        #endregion Static Data

        #region Availability

        public static void ClearIsLockedOut()
        {
            isLockedOut.Clear();
        }

        public static bool IsLockedOut( this ResearchProjectDef researchProjectDef )
        {
            // Cloak internal recursive function
            return _IsLockedOut( researchProjectDef, researchProjectDef );
        }

        internal static bool _IsLockedOut( this ResearchProjectDef researchProjectDef, ResearchProjectDef initialDef )
        {
            bool rVal = false;
            if ( !isLockedOut.TryGetValue( researchProjectDef.shortHash, out rVal ) )
            {
#if DEBUG
                CCL_Log.TraceMod(
                    researchProjectDef,
                    Verbosity.Stack,
                    "IsLockedOut()"
                );
#endif
                // Check for possible unlock
                if ( !researchProjectDef.prerequisites.NullOrEmpty() )
                {
                    // Check each prerequisite
                    foreach ( var p in researchProjectDef.prerequisites )
                    {
                        if (
                            ( p.defName == initialDef.defName )||
                            ( p._IsLockedOut( initialDef ) )
                        )
                        {
                            // Cyclical-prerequisite or parent locked means potential lock-out

                            // Check for possible unlock
                            if ( !Controller.Data.AdvancedResearchDefs.Any( a => (
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
                isLockedOut.Add( researchProjectDef.shortHash, rVal );
            }
            return rVal;
        }

        public static bool HasResearchRequirement( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "HasResearchRequirement()"
            );
#endif
            // Can't entirely rely on this one check as it's state may change mid-game
            if ( researchProjectDef.prerequisites != null )
            {
                // Fast and easy
                return true;
            }

            // Check for an advanced research unlock
            return
                Controller.Data.AdvancedResearchDefs.Any( a => (
                    ( a.IsResearchToggle )&&
                    ( !a.HideDefs )&&
                    ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                ) );
        }

        #endregion Availability

        #region Lists of affected data

        public static List<ResearchProjectDef> ExclusiveDescendants( this ResearchProjectDef research )
        {
            List<ResearchProjectDef> descendants = new List<ResearchProjectDef>();

            // recursively go through all children
            // populate initial queue
            Queue<ResearchProjectDef> queue = new Queue<ResearchProjectDef>( DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where( res => res.prerequisites.Contains( research ) ) );

            // for each item in queue, determine if there's something unlocking it
            // if not, add to the list, and queue up children.
            while ( queue.Count > 0 )
            {
                ResearchProjectDef current = queue.Dequeue();

                if ( !Controller.Data.AdvancedResearchDefs.Any(
                        ard => ard.IsResearchToggle &&
                               !ard.HideDefs &&
                               !ard.IsLockedOut() &&
                               ard.effectedResearchDefs.Contains( current ) ) &&
                     !descendants.Contains( current ) )
                {
                    descendants.Add( current );
                    foreach ( ResearchProjectDef descendant in DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where( res => res.prerequisites.Contains( current ) ) )
                    {
                        queue.Enqueue( descendant );
                    }
                }
            }

            return descendants;
        }

        public static List<ResearchProjectDef> GetPrerequisitesRecursive( this ResearchProjectDef research )
        {
            List<ResearchProjectDef> result = new List<ResearchProjectDef>();
            if ( research.prerequisites.NullOrEmpty() )
            {
                return result;
            }
            Stack<ResearchProjectDef> stack = new Stack<ResearchProjectDef>( research.prerequisites.Where( parent => parent != research ) );

            while ( stack.Count > 0 )
            {
                var parent = stack.Pop();
                result.Add( parent );

                if ( !parent.prerequisites.NullOrEmpty() )
                {
                    foreach ( var grandparent in parent.prerequisites )
                    {
                        if ( grandparent != parent )
                            stack.Push( grandparent );
                    }
                }
            }

            return result.Distinct().ToList();
        }

        public static List<Pair<Def, string>> GetUnlockDefsAndDescs( this ResearchProjectDef research )
        {
            if ( _unlocksCache.ContainsKey( research.shortHash ) )
            {
                return _unlocksCache[research.shortHash];
            }

            List<Pair<Def, string>> unlocks = new List<Pair<Def, string>>();

            // dumps recipes/plants unlocked, because of the peculiar way CCL helpdefs are done.
            List<ThingDef> dump = new List<ThingDef>();

            unlocks.AddRange( research.GetThingsUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>( d, "Fluffy.ResearchTree.AllowsBuildingX".Translate( d.LabelCap ) ) ) );
            unlocks.AddRange( research.GetTerrainUnlocked()
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>( d, "Fluffy.ResearchTree.AllowsBuildingX".Translate( d.LabelCap ) ) ) );
            unlocks.AddRange( research.GetRecipesUnlocked( ref dump )
                                      .Where( d => d.IconTexture() != null )
                                      .Select( d => new Pair<Def, string>( d, "Fluffy.ResearchTree.AllowsCraftingX".Translate( d.LabelCap ) ) ) );
            string sowTags = string.Join( " and ", research.GetSowTagsUnlocked( ref dump ).ToArray() );
            unlocks.AddRange( dump.Where( d => d.IconTexture() != null )
                                  .Select( d => new Pair<Def, string>( d, "Fluffy.ResearchTree.AllowsSowingXinY".Translate( d.LabelCap, sowTags ) ) ) );

            _unlocksCache.Add( research.shortHash, unlocks );
            return unlocks;
        }

        public static List<Def> GetResearchRequirements( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetResearchRequirements()"
            );
#endif
            var researchDefs = new List<Def>();

            if ( researchProjectDef.prerequisites != null )
            {
                if ( !researchProjectDef.prerequisites.Contains( Research.Locker ) )
                {
                    researchDefs.AddRangeUnique( researchProjectDef.prerequisites.ConvertAll<Def>( def => (Def)def ) );
                }
                else
                {
                    var advancedResearchDefs = Controller.Data.AdvancedResearchDefs.Where( a => (
                        ( a.IsResearchToggle )&&
                        ( !a.HideDefs )&&
                        ( a.effectedResearchDefs.Contains( researchProjectDef ) )
                    ) ).ToList();

                    if ( !advancedResearchDefs.NullOrEmpty() )
                    {
                        foreach ( var advancedResearchDef in advancedResearchDefs )
                        {
                            researchDefs.AddRangeUnique( advancedResearchDef.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                        }
                    }
                }
            }

            // Return the list of research required
            return researchDefs;
        }

        public static List<Def> GetResearchUnlocked( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetResearchUnlocked()"
            );
#endif
            var researchDefs = new List<Def>();

            Log.Message( "Normal" );
            researchDefs.AddRangeUnique( DefDatabase<ResearchProjectDef>.AllDefsListForReading.Where( rd =>
                                                                                                        ( !rd.prerequisites.NullOrEmpty() )&&
                                                                                                        ( rd.prerequisites.Contains( researchProjectDef ) )
                                                                                                    ).ToList().ConvertAll<Def>( def => (Def)def ) );

            Log.Message( "Advanced" );
            // same as prerequisites, but with effectedResearchDefs and researchDefs switched.
            var advancedResearchDefs = Controller.Data.AdvancedResearchDefs.Where( a => (
                 ( a.IsResearchToggle ) &&
                 ( !a.HideDefs ) &&
                 ( a.researchDefs.Contains( researchProjectDef ) )
             ) ).ToList();

            researchDefs.AddRangeUnique( advancedResearchDefs.SelectMany( ar => ar.effectedResearchDefs ).ToList().ConvertAll<Def>( Def => (Def)Def ) );

            return researchDefs;
        }

        public static List<Def> GetResearchedLockedBy( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetResearchLockedBy()"
            );
#endif
            // Advanced Research that locks it
            var researchDefs = new List<Def>();

            // Look in advanced research
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsResearchToggle )&&
                ( a.HideDefs )&&
                ( a.effectedResearchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    researchDefs.AddRangeUnique( a.researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }
            }

            return researchDefs;
        }

        public static List<ThingDef> GetThingsUnlocked( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetThingsUnlocked()"
            );
#endif
            // Buildings it unlocks
            var thingsOn = new List<ThingDef>();
            var researchThings = DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.GetResearchRequirements() != null ) &&
                ( t.GetResearchRequirements().Contains( researchProjectDef ) )
            ) ).ToList();

            if ( !researchThings.NullOrEmpty() )
            {
                thingsOn.AddRangeUnique( researchThings );
            }

            // Look in advanced research too
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsBuildingToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    thingsOn.AddRangeUnique( a.thingDefs );
                }
            }

            return thingsOn;
        }

        public static List<TerrainDef> GetTerrainUnlocked( this ResearchProjectDef researchProjectDef )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetTerrainUnlocked()"
            );
#endif
            // Buildings it unlocks
            var thingsOn = new List<TerrainDef>();
            var researchThings = DefDatabase<TerrainDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.GetResearchRequirements() != null )&&
                ( t.GetResearchRequirements().Contains( researchProjectDef ) )
            ) ).ToList();

            if ( !researchThings.NullOrEmpty() )
            {
                thingsOn.AddRangeUnique( researchThings );
            }

            return thingsOn;
        }

        public static List<RecipeDef> GetRecipesUnlocked( this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetRecipesUnlocked()"
            );
#endif
            // Recipes on buildings it unlocks
            var recipes = new List<RecipeDef>();
            if ( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Add all recipes using this research projects
            var researchRecipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where( d => (
                ( d.researchPrerequisite == researchProjectDef )
            ) ).ToList();

            if ( !researchRecipes.NullOrEmpty() )
            {
                recipes.AddRangeUnique( researchRecipes );
            }

            if ( thingDefs != null )
            {
                // Add buildings for those recipes
                foreach ( var r in recipes )
                {
                    if ( !r.recipeUsers.NullOrEmpty() )
                    {
                        thingDefs.AddRangeUnique( r.recipeUsers );
                    }
                    var recipeThings = DefDatabase<ThingDef>.AllDefsListForReading.Where( d => (
                        ( !d.recipes.NullOrEmpty() )&&
                        ( d.recipes.Contains( r ) )
                    ) ).ToList();
                    if ( !recipeThings.NullOrEmpty() )
                    {
                        thingDefs.AddRangeUnique( recipeThings );
                    }
                }
            }

            // Look in advanced research too
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsRecipeToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    recipes.AddRangeUnique( a.recipeDefs );
                    if ( thingDefs != null )
                    {
                        thingDefs.AddRangeUnique( a.thingDefs );
                    }
                }
            }

            return recipes;
        }

        public static List<RecipeDef> GetRecipesLocked( this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetRecipesLocked()"
            );
#endif
            // Recipes on buildings it locks
            var recipes = new List<RecipeDef>();
            if ( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Look in advanced research
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsRecipeToggle )&&
                ( a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    recipes.AddRangeUnique( a.recipeDefs );
                    if ( thingDefs != null )
                    {
                        thingDefs.AddRangeUnique( a.thingDefs );
                    }
                }
            }

            return recipes;
        }

        public static List<string> GetSowTagsUnlocked( this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetSowTagsUnlocked()"
            );
#endif
            var sowTags = new List<string>();
            if ( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Add all plants using this research project
            var researchPlants = DefDatabase<ThingDef>.AllDefsListForReading.Where( d => (
                ( d.plant != null )&&
                ( !d.plant.sowResearchPrerequisites.NullOrEmpty() )&&
                ( d.plant.sowResearchPrerequisites.Contains( researchProjectDef ) )
            ) ).ToList();

            if ( !researchPlants.NullOrEmpty() )
            {
                foreach ( var plant in researchPlants )
                {
                    sowTags.AddRangeUnique( plant.plant.sowTags );
                }
                if ( thingDefs != null )
                {
                    thingDefs.AddRangeUnique( researchPlants );
                }
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsPlantToggle )&&
                ( !a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate advanced research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    sowTags.AddRangeUnique( a.sowTags );
                    if ( thingDefs != null )
                    {
                        thingDefs.AddRangeUnique( a.thingDefs );
                    }
                }
            }

            return sowTags;
        }

        public static List<string> GetSowTagsLocked( this ResearchProjectDef researchProjectDef, ref List<ThingDef> thingDefs )
        {
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.Stack,
                "GetSowTagsLocked()"
            );
#endif
            var sowTags = new List<string>();
            if ( thingDefs != null )
            {
                thingDefs.Clear();
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var advancedResearch = Controller.Data.AdvancedResearchDefs.Where( a => (
                ( a.IsPlantToggle )&&
                ( a.HideDefs )&&
                ( a.researchDefs.Count == 1 )&&
                ( a.researchDefs.Contains( researchProjectDef ) )
            ) ).ToList();

            // Aggregate advanced research
            if ( !advancedResearch.NullOrEmpty() )
            {
                foreach ( var a in advancedResearch )
                {
                    sowTags.AddRangeUnique( a.sowTags );
                    if ( thingDefs != null )
                    {
                        thingDefs.AddRangeUnique( a.thingDefs );
                    }
                }
            }

            return sowTags;
        }

        #endregion Lists of affected data
    }
}