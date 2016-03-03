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

    public class Building_AutomatedFactory : Building, IHopperUser
    {

        private Dictionary<RecipeDef,bool>  productionAllowances;

        Dictionary<ThingDef,RecipeDef>      productRecipes;

        private RecipeDef                   currentRecipe;
        public RecipeDef                    CurrentRecipe
        {
            get
            {
                return currentRecipe;
            }
        }

        private int                         currentProductionTick;
        private Thing                       currentThing;
        private int                         nextProductIndex;

        private int                         currentRecipeCount = -1;

        private List<IntVec3>               _adjacentNeighbouringCells;
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

        #region Comp Properties

        private CompPowerTrader             _CompPowerTrader = null;
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

        private CompHopperUser              _CompHopperUser = null;
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

        private CompAutomatedFactory        _CompAutomatedFactory = null;
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
            productionAllowances = new Dictionary<RecipeDef, bool>();
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
            base.ExposeData();
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
            Scribe_Values.LookValue<int>( ref currentProductionTick, "currentProductionTick", 0 );
            Scribe_Collections.LookDictionary<RecipeDef,bool>( ref productionAllowances, "productionAllowances", LookMode.DefReference, LookMode.Value );
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

        private RecipeDef                   TryGetProductionReadyRecipeFor( ThingDef thingDef )
        {
            var recipe = FindRecipeForProduct( thingDef );
            bool allowed = false;
            if( productionAllowances.TryGetValue( recipe, out allowed ) )
            {
                if(
                    ( allowed )&&
                    ( HasEnoughResourcesInHoppersFor( thingDef ) )
                )
                {
                    return recipe;
                }
            }
            return (RecipeDef) null;
        }

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
                if( currentRecipe == null )
                {
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
                        return;
                    }
                    currentRecipe = recipe;
                }
                currentProductionTick = (int) currentRecipe.workAmount;
                currentThing = TryProduceThingDef( currentRecipe.products[0].thingDef );
                return;
            }

            // Recipe is done, try to dispense
            currentRecipe = null;

            // Find a cell or stack to dispense the product to
            IntVec3 useCell = IntVec3.Invalid;
            Thing stackWithThing = null;

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
                            if( stackWithThing == null )
                            {
                                stackWithThing = cellThing;
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
                    ( stackWithThing == null )
                )
                {
                    // No place to put new thing
                    return;
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
                                stackWithThing = cellThing;
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
                        ( stackWithThing == null )
                    )
                    {
                        return;
                    }
                    if( useCell == IntVec3.Invalid )
                    {
                        useCell = preferedCells.RandomElement();
                    }
                }
                else
                {
                    useCell = usableCells.RandomElement();
                }
                break;

            case FactoryOutputVector.InteractionCell:
                useCell = this.InteractionCell;
                foreach( var cellThing in useCell.GetThingList() )
                {
                    if( cellThing.def.passability == Traversability.Impassable )
                    {
                        return;
                    }
                    if( cellThing.CanStackWith( currentThing ) )
                    {
                        stackWithThing = cellThing;
                        break;
                    }
                    if( cellThing.def.EverHaulable )
                    {
                        return;
                    }
                    if( cellThing is IStoreSettingsParent )
                    {
                        var settings = cellThing as IStoreSettingsParent;
                        if( !settings.GetStoreSettings().AllowedToAccept( currentThing ) )
                        {
                            return;
                        }
                    }
                }
                break;
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

        #region Hopper Interface

        private ThingFilter                 resourceFilter = null;

        private void                        ResetAndReprogramHoppers()
        {
            resourceFilter = null;
            CompHopperUser.ResetResourceSettings();
            CompHopperUser.FindAndProgramHoppers();
        }

        public ThingFilter                  ResourceFilter
        {
            get
            {
                if( resourceFilter == null )
                {
                    resourceFilter = new ThingFilter();
                    resourceFilter.allowedHitPointsConfigurable = true;
                    resourceFilter.allowedQualitiesConfigurable = false;

                    productRecipes = new Dictionary<ThingDef,RecipeDef>();

                    // Scan recipes to build lists and resource filter
                    if( !def.AllRecipes.NullOrEmpty() )
                    {
                        
                        foreach( var recipe in def.AllRecipes )
                        {
                            bool addThisRecipe = true;
                            if( recipe.products.Count != 1 )
                            {
                                CCL_Log.TraceMod(
                                    def,
                                    Verbosity.NonFatalErrors,
                                    "Building_AutomatedFactory can only use recipes which have one product :: '" + recipe.defName + "' contains " + recipe.products.Count + " products!" );
                                addThisRecipe = false;
                            }
                            foreach( var product in recipe.products )
                            {
                                RecipeDef existingRecipeDef = null;
                                if( productRecipes.TryGetValue( product.thingDef, out existingRecipeDef ) )
                                {
                                    CCL_Log.TraceMod(
                                        def,
                                        Verbosity.NonFatalErrors,
                                        "Building_AutomatedFactory can not have multiple recipes producting the same thing :: A recipe which produces '" + product.thingDef.defName + "' already exists!" );
                                    addThisRecipe = false;
                                }
                            }
                            if( addThisRecipe )
                            {
                                if( !productionAllowances.ContainsKey( recipe ) )
                                {
                                    productionAllowances.Add( recipe, true );
                                }
                                productRecipes.Add( recipe.products[0].thingDef, recipe );
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

        #region System Interface

        private RecipeDef                   FindRecipeForProduct( ThingDef thingDef )
        {
            RecipeDef recipe = null;
            if( productRecipes.TryGetValue( thingDef, out recipe ) )
            {
                return recipe;
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
            foreach( var productRecipe in productRecipes )
            {
                products.Add( productRecipe.Key );
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
