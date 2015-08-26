using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class HelpController
    {

        //[Unsaved]

        #region Instance Data

        #endregion

        #region Process State

        public static void                  Initialize()
        {
            ResolveApparel();
            ResolveBuildings();
            ResolveWeapons();

            ResolveRecipes();

            ResolveReferences();

            Log.Message( "Community Core Library :: Help System :: Initialized" );
        }

        static void                         ResolveReferences()
        {
            foreach( var helpCategory in DefDatabase< HelpCategoryDef >.AllDefsListForReading )
            {
                helpCategory.Recache();
            }
            MainTabWindow_ModHelp.Recache();
        }

        static void                         ResolveApparel()
        {
            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( Apparel ) )
                ) ).ToList(),
                HelpCategoryDefOf.ApparelHelp
            );
        }

        static void                         ResolveWeapons()
        {
            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.IsWeapon )&&
                    ( t.category == ThingCategory.Item )
                ) ).ToList(),
                HelpCategoryDefOf.WeaponHelp
            );
        }

        static void                         ResolveBuildings()
        {
            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( !t.menuHidden )&&
                    ( !string.IsNullOrEmpty( t.designationCategory ) )&&
                    ( t.designationCategory != "None" )&&
                    ( !t.IsLockedOut() )
                ) ).ToList(),
                HelpCategoryDefOf.BuildingHelp
            );
        }

        static void                         ResolveThingDefList( List< ThingDef > thingDefs, HelpCategoryDef category )
        {
            if( ( thingDefs == null )||
                ( thingDefs.Count == 0 ) )
            {
                return;
            }

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var t in thingDefs )
            {
                if( t.researchPrerequisite.IsLockedOut() )
                {
                    // Skip anything that is locked out
                    continue;
                }

                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == t.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForBuildable( t, category );

                    // Inject the def
                    if( helpDef != null )
                    {
                        DefDatabase< HelpDef >.Add( helpDef );
                    }
                }

            }
        }

        static void                         ResolveRecipes()
        {
            // Get recipe database
            var recipes = DefDatabase< RecipeDef >.AllDefsListForReading;
            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var r in recipes )
            {
                if( r.researchPrerequisite.IsLockedOut() )
                {
                    // Skip anything that is locked out
                    continue;
                }

                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == r.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForRecipe( r, HelpCategoryDefOf.RecipeHelp );

                    // Inject the def
                    if( helpDef != null )
                    {
                        DefDatabase< HelpDef >.Add( helpDef );
                    }
                }

            }
        }

        #endregion


        #region Help Makers

        static HelpDef                      HelpForBuildable( BuildableDef b, HelpCategoryDef category )
        {
            // What research is required
            var researchRequired = new List<ResearchProjectDef>();
            if( b.researchPrerequisite != null )
            {
                researchRequired.Add( b.researchPrerequisite );
            }
            // Get advanced research which unlocks
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( !a.HideDefs )&&
                ( a.IsBuildingToggle )&&
                ( a.thingDefs.Contains( b as ThingDef ) )
            ) ).ToList();
            // Aggregate research
            foreach( var a in advancedResearch )
            {
                researchRequired.AddRange( a.researchDefs );
            }
            foreach( var r in researchRequired )
            {
                if( r.IsLockedOut() )
                {
                    // Don't generate help for locked-out buildable defs
                    return null;
                }
            }

            var helpDef = new HelpDef();
            helpDef.defName = b.defName + "_BuildableDef_Help";
            helpDef.keyDef = b.defName;
            helpDef.label = b.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( b.description );
            s.AppendLine();

            // Look at base stats
            foreach( var stat in b.statBases )
            {
                s.Append( stat.stat.LabelCap );
                s.Append( " : " );
                s.AppendLine( stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
            }
            s.AppendLine();

            var t = b as ThingDef;
            if( t != null )
            {

                // What stuff can it be made from?
                if( t.costStuffCount > 0 )
                {
                    s.Append( "Stuff Cost : " );
                    s.AppendLine( t.costStuffCount.ToString() );
                    BuildStuffCategoryDescription( s, "Stuff made from:", t.stuffCategories );
                }

                // What other things are required?
                if( ( t.costList != null )&&
                    ( t.costList.Count > 0 ) )
                {
                    s.AppendLine( "Cost to build:" );
                    foreach( var tc in t.costList )
                    {
                        s.Append( "\t" );
                        s.Append( tc.thingDef.LabelCap );
                        s.Append( " : " );
                        s.AppendLine( tc.count.ToString() );
                    }
                    s.AppendLine();
                }

                // Add list of required research
                BuildResearchDescription( s, "Required Research:", researchRequired );

                // Get list of recipes
                var baseRecipes = t.AllRecipes;
                BuildRecipeDescription( s, "Recipes:", baseRecipes );

                // Get list of recipes which are unlocked by research projects:
                var researchRecipes = DefDatabase< RecipeDef >.AllDefsListForReading
                    .Where( r => (
                        ( r.researchPrerequisite != null )&&
                        ( r.recipeUsers != null )&&
                        ( r.recipeUsers.Contains( t ) )
                    ) ).ToList();
                var recipeResearch = new List< ResearchProjectDef >();
                foreach( var r in researchRecipes )
                {
                    recipeResearch.Add( r.researchPrerequisite );
                }
                // Get List of recipes which are unlocked by advanced research:
                var advancedRecipes = ResearchController.AdvancedResearch
                    .Where( a => (
                        ( a.IsRecipeToggle )&&
                        ( !a.HideDefs )&&
                        ( a.thingDefs.Contains( t ) )
                    ) ).ToList();
                // Aggregate recipes
                foreach( var a in advancedRecipes )
                {
                    foreach( var r in a.researchDefs )
                    {
                        recipeResearch.Add( r );
                    }
                    researchRecipes.AddRange( a.recipeDefs );
                }

                // Build help for unlocked recipes associated with building
                BuildRecipeWithResearchDescription( s, "Recipes Unlocked:", "By research:", researchRecipes, recipeResearch );

                // Get List of recipes which are locked by advanced research:
                advancedRecipes = ResearchController.AdvancedResearch
                    .Where( a => (
                        ( a.IsRecipeToggle )&&
                        ( a.HideDefs )&&
                        ( a.thingDefs.Contains( t ) )
                    ) ).ToList();
                // Aggregate recipes
                recipeResearch.Clear();
                researchRecipes.Clear();
                foreach( var a in advancedRecipes )
                {
                    foreach( var r in a.researchDefs )
                    {
                        recipeResearch.Add( r );
                    }
                    researchRecipes.AddRange( a.recipeDefs );
                }

                // Build help for locked recipes associated with building
                BuildRecipeWithResearchDescription( s, "Recipes locked:", "By research:", researchRecipes, recipeResearch );

                // Get list of facilities that effect it
                var affectedBy = t.GetCompProperties( typeof( CompAffectedByFacilities ) );
                if( ( affectedBy != null )&&
                    ( affectedBy.linkableFacilities != null )&&
                    ( affectedBy.linkableFacilities.Count > 0 ) )
                {
                    BuildBuildingDescription( s, "Affected by:", affectedBy.linkableFacilities );
                }

                // Get list of buildings effected by it
                if( t.HasComp( typeof( CompFacility ) ) )
                {
                    var effectsBuildings = DefDatabase< ThingDef >.AllDefsListForReading
                        .Where( f => (
                            ( f.HasComp( typeof( CompAffectedByFacilities ) ) )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ) != null )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ).linkableFacilities != null )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ).linkableFacilities.Contains( t ) )
                        ) ).ToList();
                    if( ( effectsBuildings != null )&&
                        ( effectsBuildings.Count > 0 ) )
                    {
                        BuildBuildingDescription( s, "Effects:", effectsBuildings );
                    }
                }


            }

            helpDef.description = s.ToString();
            return helpDef;
        }

        static HelpDef                      HelpForRecipe( RecipeDef r, HelpCategoryDef category )
        {
            // What research is required
            var researchRequired = new List<ResearchProjectDef>();
            if( r.researchPrerequisite != null )
            {
                researchRequired.Add( r.researchPrerequisite );
            }
            // Get advanced research which unlocks
            var advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( !a.HideDefs )&&
                ( a.IsRecipeToggle )&&
                ( a.recipeDefs.Contains( r ) )
            ) ).ToList();
            // Aggregate research
            foreach( var a in advancedResearch )
            {
                researchRequired.AddRange( a.researchDefs );
            }
            foreach( var _r in researchRequired )
            {
                if( _r.IsLockedOut() )
                {
                    // Don't generate help for locked-out recipe defs
                    return null;
                }
            }

            // What buildings is it on
            var buildingsOn = new List<ThingDef>();
            buildingsOn.AddRange( DefDatabase<ThingDef>.AllDefsListForReading.Where( t => (
                ( !t.IsLockedOut() )&&
                ( t.AllRecipes != null )&&
                ( t.AllRecipes.Contains( r ) )
            ) ).ToList() );
            // Aggregate research
            foreach( var a in advancedResearch )
            {
                foreach( var t in a.thingDefs )
                {
                    if( !t.IsLockedOut() )
                    {
                        buildingsOn.Add( t );
                    }
                }
            }
            if( buildingsOn.Count == 0 )
            {
                // Don't generate help for locked-out recipe defs
                return null;
            }

            var helpDef = new HelpDef();
            helpDef.defName = r.defName + "_RecipeDef_Help";
            helpDef.keyDef = r.defName;
            helpDef.label = r.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( r.description );
            s.AppendLine();

            s.AppendLine( "WorkAmount".Translate() + " : " + GenText.ToStringWorkAmount( r.WorkAmountTotal( (ThingDef) null ) ) );
            s.AppendLine();

            if( ( r.skillRequirements != null )&&
                ( r.skillRequirements.Count > 0 ) )
            {
                s.AppendLine( "MinimumSkills".Translate() );
                foreach( var sr in r.skillRequirements )
                {
                    s.Append( "\t" );
                    s.AppendLine( Translator.Translate( "BillRequires", new object[] { sr.minLevel.ToString( "####0" ), sr.skill.label.ToLower() } ) );
                }
                s.AppendLine();
            }

            // List of ingredients
            if( ( r.ingredients != null )&&
                ( r.ingredients.Count > 0 ) )
            {
                s.AppendLine( "Requires ingredients:" );
                foreach( var ing in r.ingredients )
                {
                    if( !GenText.NullOrEmpty( ing.filter.Summary ) ){
                        s.AppendLine( r.IngredientValueGetter.BillRequirementsDescription( ing ) );
                    }
                }
                s.AppendLine();
            }

            // Add buildings it's on
            BuildBuildingDescription( s, "Applies to:", buildingsOn );

            // Add research required
            BuildResearchDescription( s, "Required research:", researchRequired );

            // Get advanced research which locks
            advancedResearch = ResearchController.AdvancedResearch.Where( a => (
                ( a.HideDefs )&&
                ( a.IsRecipeToggle )&&
                ( a.recipeDefs.Contains( r ) )
            ) ).ToList();
            // Aggregate research
            researchRequired.Clear();
            foreach( var a in advancedResearch )
            {
                researchRequired.AddRange( a.researchDefs );
            }
            BuildResearchDescription( s, "Removed by research:", researchRequired );

            helpDef.description = s.ToString();
            return helpDef;
        }

        #endregion

        #region Array Cleaners

        static List< ResearchProjectDef >   TidyResearchProjects( List< ResearchProjectDef > researchProjects )
        {
            if( researchProjects == null )
            {
                return null;
            }
            for( int i = 0; i < researchProjects.Count; ++i )
            {
                if( researchProjects[ i ] == null )
                {
                    researchProjects.RemoveAt( i );
                    --i;
                }
            }
            if( researchProjects.Count == 0 )
            {
                return null;
            }
            return researchProjects;
        }

        static List< RecipeDef >            TidyRecipes( List< RecipeDef > recipes )
        {
            if( recipes == null )
            {
                return null;
            }
            for( int i = 0; i < recipes.Count; ++i )
            {
                if( recipes[ i ] == null )
                {
                    recipes.RemoveAt( i );
                    --i;
                }
            }
            if( recipes.Count == 0 )
            {
                return null;
            }
            return recipes;
        }

        static List< ThingDef >             TidyBuildings( List< ThingDef > buildings )
        {
            if( buildings == null )
            {
                return null;
            }
            for( int i = 0; i < buildings.Count; ++i )
            {
                if( buildings[ i ] == null )
                {
                    buildings.RemoveAt( i );
                    --i;
                }
            }
            if( buildings.Count == 0 )
            {
                return null;
            }
            return buildings;
        }

        static List< StuffCategoryDef >     TidyStuffCategories( List< StuffCategoryDef > stuff )
        {
            if( stuff == null )
            {
                return null;
            }
            for( int i = 0; i < stuff.Count; ++i )
            {
                if( stuff[ i ] == null )
                {
                    stuff.RemoveAt( i );
                    --i;
                }
            }
            if( stuff.Count == 0 )
            {
                return null;
            }
            return stuff;
        }

        #endregion

        #region String Builders

        static bool BuildResearchDescription( StringBuilder s, string prependResearch, List< ResearchProjectDef > researchProjects )
        {
            researchProjects = TidyResearchProjects( researchProjects );
            if( researchProjects == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependResearch );

            labels.Clear();
            for( int i = 0, count = researchProjects.Count - 1; i <= count; i++ ){
                var d = researchProjects[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();

            return true;
        }

        static bool BuildRecipeDescription( StringBuilder s, string prependRecipes, List< RecipeDef > recipes )
        {
            recipes = TidyRecipes( recipes );
            if( recipes == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependRecipes );
            labels.Clear();
            for( int i = 0, count = recipes.Count - 1; i <= count; i++ ){
                var d = recipes[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();

            return true;
        }

        static bool BuildBuildingDescription( StringBuilder s, string prependBuildings, List< ThingDef > buildings )
        {
            buildings = TidyBuildings( buildings );
            if( buildings == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependBuildings );
            labels.Clear();
            for( int i = 0, count = buildings.Count - 1; i <= count; i++ ){
                var d = buildings[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();

            return true;
        }

        static bool BuildStuffCategoryDescription( StringBuilder s, string prependStuff, List< StuffCategoryDef > stuff )
        {
            stuff = TidyStuffCategories( stuff );
            if( stuff == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependStuff );
            labels.Clear();
            for( int i = 0, count = stuff.Count - 1; i <= count; i++ ){
                var d = stuff[ i ];
                if( !labels.Contains( d.label.ToLower() ) )
                {
                    labels.Add( d.label.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d.LabelCap );
                }
            }
            s.AppendLine();

            return true;
        }

        static void BuildRecipeWithResearchDescription( StringBuilder s, string prependRecipes, string prependResearch, List< RecipeDef > recipes, List< ResearchProjectDef > researchProjects )
        {
            if( BuildRecipeDescription( s, prependRecipes, recipes ) )
            {
                BuildResearchDescription( s, prependResearch, researchProjects );
            }
        }

        static void BuildRecipeWithBuildingDescription( StringBuilder s, string prependRecipes, string prependBuildings, List< RecipeDef > recipes, List< ThingDef > buildings )
        {
            if( BuildRecipeDescription( s, prependRecipes, recipes ) )
            {
                BuildBuildingDescription( s, prependBuildings, buildings );
            }
        }

        static void BuildBuildingWithResearchDescription( StringBuilder s, string prependBuildings, string prependResearch, List< ThingDef > buildings, List< ResearchProjectDef > researchProjects )
        {
            if( BuildBuildingDescription( s, prependBuildings, buildings ) )
            {
                BuildResearchDescription( s, prependResearch, researchProjects );
            }
        }

        #endregion

    }

}
