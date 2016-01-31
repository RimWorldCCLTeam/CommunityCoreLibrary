using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public static class Help
    {

        //[Unsaved]

        #region Instance Data

        #endregion

        #region Process State

        public static bool Initialize()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "Initialize()",
                "Help System"
            );
#endif

            // Items
            ResolveApparel();
            ResolveBodyParts();
            ResolveDrugs();
            ResolveMeals();
            ResolveWeapons();

            // TODO: Add stuff categories
            // TODO: Add biomes
            // TODO: Add plants
            // TODO: Add animals
            // TODO: Add workTypes
            // TODO: Add capacities
            // TODO: Add skills

            // The below are low priority  (as considered by Fluffy)
            // TODO: Add needs
            // TODO: Add building resources
            // TODO: Add factions
            // TODO: Add hediffs

            // The below are really low priority (as considered by Fluffy)
            // TODO: Add traders
            // TODO: Add tradertags

            // Buildings
            ResolveBuildings();
            ResolveMinifiableOnly();

            // Terrain
            ResolveTerrain();

            // Recipes
            ResolveRecipes();

            // Research
            ResolveResearch();
            ResolveAdvancedResearch();

            // Rebuild help caches
            ResolveReferences();

            CCL_Log.Trace(
                Verbosity.Validation,
                "Initialized",
                "Help System"
            );
            return true;
        }

        static void ResolveReferences()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveReferences()",
                "Help System"
            );
#endif

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
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveApparel()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void ResolveBodyParts()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveBodyParts()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void ResolveDrugs()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveDrugs()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void ResolveMeals()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveMeals()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        static void ResolveWeapons()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveWeapons()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        #endregion

        #region Building Resolvers

        static void ResolveBuildings()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveBuildings()",
                "Help System"
            );
#endif

            // Go through buildings by designation categories
            foreach( var designationCategoryDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading )
            {
                // Get list of things
                var thingDefs =
                    DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                        ( t.designationCategory == designationCategoryDef.defName )
                        && ( !t.IsLockedOut() )
                    ) ).ToList();

                if( !thingDefs.NullOrEmpty() )
                {
                    // Get help category
                    var helpCategoryDef = HelpCategoryForKey( designationCategoryDef.defName + "_Building" + HelpCategoryDefOf.HelpPostFix, designationCategoryDef.label, "AutoHelpCategoryBuildings".Translate() );

                    // Scan through all possible buildable defs and auto-generate help
                    ResolveDefList(
                        thingDefs,
                        helpCategoryDef
                    );
                }
            }
        }

        static void ResolveMinifiableOnly()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveMinifiableOnly()",
                "Help System"
            );
#endif

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
            ResolveDefList(
                thingDefs,
                helpCategoryDef
            );
        }

        #endregion

        #region Terrain Resolver

        static void ResolveTerrain()
        {
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveTerrain()",
                "Help System"
            );

            // Get list of natual terrain
            var terrainDefs =
                DefDatabase< TerrainDef >.AllDefsListForReading.Where( t => (
                    ( t.designationCategory.NullOrEmpty() )||
                    ( t.designationCategory == "None" )
                ) ).ToList();

            if( !terrainDefs.NullOrEmpty() )
            {
                // Get help category
                var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.TerrainHelp, "AutoHelpSubCategoryTerrain".Translate(), "AutoHelpCategoryTerrain".Translate() );

                // resolve the defs
                ResolveDefList( terrainDefs, helpCategoryDef );
            }

            // Get list of buildable floors
            terrainDefs =
                DefDatabase< TerrainDef >.AllDefsListForReading.Where( t => (
                    ( !t.designationCategory.NullOrEmpty() )&&
                    ( t.designationCategory != "None" )
                ) ).ToList();

            if( !terrainDefs.NullOrEmpty() )
            {
                // Get help category
                var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.FlooringHelp, "AutoHelpSubCategoryFlooring".Translate(), "AutoHelpCategoryTerrain".Translate() );

                // resolve the defs
                ResolveDefList( terrainDefs, helpCategoryDef );
            }

        }

        #endregion

        #region Recipe Resolvers

        static void ResolveRecipes()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveRecipes()",
                "Help System"
            );
#endif

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
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveResearch()",
                "Help System"
            );
#endif

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

            // filter duplicates and create helpdefs
            ResolveDefList( researchProjectDefs, helpCategoryDef );
        }

        static void ResolveAdvancedResearch()
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveAdvancedResearch()",
                "Help System"
            );
