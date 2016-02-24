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

    public class Building_FoodSynthesizer : Building, IHopperUser
    {

        private Dictionary<ThingDef, RecipeDef> productRecipes;
        private List<ThingDef>              potentialMeals;
        private List<ThingDef>              potentialAlcohol;

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

        #endregion

        #region Tickers

        public override void                Tick()
        {
            if( !this.IsHashIntervalTick( 60 ) )
            {
                return;
            }
            DoTicker();
        }

        public override void                TickRare()
        {
            DoTicker();
        }

        private void                        DoTicker()
        {
            if( currentRecipeCount != def.AllRecipes.Count )
            {
                ResetAndReprogramHoppers();
            }
        }

        #endregion

        #region Hopper Interface

        private int                         currentRecipeCount = -1;
        private ThingFilter                 resourceFilter = null;

        private void                        ResetAndReprogramHoppers()
        {
            currentRecipeCount = -1;
            resourceFilter = null;
            potentialMeals = null;
            potentialAlcohol = null;
            productRecipes = null;
            CompHopperUser.ResetResourceSettings();
            CompHopperUser.FindAndProgramHoppers();
        }

        private void                        MergeIngredientIntoFilter( IngredientCount ingredient )
        {
            if( ingredient.filter != null )
            {
                if( !ingredient.filter.categories.NullOrEmpty() )
                {
                    foreach( var category in ingredient.filter.categories )
                    {
                        var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                        resourceFilter.SetAllow( categoryDef, true );
                    }
                }
                if( !ingredient.filter.thingDefs.NullOrEmpty() )
                {
                    foreach( var thingDef in ingredient.filter.thingDefs )
                    {
                        resourceFilter.SetAllow( thingDef, true );
                    }
                }
            }
        }

        private void                        MergeExceptionsIntoFilter( ThingFilter exceptionFilter )
        {
            if( !exceptionFilter.exceptedCategories.NullOrEmpty() )
            {
                foreach( var category in exceptionFilter.exceptedCategories )
                {
                    var categoryDef = DefDatabase<ThingCategoryDef>.GetNamed( category, true );
                    resourceFilter.SetAllow( categoryDef, false );
                }
            }
            if( !exceptionFilter.exceptedThingDefs.NullOrEmpty() )
            {
                foreach( var thingDef in exceptionFilter.exceptedThingDefs )
                {
                    resourceFilter.SetAllow( thingDef, false );
                }
            }
        }

        private void                        MergeRecipeIntoFilter( RecipeDef recipe )
        {
            if( !recipe.ingredients.NullOrEmpty() )
            {
                foreach( var ingredient in recipe.ingredients )
                {
                    MergeIngredientIntoFilter( ingredient );
                }
            }
            if( recipe.defaultIngredientFilter != null )
            {
                MergeExceptionsIntoFilter( recipe.defaultIngredientFilter );
            }
            if( recipe.fixedIngredientFilter != null )
            {
                MergeExceptionsIntoFilter( recipe.fixedIngredientFilter );
            }
        }

        public ThingFilter                  ResourceFilter
        {
            get
            {
                if( resourceFilter == null )
                {
                    potentialAlcohol = new List<ThingDef>();
                    potentialMeals = new List<ThingDef>();
                    productRecipes = new Dictionary<ThingDef, RecipeDef>();

                    resourceFilter = new ThingFilter();
                    resourceFilter.allowedHitPointsConfigurable = true;
                    resourceFilter.allowedQualitiesConfigurable = false;

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
                                    "Building_FoodSynthesizer can only use recipes which have one product!" );
                                addThisRecipe = false;
                            }
                            foreach( var product in recipe.products )
                            {
                                if( !product.thingDef.IsIngestible() )
                                {
                                    CCL_Log.TraceMod(
                                        def,
                                        Verbosity.NonFatalErrors,
                                        "Building_FoodSynthesizer can only use recipes which products are ingestible!" );
                                    addThisRecipe = false;
                                }
                                if( product.count != 1 )
                                {
                                    CCL_Log.TraceMod(
                                        def,
                                        Verbosity.NonFatalErrors,
                                        "Building_FoodSynthesizer can only use recipes which produces one item at a time!" );
                                    addThisRecipe = false;
                                }
                                RecipeDef existingRecipeDef = null;
                                if( productRecipes.TryGetValue( product.thingDef, out existingRecipeDef ) )
                                {
                                    CCL_Log.TraceMod(
                                        def,
                                        Verbosity.NonFatalErrors,
                                        "A recipe which produces '" + product.thingDef.defName + "' already exists!" );
                                    addThisRecipe = false;
                                }
                            }
                            if( addThisRecipe )
                            {
                                foreach( var product in recipe.products )
                                {
                                    if( product.thingDef.IsAlcohol() )
                                    {
                                        potentialAlcohol.Add( product.thingDef );
                                    }
                                    else
                                    {
                                        potentialMeals.Add( product.thingDef );
                                    }
                                    productRecipes.Add( product.thingDef, recipe );
                                }
                                MergeRecipeIntoFilter( recipe );
                            }
                        }
                    }

                    // Sort meals and alcohol
                    potentialMeals.Sort( ( x, y ) => x.ingestible.preferability > y.ingestible.preferability ? -1 : 1 );
                    potentialAlcohol.Sort( ( x, y ) => x.ingestible.joy > y.ingestible.joy ? -1 : 1 );

                    // Used for a quick-check for later added recipes
                    currentRecipeCount = def.AllRecipes.Count;
                }
                return resourceFilter;
            }
        }

        #endregion

        #region System Interface

        public int                          CollectDuration( ThingDef thingDef )
        {
            RecipeDef recipe = null;
            if( !productRecipes.TryGetValue( thingDef, out recipe ) )
            {
                return 50;
            }
            return (int) recipe.workAmount;
        }

        public bool                         CanDispenseNow( ThingDef thingDef )
        {
            if( CompPowerTrader.PowerOn )
            {
                return HasEnoughFeedstockInHoppersFor( thingDef );
            }
            return false;
        }

        public bool                         CanSynthesize( ThingDef thingDef )
        {
            RecipeDef recipe = null;
            return productRecipes.TryGetValue( thingDef, out recipe );
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

        public bool                         HasEnoughFeedstockInHoppersFor( ThingDef thingDef )
        {
            RecipeDef recipe = null;
            if( !productRecipes.TryGetValue( thingDef, out recipe ) )
            {
                return false;
            }
            return CompHopperUser.EnoughResourcesInHoppers( recipe );
        }

        public ThingDef                     BestMealFrom()
        {
            RecipeDef recipe = null;
            for( int index = 0; index < potentialMeals.Count; ++index )
            {
                var thingDef = potentialMeals[ index ];
                if( productRecipes.TryGetValue( thingDef, out recipe ) )
                {
                    if( CompHopperUser.EnoughResourcesInHoppers( recipe ) )
                    {
                        return thingDef;
                    }
                }
            }
            return (ThingDef) null;
        }

        public ThingDef                     BestAlcoholFrom()
        {
            RecipeDef recipe = null;
            for( int index = 0; index < potentialAlcohol.Count; ++index )
            {
                var thingDef = potentialAlcohol[ index ];
                if( productRecipes.TryGetValue( thingDef, out recipe ) )
                {
                    if( CompHopperUser.EnoughResourcesInHoppers( recipe ) )
                    {
                        return thingDef;
                    }
                }
            }
            return (ThingDef) null;
        }

        public Thing                        TryDispenseAlcohol( ThingDef thingDef )
        {
            if( !potentialAlcohol.Contains( thingDef ) )
            {
                return null;
            }
            return TryDispenseIngestible( thingDef );
        }

        public Thing                        TryDispenseMeal( ThingDef thingDef )
        {
            if( !potentialMeals.Contains( thingDef ) )
            {
                return null;
            }
            return TryDispenseIngestible( thingDef );
        }

        public Thing                        TryDispenseIngestible( ThingDef thingDef )
        {
            RecipeDef recipe = null;
            if( !productRecipes.TryGetValue( thingDef, out recipe ) )
            {
                return null;
            }
            List<ThingAmount> chosen = new List<ThingAmount>();
            if( !CompHopperUser.RemoveResourcesFromHoppers( recipe, chosen ) )
            {
                return null;
            }
            if(
                ( thingDef.thingClass == typeof( Meal ) )||
                ( thingDef.thingClass.IsSubclassOf( typeof( Meal ) ) )
            )
            {
                var meal = (Meal) ThingMaker.MakeThing( thingDef );
                foreach( var ingredient in chosen )
                {
                    meal.RegisterIngredient( ingredient.thing.def );
                }
                return meal;
            }
            return ThingMaker.MakeThing( thingDef );
        }

        #endregion

    }

}
