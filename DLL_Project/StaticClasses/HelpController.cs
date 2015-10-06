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
            // Items
            ResolveApparel();
            ResolveBodyParts();
            ResolveDrugs();
            ResolveMeals();
            ResolveWeapons();

            // Buildings
            ResolveBuildings();
            ResolveMinifiableOnly();

            // Recipes
            ResolveRecipes();

            // Research
            ResolveResearch();
            ResolveAdvancedResearch();

            // Rebuild help caches
            ResolveReferences();

            CCL_Log.Message( "Initialized", "Help System" );
        }

        static void                         ResolveReferences()
        {
            foreach( var helpCategory in DefDatabase< HelpCategoryDef >.AllDefsListForReading )
            {
                helpCategory.Recache();
            }
            MainTabWindow_ModHelp.Recache();
        }

        #endregion

        #region Item Resolvers

        static void                         ResolveApparel()
        {
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( Apparel ) )
                ) ).ToList();

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.ApparelHelp, "AutoHelpSubCategoryApparel".Translate(), "AutoHelpCategoryItems".Translate() );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void                         ResolveBodyParts()
        {
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( ThingWithComps ) )&&
                    (
                        ( !t.thingCategories.NullOrEmpty() )&&
                        ( t.thingCategories.Contains( ThingCategoryDefOf.BodyPartsAndImplants ) )
                    )
                ) ).ToList();

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.BodyPartHelp, "AutoHelpSubCategoryBodyParts".Translate(), "AutoHelpCategoryItems".Translate() );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void                         ResolveDrugs()
        {
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( Meal ) )&&
                    ( t.ingestible.isPleasureDrug )
                ) ).ToList();

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.DrugHelp, "AutoHelpSubCategoryDrugs".Translate(), "AutoHelpCategoryItems".Translate() );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void                         ResolveMeals()
        {
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.thingClass == typeof( Meal ) )&&
                    ( !t.ingestible.isPleasureDrug )
                ) ).ToList();

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.MealHelp, "AutoHelpSubCategoryMeals".Translate(), "AutoHelpCategoryItems".Translate() );

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

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.WeaponHelp, "AutoHelpSubCategoryWeapons".Translate(), "AutoHelpCategoryItems".Translate() );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        #endregion

        #region Building Resolvers

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

                if( !thingDefs.NullOrEmpty() )
                {
                    // Get help category
                    var helpCategoryDef = HelpCategoryForKey( designationCategoryDef.defName + "_Building" + HelpCategoryDefOf.HelpPostFix, designationCategoryDef.label, "AutoHelpCategoryBuildings".Translate() );

                    // Scan through all possible buildable defs and auto-generate help
                    ResolveThingDefList(
                        thingDefs,
                        helpCategoryDef
                    );
                }
            }
        }

        static void                         ResolveMinifiableOnly()
        {
            // Get list of things
            var thingDefs = 
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.Minifiable )&&
                    (
                        (
                            ( t.designationCategory.NullOrEmpty() )||
                            ( t.designationCategory == "None" )
                        )||
                        ( t.IsLockedOut() )
                    )
                ) ).ToList();

            if( thingDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( "Special_Building" + HelpCategoryDefOf.HelpPostFix, "AutoHelpSubCategorySpecial".Translate(), "AutoHelpCategoryBuildings".Translate() );

            // Scan through all possible buildable defs and auto-generate help
            ResolveThingDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        #endregion

        #region ThingDef Resolver

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
                    //Log.Message( "HelpGen :: " + thingDef.defName );
                    helpDef = HelpForBuildable( thingDef, category );

                    // Inject the def
                    if( helpDef != null )
                    {
                        DefDatabase< HelpDef >.Add( helpDef );
                    }
                }

            }
        }

        #endregion

        #region Recipe Resolvers

        static void                         ResolveRecipes()
        {
            // Get the thing database of things which ever have recipes
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( !t.IsLockedOut() )&&
                    ( t.EverHasRecipes() )&&
                    ( t.thingClass != typeof( Corpse ) )
                ) ).ToList();

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var thingDef in thingDefs )
            {
                // Get help category
                var helpCategoryDef = HelpCategoryForKey( thingDef.defName + "_Recipe" + HelpCategoryDefOf.HelpPostFix, thingDef.label, "AutoHelpCategoryRecipes".Translate() );

                var recipeDefs = thingDef.GetRecipesAll();
                foreach( var recipeDef in recipeDefs )
                {
                    // Find an existing entry
                    var helpDef = helpDefs.Find( h => (
                        ( h.keyDef == thingDef.defName + "_" + recipeDef.defName )
                    ) );

                    if( helpDef == null )
                    {
                        // Make a new one
                        //Log.Message( "HelpGen :: " + recipeDef.defName );
                        helpDef = HelpForRecipe( thingDef, recipeDef, helpCategoryDef );

                        // Inject the def
                        if( helpDef != null )
                        {
                            helpDefs.Add( helpDef );
                        }
                    }
                }
            }
        }

        #endregion

        #region Research Resolvers

        static void                         ResolveResearch()
        {
            // Get research database
            var researchProjectDefs =
                DefDatabase< ResearchProjectDef >.AllDefsListForReading.Where( r => (
                    ( !r.IsLockedOut() )
                ) ).ToList();

            if( researchProjectDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.ResearchHelp, "AutoHelpSubCategoryProjects".Translate(), "AutoHelpCategoryResearch".Translate() );

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
                    //Log.Message( "HelpGen :: " + researchProjectDef.defName );
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
                    ( a.ResearchConsolidator == a )&&
                    ( a.HasHelp )
                ) ).ToList();

            if( advancedResearchDefs.NullOrEmpty() )
            {
                return;
            }

            // Get help category
            var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.AdvancedResearchHelp, "AutoHelpSubCategoryAdvanced".Translate(), "AutoHelpCategoryResearch".Translate() );

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
                    //Log.Message( "HelpGen :: " + advancedResearchDef.defName );
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

            #region Base Stats

            // Look at base stats
            if( !buildableDef.statBases.NullOrEmpty() )
            {
                foreach( var stat in buildableDef.statBases )
                {
                    s.Append( stat.stat.LabelCap );
                    s.Append( " : " );
                    s.AppendLine( stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
                }
                s.AppendLine();
            }
            #endregion

            #region ThingDef Specific

            var thingDef = buildableDef as ThingDef;
            if( thingDef != null )
            {

                #region Ingestible Stats

                // Look at base stats
                if( thingDef.IsNutritionSource )
                {
                    if( thingDef.ingestible.nutrition > 0.0001f )
                    {
                        s.Append( "Nutrition".Translate() );
                        s.Append( " : " );
                        s.AppendLine( thingDef.ingestible.nutrition.ToString( "0.###" ) );
                    }
                    if( thingDef.ingestible.joy > 0.0001f )
                    {
                        s.Append( "Joy".Translate() );
                        s.Append( " : " );
                        s.AppendLine( thingDef.ingestible.joy.ToString( "0.###" ) );
                    }
                    s.AppendLine();
                }

                #endregion

                #region Body Part Stats

                if( ( !thingDef.thingCategories.NullOrEmpty() )&&
                    ( thingDef.thingCategories.Contains( ThingCategoryDefOf.BodyPartsAndImplants ) )&&
                    ( thingDef.IsImplant() ) )
                {
                    var hediffDef = thingDef.GetImplantHediffDef();

                    #region Efficiency

                    if( hediffDef.addedPartProps != null )
                    {
                        s.Append( "BodyPartEfficiency".Translate() );
                        s.Append( " : " );
                        s.AppendLine( hediffDef.addedPartProps.partEfficiency.ToString( "P0" ) );
                        s.AppendLine();
                    }

                    #endregion

                    #region Capacities

                    if( ( !hediffDef.stages.NullOrEmpty() )&&
                        ( hediffDef.stages.Exists( stage => (
                            ( !stage.capMods.NullOrEmpty() )
                        ) ) )
                    )
                    {
                        foreach( var hediffStage in hediffDef.stages )
                        {
                            if( !hediffStage.capMods.NullOrEmpty() )
                            {
                                foreach( var c in hediffStage.capMods )
                                {
                                    s.Append( c.capacity.LabelCap );
                                    if( c.offset > 0 )
                                    {
                                        s.Append( " : +" );
                                    }
                                    else
                                    {
                                        s.Append( " : " );
                                    }
                                    s.AppendLine( c.offset.ToString( "P0" ) );
                                }
                            }
                        }
                        s.AppendLine();
                    }

                    #endregion

                    #region Components (Melee attack)

                    if( ( !hediffDef.comps.NullOrEmpty() )&&
                        ( hediffDef.comps.Exists( p => (
                            ( p.compClass == typeof( HediffComp_VerbGiver ) )
                        ) ) )
                    )
                    {
                        foreach( var comp in hediffDef.comps )
                        {
                            if( comp.compClass == typeof( HediffComp_VerbGiver ) )
                            {
                                if( !comp.verbs.NullOrEmpty() )
                                {
                                    foreach( var verb in comp.verbs )
                                    {
                                        if( verb.verbClass == typeof( Verb_MeleeAttack ) )
                                        {
                                            s.AppendLine( "MeleeAttack".Translate( verb.meleeDamageDef.label ) );
                                            s.Append( "\t" );
                                            s.Append( "MeleeWarmupTime".Translate() );
                                            s.Append( " : " );
                                            s.AppendLine( verb.defaultCooldownTicks.ToString() );
                                            s.Append( "\t" );
                                            s.Append( "StatsReport_MeleeDamage".Translate() );
                                            s.Append( " : " );
                                            s.AppendLine( verb.meleeDamageBaseAmount.ToString() );
                                            s.AppendLine();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Body part fixed or replaced

                    var recipeDef = thingDef.GetImplantRecipeDef();
                    if( !recipeDef.appliedOnFixedBodyParts.NullOrEmpty() )
                    {
                        s.Append( "AutoHelpSurgeryFixOrReplace".Translate() );
                        s.AppendLine( ":" );
                        foreach( var b in recipeDef.appliedOnFixedBodyParts )
                        {
                            s.Append( "\t" );
                            s.AppendLine( b.LabelCap );
                        }
                        s.AppendLine();
                    }

                    #endregion

                }

                #endregion

                #region Stuff Cost

                // What stuff can it be made from?
                if(
                    ( thingDef.costStuffCount > 0 )&&
                    ( !thingDef.stuffCategories.NullOrEmpty() )
                )
                {
                    s.AppendLine( "AutoHelpStuffCost".Translate( thingDef.costStuffCount.ToString() ) );
                    BuildDefDescription( s, "AutoHelpListStuffCategories".Translate(), thingDef.stuffCategories.ConvertAll<Def>( def => (Def)def ) );
                }

                #endregion

                #region Cost List

                // What other things are required?
                if( !thingDef.costList.NullOrEmpty() )
                {
                    s.Append( "AutoHelpThingCost".Translate() );
                    s.AppendLine( ":" );
                    foreach( var tc in thingDef.costList )
                    {
                        s.Append( "\t" );
                        s.Append( tc.thingDef.LabelCap );
                        s.Append( " : " );
                        s.AppendLine( tc.count.ToString() );
                    }
                    s.AppendLine();
                }

                #endregion

                #region Recipes & Research

                // Get list of recipes
                var recipeDefs = thingDef.AllRecipes;
                if( !recipeDefs.NullOrEmpty() )
                {
                    BuildDefDescription( s, "AutoHelpListRecipes".Translate(), recipeDefs.ConvertAll<Def>( def => (Def)def ) );
                }

                // Add list of required research
                var researchDefs = buildableDef.GetResearchRequirements();
                if( !researchDefs.NullOrEmpty() )
                {
                    BuildDefDescription( s, "AutoHelpListResearchRequired".Translate(), researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }

                // Build help for unlocked recipes associated with building
                recipeDefs = thingDef.GetRecipesUnlocked( ref researchDefs );
                if(
                    ( !recipeDefs.NullOrEmpty() )&&
                    ( !researchDefs.NullOrEmpty() )
                )
                {
                    BuildDefDescription( s, "AutoHelpListRecipesUnlocked".Translate(), recipeDefs.ConvertAll<Def>( def => (Def)def ) );
                    BuildDefDescription( s, "AutoHelpListResearchBy".Translate(), researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }

                // Build help for locked recipes associated with building
                recipeDefs = thingDef.GetRecipesLocked( ref researchDefs );
                if(
                    ( !recipeDefs.NullOrEmpty() )&&
                    ( !researchDefs.NullOrEmpty() )
                )
                {
                    BuildDefDescription( s, "AutoHelpListRecipesLocked".Translate(), recipeDefs.ConvertAll<Def>( def => (Def)def ) );
                    BuildDefDescription( s, "AutoHelpListResearchBy".Translate(), researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }

                #endregion

                #region Facilities

                // Get list of facilities that effect it
                var affectedBy = thingDef.GetCompProperties( typeof( CompAffectedByFacilities ) );
                if( ( affectedBy != null )&&
                    ( !affectedBy.linkableFacilities.NullOrEmpty() ) )
                {
                    BuildDefDescription( s, "AutoHelpListFacilitiesAffecting".Translate(), affectedBy.linkableFacilities.ConvertAll<Def>( def => (Def)def ) );
                }

                // Get list of buildings effected by it
                if( thingDef.HasComp( typeof( CompFacility ) ) )
                {
                    var effectsBuildings = DefDatabase< ThingDef >.AllDefsListForReading
                        .Where( f => (
                            ( f.HasComp( typeof( CompAffectedByFacilities ) ) )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ) != null )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ).linkableFacilities != null )&&
                            ( f.GetCompProperties( typeof( CompAffectedByFacilities ) ).linkableFacilities.Contains( thingDef ) )
                        ) ).ToList();
                    if( !effectsBuildings.NullOrEmpty() )
                    {
                        var facilityProperties = thingDef.GetCompProperties( typeof( CompFacility ) );

                        s.AppendLine( "AutoHelpMaximumAffected".Translate( facilityProperties.maxSimultaneous.ToString() ) );

                        // Look at stats modifiers
                        foreach( var stat in facilityProperties.statOffsets )
                        {
                            s.Append( stat.stat.LabelCap );
                            s.Append( " : " );
                            s.AppendLine( stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
                        }
                        s.AppendLine();

                        BuildDefDescription( s, "AutoHelpListFacilitiesAffected".Translate(), effectsBuildings.ConvertAll<Def>( def => (Def)def ) );
                    }
                }

                #endregion

                #region Joy

                // Get valid joy givers
                var joyGiverDefs = DefDatabase< JoyGiverDef >.AllDefsListForReading
                    .Where( j => (
                        ( j.thingDef == thingDef )&&
                        ( j.jobDef != null )
                    ) ).ToList();

                if( !joyGiverDefs.NullOrEmpty() )
                {
                    s.AppendLine( "AutoHelpListJoyActivities".Translate() );
                    foreach( var joyGiverDef in joyGiverDefs )
                    {
                        // Get job driver stats
                        s.Append( "\t" );
                        s.AppendLine( joyGiverDef.jobDef.reportString );
                        s.Append( "\t" );
                        s.AppendLine( "AutoHelpMaximumParticipants".Translate( joyGiverDef.jobDef.joyMaxParticipants.ToString() ) );
                        s.Append( "\t" );
                        s.AppendLine( "AutoHelpJoyKind".Translate( joyGiverDef.jobDef.joyKind.LabelCap ) );

                        if( joyGiverDef.jobDef.joySkill != null )
                        {
                            s.Append( "\t" );
                            s.AppendLine( "AutoHelpJoySkill".Translate( joyGiverDef.jobDef.joySkill.LabelCap ) );
                        }
                    }
                    s.AppendLine();
                }

                #endregion

            }

            #endregion

            helpDef.description = s.ToString();
            return helpDef;
        }

        static HelpDef                      HelpForRecipe( ThingDef thingDef, RecipeDef recipeDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.keyDef = thingDef.defName +"_" + recipeDef.defName;
            helpDef.defName = helpDef.keyDef + "_RecipeDef_Help";
            helpDef.label = recipeDef.label;
            helpDef.category = category;

            var s = new StringBuilder();

            s.AppendLine( recipeDef.description );
            s.AppendLine();

            #region Base Stats

            s.AppendLine( "WorkAmount".Translate() + " : " + GenText.ToStringWorkAmount( recipeDef.WorkAmountTotal( (ThingDef) null ) ) );
            s.AppendLine();

            #endregion

            #region Skill Requirements

            if( !recipeDef.skillRequirements.NullOrEmpty() )
            {
                s.AppendLine( "MinimumSkills".Translate() );
                foreach( var sr in recipeDef.skillRequirements )
                {
                    s.Append( "\t" );
                    s.AppendLine( "BillRequires".Translate( new object[] {
                        sr.minLevel.ToString( "####0" ),
                        sr.skill.label.ToLower()
                    } ) );
                }
                s.AppendLine();
            }

            #endregion

            #region Ingredients

            // List of ingredients
            if( !recipeDef.ingredients.NullOrEmpty() )
            {
                s.Append( "Ingredients".Translate() );
                s.AppendLine( ":" );
                foreach( var ing in recipeDef.ingredients )
                {
                    if( !ing.filter.Summary.NullOrEmpty() )
                    {
                        s.Append( "\t" );
                        s.AppendLine( recipeDef.IngredientValueGetter.BillRequirementsDescription( ing ) );
                    }
                }
                s.AppendLine();
            }

            #endregion

            #region Products

            // List of products
            if( !recipeDef.products.NullOrEmpty() )
            {
                s.AppendLine( "AutoHelpListRecipeProducts".Translate() );
                foreach( var ing in recipeDef.products )
                {
                    s.Append( "\t" );
                    s.Append( ing.thingDef.LabelCap );
                    s.Append( " : " );
                    s.AppendLine( ing.count.ToString() );
                }
                s.AppendLine();
            }

            #endregion

            #region Things & Research

            // Add things it's on
            var thingDefs = recipeDef.GetThingsCurrent();
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListRecipesOnThings".Translate(), thingDefs.ConvertAll<Def>( def => (Def)def ) );
            }

            // Add research required
            var researchDefs = recipeDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListResearchRequired".Translate(), researchDefs );
            }

            // What things is it on after research
            thingDefs = recipeDef.GetThingsUnlocked( ref researchDefs );
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListRecipesOnThingsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def => (Def)def ) );
                if( !researchDefs.NullOrEmpty() )
                {
                    BuildDefDescription( s, "AutoHelpListResearchBy".Translate(), researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }
            }

            // Get research which locks recipe
            thingDefs = recipeDef.GetThingsLocked( ref researchDefs );
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListRecipesOnThingsLocked".Translate(), thingDefs.ConvertAll<Def>( def => (Def)def ) );
                if( !researchDefs.NullOrEmpty() )
                {
                    BuildDefDescription( s, "AutoHelpListResearchBy".Translate(), researchDefs.ConvertAll<Def>( def => (Def)def ) );
                }
            }

            #endregion

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

            #region Base Stats

            s.AppendLine( "AutoHelpTotalCost".Translate( researchProjectDef.totalCost.ToString() ) );
            s.AppendLine();

            #endregion

            #region Research, Buildings, Recipes and SowTags

            // Add research required
            var researchDefs = researchProjectDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListResearchRequired".Translate(), researchDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add buildings it unlocks
            var thingDefs = researchProjectDef.GetThingsUnlocked();
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListThingsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add recipes it unlocks
            var recipeDefs = researchProjectDef.GetRecipesUnlocked( ref thingDefs );
            if(
                ( !recipeDefs.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "AutoHelpListRecipesUnlocked".Translate(), recipeDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildDefDescription( s, "AutoHelpListRecipesOnThingsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var sowTags = researchProjectDef.GetSowTagsUnlocked( ref thingDefs );
            if(
                ( !sowTags.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "AutoHelpListPlantsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildStringDescription( s, "AutoHelpListPlantsIn".Translate(), sowTags );
            }

            #endregion

            #region Lockouts

            // Get advanced research which locks
            researchDefs = researchProjectDef.GetResearchedLockedBy();
            if( !researchDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListResearchLockout".Translate(), researchDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            #endregion

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

            #region Base Stats

            s.AppendLine( "AutoHelpTotalCost".Translate( advancedResearchDef.TotalCost.ToString() ) );
            s.AppendLine();

            #endregion

            #region Research, Buildings, Recipes and SowTags

            // Add research required
            var researchDefs = advancedResearchDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListResearchRequired".Translate(), researchDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add buildings it unlocks
            var thingDefs = advancedResearchDef.GetThingsUnlocked();
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListThingsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add recipes it unlocks
            var recipeDefs = advancedResearchDef.GetRecipesUnlocked( ref thingDefs );
            if(
                ( !recipeDefs.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "AutoHelpListRecipesUnlocked".Translate(), recipeDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildDefDescription( s, "AutoHelpListRecipesOnThings".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add plants and sow tags it unlocks
            var sowTags = advancedResearchDef.GetSowTagsUnlocked( ref thingDefs );
            if(
                ( !sowTags.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "AutoHelpListPlantsUnlocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildStringDescription( s, "AutoHelpListPlantsIn".Translate(), sowTags );
            }

            #endregion

            #region Lockouts

            // Add buildings it locks
            thingDefs = advancedResearchDef.GetThingsLocked();
            if( !thingDefs.NullOrEmpty() )
            {
                BuildDefDescription( s, "AutoHelpListThingsLocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add recipes it locks
            recipeDefs = advancedResearchDef.GetRecipesLocked( ref thingDefs );
            if(
                ( !recipeDefs.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "Prevents recipes:", recipeDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildDefDescription( s, "AutoHelpListRecipesOnThings".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
            }

            // Add plants and sow tags it locks
            sowTags = advancedResearchDef.GetSowTagsLocked( ref thingDefs );
            if(
                ( !sowTags.NullOrEmpty() )&&
                ( !thingDefs.NullOrEmpty() )
            )
            {
                BuildDefDescription( s, "AutoHelpListPlantsLocked".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );
                BuildStringDescription( s, "AutoHelpListPlantsIn".Translate(), sowTags );
            }

            #endregion

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

        static void BuildDefDescription( StringBuilder baseDescription, string prependDefs, List< Def > defs )
        {
            defs = TidyDefs( defs );
            if( defs.NullOrEmpty() )
            {
                return;
            }

            var labels = new List< string >();
            var s = new StringBuilder();

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

            //Log.Message( s.ToString() );
            baseDescription.Append( s.ToString() );
        }

        static void BuildStringDescription( StringBuilder baseDescription, string prependStrings, List< string > strings )
        {
            strings = TidyStrings( strings );
            if( strings.NullOrEmpty() )
            {
                return;
            }

            var labels = new List< string >();
            var s = new StringBuilder();

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

            //Log.Message( s.ToString() );
            baseDescription.Append( s.ToString() );
        }

        /*
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
        */

        #endregion

    }

}
