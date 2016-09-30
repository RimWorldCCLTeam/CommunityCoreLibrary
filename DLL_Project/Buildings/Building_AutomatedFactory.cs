using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary
{

    [StaticConstructorOnStartup]
    public class Building_AutomatedFactory : Building, IHopperUser
    {

        public class Allowances
        {
            public RecipeDef                recipe;
            public bool                     allowed;

            public                          Allowances( RecipeDef recipe, bool allowed )
            {
                this.recipe = recipe;
                this.allowed = allowed;
            }

        }

        private Dictionary<ThingDef,Allowances> productionAllowances;
        private Dictionary<RecipeDef, bool>     recipeAllowances;

        private RecipeDef                   currentRecipe;

        private int                         currentProductionTick;
        private Thing                       currentThing;

        private int                         nextProductIndex;
        private int                         currentRecipeCount = -1;

        private List<IntVec3>               _adjacentNeighbouringCells;

        private CompPowerTrader             _CompPowerTrader = null;
        private CompHopperUser              _CompHopperUser = null;
        private CompAutomatedFactory        _CompAutomatedFactory = null;

        #region Reflections

        private List<IntVec3>               AdjacentNeighbouringCells
        {
            get
            {
                //Log.Message( string.Format( "{0}.AdjacentNeighbouringCells()", this.ThingID ) );
                if( _adjacentNeighbouringCells == null )
                {
                    _adjacentNeighbouringCells = GenAdj.CellsAdjacentCardinal( this ).ToList();
                }
                return _adjacentNeighbouringCells;
            }
        }

        #endregion

        #region Properties

        public RecipeDef                    CurrentRecipe
        {
            get
            {
                return currentRecipe;
            }
        }

        public CompPowerTrader              CompPowerTrader
        {
            get
            {
                if( _CompPowerTrader == null )
                {
                    _CompPowerTrader = this.GetComp<CompPowerTrader>();
                }
                return _CompPowerTrader;
            }
        }

        public CompHopperUser               CompHopperUser
        {
            get
            {
                if( _CompHopperUser == null )
                {
                    _CompHopperUser = this.GetComp<CompHopperUser>();
                }
                return _CompHopperUser;
            }
        }

        public CompAutomatedFactory         CompAutomatedFactory
        {
            get
            {
                if( _CompAutomatedFactory == null )
                {
                    _CompAutomatedFactory = this.GetComp<CompAutomatedFactory>();
                }
                return _CompAutomatedFactory;
            }
        }

        public static bool                  DefOutputToPawnsDirectly( ThingDef thingDef )
        {
            var compAutomatedFactory = thingDef.GetCompProperty<CompProperties_AutomatedFactory>();
            if( compAutomatedFactory == null )
            {
                return false;
            }
            if( compAutomatedFactory.outputVector != FactoryOutputVector.DirectToPawn )
            {
                return false;
            }
            return compAutomatedFactory.productionMode == FactoryProductionMode.PawnInteractionOnly;
        }

        public bool                         OutputToPawnsDirectly
        {
            get
            {
                return DefOutputToPawnsDirectly( this.def );
            }
        }

        #endregion

        #region Class Constructor

        public                              Building_AutomatedFactory()
        {
            //Log.Message( "Building_AutomatedFactory.cTor()" );
            nextProductIndex = 0;
            currentRecipe = null;
            currentProductionTick = 0;
            productionAllowances = new Dictionary<ThingDef, Allowances>();
            recipeAllowances = new Dictionary<RecipeDef, bool>();
        }

        #endregion

        #region Base Class Overrides

#if DEBUG
        public override void                SpawnSetup()
        {
            //Log.Message( string.Format( "{0}.SpawnSetup()", this.ThingID ) );
            base.SpawnSetup();
            if( CompAutomatedFactory == null )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "Requires CompAutomatedFactory",
                    "Building_AutomatedFactory"
                );
                return;
            }
            if( CompAutomatedFactory.Properties == null )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "Requires CompProperties_AutomatedFactory",
                    "CompAutomatedFactory"
                );
                return;
            }
            if( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.Invalid )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "outputVector is Invalid",
                    "CompAutomatedFactory"
                );
                return;
            }
            if( CompAutomatedFactory.Properties.productionMode == FactoryProductionMode.None )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "productionMode is None",
                    "CompAutomatedFactory"
                );
                return;
            }
            if(
                ( CompAutomatedFactory.Properties.productionMode != FactoryProductionMode.PawnInteractionOnly )&&
                ( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.DirectToPawn )
            )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "productionMode is PawnInteractionOnly but outputVector is not DirectToPawn",
                    "CompAutomatedFactory"
                );
                return;
            }
            if(
                ( CompAutomatedFactory.Properties.productionMode == FactoryProductionMode.Automatic )&&
                (
                    ( CompAutomatedFactory.Properties.outputVector != FactoryOutputVector.Ground )&&
                    ( CompAutomatedFactory.Properties.outputVector != FactoryOutputVector.InteractionCell )
                )
            )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "productionMode is Automatic but outputVector is not Ground or InteractionCell",
                    "CompAutomatedFactory"
                );
                return;
            }
        }
