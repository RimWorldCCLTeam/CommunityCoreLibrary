using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

	public class MiniMapOverlay_PowerGrid : MiniMapOverlay
	{

		#region Constructors

        public MiniMapOverlay_PowerGrid( MiniMap minimap, MiniMapOverlayData overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Base Overrides

		public override void Update()
		{
			// clear existing image
            texture.SetPixels( MiniMap.GetClearPixelArray );

			// get all powercomps attached to things of our colony
			var powerComps = Find.ListerBuildings.allBuildingsColonist.SelectMany( building => building.AllComps ).OfType<CompPower>();

			// throw them all to the rendered
			foreach( var comp in powerComps )
			{
				DrawConnection( comp );
			}

			// apply changes.
			texture.Apply();
		}

		#endregion

		#region Internal Rendering

		private void DrawConnection( CompPower powerComp )
		{
			var transmitter = powerComp as CompPowerTransmitter;
			if( transmitter != null )
			{
				DrawTransmitter( transmitter );
			}

			var trader = powerComp as CompPowerTrader;
			if( trader != null )
			{
				DrawTrader( trader );
			}

			var battery = powerComp as CompPowerBattery;
			if( battery != null )
			{
				DrawBattery( battery );
			}
		}

		private void DrawTransmitter( CompPowerTransmitter transmitter )
		{
			if( transmitter == null )
			{
				throw new ArgumentNullException( "transmitter" );
			}

			// get a nice descriptive color
			Color color = Color.clear;


			if( transmitter.transNet == null )
			{   // not connected
				color = Color.gray;
			}
			else
			{   // connected
				if( transmitter.transNet.CurrentEnergyGainRate() > 0f )
				{   // excess power
					color = GenUI.MouseoverColor;
				}
				else if( transmitter.transNet.CurrentStoredEnergy() > 0f )
				{   // stored power
					color = Color.green;
				}
				else
				{   // not enough power
					color = Color.red;
				}
			}

			DrawThing( transmitter.parent, color );
		}

		private void DrawTrader( CompPowerTrader trader )
		{
			if( trader == null )
			{
				throw new ArgumentNullException( "trader" );
			}

			// get a nice descriptive color
			Color color = Color.clear;
			if( trader.PowerOn )
			{
				color = GenUI.MouseoverColor;
			}
			else if(
				( !trader.PowerOn ) &&
				( trader.DesirePowerOn )
			)
			{
				color = Color.red;
			}
			else
			{
				color = Color.grey;
			}

			DrawThing( trader.parent, color );
		}

		private void DrawBattery( CompPowerBattery battery )
		{
			if( battery == null )
			{
				throw new ArgumentNullException( "battery" );
			}

			var color = battery.StoredEnergy > 1f ? GenUI.MouseoverColor : Color.gray;

			DrawThing( battery.parent, color );
		}

		private void DrawThing( Thing thing, Color color )
		{
#if DEVELOPER
            CCL_Log.Message( "Painting cells for " + thing.LabelCap + thing.Position + color );
#endif

			// check if this makes sense
			if( thing == null )
			{
				CCL_Log.Error( "tried to get occupied rect for NULL thing" );
				return;
			}
			if(
				( thing.OccupiedRect().Cells == null ) ||
				( thing.OccupiedRect().Cells.Count() == 0 )
			)
			{
				CCL_Log.Error( "tried to get occupier rect for " + thing.LabelCap + " but it is NULL or empty" );
				return;
			}

			// paint all cells occupied by thing in 'color'.
			foreach( var cell in thing.OccupiedRect().Cells )
			{
				if( cell.InBounds() )
				{
					texture.SetPixel( cell.x, cell.z, color );
				}
			}
		}

		#endregion Methods

	}

}
