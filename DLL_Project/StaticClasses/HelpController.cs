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

        public static void Initialize()
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

        static void ResolveReferences()
        {
            foreach( var helpCategory in DefDatabase<HelpCategoryDef>.AllDefsListForReading )
            {
                helpCategory.Recache();
            }
            MainTabWindow_ModHelp.Recache();
        }

        #endregion

        #region Item Resolvers

        static void ResolveApparel()
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

        static void ResolveBodyParts()
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

        static void ResolveDrugs()
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

        static void ResolveMeals()
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

        static void ResolveWeapons()
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

        static void ResolveBuildings()
        {
            // Go through buildings by designation categories
            foreach( var designationCategoryDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading )
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

        static void ResolveMinifiableOnly()
        {
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.Minifiable )&&
                    (
                        (
                            ( t.designationCategory.NullOrEmpty() )||
                            ( t.designationCategory == "None" )
                        )
                    )&&
                    ( !t.IsLockedOut() )
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

        static void ResolveThingDefList( List<ThingDef> thingDefs, HelpCategoryDef category )
        {
            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var thingDef in thingDefs )
            {
                // Find an existing entry
                var helpDef = helpDefs.Find( h => (
                    ( h.keyDef == thingDef )
                ) );

                if( helpDef == null )
                {
                    // Make a new one
                    //Log.Message( "HelpGen :: " + thingDef.defName );
                    helpDef = HelpForBuildable( thingDef, category );

                    // Inject the def
                    if( helpDef != null )
                    {
                        DefDatabase<HelpDef>.Add( helpDef );
                    }
                }

            }
        }

        #endregion

        #region Recipe Resolvers

        static void ResolveRecipes()
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
                        ( h.keyDef == recipeDef )
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

        static void ResolveResearch()
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
                    ( h.keyDef == researchProjectDef )
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

        static void ResolveAdvancedResearch()
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
                    ( h.keyDef == advancedResearchDef )
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

        static HelpCategoryDef HelpCategoryForKey( string key, string label, string modname )
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

                DefDatabase<HelpCategoryDef>.Add( helpCategoryDef );
            }

            return helpCategoryDef;
        }

        static HelpDef HelpForBuildable( BuildableDef buildableDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = buildableDef.defName + "_BuildableDef_Help";
            helpDef.keyDef = buildableDef;
            helpDef.label = buildableDef.label;
            helpDef.category = category;
            helpDef.description = buildableDef.description;

            #region Base Stats

            // Look at base stats
            HelpDetailSection baseStats = new HelpDetailSection(
                null,
                buildableDef.statBases.Select(sb => sb.stat).ToList().ConvertAll(def => (Def)def),
                null,
                buildableDef.statBases.Select(sb => sb.stat.ValueToString(sb.value, sb.stat.toStringNumberSense)).ToArray());

            helpDef.HelpDetailSections.Add( baseStats );

            #endregion

            #region ThingDef Specific

            var thingDef = buildableDef as ThingDef;
            if( thingDef != null )
            {

                #region Ingestible Stats
                // TODO: Add raw food helpDefs?
                // Look at base stats
                if( thingDef.IsNutritionSource )
                {
                    string[] ingestibleStats =
                    {
                        "Nutrition".Translate() + ": " + thingDef.ingestible.nutrition.ToString( "0.###" ),
                        "Joy".Translate() + ": " + thingDef.ingestible.joy.ToString( "0.###" )
                    };

                    helpDef.HelpDetailSections.Add( new HelpDetailSection( null, ingestibleStats ) );
                }

                #endregion

                #region Body Part Stats

                if( (!thingDef.thingCategories.NullOrEmpty()) &&
                    (thingDef.thingCategories.Contains( ThingCategoryDefOf.BodyPartsAndImplants )) &&
                    (thingDef.IsImplant()) )
                {
                    var hediffDef = thingDef.GetImplantHediffDef();

                    #region Efficiency

                    if( hediffDef.addedPartProps != null )
                    {
                        helpDef.HelpDetailSections.Add( new HelpDetailSection( "BodyPartEfficiency".Translate(), new[] { hediffDef.addedPartProps.partEfficiency.ToString( "P0" ) } ) );
                    }

                    #endregion

                    #region Capacities
                    // TODO: add capacity helpdefs?
                    if( (!hediffDef.stages.NullOrEmpty()) &&
                        (hediffDef.stages.Exists( stage => (
                           (!stage.capMods.NullOrEmpty())
                       ) ))
                    )
                    {
                        HelpDetailSection capacityMods = new HelpDetailSection(
                            "CapacityModifiers".Translate(),
                            hediffDef.stages.Where(s => !s.capMods.NullOrEmpty())
                                            .SelectMany(s => s.capMods)
                                            .Select(cm => cm.capacity)
                                            .ToList()
                                            .ConvertAll(def => (Def)def),
                            null,
                            hediffDef.stages
                                     .Where(s => !s.capMods.NullOrEmpty())
                                     .SelectMany(s => s.capMods)
                                     .Select(
                                        cm => (cm.offset > 0 ? ": +" : ": ") + cm.offset.ToString("P0"))
                                     .ToArray());

                        helpDef.HelpDetailSections.Add( capacityMods );
                    }

                    #endregion

                    #region Components (Melee attack)

                    if( (!hediffDef.comps.NullOrEmpty()) &&
                        (hediffDef.comps.Exists( p => (
                           (p.compClass == typeof( HediffComp_VerbGiver ))
                       ) ))
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
                                            helpDef.HelpDetailSections.Add( new HelpDetailSection(
                                                    "MeleeAttack".Translate( verb.meleeDamageDef.label ),
                                                    new[]
                                                    {
                                                        "MeleeWarmupTime".Translate() + verb.defaultCooldownTicks,
                                                        "StatsReport_MeleeDamage".Translate() + verb.meleeDamageBaseAmount
                                                    }
                                                ) );
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region Body part fixed or replaced
                    // TODO: bodypart helddefs
                    var recipeDef = thingDef.GetImplantRecipeDef();
                    if( !recipeDef.appliedOnFixedBodyParts.NullOrEmpty() )
                    {
                        helpDef.HelpDetailSections.Add( new HelpDetailSection(
                            "AutoHelpSurgeryFixOrReplace".Translate(),
                            recipeDef.appliedOnFixedBodyParts.ToList().ConvertAll( def => (Def)def ) ) );
                    }

                    #endregion

                }

                #endregion

                #region Cost List

                // What other things are required?
                if( !thingDef.costList.NullOrEmpty() )
                {
                    HelpDetailSection costs = new HelpDetailSection(
                        "AutoHelpCost".Translate(),
                        thingDef.costList.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                        null,
                        thingDef.costList.Select(tc => ": " + tc.count.ToString()).ToArray());

                    helpDef.HelpDetailSections.Add( costs );
                }

                #endregion

                #region Stuff Cost

                // What stuff can it be made from?
                if(
                    (thingDef.costStuffCount > 0) &&
                    (!thingDef.stuffCategories.NullOrEmpty())
                )
                {
                    helpDef.HelpDetailSections.Add( new HelpDetailSection(
                        "AutoHelpStuffCost".Translate( thingDef.costStuffCount.ToString() ),
                        thingDef.stuffCategories.ToList().ConvertAll( def => (Def)def ) ) );
                }

                #endregion

                #region Recipes & Research

                // Get list of recipes
                var recipeDefs = thingDef.AllRecipes;
                if( !recipeDefs.NullOrEmpty() )
                {
                    HelpDetailSection recipes = new HelpDetailSection(
                        "AutoHelpListRecipes".Translate(),
                        recipeDefs.ConvertAll(def => (Def)def));
                    helpDef.HelpDetailSections.Add( recipes );
                }

                // Add list of required research
                var researchDefs = buildableDef.GetResearchRequirements();
                if( !researchDefs.NullOrEmpty() )
                {
                    HelpDetailSection reqResearch = new HelpDetailSection(
                        "AutoHelpListResearchRequired".Translate(),
                        researchDefs.ConvertAll(def => (Def)def));
                    helpDef.HelpDetailSections.Add( reqResearch );
                }

                // Build help for unlocked recipes associated with building
                recipeDefs = thingDef.GetRecipesUnlocked( ref researchDefs );
                if(
                    (!recipeDefs.NullOrEmpty()) &&
                    (!researchDefs.NullOrEmpty())
                )
                {
                    HelpDetailSection unlockRecipes = new HelpDetailSection(
                        "AutoHelpListRecipesUnlocked".Translate(),
                        recipeDefs.ConvertAll<Def>(def => (Def)def));
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));
                    helpDef.HelpDetailSections.Add( unlockRecipes );
                    helpDef.HelpDetailSections.Add( researchBy );
                }

                // Build help for locked recipes associated with building
                recipeDefs = thingDef.GetRecipesLocked( ref researchDefs );
                if(
                    (!recipeDefs.NullOrEmpty()) &&
                    (!researchDefs.NullOrEmpty())
                )
                {
                    HelpDetailSection unlockRecipes = new HelpDetailSection(
                        "AutoHelpListRecipesUnlocked".Translate(),
                        recipeDefs.ConvertAll<Def>(def => (Def)def));
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));
                    helpDef.HelpDetailSections.Add( unlockRecipes );
                    helpDef.HelpDetailSections.Add( researchBy );
                }

                #endregion

                #region Facilities

                // Get list of facilities that effect it
                var affectedBy = thingDef.GetCompProperties( typeof( CompAffectedByFacilities ) );
                if( (affectedBy != null) &&
                    (!affectedBy.linkableFacilities.NullOrEmpty()) )
                {
                    HelpDetailSection facilitiesAffecting = new HelpDetailSection(
                        "AutoHelpListFacilitiesAffecting".Translate(),
                        affectedBy.linkableFacilities.ConvertAll<Def>(def => (Def)def));
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

                        List<string> facilityStats = new List<string>();
                        facilityStats.Add(
                            "AutoHelpMaximumAffected".Translate( facilityProperties.maxSimultaneous.ToString() ) );

                        // Look at stats modifiers
                        foreach( var stat in facilityProperties.statOffsets )
                        {
                            facilityStats.Add( stat.stat.LabelCap + ": " + stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) );
                        }

                        HelpDetailSection facilityDetailSection = new HelpDetailSection(
                            "AutoHelpFacilityStats".Translate(),
                            facilityStats.ToArray());

                        HelpDetailSection facilitiesAffected = new HelpDetailSection(
                            "AutoHelpListFacilitiesAffected".Translate(),
                            effectsBuildings.ConvertAll<Def>(def => (Def)def));

                        helpDef.HelpDetailSections.Add( facilityDetailSection );
                        helpDef.HelpDetailSections.Add( facilitiesAffected );
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
                    List<string> joyStats = new List<string>();
                    foreach( var joyGiverDef in joyGiverDefs )
                    {
                        // Get job driver stats
                        joyStats.Add( joyGiverDef.jobDef.reportString );
                        joyStats.Add( "AutoHelpMaximumParticipants".Translate( joyGiverDef.jobDef.joyMaxParticipants.ToString() ) );
                        joyStats.Add( "AutoHelpJoyKind".Translate( joyGiverDef.jobDef.joyKind.LabelCap ) );
                        if( joyGiverDef.jobDef.joySkill != null )
                        {
                            joyStats.Add( "AutoHelpJoySkill".Translate( joyGiverDef.jobDef.joySkill.LabelCap ) );
                        }
                    }

                    HelpDetailSection joyDetailSection = new HelpDetailSection(
                        "AutoHelpListJoyActivities".Translate(),
                        joyStats.ToArray());

                    helpDef.HelpDetailSections.Add( joyDetailSection );
                }

                #endregion

            }

            #endregion

            return helpDef;
        }

        static HelpDef HelpForRecipe( ThingDef thingDef, RecipeDef recipeDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.keyDef = recipeDef;
            helpDef.defName = helpDef.keyDef + "_RecipeDef_Help";
            helpDef.label = recipeDef.label;
            helpDef.category = category;
            helpDef.description = recipeDef.description;


            #region Base Stats

            helpDef.HelpDetailSections.Add( new HelpDetailSection( null, new[] { "WorkAmount".Translate() + " : " + recipeDef.WorkAmountTotal( (ThingDef)null ).ToStringWorkAmount() } ) );

            #endregion

            #region Skill Requirements

            if( !recipeDef.skillRequirements.NullOrEmpty() )
            {
                helpDef.HelpDetailSections.Add( new HelpDetailSection(
                    "MinimumSkills".Translate(),
                    recipeDef.skillRequirements.Select( sr => sr.skill ).ToList().ConvertAll( sd => (Def)sd ),
                    null,
                    recipeDef.skillRequirements.Select( sr => sr.minLevel.ToString( "####0" ) ).ToArray() ) );
            }

            #endregion

            #region Ingredients

            // List of ingredients
            if( !recipeDef.ingredients.NullOrEmpty() )
            {
                // TODO: find the actual thingDefs of ingredients so we can use defs instead of strings.
                HelpDetailSection ingredients = new HelpDetailSection(
                    "Ingredients".Translate(),
                    recipeDef.ingredients.Select(ic => recipeDef.IngredientValueGetter.BillRequirementsDescription( ic )).ToArray());

                helpDef.HelpDetailSections.Add( ingredients );
            }

            #endregion

            #region Products

            // List of products
            if( !recipeDef.products.NullOrEmpty() )
            {
                HelpDetailSection products = new HelpDetailSection(
                    "AutoHelpListRecipeProducts".Translate(),
                    recipeDef.products.Select(tc => tc.thingDef).ToList().ConvertAll(def => (Def)def),
                    recipeDef.products.Select(tc => tc.count.ToString()).ToArray());

                helpDef.HelpDetailSections.Add( products );
            }

            #endregion

            #region Things & Research

            // Add things it's on
            var thingDefs = recipeDef.GetThingsCurrent();
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection billgivers = new HelpDetailSection(
                    "AutoHelpListRecipesOnThings".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( billgivers );
            }

            // Add research required
            var researchDefs = recipeDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection requiredResearch = new HelpDetailSection(
                    "AutoHelpListResearchRequired".Translate(),
                    researchDefs);

                helpDef.HelpDetailSections.Add( requiredResearch );
            }

            // What things is it on after research
            thingDefs = recipeDef.GetThingsUnlocked( ref researchDefs );
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection recipesOnThingsUnlocked = new HelpDetailSection(
                    "AutoHelpListRecipesOnThingsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( recipesOnThingsUnlocked );

                if( !researchDefs.NullOrEmpty() )
                {
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));

                    helpDef.HelpDetailSections.Add( researchBy );
                }
            }

            // Get research which locks recipe
            thingDefs = recipeDef.GetThingsLocked( ref researchDefs );
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection recipesOnThingsLocked = new HelpDetailSection(
                    "AutoHelpListRecipesOnThingsLocked".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( recipesOnThingsLocked );

                if( !researchDefs.NullOrEmpty() )
                {
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));

                    helpDef.HelpDetailSections.Add( researchBy );
                }
            }

            #endregion

            return helpDef;
        }

        static HelpDef HelpForResearch( ResearchProjectDef researchProjectDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = researchProjectDef.defName + "_ResearchProjectDef_Help";
            helpDef.keyDef = researchProjectDef;
            helpDef.label = researchProjectDef.label;
            helpDef.category = category;
            helpDef.description = researchProjectDef.description;

            #region Base Stats

            HelpDetailSection totalCost = new HelpDetailSection(null, new [] { "AutoHelpTotalCost".Translate(researchProjectDef.totalCost.ToString()) });
            helpDef.HelpDetailSections.Add( totalCost );

            #endregion

            #region Research, Buildings, Recipes and SowTags

            // Add research required
            var researchDefs = researchProjectDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection researchRequirements = new HelpDetailSection(
                    "AutoHelpListResearchRequired".Translate(),
                    researchDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( researchRequirements );
            }

            // Add research unlocked
            //CCL_Log.Message(researchProjectDef.label, "getting unlocked research");
            researchDefs = researchProjectDef.GetResearchUnlocked();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection reseachUnlocked = new HelpDetailSection(
                    "AutoHelpListResearchLeadsTo".Translate(),
                    researchDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( reseachUnlocked );
            }

            // Add buildings it unlocks
            var thingDefs = researchProjectDef.GetThingsUnlocked();
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection thingsUnlocked = new HelpDetailSection(
                    "AutoHelpListThingsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( thingsUnlocked );
            }

            // Add recipes it unlocks
            var recipeDefs = researchProjectDef.GetRecipesUnlocked( ref thingDefs );
            if(
                (!recipeDefs.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection recipesUnlocked = new HelpDetailSection(
                    "AutoHelpListRecipesUnlocked".Translate(),
                    recipeDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( recipesUnlocked );

                HelpDetailSection recipesOnThingsUnlocked = new HelpDetailSection(
                    "AutoHelpListRecipesOnThingsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( recipesOnThingsUnlocked );
            }

            // Look in advanced research to add plants and sow tags it unlocks
            var sowTags = researchProjectDef.GetSowTagsUnlocked( ref thingDefs );
            if(
                (!sowTags.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection plantsUnlocked = new HelpDetailSection(
                    "AutoHelpListPlantsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>( def =>(Def)def ));

                helpDef.HelpDetailSections.Add( plantsUnlocked );

                HelpDetailSection plantsIn = new HelpDetailSection(
                    "AutoHelpListPlantsIn".Translate(),
                    sowTags.ToArray());

                helpDef.HelpDetailSections.Add( plantsIn );
            }

            #endregion

            #region Lockouts

            // Get advanced research which locks
            researchDefs = researchProjectDef.GetResearchedLockedBy();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection researchLockout = new HelpDetailSection(
                    "AutoHelpListResearchLockout".Translate(),
                    researchDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( researchLockout );
            }

            #endregion

            return helpDef;
        }

        static HelpDef HelpForAdvancedResearch( AdvancedResearchDef advancedResearchDef, HelpCategoryDef category )
        {
            var helpDef = new HelpDef();
            helpDef.defName = advancedResearchDef.defName + "_AdvancedResearchDef_Help";
            helpDef.keyDef = advancedResearchDef;
            helpDef.label = advancedResearchDef.label;
            helpDef.description = advancedResearchDef.description;

            if( advancedResearchDef.helpCategoryDef == null )
            {
                advancedResearchDef.helpCategoryDef = category;
            }
            if( advancedResearchDef.IsHelpEnabled )
            {
                helpDef.category = advancedResearchDef.helpCategoryDef;
            }
            #region Base Stats

            HelpDetailSection totalCost = new HelpDetailSection(
                null,
                new [] {"AutoHelpTotalCost".Translate( advancedResearchDef.TotalCost.ToString() )});

            helpDef.HelpDetailSections.Add( totalCost );

            #endregion

            #region Research, Buildings, Recipes and SowTags

            // Add research required
            var researchDefs = advancedResearchDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection researchRequired = new HelpDetailSection(
                    "AutoHelpListResearchRequired".Translate(),
                    researchDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( researchRequired );
            }

            // Add buildings it unlocks
            var thingDefs = advancedResearchDef.GetThingsUnlocked();
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection thingsUnlocked = new HelpDetailSection(
                    "AutoHelpListThingsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( thingsUnlocked );
            }

            // Add recipes it unlocks
            var recipeDefs = advancedResearchDef.GetRecipesUnlocked( ref thingDefs );
            if(
                (!recipeDefs.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection recipesUnlocked = new HelpDetailSection(
                    "AutoHelpListRecipesUnlocked".Translate(),
                    recipeDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( recipesUnlocked );

                HelpDetailSection recipesOnThingsUnlocked = new HelpDetailSection(
                    "AutoHelpListRecipesOnThings".Translate(),
                    thingDefs.ConvertAll<Def>(def => (Def)def));

                helpDef.HelpDetailSections.Add( recipesOnThingsUnlocked );
            }

            // Add plants and sow tags it unlocks
            var sowTags = advancedResearchDef.GetSowTagsUnlocked( ref thingDefs );
            if(
                (!sowTags.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection plantsUnlocked = new HelpDetailSection(
                    "AutoHelpListPlantsUnlocked".Translate(),
                    thingDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( plantsUnlocked );

                HelpDetailSection recipesOnThingsUnlocked = new HelpDetailSection(
                    "AutoHelpListPlantsIn".Translate(),
                    sowTags.ToArray() );

                helpDef.HelpDetailSections.Add( recipesOnThingsUnlocked );
            }

            #endregion

            #region Lockouts

            // Add buildings it locks
            thingDefs = advancedResearchDef.GetThingsLocked();
            if( !thingDefs.NullOrEmpty() )
            {
                HelpDetailSection thingsLocked = new HelpDetailSection(
                    "AutoHelpListThingsLocked".Translate(),
                    thingDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( thingsLocked );
            }

            // Add recipes it locks
            recipeDefs = advancedResearchDef.GetRecipesLocked( ref thingDefs );
            if(
                (!recipeDefs.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection recipesLocked = new HelpDetailSection(
                    "AutoHelpListRecipesLocked".Translate(), recipeDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( recipesLocked );

                HelpDetailSection recipesOnThings = new HelpDetailSection(
                    "AutoHelpListRecipesOnThings".Translate(), thingDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( recipesOnThings );
            }

            // Add plants and sow tags it locks
            sowTags = advancedResearchDef.GetSowTagsLocked( ref thingDefs );
            if(
                (!sowTags.NullOrEmpty()) &&
                (!thingDefs.NullOrEmpty())
            )
            {
                HelpDetailSection plantsLocked = new HelpDetailSection(
                    "AutoHelpListPlantsLocked".Translate(),
                    thingDefs.ConvertAll<Def>( def =>(Def)def ) );

                helpDef.HelpDetailSections.Add( plantsLocked );

                HelpDetailSection plantsIn = new HelpDetailSection(
                    "AutoHelpListPlantsIn".Translate(),
                    sowTags.ToArray() );

                helpDef.HelpDetailSections.Add( plantsIn );
            }

            #endregion

            advancedResearchDef.HelpDef = helpDef;
            return helpDef;
        }

        #endregion

        #region Array Cleaners
        // TODO: implement the cleaners in the new stringbuilders. Doesn't seem necessary atm though. -Fluffy.
        //static List< Def >                  TidyDefs( List< Def > defs )
        //{
        //    if( defs == null )
        //    {
        //        return null;
        //    }
        //    for( int i = 0; i < defs.Count; ++i )
        //    {
        //        if( defs[ i ] == null )
        //        {
        //            defs.RemoveAt( i );
        //            --i;
        //        }
        //    }
        //    if( defs.Count == 0 )
        //    {
        //        return null;
        //    }
        //    return defs;
        //}

        //static List< string >               TidyStrings( List< string > strings )
        //{
        //    if( strings == null )
        //    {
        //        return null;
        //    }
        //    for( int i = 0; i < strings.Count; ++i )
        //    {
        //        if( strings[ i ] == null )
        //        {
        //            strings.RemoveAt( i );
        //            --i;
        //        }
        //    }
        //    if( strings.Count == 0 )
        //    {
        //        return null;
        //    }
        //    return strings;
        //}

        #endregion

        #region String Builders

        //static void BuildDefDescription( StringBuilder baseDescription, string prependDefs, List< Def > defs )
        //{
        //    defs = TidyDefs( defs );
        //    if( defs.NullOrEmpty() )
        //    {
        //        return;
        //    }

        //    var labels = new List< string >();
        //    var s = new StringBuilder();

        //    s.AppendLine( prependDefs );

        //    labels.Clear();
        //    for( int i = 0, count = defs.Count - 1; i <= count; i++ ){
        //        var d = defs[ i ];
        //        if( !labels.Contains( d.label.ToLower() ) )
        //        {
        //            labels.Add( d.label.ToLower() );
        //            s.Append( "\t" );
        //            s.AppendLine( d.LabelCap );
        //        }
        //    }
        //    s.AppendLine();

        //    //Log.Message( s.ToString() );
        //    baseDescription.Append( s.ToString() );
        //}

        //static void BuildStringDescription( StringBuilder baseDescription, string prependStrings, List< string > strings )
        //{
        //    strings = TidyStrings( strings );
        //    if( strings.NullOrEmpty() )
        //    {
        //        return;
        //    }

        //    var labels = new List< string >();
        //    var s = new StringBuilder();

        //    s.AppendLine( prependStrings );
        //    labels.Clear();
        //    for( int i = 0, count = strings.Count - 1; i <= count; i++ ){
        //        var d = strings[ i ];
        //        if( !labels.Contains( d.ToLower() ) )
        //        {
        //            labels.Add( d.ToLower() );
        //            s.Append( "\t" );
        //            s.AppendLine( d );
        //        }
        //    }
        //    s.AppendLine();

        //    //Log.Message( s.ToString() );
        //    baseDescription.Append( s.ToString() );
        //}

        #endregion

    }

}