#endif

        public override void                ExposeData()
        {
            //Log.Message( string.Format( "Building_AutomatedFactory.ExposeData( {0} )", Scribe.mode.ToString() ) );
            // Scribe base data
            base.ExposeData();

            string recipe = string.Empty;
            if( Scribe.mode == LoadSaveMode.Saving )
            {
                if( currentRecipe != null )
                {
                    recipe = currentRecipe.defName;
                }
                foreach( var entry in productionAllowances )
                {
                    if( !recipeAllowances.ContainsKey( entry.Value.recipe ) )
                    {
                        recipeAllowances.Add( entry.Value.recipe, entry.Value.allowed );
                    }
                }
            }

            // Scribe data
            Scribe_Values.LookValue<string>( ref recipe, "currentRecipe", string.Empty );
            Scribe_Values.LookValue<int>( ref currentProductionTick, "currentProductionTick", 0 );
            Scribe_Collections.LookDictionary<RecipeDef,bool>( ref recipeAllowances, "productionAllowances", LookMode.DefReference, LookMode.Value );
            Scribe_Deep.LookDeep<Thing>( ref currentThing, "currentThing", null );

            // Resolve cross-references
            if( Scribe.mode == LoadSaveMode.ResolvingCrossRefs )
            {
                if( !recipe.NullOrEmpty() )
                {
                    currentRecipe = DefDatabase<RecipeDef>.GetNamed( recipe, true );
                }
                foreach( var pair in recipeAllowances )
                {
                    var key = pair.Key.products[0].thingDef;
                    SetAllowed( pair.Key, pair.Value );
                }
                //ResetAndReprogramHoppers();
                currentRecipeCount = this.def.AllRecipes.Count;
            }
        }

        public override void                Tick()
        {
            base.Tick();
            ProductionTick( 1 );
            if( !this.IsHashIntervalTick( 60 ) )
            {
                return;
            }
            RescanTick();
        }

        public override void                TickRare()
        {
            base.TickRare();
            ProductionTick( 250 );
            RescanTick();
        }

        public override string              GetInspectString()
        {
            string str = base.GetInspectString();
            if( currentRecipe != null )
            {
                str += currentRecipe.jobString;
                str += "\n";
                str += "Building_AutomatedFactory_WorkRemaining".Translate( currentProductionTick );
            }
            else if( currentThing != null )
            {
                str += "Building_AutomatedFactory_WaitingToDispense".Translate( currentThing.def.label );
            }
            return str;
        }

        #endregion

        #region Tickers

        private void                        ProductionTick( int ticks )
        {
            //Log.Message( string.Format( "{0}.ProductionTick( {1} )", this.ThingID, ticks ) );
            if( !CompPowerTrader.PowerOn )
            {
                return;
            }
            if( CompAutomatedFactory.Properties.productionMode != FactoryProductionMode.Automatic )
            {
                return;
            }
            currentProductionTick -= ticks;
            if( currentProductionTick > 0 )
            {
                return;
            }
            // Set to 30 for now, if something actually starts production
            // then this will be changed to match the recipe
            currentProductionTick = 30;

            // Find something to produce
            if( currentThing == null )
            {
                currentThing = NextProductToProduce();
                return;
            }

            // Recipe is done, try to dispense
            currentRecipe = null;

            // Find a cell or stack to dispense the product to
            IntVec3 useCell = IntVec3.Invalid;
            Thing stackWithThing = null;

            if( !OutputThingTo( out stackWithThing, out useCell ) )
            {
                // No place to put the thing
                return;
            }

            if( stackWithThing != null )
            {
                if(
                    ( !stackWithThing.TryAbsorbStack( currentThing, true ) )&&
                    ( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.InteractionCell )
                )
                {
                    return;
                }
            }
            if(
                ( currentThing.stackCount < 1 )||
                ( currentThing.Destroyed )
            )
            {
                currentThing = null;
                currentProductionTick = 0;
                return;
            }
            if(
                ( useCell != IntVec3.Invalid )&&
                ( currentThing.stackCount > 0 )
            )
            {
                GenSpawn.Spawn( currentThing, useCell );
                currentThing = null;
                currentProductionTick = 0;
            }
        }

        private void                        RescanTick()
        {
            //Log.Message( string.Format( "{0}.RescanTick()", this.ThingID ) );
            if( currentRecipeCount != this.def.AllRecipes.Count )
            {
                ResetAndReprogramHoppers();
            }
        }

        #endregion

        #region IHopperUser

        private ThingFilter                 resourceFilter = null;

        public void                         ResetAndReprogramHoppers()
        {
            //Log.Message( string.Format( "{0}.ResetAndReprogramHoppers()", this.ThingID ) );
            resourceFilter = null;
            CompHopperUser.ResetResourceSettings();
            CompHopperUser.FindAndProgramHoppers();
        }

        public ThingFilter                  ResourceFilter
        {
            get
            {
                //Log.Message( string.Format( "{0}.ResourceFilter", this.ThingID ) );
                if( def.AllRecipes.NullOrEmpty() )
                {
                    CCL_Log.TraceMod(
                        this.def,
                        Verbosity.NonFatalErrors,
                        "No recipes to build resources filter from" );
                    return null;
                }

                if( resourceFilter == null )
                {
                    resourceFilter = new ThingFilter();
                    //resourceFilter.allowedHitPointsConfigurable = true;
                    //resourceFilter.allowedQualitiesConfigurable = false;

                    productionAllowances.Clear();

                    // Scan recipes to build lists and resource filter
                    foreach( var recipe in def.AllRecipes )
                    {
                        if( recipe.products.Count != 1 )
                        {
                            CCL_Log.TraceMod(
                                def,
                                Verbosity.NonFatalErrors,
                                "Building_AutomatedFactory can only use recipes which have one product :: '" + recipe.defName + "' contains " + recipe.products.Count + " products!" );
                        }
                        else
                        {
                            var product = recipe.products[ 0 ].thingDef;
                            Allowances allowance;
                            if( productionAllowances.TryGetValue( product, out allowance ) )
                            {
                                if( allowance.recipe != recipe )
                                {
                                    // Different recipe for same product
                                    CCL_Log.TraceMod(
                                        def,
                                        Verbosity.NonFatalErrors,
                                        "Building_AutomatedFactory can not have multiple recipes producing the same thing :: A recipe which produces '" + product.defName + "' already exists!" );
                                }
                                else if( !CompHopperUser.IsRecipeInFilter( recipe ) )
                                {
                                    // Same recipe for product (may happen immediately after loading a save game
                                    // or a recipe is unlocked via research as the dictionary is not cleared)
                                    CompHopperUser.MergeRecipeIntoFilter( resourceFilter, recipe );
                                }
                            }
                            else
                            {
                                productionAllowances.Add( product, new Allowances( recipe, true ) );
                                CompHopperUser.MergeRecipeIntoFilter( resourceFilter, recipe );
                            }
                        }
                    }
                    currentRecipeCount = this.def.AllRecipes.Count;
                }
                return resourceFilter;
            }
        }

        #endregion

        #region Production Allowances

        public void                         SetAllowed( ThingDef thingDef, bool allowed )
        {
            //Log.Message( string.Format( "{0}.SetAllowed( {1}, {2} )", this.ThingID, thingDef == null ? "null" : thingDef.defName, allowed ) );
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                allowed &= (
                    ( allowance.recipe.researchPrerequisite == null )||
                    ( allowance.recipe.researchPrerequisite.IsFinished )
                );
                allowance.allowed = allowed;
                productionAllowances[ thingDef ] = allowance;
            }
        }

        public void                         SetAllowed( RecipeDef recipeDef, bool allowed )
        {
            //Log.Message( string.Format( "{0}.SetAllowed( {1}, {2} )", this.ThingID, recipeDef == null ? "null" : recipeDef.defName, allowed ) );
            var inAllowed = allowed;
            var product = recipeDef.products[ 0 ].thingDef;
            allowed &= (
                ( recipeDef.researchPrerequisite == null )||
                ( recipeDef.researchPrerequisite.IsFinished )
            );
            Allowances allowance;
            if( productionAllowances.TryGetValue( product, out allowance ) )
            {
                allowance.allowed = allowed;
                productionAllowances[ product ].allowed = allowed;
            }
            else
            {
                allowance = new Allowances( recipeDef, allowed );
                productionAllowances.Add( product, allowance );
            }
        }

        public bool                         GetAllowed( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.GetAllowed( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                return allowance.allowed;
            }
            return false;
        }

        public bool                         GetAllowed( RecipeDef recipeDef )
        {
            //Log.Message( string.Format( "{0}.GetAllowed( {1} )", this.ThingID, recipeDef == null ? "null" : recipeDef.defName ) );
            foreach( var pair in productionAllowances )
            {
                if( pair.Value.recipe == recipeDef )
                {
                    return pair.Value.allowed;
                }
            }
            return false;
        }

        public Allowances                   GetAllowance( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.GetAllowance( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                return allowance;
            }
            return null;
        }

        public Allowances                   GetAllowance( RecipeDef recipeDef )
        {
            //Log.Message( string.Format( "{0}.GetAllowance( {1} )", this.ThingID, recipeDef == null ? "null" : recipeDef.defName ) );
            foreach( var pair in productionAllowances )
            {
                if( pair.Value.recipe == recipeDef )
                {
                    return pair.Value;
                }
            }
            return null;
        }

        #endregion

        #region Internal Interface

        private Thing                       NextProductToProduce()
        {
            //Log.Message( string.Format( "{0}.NextProductToProduce()", this.ThingID ) );
            if( currentRecipe != null )
            {
                return null;
            }

            var products = AllProducts();
            RecipeDef recipe = null;
            for( int index = nextProductIndex; index < products.Count; index++ )
            {
                var thingDef = products[ index ];
                recipe = TryGetProductionReadyRecipeFor( thingDef );
                if( recipe != null )
                {
                    break;
                }
            }
            if(
                ( recipe == null )&&
                ( nextProductIndex > 0 )
            )
            {
                for( int index = 0; index < nextProductIndex; index++ )
                {
                    var thingDef = products[ index ];
                    recipe = TryGetProductionReadyRecipeFor( thingDef );
                    if( recipe != null )
                    {
                        break;
                    }
                }
            }
            nextProductIndex++;
            nextProductIndex %= products.Count;
            if( recipe == null )
            {
                return null;
            }
            currentRecipe = recipe;
            currentProductionTick = (int) currentRecipe.workAmount;
            return TryProduceThingDef( currentRecipe.products[0].thingDef );
        }

        private bool                        OutputThingTo( out Thing stackWith, out IntVec3 dropCell )
        {
            //Log.Message( string.Format( "{0}.OutputThingTo()", this.ThingID ) );
            stackWith = null;
            dropCell = IntVec3.Invalid;

            if( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.Ground )
            {
                var usableCells = new List<IntVec3>();
                var preferedCells = new List<IntVec3>();

                foreach( var cell in AdjacentNeighbouringCells )
                {
                    bool addToUsable = true;
                    bool addToPrefered = false;
                    foreach( var cellThing in cell.GetThingList() )
                    {
                        if( cellThing is IStoreSettingsParent )
                        {
                            addToUsable = false;
                            var settings = cellThing as IStoreSettingsParent;
                            if( settings.GetStoreSettings().AllowedToAccept( currentThing ) )
                            {
                                addToPrefered = true;
                            }
                        }
                        if(
                            ( cellThing.CanStackWith( currentThing ) )&&
                            ( cellThing.stackCount < cellThing.def.stackLimit )
                        )
                        {
                            stackWith = cellThing;
                            return true;
                        }
                        if(
                            ( cellThing.def.EverHaulable )||
                            ( cellThing.def.IsEdifice() )||
                            ( cellThing.def.passability == Traversability.Impassable )
                        )
                        {
                            addToPrefered = false;
                            addToUsable = false;
                        }
                    }
                    if( addToUsable )
                    {
                        usableCells.Add( cell );
                    }
                    if( addToPrefered )
                    {
                        preferedCells.Add( cell );
                    }
                }
                if( !preferedCells.NullOrEmpty() )
                {
                    // Try a prefered cell first
                    for( int index = 0; index < preferedCells.Count; ++index )
                    {
                        var cell = preferedCells[ index ];
                        foreach( var cellThing in cell.GetThingList() )
                        {
                            if(
                                ( cellThing.CanStackWith( currentThing ) )&&
                                ( cellThing.stackCount < cellThing.def.stackLimit )
                            )
                            {
                                stackWith = cellThing;
                                return true;
                            }
                        }
                    }
                    dropCell = preferedCells.RandomElement();
                    return true;
                }
                if( !usableCells.NullOrEmpty() )
                {
                    dropCell = usableCells.RandomElement();
                    return true;
                }

                // No place to put new thing
                return false;
            }

            if( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.InteractionCell )
            {
                foreach( var cellThing in this.InteractionCell.GetThingList() )
                {
                    if(
                        ( cellThing.CanStackWith( currentThing ) )&&
                        ( cellThing.stackCount < cellThing.def.stackLimit )
                    )
                    {
                        stackWith = cellThing;
                    }
                    else if( cellThing is IStoreSettingsParent )
                    {
                        var settings = cellThing as IStoreSettingsParent;
                        if( !settings.GetStoreSettings().AllowedToAccept( currentThing ) )
                        {
                            return false;
                        }
                    }
                    else if(
                        ( cellThing.def.EverHaulable )||
                        ( cellThing.def.IsEdifice() )||
                        ( cellThing.def.passability == Traversability.Impassable )
                    )
                    {
                        // Interaction cell is blocked
                        return false;
                    }
                }

                dropCell = this.InteractionCell;
                return true;
            }

            // All output cells are blocked
            return false;
        }

        private RecipeDef                   TryGetProductionReadyRecipeFor( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.TryGetProductionReadyRecipeFor( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                if(
                    ( allowance.allowed )&&
                    (
                        ( allowance.recipe.researchPrerequisite == null )||
                        ( allowance.recipe.researchPrerequisite.IsFinished )
                    )&&
                    ( HasEnoughResourcesInHoppersFor( thingDef ) )
                )
                {
                    return allowance.recipe;
                }
            }
            return (RecipeDef) null;
        }

        #endregion

        #region Public Interface

        public RecipeDef                    FindRecipeForProduct( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.FindRecipeForProduct( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                if(
                    ( allowance.recipe.researchPrerequisite == null )||
                    ( allowance.recipe.researchPrerequisite.IsFinished )
                )
                {
                    return allowance.recipe;
                }
            }
            return (RecipeDef) null;
        }

        public int                          ProductionTicks( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.ProductionTicks( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return 50;
            }
            return (int) recipe.workAmount;
        }

        public bool                         CanDispenseNow( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.CanDispenseNow( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            if( CompPowerTrader.PowerOn )
            {
                return HasEnoughResourcesInHoppersFor( thingDef );
            }
            return false;
        }

        public bool                         CanProduce( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.CanProduce( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            return FindRecipeForProduct( thingDef ) != null;
        }

        public Building                     AdjacentReachableHopper( Pawn reacher )
        {
            //Log.Message( string.Format( "{0}.AdjacentReachableHopper( {1} )", this.ThingID, reacher == null ? "null" : reacher.NameStringShort ) );
            var hoppers = CompHopperUser.FindHoppers();
            if( !hoppers.NullOrEmpty() )
            {
                foreach( var hopper in hoppers )
                {
                    if(
                        reacher.CanReach(
                            ( TargetInfo )( ( Thing )hopper.parent ),
                            PathEndMode.Touch,
                            reacher.NormalMaxDanger(),
                            false )
                    )
                    {
                        return (Building) hopper.parent;
                    }
                }
            }
            return (Building) null;
        }

        public bool                         HasEnoughResourcesInHoppersFor( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.HasEnoughResourcesInHoppersFor( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return false;
            }
            return CompHopperUser.EnoughResourcesInHoppers( recipe );
        }

        public List<ThingDef>               AllProducts()
        {
            //Log.Message( string.Format( "{0}.AllProducts()", this.ThingID ) );
            var products = new List<ThingDef>();
            foreach( var pair in productionAllowances )
            {
                if(
                    ( pair.Value.recipe.researchPrerequisite == null )||
                    ( pair.Value.recipe.researchPrerequisite.IsFinished )
                )
                {
                    products.Add( pair.Value.recipe.products[ 0 ].thingDef );
                }
            }
            return products;
        }

        public List<ThingDef>               AllowedProducts()
        {
            //Log.Message( string.Format( "{0}.AllowedProducts()", this.ThingID ) );
            var products = new List<ThingDef>();
            foreach( var pair in productionAllowances )
            {
                if(
                    (
                        ( pair.Value.recipe.researchPrerequisite == null )||
                        ( pair.Value.recipe.researchPrerequisite.IsFinished )
                    )&&
                    ( pair.Value.allowed )
                )
                {
                    products.Add( pair.Value.recipe.products[ 0 ].thingDef );
                }
            }
            return products;
        }

        public ThingDef                     BestProduct( Func<ThingDef,bool> where, Func<ThingDef,ThingDef,int> sort )
        {
            //Log.Message( string.Format( "{0}.BestProduct()", this.ThingID ) );

            var thingDefs = AllProducts().Where( where.Invoke ).ToList();
            thingDefs.Sort( sort.Invoke );

            foreach( var thingDef in thingDefs )
            {
                var recipe = FindRecipeForProduct( thingDef );
                if( CompHopperUser.EnoughResourcesInHoppers( recipe ) )
                {
                    return thingDef;
                }
            }
            return (ThingDef) null;
        }

        public Thing                        TryProduceThingDef( ThingDef thingDef )
        {
            //Log.Message( string.Format( "{0}.TryProduceThingDef( {1} )", this.ThingID, thingDef == null ? "null" : thingDef.defName ) );
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return (Thing) null;
            }
            var chosen = new List<ThingAmount>();
            if( !CompHopperUser.RemoveResourcesFromHoppers( recipe, chosen ) )
            {
                return null;
            }
            var thing = ThingMaker.MakeThing( thingDef );
            thing.stackCount = recipe.products[0].count;
            var ingredients = thing.TryGetComp<CompIngredients>();
            if ( ingredients != null )
            {
                foreach( var ingredient in chosen )
                {
                    ingredients.RegisterIngredient( ingredient.thing.def );
                }
            }
            return thing;
        }

        #endregion

    }

}