#endif

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

            // filter duplicates and create helpDefs
            ResolveDefList( advancedResearchDefs, helpCategoryDef );
        }

        #endregion

        #region ThingDef Resolver

        static void ResolveDefList<T>( List<T> defs, HelpCategoryDef category ) where T : Def
        {
#if DEBUG
            CCL_Log.Trace(
                Verbosity.Stack,
                "ResolveDefList()",
                "Help System"
                );
#endif

            // Get help database
            HashSet<Def> processedDefs =
                new HashSet<Def>( DefDatabase<HelpDef>.AllDefsListForReading.Select( h => h.keyDef ) );

            // Scan through defs and auto-generate help
            foreach( T def in defs )
            {
                // Check if the def doesn't already have a help entry
                if( !processedDefs.Contains( def ) )
                {
                    // Make a new one
                    HelpDef helpDef = HelpForDef( def, category );

                    // Inject the def
                    if( helpDef != null )
                    {
                        DefDatabase<HelpDef>.Add( helpDef );
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

#if DEBUG
                CCL_Log.Trace(
                    Verbosity.Stack,
                    "HelpCategoryForKey() :: " + key,
                    "Help System"
                );
#endif

                DefDatabase<HelpCategoryDef>.Add( helpCategoryDef );
            }

            return helpCategoryDef;
        }

        static HelpDef HelpForDef<T>( T def, HelpCategoryDef category ) where T : Def
        {
            // both thingdefs (buildings, items) and terraindefs (floors) are derived from buildableDef
            if( def is BuildableDef )
            {
                return HelpForBuildable( def as BuildableDef, category );
            }
            if( def is ResearchProjectDef )
            {
                return HelpForResearch( def as ResearchProjectDef, category );
            }
            if( def is AdvancedResearchDef )
            {
                return HelpForAdvancedResearch( def as AdvancedResearchDef, category );
            }
            if( def is RecipeDef )
            {
                CCL_Log.Error( "HelpForDef() cannot be used for recipedefs. Use HelpForRecipeDef() directly.", "HelpGen" );
                return null;
            }

            CCL_Log.Error( "HelpForDef() used with a def type (" + def.GetType().ToString() + ") that is not handled.", "HelpGen" );
            return null;
        }

        static HelpDef HelpForBuildable( BuildableDef buildableDef, HelpCategoryDef category )
        {
#if DEBUG
            CCL_Log.TraceMod(
                buildableDef,
                Verbosity.AutoGenCreation,
                "HelpForBuildable()"
            );
#endif
            
            // we need the thingdef in several places
            ThingDef thingDef = buildableDef as ThingDef;

            // set up empty helpdef
            var helpDef = new HelpDef();
            helpDef.defName = buildableDef.defName + "_BuildableDef_Help";
            helpDef.keyDef = buildableDef;
            helpDef.label = buildableDef.label;
            helpDef.category = category;
            helpDef.description = buildableDef.description;

            List<HelpDetailSection> statParts = new List<HelpDetailSection>();
            List<HelpDetailSection> linkParts = new List<HelpDetailSection>();

            #region Base Stats

            if ( !buildableDef.statBases.NullOrEmpty() )
            {
                // Look at base stats
                HelpDetailSection baseStats = new HelpDetailSection(
                    null,
                    buildableDef.statBases.Select( sb => sb.stat ).ToList().ConvertAll( def => (Def)def ),
                    null,
                    buildableDef.statBases.Select( sb => sb.stat.ValueToString( sb.value, sb.stat.toStringNumberSense ) )
                                .ToArray() );

                statParts.Add( baseStats );
            }

            #endregion
            
            #region required research
            // Add list of required research
            var researchDefs = buildableDef.GetResearchRequirements();
            if( !researchDefs.NullOrEmpty() )
            {
                HelpDetailSection reqResearch = new HelpDetailSection(
                        "AutoHelpListResearchRequired".Translate(),
                        researchDefs.ConvertAll(def => (Def)def));
                helpDef.HelpDetailSections.Add( reqResearch );
            }
            #endregion
            
            #region Cost List
            // specific thingdef costs (terrainDefs are buildable with costlist, but do not have stuff cost (oddly)).
            if( !buildableDef.costList.NullOrEmpty() )
            {
                HelpDetailSection costs = new HelpDetailSection(
                    "AutoHelpCost".Translate(),
                    buildableDef.costList.Select( tc => tc.thingDef ).ToList().ConvertAll( def => (Def)def ),
                    null,
                    buildableDef.costList.Select( tc => tc.count.ToString() ).ToArray() );

                helpDef.HelpDetailSections.Add( costs );
            }
            #endregion

            #region ThingDef Specific
            if( thingDef != null )
            {
                #region stat offsets

                if( !thingDef.equippedStatOffsets.NullOrEmpty() )
                {
                    HelpDetailSection equippedOffsets = new HelpDetailSection(
                    "AutoHelpListStatOffsets".Translate(),
                    thingDef.equippedStatOffsets.Select( so => so.stat ).ToList().ConvertAll( def => (Def)def ),
                    null,
                    thingDef.equippedStatOffsets.Select( so => so.stat.ValueToString( so.value, so.stat.toStringNumberSense ) )
                                .ToArray() );

                    statParts.Add( equippedOffsets );
                }

                #endregion

                #region Stuff Cost

                // What stuff can it be made from?
                if(
                    ( thingDef.costStuffCount > 0 ) &&
                    ( !thingDef.stuffCategories.NullOrEmpty() )
                )
                {
                    linkParts.Add( new HelpDetailSection(
                        "AutoHelpStuffCost".Translate( thingDef.costStuffCount.ToString() ),
                        thingDef.stuffCategories.ToList().ConvertAll( def => (Def)def ) ) );
                }

                #endregion

                #region Recipes (to make thing)
                List<RecipeDef> recipeDefs = buildableDef.GetRecipeDefs();
                if ( !recipeDefs.NullOrEmpty() )
                {
                    HelpDetailSection recipes = new HelpDetailSection(
                        "AutoHelpListRecipes".Translate(),
                        recipeDefs.ConvertAll( def => (Def)def ) );
                    linkParts.Add( recipes );

                    try
                    {
                        // for some odd reasons meals and beer give errors when doing this.
                        var tableDefs = recipeDefs.SelectMany( r => r.GetRecipeUsers() )
                                                  .ToList()
                                                  .ConvertAll( def => def as Def );
                        HelpDetailSection tables = new HelpDetailSection(
                            "AutoHelpListRecipesOnThingsUnlocked".Translate(), tableDefs );
                        linkParts.Add( tables );
                    }
                    catch
                    {
                        CCL_Log.Error( "Error loading recipe providers for " + thingDef.LabelCap, "HelpGen" );
                    }
                }
                #endregion

                #region Ingestible Stats
                // Look at base stats
                if( thingDef.IsNutritionSource )
                {
                    List<Def> needDefs = new List<Def>();
                    needDefs.Add( NeedDefOf.Food );
                    needDefs.Add( NeedDefOf.Joy );

                    string[] suffixes =
                    {
                        thingDef.ingestible.nutrition.ToString( "0.###" ),
                        thingDef.ingestible.joy.ToString( "0.###" )
                    };

                    statParts.Add( 
                        new HelpDetailSection( "AutoHelpListNutrition".Translate(), needDefs, null, suffixes ) );
                }

                #endregion

                #region Body Part Stats

                if( ( !thingDef.thingCategories.NullOrEmpty() ) &&
                    ( thingDef.thingCategories.Contains( ThingCategoryDefOf.BodyPartsAndImplants ) ) &&
                    ( thingDef.IsImplant() ) )
                {
                    var hediffDef = thingDef.GetImplantHediffDef();

                    #region Efficiency

                    if( hediffDef.addedPartProps != null )
                    {
                        statParts.Add( new HelpDetailSection( "BodyPartEfficiency".Translate(), new[] { hediffDef.addedPartProps.partEfficiency.ToString( "P0" ) } ) );
                    }

                    #endregion

                    #region Capacities
                    if( ( !hediffDef.stages.NullOrEmpty() ) &&
                        ( hediffDef.stages.Exists( stage => (
                            ( !stage.capMods.NullOrEmpty() )
                        ) ) )
                    )
                    {
                        HelpDetailSection capacityMods = new HelpDetailSection(
                            "AutoHelpListCapacityModifiers".Translate(),
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

                        statParts.Add( capacityMods );
                    }

                    #endregion

                    #region Components (Melee attack)

                    if( ( !hediffDef.comps.NullOrEmpty() ) &&
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
                                            statParts.Add( new HelpDetailSection(
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
                    var recipeDef = thingDef.GetImplantRecipeDef();
                    if( !recipeDef.appliedOnFixedBodyParts.NullOrEmpty() )
                    {
                        linkParts.Add( new HelpDetailSection(
                            "AutoHelpSurgeryFixOrReplace".Translate(),
                            recipeDef.appliedOnFixedBodyParts.ToList().ConvertAll( def => (Def)def ) ) );
                    }

                    #endregion

                }

                #endregion

                #region Recipes & Research (on building)

                // Get list of recipes
                recipeDefs = thingDef.AllRecipes;
                if( !recipeDefs.NullOrEmpty() )
                {
                    HelpDetailSection recipes = new HelpDetailSection(
                        "AutoHelpListRecipes".Translate(),
                        recipeDefs.ConvertAll(def => (Def)def));
                    linkParts.Add( recipes );
                }

                // Build help for unlocked recipes associated with building
                recipeDefs = thingDef.GetRecipesUnlocked( ref researchDefs );
                if(
                    ( !recipeDefs.NullOrEmpty() ) &&
                    ( !researchDefs.NullOrEmpty() )
                )
                {
                    HelpDetailSection unlockRecipes = new HelpDetailSection(
                        "AutoHelpListRecipesUnlocked".Translate(),
                        recipeDefs.ConvertAll<Def>(def => (Def)def));
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));
                    linkParts.Add( unlockRecipes );
                    linkParts.Add( researchBy );
                }

                // Build help for locked recipes associated with building
                recipeDefs = thingDef.GetRecipesLocked( ref researchDefs );
                if(
                    ( !recipeDefs.NullOrEmpty() ) &&
                    ( !researchDefs.NullOrEmpty() )
                )
                {
                    HelpDetailSection unlockRecipes = new HelpDetailSection(
                        "AutoHelpListRecipesLocked".Translate(),
                        recipeDefs.ConvertAll<Def>(def => (Def)def));
                    HelpDetailSection researchBy = new HelpDetailSection(
                        "AutoHelpListResearchBy".Translate(),
                        researchDefs.ConvertAll<Def>(def => (Def)def));
                    linkParts.Add( unlockRecipes );
                    linkParts.Add( researchBy );
                }

                #endregion (on building)

                #region Facilities

                // Get list of facilities that effect it
                var affectedBy = thingDef.GetCompProperties( typeof( CompAffectedByFacilities ) );
                if( ( affectedBy != null ) &&
                    ( !affectedBy.linkableFacilities.NullOrEmpty() ) )
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

                        statParts.Add( facilityDetailSection );
                        linkParts.Add( facilitiesAffected );
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

                    linkParts.Add( joyDetailSection );
                }

                #endregion

            }

            #endregion

            #region Terrain Specific
            TerrainDef terrainDef = buildableDef as TerrainDef;
            if ( terrainDef != null )
            {
                string[] stats = new[]
                {
                    "AutoHelpListFertility".Translate() + ": " + terrainDef.fertility.ToStringPercent(),
                    "AutoHelpListPathCost".Translate() + ": " + terrainDef.pathCost.ToString()
                };
                
                statParts.Add( new HelpDetailSection( null, stats ) );
            }
            #endregion

            helpDef.HelpDetailSections.AddRange( statParts );
            helpDef.HelpDetailSections.AddRange( linkParts );

            return helpDef;
        }

        static HelpDef HelpForRecipe( ThingDef thingDef, RecipeDef recipeDef, HelpCategoryDef category )
        {
#if DEBUG
            CCL_Log.TraceMod(
                recipeDef,
                Verbosity.AutoGenCreation,
                "HelpForRecipe()"
            );
#endif
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
#if DEBUG
            CCL_Log.TraceMod(
                researchProjectDef,
                Verbosity.AutoGenCreation,
                "HelpForResearch()"
            );
#endif
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

            // Add buildables unlocked (items, buildings and terrain)
            List<Def> buildableDefs = new List<Def>();

            // items and buildings
            buildableDefs.AddRange( researchProjectDef.GetThingsUnlocked().ConvertAll<Def>( def => (Def)def ) );

            // terrain
            buildableDefs.AddRange( researchProjectDef.GetTerrainUnlocked().ConvertAll<Def>( def => (Def)def) );

            // create help section
            if( !buildableDefs.NullOrEmpty() )
            {
                HelpDetailSection thingsUnlocked = new HelpDetailSection(
                    "AutoHelpListThingsUnlocked".Translate(),
                    buildableDefs);

                helpDef.HelpDetailSections.Add( thingsUnlocked );
            }

            // filter down to thingdefs for recipes etc.
            List<ThingDef> thingDefs =
                buildableDefs.Where( def => def is ThingDef )
                             .ToList()
                             .ConvertAll<ThingDef>( def => (ThingDef)def );

            // Add recipes it unlocks
            var recipeDefs = researchProjectDef.GetRecipesUnlocked( ref thingDefs );
            if(
                ( !recipeDefs.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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
                ( !sowTags.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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
#if DEBUG
            CCL_Log.TraceMod(
                advancedResearchDef,
                Verbosity.AutoGenCreation,
                "HelpForAdvancedResearch()"
            );
#endif
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
                ( !recipeDefs.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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
                ( !sowTags.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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
                ( !recipeDefs.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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
                ( !sowTags.NullOrEmpty() ) &&
                ( !thingDefs.NullOrEmpty() )
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

        #region HelpDef getters

        public static List<HelpDef> GetAllHelpDefs()
        {
            return DefDatabase<HelpDef>.AllDefsListForReading;
        }

        #endregion

    }

}
