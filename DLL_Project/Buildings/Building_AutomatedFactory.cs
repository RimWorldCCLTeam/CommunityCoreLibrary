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

        #endregion

        #region Class Constructor

        public                              Building_AutomatedFactory()
        {
            nextProductIndex = 0;
            currentRecipe = null;
            currentProductionTick = 0;
            productionAllowances = new Dictionary<ThingDef, Allowances>();
        }

        #endregion

        #region Base Class Overrides

#if DEBUG
        public override void                SpawnSetup()
        {
            base.SpawnSetup();
            if( CompAutomatedFactory == null )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "Building_AutomatedFactory requires CompAutomatedFactory" );
                return;
            }
            if( CompAutomatedFactory.Properties == null )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "CompAutomatedFactory requires CompProperties_AutomatedFactory" );
                return;
            }
            if( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.Invalid )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "CompAutomatedFactory.outputVector is invalid" );
                return;
            }
            if( CompAutomatedFactory.Properties.productionMode == FactoryProductionMode.None )
            {
                CCL_Log.TraceMod(
                    this.def,
                    Verbosity.FatalErrors,
                    "CompAutomatedFactory.productionMode is invalid" );
                return;
            }
        }
#endif
        
        public override void                ExposeData()
        {
            // Scribe base data
            base.ExposeData();

            // Scribe current recipe
            string recipe = string.Empty;
            if( currentRecipe != null )
            {
                recipe = currentRecipe.defName;
            }
            Scribe_Values.LookValue<string>( ref recipe, "currentRecipe", string.Empty );
            if( !recipe.NullOrEmpty() )
            {
                currentRecipe = DefDatabase<RecipeDef>.GetNamed( recipe, true );
            }

            // Scribe current production tick
            Scribe_Values.LookValue<int>( ref currentProductionTick, "currentProductionTick", 0 );

            // Scribe recipe allowances
            var pa = new Dictionary<RecipeDef, bool>();
            if( Scribe.mode == LoadSaveMode.Saving )
            {
                foreach( var entry in productionAllowances )
                {
                    if( !pa.ContainsKey( entry.Value.recipe ) )
                    {
                        pa.Add( entry.Value.recipe, entry.Value.allowed );
                    }
                }
            }
            Scribe_Collections.LookDictionary<RecipeDef,bool>( ref pa, "productionAllowances", LookMode.DefReference, LookMode.Value );
            if( Scribe.mode == LoadSaveMode.LoadingVars )
            {
                foreach( var pair in pa )
                {
                    var key = pair.Key.products[0].thingDef;
                    SetAllowed( pair.Key, pair.Value );
                }
            }

            // Scribe current thing
            Scribe_Deep.LookDeep<Thing>( ref currentThing, "currentThing", null );
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
            if( !CompPowerTrader.PowerOn )
            {
                return;
            }
            if(
                ( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.Invalid )||
                ( CompAutomatedFactory.Properties.outputVector == FactoryOutputVector.DirectToPawn )
            )
            {
                return;
            }
            currentProductionTick -= ticks;
            if( currentProductionTick > 0 )
            {
                return;
            }
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
            if( currentThing.stackCount > 0 )
            {
                GenSpawn.Spawn( currentThing, useCell );
            }
            currentThing = null;
            currentProductionTick = 0;
        }

        private void                        RescanTick()
        {
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
            resourceFilter = null;
            CompHopperUser.ResetResourceSettings();
            CompHopperUser.FindAndProgramHoppers();
        }

        public ThingFilter                  ResourceFilter
        {
            get
            {
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
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                return allowance.allowed;
            }
            return false;
        }

        public bool                         GetAllowed( RecipeDef recipeDef )
        {
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
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                return allowance;
            }
            return null;
        }

        public Allowances                   GetAllowance( RecipeDef recipeDef )
        {
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
            stackWith = null;
            dropCell = IntVec3.Invalid;
            switch( CompAutomatedFactory.Properties.outputVector )
            {
                case FactoryOutputVector.Ground:
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
                        else if(
                            ( cellThing.CanStackWith( currentThing ) )&&
                            ( cellThing.stackCount < cellThing.def.stackLimit )
                        )
                        {
                            addToPrefered = true;
                            if( stackWith == null )
                            {
                                stackWith = cellThing;
                            }
                        }
                        else if(
                            ( cellThing.def.EverHaulable )||
                            ( cellThing.def.IsEdifice() )||
                            ( cellThing.def.passability == Traversability.Impassable )
                        )
                        {
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
                if(
                    ( preferedCells.NullOrEmpty() )&&
                    ( usableCells.NullOrEmpty() )&&
                    ( stackWith == null )
                )
                {
                    // No place to put new thing
                    return false;
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
                            }
                            else if( cellThing.def.EverHaulable )
                            {
                                preferedCells.Remove( cell );
                                break;
                            }
                        }
                    }
                    if(
                        ( preferedCells.NullOrEmpty() )&&
                        ( stackWith == null )
                    )
                    {
                        return false;
                    }
                    if( !preferedCells.NullOrEmpty() )
                    {
                        dropCell = preferedCells.RandomElement();
                    }
                }
                else if( !usableCells.NullOrEmpty() )
                {
                    dropCell = usableCells.RandomElement();
                }
                break;

                case FactoryOutputVector.InteractionCell:
                dropCell = this.InteractionCell;
                foreach( var cellThing in dropCell.GetThingList() )
                {
                    if( cellThing.def.passability == Traversability.Impassable )
                    {
                        return false;
                    }
                    if(
                        ( cellThing.CanStackWith( currentThing ) )&&
                        ( cellThing.stackCount < cellThing.def.stackLimit )
                    )
                    {
                        stackWith = cellThing;
                    }
                    else if( cellThing.def.EverHaulable )
                    {
                        return false;
                    }
                    if( cellThing is IStoreSettingsParent )
                    {
                        var settings = cellThing as IStoreSettingsParent;
                        if( !settings.GetStoreSettings().AllowedToAccept( currentThing ) )
                        {
                            return false;
                        }
                    }
                }
                break;
            }
            if(
                ( dropCell != IntVec3.Invalid )||
                ( stackWith != null )
            )
            {
                // Found some place for it
                return true;
            }
            // All output cells are blocked
            return false;
        }

        private RecipeDef                   TryGetProductionReadyRecipeFor( ThingDef thingDef )
        {
            Allowances allowance;
            if( productionAllowances.TryGetValue( thingDef, out allowance ) )
            {
                if(
                    ( allowance.allowed )&&
                    ( allowance.recipe.researchPrerequisite.IsFinished )&&
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
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return 50;
            }
            return (int) recipe.workAmount;
        }

        public bool                         CanDispenseNow( ThingDef thingDef )
        {
            if( CompPowerTrader.PowerOn )
            {
                return HasEnoughResourcesInHoppersFor( thingDef );
            }
            return false;
        }

        public bool                         CanProduce( ThingDef thingDef )
        {
            return FindRecipeForProduct( thingDef ) != null;
        }

        public Building                     AdjacentReachableHopper( Pawn reacher )
        {
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
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return false;
            }
            return CompHopperUser.EnoughResourcesInHoppers( recipe );
        }

        public List<ThingDef>               AllProducts()
        {
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
            var recipe = FindRecipeForProduct( thingDef );
            if( recipe == null )
            {
                return (Thing) null;
            }
            List<ThingAmount> chosen = new List<ThingAmount>();
            if( !CompHopperUser.RemoveResourcesFromHoppers( recipe, chosen ) )
            {
                return null;
            }
            var thing = ThingMaker.MakeThing( thingDef );
            thing.stackCount = recipe.products[0].count;
            if(
                ( thingDef.thingClass == typeof( Meal ) )||
                ( thingDef.thingClass.IsSubclassOf( typeof( Meal ) ) )
            )
            {
                var meal = (Meal) thing;
                foreach( var ingredient in chosen )
                {
                    meal.RegisterIngredient( ingredient.thing.def );
                }
            }
            return thing;
        }

        #endregion

    }

}
