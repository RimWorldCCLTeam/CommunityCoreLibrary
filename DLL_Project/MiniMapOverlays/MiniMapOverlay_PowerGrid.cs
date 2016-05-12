using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public class MiniMapOverlay_PowerGrid : MiniMapOverlay
	{

		#region Constructors

        public MiniMapOverlay_PowerGrid( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Base Overrides

		public override void Update()
		{
            // clear existing image
            ClearTexture();

			// get all powercomps attached to things of our colony
			var powerComps = Find.ListerBuildings.allBuildingsColonist.SelectMany( building => building.AllComps ).OfType<CompPower>();

			// throw them all to the rendered
			foreach( var comp in powerComps )
			{
				DrawConnection( comp );
			}
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

			MiniMap_Utilities.DrawThing( texture, transmitter.parent, color );
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

            MiniMap_Utilities.DrawThing( texture, trader.parent, color );
		}

		private void DrawBattery( CompPowerBattery battery )
		{
			if( battery == null )
			{
				throw new ArgumentNullException( "battery" );
			}

			var color = battery.StoredEnergy > 1f ? GenUI.MouseoverColor : Color.gray;

            MiniMap_Utilities.DrawThing( texture, battery.parent, color );
		}

		#endregion Methods

	}

}
