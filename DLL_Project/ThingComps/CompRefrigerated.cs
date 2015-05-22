using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
	public class RefrigeratorContents
	{
		// This is to handle the degredation of items in the refrigerator
		// It is capable of handling multiple items in multiple cells
		public ThingWithComps			thing;
		public int						HitPoints;
		public bool						stillHere;

		public RefrigeratorContents( ThingWithComps t )
		{
			thing = t;
			HitPoints = t.HitPoints;
			stillHere = true;
		}
	}

	public class CompRefrigerated : ThingComp
	{
		private CompPowerTrader			compPower;
		private Building				thisBuilding;
		private List< RefrigeratorContents >	contents = new List< RefrigeratorContents >();

		private bool					okToProcess = true;

        public override void PostExposeData()
        {

            //Log.Message( parent.def.defName + " - PostExposeData()" );
            base.PostExposeData();

            Scribe_Collections.LookList< RefrigeratorContents >( ref contents, "contents", LookMode.DefReference, LookMode.Value );
        }

		public override void PostSpawnSetup()
		{
			//Log.Message( parent.def.defName + " - PostSpawnSetup()" );
			base.PostSpawnSetup();

			// Get this building
			thisBuilding = (Building)parent;
			if( thisBuilding == null ) {
				okToProcess = false;
				Log.Message( "Community Core Library :: CompRefrigerated :: Unable to cast '" + parent.def.defName + "' to Building" );
				return;
			}

			// Get the power comp
			compPower = parent.GetComp<CompPowerTrader>();
			if( compPower == null )
			{
				okToProcess = false;
				Log.Message( "Community Core Library :: CompRefrigerated :: '" + parent.def.defName + "' needs compPowerTrader!" );
				return;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
            RefrigerateContents();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            RefrigerateContents();
        }

        private void RefrigerateContents()
        {
            if( !okToProcess ) return;

			// Only refrigerate if it has power
			if( compPower.PowerOn == false ){
				// Don't remember the contents anymore
				if( contents.Count > 0 ){
					contents = new List< RefrigeratorContents >();
				}
				// Now leave
				return;
			}

			// Assume it's been removed
			foreach( RefrigeratorContents item in contents )
			{
				item.stillHere = false;
			}

			foreach( IntVec3 curPos in GenAdj.CellsOccupiedBy( thisBuilding ) )
			{
				foreach( Thing thing in Find.ThingGrid.ThingsListAt( curPos ) )
				{
					if( thing.GetType() == typeof( ThingWithComps ) )
					{
						ThingWithComps curThing = (ThingWithComps)thing;
						CompRottable compRottable = curThing.GetComp<CompRottable>();
						if( compRottable != null )
						{
							Refrigerate( curThing, compRottable );
						}
					}
				}
			}

			// Clean out junk
			foreach( RefrigeratorContents item in contents )
			{
				if( !item.stillHere )
				{
					contents.Remove( item );
				}
			}
		}

		private void Refrigerate( ThingWithComps thing, CompRottable compRottable )
		{
			RefrigeratorContents bucket = null;

			// Find an existing item in the refrigerator
			foreach( RefrigeratorContents item in contents )
			{
				if( item.thing == thing )
				{
					bucket = item;
					break;
				}
			}

			// Create a new item in the refrigerator
			if( bucket == null )
			{
				bucket = new RefrigeratorContents( thing );
				contents.Add( bucket );
			}

			// Restore the item to when refrigeration started
			thing.HitPoints = bucket.HitPoints;
			compRottable.rotProgress = 0f;
			bucket.stillHere = true;
		}
	}
}