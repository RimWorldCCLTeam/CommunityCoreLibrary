using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{
	
	public class Building_NutrientPasteDispenser : Building
	{
		public static int				CollectDuration = 50;
		public CompPowerTrader			powerComp;

		private List<IntVec3>			cachedAdjCellsCardinal;
		private List<Building>			cachedAdjHoppers;

		public override void SpawnSetup()
		{
			base.SpawnSetup();
			powerComp = GetComp<CompPowerTrader>();
			if( powerComp == null )
			{
				Log.Message( def.defName + " - Needs compPowerTrader!" );
			}
		}

		public virtual bool CanDispenseNow
		{
			get
			{
				if( powerComp.PowerOn )
				{
					FindAdjacentHoppers();
					return HasEnoughFoodInHoppers();
				}
				return false;
			}
		}

		public virtual List<IntVec3> AdjCellsCardinal
		{
			get
			{
				if( cachedAdjCellsCardinal == null )
					cachedAdjCellsCardinal = Enumerable.ToList<IntVec3>( GenAdj.CellsAdjacentCardinal( ( Thing ) this ) );
				return cachedAdjCellsCardinal;
			}
		}

		public virtual void FindAdjacentHoppers()
		{
			cachedAdjHoppers = new List<Building>();
			foreach( IntVec3 c in AdjCellsCardinal )
			{
				Building hopper = GridsUtility.GetEdifice( c );
				if( ( hopper != null )&&( hopper.def.GetCompProperties( typeof( CompProperties_Hopper ) ) != null ) )
				{
					cachedAdjHoppers.Add( hopper );
				}
			}
		}

		public virtual Building AdjacentReachableHopper( Pawn reacher )
		{
			FindAdjacentHoppers();
			foreach( Building hopper in cachedAdjHoppers )
			{
				if( Reachability.CanReach( 
					reacher, 
					(TargetInfo)( ( Thing ) hopper ), 
					PathMode.Touch, 
					Danger.Deadly ) )
					return hopper;
			}
			return null;
		}

		public virtual int foodDispenseCost
		{
			get
			{
				if( Find.ResearchManager.IsFinished( ResearchProjectDef.Named( "NutrientResynthesis" ) ) )
					return ( this.def.building.foodCostPerDispense - 1 );
				return this.def.building.foodCostPerDispense;
			}
		}

		public virtual Thing TryDispenseFood()
		{
			if( CanDispenseNow )
				return null;
			
			int foodCost = foodDispenseCost;

			int ingredientCount = 0;
			List<ThingDef> ingredients = new List<ThingDef>();
			do
			{
				Thing foodInAnyHopper = FindFoodInAnyHopper();
				if( foodInAnyHopper == null )
				{
					Log.Error( "Did not find enough food in hoppers while trying to dispense." );
					return null;
				}
				int count = Mathf.Min( foodInAnyHopper.stackCount, foodCost );
				ingredientCount += count;
				ingredients.Add( foodInAnyHopper.def );
				foodInAnyHopper.SplitOff( count );
			}
			while( ingredientCount < foodCost );

			SoundStarter.PlayOneShot( this.def.building.soundDispense, (SoundInfo) this.Position );

			Meal meal = (Meal)ThingMaker.MakeThing( ThingDefOf.MealNutrientPaste, (ThingDef) null);
			for( int index = 0; index < ingredients.Count; ++index )
				meal.RegisterIngredient( ingredients[ index ] );

			return (Thing) meal;
		}

		public virtual Thing FindFoodInAnyHopper()
		{
			foreach( Building hopper in cachedAdjHoppers )
			{
				foreach( Thing ingredient in GridsUtility.GetThingList( hopper.Position ) )
				{
					if( ( ingredient != hopper )&&
						( ingredient.def.IsNutritionSource ) )
						return ingredient;
				}
			}
			return null;
		}

		public virtual bool HasEnoughFoodInHoppers()
		{
			int ingredientCount = 0;
			foreach( Building hopper in cachedAdjHoppers )
			{
				foreach( Thing thing in GridsUtility.GetThingList( hopper.Position ) )
				{
					if( ( thing != hopper )&&
						( thing.def.IsNutritionSource ) )
						ingredientCount += thing.stackCount;
					if( ingredientCount >= foodDispenseCost ) return true;
				}
			}
			return false;
		}
	}
}

