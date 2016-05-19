using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_PowerGrid : MiniMapOverlay, IConfigurable
    {
        #region Fields

        private Color
            poweredColor = GenUI.MouseoverColor,
            poweredByBatteriesColor = Color.green,
            notPoweredColor = Color.red,
            offColor = Color.grey;

        private UI.LabeledInput_Color
            poweredInput,
            poweredByBatteriesInput,
            notPoweredInput,
            offInput;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_PowerGrid( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            poweredInput.Draw( row );
            poweredColor = poweredInput.Value;
            row.y += 30f;

            poweredByBatteriesInput.Draw( row );
            poweredByBatteriesColor = poweredByBatteriesInput.Value;
            row.y += 30f;

            notPoweredInput.Draw( row );
            notPoweredColor = notPoweredInput.Value;
            row.y += 30f;

            offInput.Draw( row );
            offColor = offInput.Value;
            row.y += 30f;

            return 4 * 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref poweredColor, "poweredColor" );
            Scribe_Values.LookValue( ref poweredByBatteriesColor, "poweredByBatteriesColor" );
            Scribe_Values.LookValue( ref notPoweredColor, "notPoweredColor" );
            Scribe_Values.LookValue( ref offColor, "offColor" );

            if ( Scribe.mode == LoadSaveMode.LoadingVars )
                UpdateInputFields();
        }

        public override void Update()
        {
            // clear existing image
            ClearTexture();

            // get all powercomps attached to things of our colony
            var powerComps = Find.ListerBuildings.allBuildingsColonist.SelectMany( building => building.AllComps ).OfType<CompPower>();

            // throw them all to the rendered
            foreach ( var comp in powerComps )
            {
                DrawConnection( comp );
            }
        }

        private void CreateInputFields()
        {
            poweredInput            = new UI.LabeledInput_Color( poweredColor, "MiniMap.Power.poweredColor".Translate(), "MiniMap.Power.poweredColorTip".Translate() );
            poweredByBatteriesInput = new UI.LabeledInput_Color( poweredByBatteriesColor, "MiniMap.Power.batteryColor".Translate(), "MiniMap.Power.batteryColorTip".Translate() );
            notPoweredInput         = new UI.LabeledInput_Color( notPoweredColor, "MiniMap.Power.notPoweredColor".Translate(), "MiniMap.Power.notPoweredColorTip".Translate() );
            offInput                = new UI.LabeledInput_Color( offColor, "MiniMap.Power.offColor".Translate(), "MiniMap.Power.offColorTip".Translate() );
        }

        private void DrawBattery( CompPowerBattery battery )
        {
            if ( battery == null )
            {
                throw new ArgumentNullException( "battery" );
            }

            var color = Color.clear;

            // blue if gaining energy
            if ( battery.PowerNet?.CurrentEnergyGainRate() > 1f )
                color = GenUI.MouseoverColor;

            // green if draining but has power
            else if ( battery.StoredEnergy > 1f )
                color = Color.green;

            // red if out of power
            else
                color = Color.red;

            MiniMap_Utilities.DrawThing( texture, battery.parent, color );
        }

        private void DrawConnection( CompPower powerComp )
        {
            var transmitter = powerComp as CompPowerTransmitter;
            if ( transmitter != null )
            {
                DrawTransmitter( transmitter );
            }

            var trader = powerComp as CompPowerTrader;
            if ( trader != null )
            {
                DrawTrader( trader );
            }

            var battery = powerComp as CompPowerBattery;
            if ( battery != null )
            {
                DrawBattery( battery );
            }
        }

        private void DrawTrader( CompPowerTrader trader )
        {
            if ( trader == null )
            {
                throw new ArgumentNullException( "trader" );
            }

            // get a nice descriptive color
            Color color = Color.clear;
            if ( trader.PowerOn )
            {
                color = GenUI.MouseoverColor;
            }
            else if (
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

        private void DrawTransmitter( CompPowerTransmitter transmitter )
        {
            if ( transmitter == null )
            {
                throw new ArgumentNullException( "transmitter" );
            }

            // get a nice descriptive color
            Color color = Color.clear;

            if ( transmitter.transNet == null )
            {   // not connected
                color = Color.red;
            }
            else
            {   // connected
                if ( transmitter.transNet.CurrentEnergyGainRate() > 0f )
                {   // excess power
                    color = GenUI.MouseoverColor;
                }
                else if ( transmitter.transNet.CurrentStoredEnergy() > 1f )
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

        private void UpdateInputFields()
        {
            poweredInput.Value            = poweredColor;
            poweredByBatteriesInput.Value = poweredByBatteriesColor;
            notPoweredInput.Value         = notPoweredColor;
            offInput.Value                = offColor;
        }

        #endregion Methods
    }
}