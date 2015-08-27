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
        // TODO: Replace static strings with keyed translations

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

            ResolveResearch();
            ResolveAdvancedResearch();

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
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( Apparel ) )
                ) ).ToList();

            if( ( thingDefs == null )||
                ( thingDefs.Count == 0 ) )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.ApparelHelp, "Apparel", "Items" );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void                         ResolveWeapons()
        {
            // Get list of things
            var thingDefs = 
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.IsWeapon )&&
                    ( t.category == ThingCategory.Item )
                ) ).ToList();

            if( ( thingDefs == null )||
                ( thingDefs.Count == 0 ) )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.WeaponHelp, "Weapons", "Items" );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void                         ResolveBuildings()
        {
            // Go through buildings by designation categories
            foreach( var designationCategoryDef in DefDatabase< DesignationCategoryDef >.AllDefsListForReading )
            {
                // Get list of things
                var thingDefs =
                    DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                        ( t.designationCategory == designationCategoryDef.defName )&&
                        ( !t.IsLockedOut() )
                    ) ).ToList();

                if( ( thingDefs != null )&&
                    ( thingDefs.Count > 0 ) )
                {
                    // Get help category
                    var helpCategoryDef = HelpCategoryForKey( designationCategoryDef.defName, designationCategoryDef.label, "Buildings" );

                    // Scan through all possible buildable defs and auto-generate help
                    ResolveThingDefList(
                        thingDefs,
                        helpCategoryDef
                    );
                }
            }
        }

        static void                         ResolveThingDefList( List< ThingDef > thingDefs, HelpCategoryDef category )
        {
            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var thingDef in thingDefs )
            {
                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == thingDef.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForBuildable( thingDef, category );

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
            var recipeDefs =
                DefDatabase< RecipeDef >.AllDefsListForReading.Where( r => (
                    ( !r.IsLockedOut() )
                ) ).ToList();

            if( ( recipeDefs == null )||
                ( recipeDefs.Count == 0 ) )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.RecipeHelp, "Recipes", "Items" );

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var recipeDef in recipeDefs )
            {
                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == recipeDef.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForRecipe( recipeDef, helpCategoryDef );

                    // Inject the def
                    if( helpDef != null )
                    {
                        helpDefs.Add( helpDef );
                    }
                }

            }
        }

        static void                         ResolveResearch()
        {
            // Get research database
            var researchProjectDefs =
                DefDatabase< ResearchProjectDef >.AllDefsListForReading.Where( r => (
                    ( !r.IsLockedOut() )
                ) ).ToList();

            if( ( researchProjectDefs == null )||
                ( researchProjectDefs.Count == 0 ) )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.ResearchHelp, "Projects", "Research" );

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var researchProjectDef in researchProjectDefs )
            {
                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == researchProjectDef.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForResearch( researchProjectDef, helpCategoryDef );

                    // Inject the def
                    if( helpDef != null )
                    {
                        helpDefs.Add( helpDef );
                    }
                }

            }
        }

        static void                         ResolveAdvancedResearch()
        {
            // Get advanced research database
            var advancedResearchDefs =
                ResearchController.AdvancedResearch.Where( a => (
                    ( a.HasHelp )
                ) ).ToList();

            if( ( advancedResearchDefs == null )||
                ( advancedResearchDefs.Count == 0 ) )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.AdvancedResearchHelp, "Advanced", "Research" );

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var advancedResearchDef in advancedResearchDefs )
            {
                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == advancedResearchDef.defName )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    helpDef = HelpForAdvancedResearch( advancedResearchDef, helpCategoryDef );

                    // Inject the def
                    if( helpDef != null )
                    {
                        helpDefs.Add( helpDef );
                    }
                }

            }
        }

        #endregion


        #region Help Makers

        static HelpCategoryDef              HelpCategoryForKey( string key, string label, string modname )
        {
            // Get help category
            var helpCategoryDef = DefDatabase< HelpCategoryDef >.GetNamed( key, false );

            if( helpCategoryDef == null )
            {
                // Create new designation help category
                helpCategoryDef = new HelpCategoryDef();
                helpCategoryDef.defName = key;
                helpCategoryDef.keyDef = key;
                helpCategoryDef.label = label;
                helpCategoryDef.ModName = modname;

                DefDatabase< HelpCategoryDef >.Add( helpCategoryDef );
            }

            return helpCategoryDef;
        }

        static HelpDef                      HelpForBuildable( BuildableDef buildableDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = buildableDef.defName + "_BuildableDef_Help";
            helpDef.keyDef = buildableDef.defName;
            helpDef.label = buildableDef.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( buildableDef.description );
            s.AppendLine();

            // Look at base stats
            foreach( var stat in buildableDef.statBases )
            {
                s.Append( stat.stat.LabelCap );
                s.Append( " : " );
                s.AppendLine( stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
            }
            s.AppendLine();

            var t = buildableDef as ThingDef;
            if( t != null )
            {

                // What stuff can it be made from?
                if( t.costStuffCount > 0 )
                {
                    s.Append( "Stuff Cost : " );
                    s.AppendLine( t.costStuffCount.ToString() );
                    BuildDefDescription( s, "Stuff made from:", t.stuffCategories.ConvertAll<Def>( def => (Def)def ) );
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

                // Get list of recipes
                var recipes = t.AllRecipes;
                BuildDefDescription( s, "Recipes:", recipes.ConvertAll<Def>( def => (Def)def ) );

                // Add list of required research
                var researchRequired = buildableDef.GetResearchRequirements();
                BuildDefDescription( s, "Required Research:", researchRequired.ConvertAll<Def>( def => (Def)def ) );

                // Build help for unlocked recipes associated with building
                t.GetRecipesByResearchUnlocked( ref recipes, ref researchRequired );
                BuildDefWithDefDescription( s, "Recipes Unlocked:", "By research:", recipes.ConvertAll<Def>( def => (Def)def ), researchRequired.ConvertAll<Def>( def => (Def)def ) );

                // Build help for locked recipes associated with building
                t.GetRecipesByResearchLocked( ref recipes, ref researchRequired );
                BuildDefWithDefDescription( s, "Recipes Locked:", "By research:", recipes.ConvertAll<Def>( def => (Def)def ), researchRequired.ConvertAll<Def>( def => (Def)def ) );

                // Get list of facilities that effect it
                var affectedBy = t.GetCompProperties( typeof( CompAffectedByFacilities ) );
                if( ( affectedBy != null )&&
                    ( affectedBy.linkableFacilities != null )&&
                    ( affectedBy.linkableFacilities.Count > 0 ) )
                {
                    BuildDefDescription( s, "Affected by facilities:", affectedBy.linkableFacilities.ConvertAll<Def>( def => (Def)def ) );
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
                        var facilityProperties = t.GetCompProperties( typeof( CompFacility ) );
                        s.Append( "Maximum affected buildings : " );
                        s.AppendLine( facilityProperties.maxSimultaneous.ToString() );
                        // Look at stats modifiers
                        foreach( var stat in facilityProperties.statOffsets )
                        {
                            s.Append( stat.stat.LabelCap );
                            s.Append( " : " );
                            s.AppendLine( stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
                        }
                        s.AppendLine();
                        BuildDefDescription( s, "Buildings affected:", effectsBuildings.ConvertAll<Def>( def => (Def)def ) );
                    }
                }


            }

            helpDef.description = s.ToString();
            return helpDef;
        }

        static HelpDef                      HelpForRecipe( RecipeDef recipeDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = recipeDef.defName + "_RecipeDef_Help";
            helpDef.keyDef = recipeDef.defName;
            helpDef.label = recipeDef.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( recipeDef.description );
            s.AppendLine();

            s.AppendLine( "WorkAmount".Translate() + " : " + GenText.ToStringWorkAmount( recipeDef.WorkAmountTotal( (ThingDef) null ) ) );
            s.AppendLine();

            if( ( recipeDef.skillRequirements != null )&&
                ( recipeDef.skillRequirements.Count > 0 ) )
            {
                s.AppendLine( "MinimumSkills".Translate() );
                foreach( var sr in recipeDef.skillRequirements )
                {
                    s.Append( "\t" );
                    s.AppendLine( Translator.Translate( "BillRequires", new object[] { sr.minLevel.ToString( "####0" ), sr.skill.label.ToLower() } ) );
                }
                s.AppendLine();
            }

            // List of ingredients
            if( ( recipeDef.ingredients != null )&&
                ( recipeDef.ingredients.Count > 0 ) )
            {
                s.AppendLine( "Requires ingredients:" );
                foreach( var ing in recipeDef.ingredients )
                {
                    if( !GenText.NullOrEmpty( ing.filter.Summary ) )
                    {
                        s.Append( "\t" );
                        s.AppendLine( recipeDef.IngredientValueGetter.BillRequirementsDescription( ing ) );
                    }
                }
                s.AppendLine();
            }

            // List of products
            if( ( recipeDef.products != null )&&
                ( recipeDef.products.Count > 0 ) )
            {
                s.AppendLine( "Produces:" );
                foreach( var ing in recipeDef.products )
                {
                    s.Append( "\t" );
                    s.Append( ing.thingDef.LabelCap );
                    s.Append( " : " );
                    s.AppendLine( ing.count.ToString() );
                }
                s.AppendLine();
            }

            // Add things it's on
            var thingsOn = recipeDef.GetThingsCurrent();
            BuildDefDescription( s, "Applies to:", thingsOn.ConvertAll<Def>( def => (Def)def ) );

            // Add research required
            var researchRequired = recipeDef.GetResearchRequirements();
            BuildDefDescription( s, "Required research:", researchRequired );

            // What things is it on after research
            recipeDef.GetThingsByResearchUnlocked( ref thingsOn, ref researchRequired );
            BuildDefWithDefDescription( s, "Available on:", "After researching:", thingsOn.ConvertAll<Def>( def => (Def)def ), researchRequired.ConvertAll<Def>( def => (Def)def ) );

            // Get research which locks recipe
            recipeDef.GetThingsByResearchLocked( ref thingsOn, ref researchRequired );
            BuildDefWithDefDescription( s, "Removed from:", "After researching:", thingsOn.ConvertAll<Def>( def => (Def)def ), researchRequired.ConvertAll<Def>( def => (Def)def ) );

            helpDef.description = s.ToString();
            return helpDef;
        }

        static HelpDef                      HelpForResearch( ResearchProjectDef researchProjectDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = researchProjectDef.defName + "_ResearchProjectDef_Help";
            helpDef.keyDef = researchProjectDef.defName;
            helpDef.label = researchProjectDef.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( researchProjectDef.description );
            s.AppendLine();

            s.Append( "Total Cost : " );
            s.AppendLine( researchProjectDef.totalCost.ToString() );
            s.AppendLine();

            // Add research required
            var researchRequired = researchProjectDef.GetResearchRequirements();
            BuildDefDescription( s, "Required research:", researchRequired.ConvertAll<Def>( def =>(Def)def ) );

            // Add buildings it unlocks
            var thingsOn = researchProjectDef.GetBuildsUnlocked();
            BuildDefDescription( s, "Allows construction of:", thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Add recipes it unlocks
            List< RecipeDef > recipes = null;
            researchProjectDef.GetRecipesOnBuildingsUnlocked( ref recipes, ref thingsOn );
            BuildDefWithDefDescription( s, "Allows recipes:", "On:", recipes.ConvertAll<Def>( def =>(Def)def ), thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Look in advanced research to add plants and sow tags it unlocks
            List< string > sowTags = null;
            researchProjectDef.GetSowTagsOnPlantsUnlocked( ref sowTags, ref thingsOn );
            BuildDefWithStringDescription( s, "Allows planting:", "In:", thingsOn.ConvertAll<Def>( def =>(Def)def ), sowTags );

            // Get advanced research which locks
            researchRequired = researchProjectDef.GetResearchedLockedBy();
            BuildDefDescription( s, "Hidden by research:", researchRequired.ConvertAll<Def>( def =>(Def)def ) );

            helpDef.description = s.ToString();
            return helpDef;
        }

        static HelpDef                      HelpForAdvancedResearch( AdvancedResearchDef advancedResearchDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = advancedResearchDef.defName + "_AdvancedResearchDef_Help";
            helpDef.keyDef = advancedResearchDef.defName;
            helpDef.label = advancedResearchDef.label;
            if( advancedResearchDef.helpCategoryDef == null )
            {
                advancedResearchDef.helpCategoryDef = category;
            }
            if( advancedResearchDef.IsHelpEnabled )
            {
                helpDef.category = advancedResearchDef.helpCategoryDef;
            }

            var s = new StringBuilder();

            s.AppendLine( advancedResearchDef.description );
            s.AppendLine();

            s.Append( "Total Cost : " );
            s.AppendLine( advancedResearchDef.TotalCost.ToString() );
            s.AppendLine();

            // Add research required
            var researchRequired = advancedResearchDef.GetResearchRequirements();
            BuildDefDescription( s, "Required research:", researchRequired.ConvertAll<Def>( def =>(Def)def ) );

            // Add buildings it unlocks
            var thingsOn = advancedResearchDef.GetBuildsUnlocked();
            BuildDefDescription( s, "Allows construction of:", thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Add recipes it unlocks
            List< RecipeDef > recipes = null;
            advancedResearchDef.GetRecipesOnBuildingsUnlocked( ref recipes, ref thingsOn );
            BuildDefWithDefDescription( s, "Allows recipes:", "On:", recipes.ConvertAll<Def>( def =>(Def)def ), thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Add plants and sow tags it unlocks
            List< string > sowTags = null;
            advancedResearchDef.GetSowTagsOnPlantsUnlocked( ref sowTags, ref thingsOn );
            BuildDefWithStringDescription( s, "Allows planting:", "In:", thingsOn.ConvertAll<Def>( def =>(Def)def ), sowTags );

            // Add buildings it locks
            thingsOn = advancedResearchDef.GetBuildsLocked();
            BuildDefDescription( s, "Prevents construction of:", thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Add recipes it locks
            advancedResearchDef.GetRecipesOnBuildingsLocked( ref recipes, ref thingsOn );
            BuildDefWithDefDescription( s, "Prevents recipes:", "On:", recipes.ConvertAll<Def>( def =>(Def)def ), thingsOn.ConvertAll<Def>( def =>(Def)def ) );

            // Add plants and sow tags it locks
            advancedResearchDef.GetSowTagsOnPlantsLocked( ref sowTags, ref thingsOn );
            BuildDefWithStringDescription( s, "Prevents planting:", "In:", thingsOn.ConvertAll<Def>( def =>(Def)def ), sowTags );

            helpDef.description = s.ToString();
            advancedResearchDef.HelpDef = helpDef;
            return helpDef;
        }

        #endregion

        #region Array Cleaners

        static List< Def >                  TidyDefs( List< Def > defs )
        {
            if( defs == null )
            {
                return null;
            }
            for( int i = 0; i < defs.Count; ++i )
            {
                if( defs[ i ] == null )
                {
                    defs.RemoveAt( i );
                    --i;
                }
            }
            if( defs.Count == 0 )
            {
                return null;
            }
            return defs;
        }

        static List< string >               TidyStrings( List< string > strings )
        {
            if( strings == null )
            {
                return null;
            }
            for( int i = 0; i < strings.Count; ++i )
            {
                if( strings[ i ] == null )
                {
                    strings.RemoveAt( i );
                    --i;
                }
            }
            if( strings.Count == 0 )
            {
                return null;
            }
            return strings;
        }

        #endregion

        #region String Builders

        static bool BuildDefDescription( StringBuilder s, string prependDefs, List< Def > defs )
        {
            defs = TidyDefs( defs );
            if( defs == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependDefs );

            labels.Clear();
            for( int i = 0, count = defs.Count - 1; i <= count; i++ ){
                var d = defs[ i ];
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

        static bool BuildStringDescription( StringBuilder s, string prependStrings, List< string > strings )
        {
            strings = TidyStrings( strings );
            if( strings == null )
            {
                return false;
            }

            var labels = new List< string >();

            s.AppendLine( prependStrings );
            labels.Clear();
            for( int i = 0, count = strings.Count - 1; i <= count; i++ ){
                var d = strings[ i ];
                if( !labels.Contains( d.ToLower() ) )
                {
                    labels.Add( d.ToLower() );
                    s.Append( "\t" );
                    s.AppendLine( d );
                }
            }
            s.AppendLine();

            return true;
        }

        static void BuildDefWithDefDescription( StringBuilder s, string prependDef1, string prependDef2, List< Def > defs1, List< Def > defs2 )
        {
            if( BuildDefDescription( s, prependDef1, defs1 ) )
            {
                BuildDefDescription( s, prependDef2, defs2 );
            }
        }

        static void BuildDefWithStringDescription( StringBuilder s, string prependDefs, string prependStrings, List< Def > defs, List< string > strings )
        {
            if( BuildDefDescription( s, prependDefs, defs ) )
            {
                BuildStringDescription( s, prependStrings, strings );
            }
        }

        static void BuildStringWithDefDescription( StringBuilder s, string prependStrings, string prependDefs, List< string > strings, List< Def > defs )
        {
            if( BuildStringDescription( s, prependStrings, strings ) )
            {
                BuildDefDescription( s, prependDefs, defs );
            }
        }

        #endregion

    }

}
