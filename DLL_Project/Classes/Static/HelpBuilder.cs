using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class HelpBuilder
    {

        //[Unsaved]

        #region Instance Data

        #endregion

        #region Process State

        public static bool ResolveImpliedDefs()
        {

            // Items
            ResolveApparel();
            ResolveBodyParts();
            ResolveDrugs();
            ResolveMeals();
            ResolveWeapons();

            // TODO: Add stuff categories
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

            // flora and fauna
            ResolvePlants();
            ResolvePawnkinds();
            ResolveBiomes();

            // Recipes
            ResolveRecipes();

            // Research
            ResolveResearch();
            ResolveAdvancedResearch();

            // Rebuild help caches
            ResolveReferences();

            return true;
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
            ResolveDefList(
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
                        // A14 - BodyPartsAndImplants => BodyParts + BodyPartsArtifical? (Artificial has no DefOf entry?
                        // TODO!
                        // - Fluffy
                        ( t.thingCategories.Contains( ThingCategoryDefOf.BodyParts ) )
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
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.IsIngestible )&&
                    ( t.IsDrug )
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
            // Get list of things
            var thingDefs =
                DefDatabase< ThingDef >.AllDefsListForReading.Where( t => (
                    ( t.IsNutritionGivingIngestible )&&
                    ( !t.IsDrug )
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
            // Get list of terrainDefs without designation category that occurs as a byproduct of mining (rocky),
            // or is listed in biomes (natural terrain). This excludes terrains that are not normally visible (e.g. Underwall).
            string[] rockySuffixes = new[] { "_Rough", "_Smooth", "_RoughHewn" };

            List<TerrainDef> terrainDefs =
                DefDatabase<TerrainDef>.AllDefsListForReading
                                       .Where( 
                                            // not buildable
                                            t => String.IsNullOrEmpty( t.designationCategory )
                                            && (
                                                // is a type generated from rock
                                                rockySuffixes.Any( s => t.defName.EndsWith( s ) )

                                                // or is listed in any biome
                                                || DefDatabase<BiomeDef>.AllDefsListForReading.Any(
                                                    b => b.AllTerrainDefs().Contains( t ) )
                                                ) )
                                       .ToList();

            if( !terrainDefs.NullOrEmpty() )
            {
                // Get help category
                var helpCategoryDef = HelpCategoryForKey( HelpCategoryDefOf.TerrainHelp, "AutoHelpSubCategoryTerrain".Translate(), "AutoHelpCategoryTerrain".Translate() );

                // resolve the defs
                ResolveDefList( terrainDefs, helpCategoryDef );
            }

            // Get list of buildable floors per designation category
            foreach ( var categoryDef in DefDatabase<DesignationCategoryDef>.AllDefsListForReading )
            {
                terrainDefs =
                    DefDatabase<TerrainDef>.AllDefsListForReading.Where( t => t.designationCategory == categoryDef.defName ).ToList();

                if( !terrainDefs.NullOrEmpty() )
                {
                    // Get help category
                    var helpCategoryDef = HelpCategoryForKey( categoryDef.defName + HelpCategoryDefOf.HelpPostFix, categoryDef.LabelCap, "AutoHelpCategoryTerrain".Translate() );

                    // resolve the defs
                    ResolveDefList( terrainDefs, helpCategoryDef );
                }
            }
        }

        #endregion

        #region Flora and Fauna resolvers

        static void ResolvePlants()
        {
            // plants
            List<ThingDef> plants = DefDatabase<ThingDef>.AllDefsListForReading.Where( t => t.plant != null ).ToList();
            HelpCategoryDef category = HelpCategoryForKey( HelpCategoryDefOf.Plants, "AutoHelpSubCategoryPlants".Translate(),
                                               "AutoHelpCategoryFloraAndFauna".Translate() );

            ResolveDefList( plants, category );
        }

        static void ResolvePawnkinds()
        {
            // animals
            List<PawnKindDef> pawnkinds =
                DefDatabase<PawnKindDef>.AllDefsListForReading.Where( t => t.race.race.Animal ).ToList();
            HelpCategoryDef category = HelpCategoryForKey( HelpCategoryDefOf.Animals, "AutoHelpSubCategoryAnimals".Translate(),
                                               "AutoHelpCategoryFloraAndFauna".Translate() );
            ResolveDefList( pawnkinds, category );

            // mechanoids
            pawnkinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where( t => t.race.race.IsMechanoid ).ToList();
            category = HelpCategoryForKey( HelpCategoryDefOf.Mechanoids, "AutoHelpSubCategoryMechanoids".Translate(),
                                           "AutoHelpCategoryFloraAndFauna".Translate() );
            ResolveDefList( pawnkinds, category );

            // humanoids
            pawnkinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where( t => !t.race.race.Animal && !t.race.race.IsMechanoid).ToList();
            category = HelpCategoryForKey( HelpCategoryDefOf.Humanoids, "AutoHelpSubCategoryHumanoids".Translate(),
                                           "AutoHelpCategoryFloraAndFauna".Translate() );
            ResolveDefList( pawnkinds, category );

        }

        static void ResolveBiomes()
        {
            var biomes = DefDatabase<BiomeDef>.AllDefsListForReading;
            var category = HelpCategoryForKey( HelpCategoryDefOf.Biomes, "AutoHelpSubCategoryBiomes".Translate(),
                                               "AutoHelpCategoryFloraAndFauna".Translate() );
            ResolveDefList( biomes, category );
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
                    ( t.thingClass != typeof( Corpse ) )&&
                    ( t.thingClass != typeof( Pawn ) )
                ) ).ToList();

            // Get help database
            var helpDefs = DefDatabase< HelpDef >.AllDefsListForReading;

            // Scan through defs and auto-generate help
            foreach( var thingDef in thingDefs )
            {
                var recipeDefs = thingDef.GetRecipesAll();
                if( !recipeDefs.NullOrEmpty() )
                {
                    // Get help category
                    var helpCategoryDef = HelpCategoryForKey( thingDef.defName + "_Recipe" + HelpCategoryDefOf.HelpPostFix, thingDef.label, "AutoHelpCategoryRecipes".Translate() );

                    foreach( var recipeDef in recipeDefs )
                    {
                        // Find an existing entry
                        var helpDef = helpDefs.Find( h => (
                            ( h.keyDef == recipeDef )&&
                            ( h.secondaryKeyDef == thingDef )
                        ) );

                        if( helpDef == null )
                        {
                            // Make a new one
                            //Log.Message( "Help System :: " + recipeDef.defName );
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

            // filter duplicates and create helpdefs
            ResolveDefList( researchProjectDefs, helpCategoryDef );
        }

        static void ResolveAdvancedResearch()
        {
            // Get advanced research database
            var advancedResearchDefs =
                Controller.Data.AdvancedResearchDefs.Where( a => (
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

        #region Help Makers

        static void ResolveDefList<T>( List<T> defs, HelpCategoryDef category ) where T : Def
        {
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
            if (def is PawnKindDef )
            {
                return HelpForPawnKind( def as PawnKindDef, category );
            }
            if( def is RecipeDef )
            {
                CCL_Log.Error( "HelpForDef() cannot be used for recipedefs. Use HelpForRecipeDef() directly.", "Help System" );
                return null;
            }
            if ( def is BiomeDef )
            {
                return HelpForBiome( def as BiomeDef, category );
            }

            CCL_Log.Error( "HelpForDef() used with a def type (" + def.GetType().ToString() + ") that is not handled.", "Help System" );
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
                linkParts.Add( reqResearch );
            }
            #endregion
            
            #region Cost List
            // specific thingdef costs (terrainDefs are buildable with costlist, but do not have stuff cost (oddly)).
            if( !buildableDef.costList.NullOrEmpty() )
            {
                HelpDetailSection costs = new HelpDetailSection(
                    "AutoHelpCost".Translate(),
                    buildableDef.costList.Select( tc => tc.thingDef ).ToList().ConvertAll( def => (Def)def ),
                    buildableDef.costList.Select( tc => tc.count.ToString() ).ToArray() );

                linkParts.Add( costs );
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

                    // TODO: Figure out why this fails on a few select recipes (e.g. MVP's burger recipes and Apparello's Hive Armor), but works when called directly in these recipe's helpdefs.
                    var tableDefs = recipeDefs.SelectMany( r => r.GetRecipeUsers() )
                                              .ToList()
                                              .ConvertAll( def => def as Def );

                    if ( !tableDefs.NullOrEmpty() )
                    {
                        HelpDetailSection tables = new HelpDetailSection(
                        "AutoHelpListRecipesOnThingsUnlocked".Translate(), tableDefs );
                        linkParts.Add( tables );
                    }
#if DEBUG
                    else 
                    {
                        CCL_Log.TraceMod(
                            buildableDef,
                            Verbosity.NonFatalErrors,
                            "Loading 'available on' failed for " + thingDef.defName,
                            "Help System" );
                    }
#endif
                }
                #endregion

                #region Ingestible Stats
                // Look at base stats
                if( thingDef.IsIngestible )
                {
                    // only show Joy if it's non-zero
                    List<Def> needDefs = new List<Def>();
                    needDefs.Add( NeedDefOf.Food );
                    if ( Math.Abs( thingDef.ingestible.joy ) > 1e-3 )
                    {
                        needDefs.Add( NeedDefOf.Joy );
                    }

                    List<string> suffixes = new List<string>();
                    suffixes.Add( thingDef.ingestible.nutrition.ToString( "0.###" ) );
                    if( Math.Abs( thingDef.ingestible.joy ) > 1e-3 )
                    {
                        suffixes.Add( thingDef.ingestible.joy.ToString( "0.###" ) );
                    }

                    // show different label for plants to show we're talking about the actual plant, not the grown veggie/fruit/etc.
                    string statLabel = "AutoHelpListNutrition".Translate();
                    if( thingDef.plant != null )
                    {
                        statLabel = "AutoHelpListNutritionPlant".Translate();
                    }

                    statParts.Add( 
                        new HelpDetailSection( statLabel, needDefs, null, suffixes.ToArray() ) );
                }

                #endregion

                #region Body Part Stats

                if( ( !thingDef.thingCategories.NullOrEmpty() ) &&
                    ( thingDef.thingCategories.Contains( ThingCategoryDefOf.BodyParts ) ) &&
                    ( thingDef.isBodyPartOrImplant ) )
                {
                    var hediffDef = thingDef.GetImplantHediffDef();

                    #region Efficiency

                    if( hediffDef.addedPartProps != null )
                    {
                        statParts.Add( new HelpDetailSection( "BodyPartEfficiency".Translate(), new[] { hediffDef.addedPartProps.partEfficiency.ToString( "P0" ) }, null, null ) );
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
                                        cm => (cm.offset > 0 ? "+" : "") + cm.offset.ToString("P0"))
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
                                                        "MeleeWarmupTime".Translate(),
                                                        "StatsReport_MeleeDamage".Translate()
                                                    },
                                                    null,
                                                    new[]
                                                    {
                                                        verb.defaultCooldownTicks.ToString(),
                                                        verb.meleeDamageBaseAmount.ToString()
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

                #endregion

                #region Power

                var powerSectionList = new List<StringDescTriplet>();

                // Get power required or generated
                var compPowerTrader = thingDef.GetCompProperties<CompProperties_Power>();
                if( compPowerTrader != null )
                {
                    if( compPowerTrader.basePowerConsumption > 0 )
                    {
                        var basePowerConsumption = (int) compPowerTrader.basePowerConsumption;
                        powerSectionList.Add( new StringDescTriplet( "AutoHelpRequired".Translate(), null, basePowerConsumption.ToString() ) );

                        var compPowerIdle = thingDef.GetCompProperties<CompProperties_LowIdleDraw>();
                        if( compPowerIdle != null )
                        {
                            int idlePower;
                            if( compPowerIdle.idlePowerFactor < 1.0f )
                            {
                                idlePower = (int)( compPowerTrader.basePowerConsumption * compPowerIdle.idlePowerFactor );
                            }
                            else
                            {
                                idlePower = (int) compPowerIdle.idlePowerFactor;
                            }
                            powerSectionList.Add( new StringDescTriplet( "AutoHelpIdlePower".Translate(), null, idlePower.ToString() ) );
                        }
                    }
                    else if( compPowerTrader.basePowerConsumption < 0 )
                    {
                        // A14 - check this!
                        if( thingDef.HasComp( typeof( CompPowerPlantWind ) ) )
                        {
                            powerSectionList.Add( new StringDescTriplet( "AutoHelpGenerates".Translate(), null, "1700" ) );
                        }
                        else
                        {
                            var basePowerConsumption = (int) -compPowerTrader.basePowerConsumption;
                            powerSectionList.Add( new StringDescTriplet( "AutoHelpGenerates".Translate(), null, basePowerConsumption.ToString() ) );
                        }
                    }
                }
                var compBattery = thingDef.GetCompProperties<CompProperties_Battery>();
                if( compBattery != null )
                {
                    var stored = (int) compBattery.storedEnergyMax;
                    var efficiency = (int) ( compBattery.efficiency * 100f );
                    powerSectionList.Add( new StringDescTriplet( "AutoHelpStores".Translate(), null, stored.ToString() ) );
                    powerSectionList.Add( new StringDescTriplet( "AutoHelpEfficiency".Translate(), null, efficiency.ToString() + "%" ) );
                }

                if( !powerSectionList.NullOrEmpty() )
                {
                    HelpDetailSection powerSection = new HelpDetailSection(
                        "AutoHelpPower".Translate(),
                        null,
                        powerSectionList );
                    statParts.Add( powerSection );
                }

                #endregion

                #region Facilities

                // Get list of facilities that effect it
                // TODO: This was never implemented?
                var affectedBy = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
                if (
                    ( affectedBy != null )&&
                    ( !affectedBy.linkableFacilities.NullOrEmpty() )
                )
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
                            ( f.GetCompProperties<CompProperties_AffectedByFacilities>() != null )&&
                            ( f.GetCompProperties<CompProperties_AffectedByFacilities>().linkableFacilities != null )&&
                            ( f.GetCompProperties<CompProperties_AffectedByFacilities>().linkableFacilities.Contains( thingDef ) )
                        ) ).ToList();
                    if( !effectsBuildings.NullOrEmpty() )
                    {
                        var facilityProperties = thingDef.GetCompProperties<CompProperties_Facility>();

                        List<DefStringTriplet> facilityDefs = new List<DefStringTriplet>();
                        List<StringDescTriplet> facilityStrings = new List<StringDescTriplet>();
                        facilityStrings.Add( new StringDescTriplet( "AutoHelpMaximumAffected".Translate(), null, facilityProperties.maxSimultaneous.ToString() ) );

                        // Look at stats modifiers
                        foreach( var stat in facilityProperties.statOffsets )
                        {
                            facilityDefs.Add( new DefStringTriplet( stat.stat, null, ": " + stat.stat.ValueToString( stat.value, stat.stat.toStringNumberSense ) ) );
                        }

                        HelpDetailSection facilityDetailSection = new HelpDetailSection(
                            "AutoHelpFacilityStats".Translate(),
                            facilityDefs, facilityStrings);

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
                var joyGiverDefs = thingDef.GetJoyGiverDefsUsing();

                if( !joyGiverDefs.NullOrEmpty() )
                {
                    foreach( var joyGiverDef in joyGiverDefs )
                    {
                        // Get job driver stats
                        if( joyGiverDef.jobDef != null )
                        {
                            List<DefStringTriplet> defs = new List<DefStringTriplet>();
                            List<StringDescTriplet> strings = new List<StringDescTriplet>();

                            strings.Add( new StringDescTriplet( joyGiverDef.jobDef.reportString ) );
                            strings.Add( new StringDescTriplet( joyGiverDef.jobDef.joyMaxParticipants.ToString(), "AutoHelpMaximumParticipants".Translate() ) );
                            defs.Add( new DefStringTriplet( joyGiverDef.jobDef.joyKind, "AutoHelpJoyKind".Translate() ) );
                            if( joyGiverDef.jobDef.joySkill != null )
                            {
                                defs.Add( new DefStringTriplet( joyGiverDef.jobDef.joySkill, "AutoHelpJoySkill".Translate() ) );
                            }

                            linkParts.Add( new HelpDetailSection(
                                "AutoHelpListJoyActivities".Translate(),
                                defs, strings ) );
                        }
                    }
                }

                #endregion

            }

            #endregion

            #region plant extras

            if(
                ( thingDef != null )&&
                ( thingDef.plant != null )
            )
            {
                HelpPartsForPlant( thingDef, ref statParts, ref linkParts );
            }

            #endregion

            #region Terrain Specific
            TerrainDef terrainDef = buildableDef as TerrainDef;
            if( terrainDef != null )
            {
                HelpPartsForTerrain( terrainDef, ref statParts, ref linkParts );
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
            helpDef.secondaryKeyDef = thingDef;
            helpDef.defName = helpDef.keyDef + "_RecipeDef_Help";
            helpDef.label = recipeDef.label;
            helpDef.category = category;
            helpDef.description = recipeDef.description;

            #region Base Stats

            helpDef.HelpDetailSections.Add( new HelpDetailSection( null, 
                new[] { recipeDef.WorkAmountTotal( (ThingDef)null ).ToStringWorkAmount() },
                new[] { "WorkAmount".Translate() + " : " },
                null ) );

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
                    recipeDef.ingredients.Select(ic => recipeDef.IngredientValueGetter.BillRequirementsDescription( ic )).ToArray(), null, null);

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
            var thingDefs = recipeDef.GetRecipeUsers();
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
            HelpDetailSection totalCost = new HelpDetailSection(null, 
                                                                new [] { researchProjectDef.baseCost.ToString() },
                                                                new [] { "AutoHelpTotalCost".Translate() },
                                                                null );
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
            if ( !buildableDefs.NullOrEmpty() )
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
                    sowTags.ToArray(), null, null);

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
                new [] { advancedResearchDef.TotalCost.ToString() },
                new [] {"AutoHelpTotalCost".Translate()},
                null );

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
                    sowTags.ToArray(), null, null );

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
                    sowTags.ToArray(), null, null );

                helpDef.HelpDetailSections.Add( plantsIn );
            }

            #endregion

            advancedResearchDef.HelpDef = helpDef;
            return helpDef;
        }

        static HelpDef HelpForBiome( BiomeDef biomeDef, HelpCategoryDef category )
        {
#if DEBUG
            CCL_Log.TraceMod(
                biomeDef,
                Verbosity.AutoGenCreation,
                "HelpForBiome()"
            );
#endif
            var helpDef = new HelpDef();
            helpDef.keyDef = biomeDef;
            helpDef.defName = helpDef.keyDef + "_RecipeDef_Help";
            helpDef.label = biomeDef.label;
            helpDef.category = category;
            helpDef.description = biomeDef.description;

            List<Def> defs = new List<Def>();
            List<string> chances = new List<string>();

            #region Generic (temp, rainfall, elevation)
            // we can't get to these stats. They seem to be hardcoded in RimWorld.Planet.WorldGenerator_Grid.BiomeFrom()
            // hacky solution would be to reverse-engineer them by taking a loaded world and 5th and 95th percentiles from worldsquares with this biome.
            // however, that requires a world to be loaded.
            #endregion

            #region Diseases

            var diseases = biomeDef.AllDiseases();
            if( !diseases.NullOrEmpty() )
            {
                foreach( var disease in diseases )
                {
                    var diseaseCommonality = biomeDef.CommonalityOfDisease( disease ) / ( biomeDef.diseaseMtbDays * GenDate.DaysPerYear );
                    defs.Add( disease.diseaseIncident );
                    chances.Add( diseaseCommonality.ToStringPercent() );
                }

                helpDef.HelpDetailSections.Add( new HelpDetailSection(
                                                    "AutoHelpListBiomeDiseases".Translate(),
                                                    defs, null, chances.ToArray() ) );
            }
            defs.Clear();

            #endregion

            #region Terrain

            defs = biomeDef.AllTerrainDefs().ConvertAll( def => (Def)def );
            // commonalities unknown
            if ( !defs.NullOrEmpty() )
            {
                helpDef.HelpDetailSections.Add( new HelpDetailSection(
                                                    "AutoHelpListBiomeTerrain".Translate(),
                                                    defs ) );
            }

            #endregion

            #region Plants

            defs = DefDatabase<ThingDef>.AllDefsListForReading
                                        .Where( t => biomeDef.AllWildPlants.Contains( t ) )
                                        .ToList().ConvertAll( def => (Def)def );
            if( !defs.NullOrEmpty() )
            {
                helpDef.HelpDetailSections.Add( new HelpDetailSection(
                                                    "AutoHelpListBiomePlants".Translate(),
                                                    defs ) );
            }

            #endregion

            #region Animals

            defs = DefDatabase<PawnKindDef>.AllDefsListForReading
                                        .Where( t => biomeDef.AllWildAnimals.Contains( t ) )
                                        .Distinct()
                                        .ToList().ConvertAll( def => (Def)def );
            if( !defs.NullOrEmpty() )
            {
                helpDef.HelpDetailSections.Add( new HelpDetailSection(
                                                    "AutoHelpListBiomeAnimals".Translate(),
                                                    defs ) );
            }

            #endregion

            return helpDef;
        }

        static HelpDef HelpForPawnKind( PawnKindDef kindDef, HelpCategoryDef category )
        {

#if DEBUG
            CCL_Log.TraceMod(
                kindDef,
                Verbosity.AutoGenCreation,
                "HelpForBuildable()"
            );
#endif

            // we need the thingdef in several places
            ThingDef raceDef = kindDef.race;

            // set up empty helpdef
            var helpDef = new HelpDef();
            helpDef.defName = kindDef.defName + "_PawnKindDef_Help";
            helpDef.keyDef = kindDef;
            helpDef.label = kindDef.label;
            helpDef.category = category;
            helpDef.description = kindDef.description;

            List<HelpDetailSection> statParts = new List<HelpDetailSection>();
            List<HelpDetailSection> linkParts = new List<HelpDetailSection>();

            #region Base Stats

            if( !raceDef.statBases.NullOrEmpty() )
            {
                // Look at base stats
                HelpDetailSection baseStats = new HelpDetailSection(
                    null,
                    raceDef.statBases.Select( sb => sb.stat ).ToList().ConvertAll( def => (Def)def ),
                    null,
                    raceDef.statBases.Select( sb => sb.stat.ValueToString( sb.value, sb.stat.toStringNumberSense ) )
                                .ToArray() );

                statParts.Add( baseStats );
            }

            #endregion

            HelpPartsForAnimal( kindDef, ref statParts, ref linkParts );
            
            helpDef.HelpDetailSections.AddRange( statParts );
            helpDef.HelpDetailSections.AddRange( linkParts );

            return helpDef;
        }

        #endregion

        #region Help maker helpers

        static void HelpPartsForTerrain( TerrainDef terrainDef, ref List<HelpDetailSection> statParts, ref List<HelpDetailSection> linkParts )
        {
            statParts.Add( new HelpDetailSection( null, 
                                                  new []
                                                  {
                                                      terrainDef.fertility.ToStringPercent(),
                                                      terrainDef.pathCost.ToString()
                                                  },
                                                  new []
                                                  {
                                                      "AutoHelpListFertility".Translate() + ":",
                                                      "AutoHelpListPathCost".Translate() + ":"
                                                  },
                                                  null ) );

            // wild biome tags
            var biomes = DefDatabase<BiomeDef>.AllDefsListForReading
                                              .Where( b => b.AllTerrainDefs().Contains( terrainDef ) )
                                              .ToList();
            if( !biomes.NullOrEmpty() )
            {
                linkParts.Add( new HelpDetailSection( "AutoHelpListAppearsInBiomes".Translate(),
                                                      biomes.Select( r => r as Def ).ToList() ) );
            }

        }

        static void HelpPartsForPlant( ThingDef thingDef, ref List<HelpDetailSection> statParts, ref List<HelpDetailSection> linkParts )
        {
            var plant = thingDef.plant;

            // non-def stat part
            statParts.Add( new HelpDetailSection( null, 
                                                  new []
                                                  {
                                                      plant.growDays.ToString(),
                                                      plant.fertilityMin.ToStringPercent(),
                                                      plant.growMinGlow.ToStringPercent() + " - " + plant.growOptimalGlow.ToStringPercent()
                                                  },
                                                  new []
                                                  {
                                                      "AutoHelpGrowDays".Translate(),
                                                      "AutoHelpMinFertility".Translate(),
                                                      "AutoHelpLightRange".Translate()
                                                  },
                                                  null ) );

            if( plant.Harvestable )
            {
                // yield
                linkParts.Add( new HelpDetailSection(
                                   "AutoHelpListPlantYield".Translate(),
                                   new List<Def>( new[] { plant.harvestedThingDef } ),
                                   new[] { plant.harvestYield.ToString() }
                                   ) );
            }

            // sowtags
            if( plant.Sowable )
            {
                linkParts.Add( new HelpDetailSection( "AutoHelpListCanBePlantedIn".Translate(),
                                                      plant.sowTags.ToArray(), null, null ) );
            }

            // unlockable sowtags
            List<DefStringTriplet> unlockableSowtags = new List<DefStringTriplet>();
            foreach( AdvancedResearchDef def in DefDatabase<AdvancedResearchDef>.AllDefsListForReading )
            {
                if( !def.IsLockedOut() &&
                     !def.HideDefs &&
                     def.IsPlantToggle &&
                     def.thingDefs.Contains( thingDef ) )
                {
                    foreach( string sowTag in def.sowTags )
                    {
                        foreach( ResearchProjectDef res in def.researchDefs )
                        {
                            unlockableSowtags.Add( new DefStringTriplet( res, sowTag + " (", ")" ) );
                        }
                    }
                }
            }
            if( !unlockableSowtags.NullOrEmpty() )
            {
                linkParts.Add( new HelpDetailSection( "AutoHelpListUnlockableSowTags".Translate(), unlockableSowtags, null ) );
            }

            // wild biome tags
            var biomes = DefDatabase<BiomeDef>.AllDefsListForReading.Where( b => b.AllWildPlants.Contains( thingDef ) ).ToList();
            if( !biomes.NullOrEmpty() )
            {
                linkParts.Add( new HelpDetailSection( "AutoHelpListAppearsInBiomes".Translate(),
                                                      biomes.Select( r => r as Def ).ToList() ) );
            }
        }

        static void HelpPartsForAnimal( PawnKindDef kindDef, ref List<HelpDetailSection> statParts,
                                        ref List<HelpDetailSection> linkParts )
        {
            RaceProperties race = kindDef.race.race;
            float maxSize = race.lifeStageAges.Select( lsa => lsa.def.bodySizeFactor * race.baseBodySize ).Max();

            // set up vars
            List<Def> defs = new List<Def>();
            List<string> stringDescs = new List<string>();
            List<string> prefixes = new List<string>();
            List<String> suffixes = new List<string>();

            #region Health, diet and intelligence
            
            statParts.Add( new HelpDetailSection( null, 
                new []
                {
                    ( race.baseHealthScale * race.lifeStageAges.Last().def.healthScaleFactor ).ToStringPercent(),
                    race.lifeExpectancy.ToStringApproxAge(),
                    race.foodType.ToHumanString(),
                    race.trainableIntelligence.ToString()
                },
                new []
                {
                    "AutoHelpHealthScale".Translate(),
                    "AutoHelpLifeExpectancy".Translate(),
                    "AutoHelpDiet".Translate(),
                    "AutoHelpIntelligence".Translate()
                },
                null ) );

            #endregion

            #region Training

            if( race.Animal )
            {
                List<DefStringTriplet> DST = new List<DefStringTriplet>();
                
                foreach( TrainableDef def in DefDatabase<TrainableDef>.AllDefsListForReading )
                {
                    // skip if explicitly disallowed
                    if( !race.untrainableTags.NullOrEmpty() &&
                         race.untrainableTags.Any( tag => def.MatchesTag( tag ) ) )
                    {
                        continue;
                    }

                    // explicitly allowed tags.
                    if( !race.trainableTags.NullOrEmpty() &&
                         race.trainableTags.Any( tag => def.MatchesTag( tag ) ) &&
                         maxSize >= def.minBodySize )
                    {
                        DST.Add( new DefStringTriplet( def ) );
                        continue;
                    }

                    // normal proceedings
                    if( maxSize >= def.minBodySize &&
                         race.trainableIntelligence >= def.requiredTrainableIntelligence &&
                         def.defaultTrainable )
                    {
                        DST.Add( new DefStringTriplet( def ) );
                    }
                }

                if( DST.Count > 0 )
                {
                    linkParts.Add( new HelpDetailSection(
                                       "AutoHelpListTrainable".Translate(),
                                       DST, null ) );
                }
                defs.Clear();
            }

            #endregion

            #region Lifestages

            List<float> ages = race.lifeStageAges.Select( age => age.minAge ).ToList();
            for( int i = 0; i < race.lifeStageAges.Count; i++ )
            {
                defs.Add( race.lifeStageAges[i].def );
                // final lifestage
                if( i == race.lifeStageAges.Count - 1 )
                {
                    suffixes.Add( ages[i].ToStringApproxAge() + " - ~" +
                                  race.lifeExpectancy.ToStringApproxAge() );
                }
                else
                // other lifestages
                {
                    suffixes.Add( ages[i].ToStringApproxAge() + " - " +
                                  ages[i + 1].ToStringApproxAge() );
                }
            }

            // only print if interesting (i.e. more than one lifestage).
            if( defs.Count > 1 )
            {
                statParts.Add( new HelpDetailSection(
                    "AutoHelpListLifestages".Translate(),
                    defs,
                    null,
                    suffixes.ToArray() ) );
            }
            defs.Clear();
            suffixes.Clear();

            #endregion

            #region Reproduction

            if( kindDef.race.HasComp( typeof( CompEggLayer ) ) )
            {
                // egglayers
                var eggComp =  kindDef.race.GetCompProperties<CompProperties_EggLayer>();
                string range;
                if( eggComp.eggCountRange.min == eggComp.eggCountRange.max )
                {
                    range = eggComp.eggCountRange.min.ToString();
                }
                else
                {
                    range = eggComp.eggCountRange.ToString();
                }
                stringDescs.Add( "AutoHelpEggLayer".Translate( range,
                    ( eggComp.eggLayIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear ).ToStringApproxAge() ) );

                statParts.Add( new HelpDetailSection(
                                   "AutoHelpListReproduction".Translate(),
                                   stringDescs.ToArray(), null, null ) );
                stringDescs.Clear();
            }
            else if(
                ( race.hasGenders )&&
                ( race.lifeStageAges.Any( lsa => lsa.def.reproductive ) )
            )
            {
                // mammals
                List<StringDescTriplet> SDT = new List<StringDescTriplet>();
                SDT.Add( new StringDescTriplet( 
                    ( race.gestationPeriodDays * GenDate.TicksPerDay / GenDate.TicksPerYear ).ToStringApproxAge(),
                    "AutoHelpGestationPeriod".Translate() ) );

                if(
                    ( race.litterSizeCurve != null )&&
                    ( race.litterSizeCurve.PointsCount >= 3 )
                )
                {
                    // if size is three, there is actually only one option (weird boundary restrictions by Tynan require a +/- .5 min/max)
                    if( race.litterSizeCurve.PointsCount == 3 )
                    {
                        SDT.Add( new StringDescTriplet(
                            race.litterSizeCurve[1].x.ToString(),
                            "AutoHelpLitterSize".Translate() ) );
                    }

                    // for the same reason, if more than one choice, indeces are second and second to last.
                    else
                    {
                        SDT.Add( new StringDescTriplet(
                            race.litterSizeCurve[1].x.ToString() + " - " +
                            race.litterSizeCurve[race.litterSizeCurve.PointsCount - 2].x.ToString(),
                            "AutoHelpLitterSize".Translate() ) );
                        stringDescs.Add( "AutoHelpLitterSize".Translate(  ) );
                    }
                }
                else
                {
                    // if litterSize is not defined in XML, it's always 1
                    SDT.Add( new StringDescTriplet(
                        "1",
                        "AutoHelpLitterSize".Translate() ) );
                }

                statParts.Add( new HelpDetailSection(
                                   "AutoHelpListReproduction".Translate(),
                                   null, SDT ) );
            }

            #endregion

            #region Biomes

            var kinds = DefDatabase<PawnKindDef>.AllDefsListForReading.Where( t => t.race ==  kindDef.race );
            foreach( PawnKindDef kind in kinds )
            {
                foreach( BiomeDef biome in DefDatabase<BiomeDef>.AllDefsListForReading )
                {
                    if( biome.AllWildAnimals.Contains( kind ) )
                    {
                        defs.Add( biome );
                    }
                }
            }
            defs = defs.Distinct().ToList();

            if( !defs.NullOrEmpty() )
            {
                linkParts.Add( new HelpDetailSection(
                                   "AutoHelpListAppearsInBiomes".Translate(),
                                   defs ) );
            }
            defs.Clear();

            #endregion

            #region Butcher products

            if( race.IsFlesh )
            {
                // fleshy pawns ( meat + leather )
                defs.Add( race.meatDef );
                prefixes.Add( "~" + maxSize * StatDefOf.MeatAmount.defaultBaseValue );

                if( race.leatherDef != null )
                {
                    defs.Add( race.leatherDef );
                    prefixes.Add( "~" + maxSize * kindDef.race.statBases.Find( sb => sb.stat == StatDefOf.LeatherAmount ).value );
                }

                statParts.Add( new HelpDetailSection(
                    "AutoHelpListButcher".Translate(),
                    defs,
                    prefixes.ToArray() ) );
            }
            else if(
                ( race.IsMechanoid )&&
                ( !kindDef.race.butcherProducts.NullOrEmpty() )
            )
            {
                // metallic pawns ( mechanoids )
                linkParts.Add( new HelpDetailSection(
                                   "AutoHelpListDisassemble".Translate(),
                                    kindDef.race.butcherProducts.Select( tc => tc.thingDef ).ToList().ConvertAll( def => (Def)def ),
                                    kindDef.race.butcherProducts.Select( tc => tc.count.ToString() ).ToArray() ) );
            }
            defs.Clear();
            prefixes.Clear();

            #endregion

            #region Milking products

            // Need to handle subclasses (such as CompMilkableRenameable)
            var milkComp = kindDef.race.comps.Find( c => (
                ( c.compClass == typeof( CompMilkable ) )||
                ( c.compClass.IsSubclassOf( typeof( CompMilkable ) ) )
            ) ) as CompProperties_Milkable;
            if( milkComp != null )
            {
                defs.Add( milkComp.milkDef );
                prefixes.Add( milkComp.milkAmount.ToString() );
                suffixes.Add( "AutoHelpEveryX".Translate( ( (float)milkComp.milkIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear ).ToStringApproxAge() ) );

                linkParts.Add( new HelpDetailSection(
                                   "AutoHelpListMilk".Translate(),
                                   defs,
                                   prefixes.ToArray(),
                                   suffixes.ToArray() ) );
            }
            defs.Clear();
            prefixes.Clear();
            suffixes.Clear();

            #endregion

            #region Shearing products

            // Need to handle subclasses (such as CompShearableRenameable)
            var shearComp = kindDef.race.comps.Find( c => (
                ( c.compClass == typeof( CompShearable ) )||
                ( c.compClass.IsSubclassOf( typeof( CompShearable ) ) )
            ) ) as CompProperties_Shearable;
            if( shearComp != null )
            {
                defs.Add( shearComp.woolDef );
                prefixes.Add( shearComp.woolAmount.ToString() );
                suffixes.Add( "AutoHelpEveryX".Translate( ( (float)shearComp.shearIntervalDays * GenDate.TicksPerDay / GenDate.TicksPerYear ).ToStringApproxAge() ) );

                linkParts.Add( new HelpDetailSection(
                                   "AutoHelpListShear".Translate(),
                                   defs,
                                   prefixes.ToArray(),
                                   suffixes.ToArray() ) );
            }
            defs.Clear();
            prefixes.Clear();
            suffixes.Clear();

            #endregion

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
