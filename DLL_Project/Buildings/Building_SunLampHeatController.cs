using System;
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
    internal class Building_SunLampHeatController : Building
    {
        public CompTempControl compTempControl;
        public CompPowerTrader compPowerTrader;

        public override void SpawnSetup()
        {
            base.SpawnSetup();
            this.compTempControl = base.GetComp<CompTempControl>();
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
        }

        public IEnumerable<IntVec3> GrowableCells
        {
            get
            {
                return GenRadial.RadialCellsAround( base.Position, this.def.specialDisplayRadius, true );
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            // Default gizmos
            foreach( Gizmo curGizmo in base.GetGizmos() ) yield return curGizmo;

            // Grow zone gizmo
            Command_Action comActGrowZone = new Command_Action();
            if( comActGrowZone != null )
            {
                comActGrowZone.icon = Icon.GrowZone;
                comActGrowZone.defaultLabel = "CommandSunLampMakeGrowingZoneLabel".Translate();
                comActGrowZone.defaultDesc = "CommandSunLampMakeGrowingZoneDesc".Translate();
                comActGrowZone.activateSound = SoundDef.Named( "Click" );
                comActGrowZone.action = new Action( MakeMatchingGrowZone );
                if( comActGrowZone.action != null )
                {
                    yield return comActGrowZone;
                }
            }

            // No more gizmos
            yield break;
        }

        private void MakeMatchingGrowZone()
        {
            Designator_ZoneAdd_Growing designator = new Designator_ZoneAdd_Growing();
            designator.DesignateMultiCell(
                from tempCell in this.GrowableCells
                where designator.CanDesignateCell( tempCell ).Accepted
                select tempCell );
        }

        public override void TickRare()
        {
            if( this.compPowerTrader.PowerOn )
            {
                float temperature = base.Position.GetTemperature();
                float num;
                if( temperature < 20f )
                {
                    num = 1f;
                }
                else if( temperature > 120f )
                {
                    num = 0f;
                }
                else
                {
                    num = Mathf.InverseLerp( 120f, 20f, temperature );
                }
                float energyLimit = this.compTempControl.props.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange( base.Position, energyLimit, this.compTempControl.targetTemperature );
                bool flag = !Mathf.Approximately( num2, 0f );
                if( flag )
                {
                    base.Position.GetRoom().Temperature += num2;
                    this.compPowerTrader.PowerOutput = -this.compPowerTrader.props.basePowerConsumption;
                }
                else
                {
                    this.compPowerTrader.PowerOutput = -this.compPowerTrader.props.basePowerConsumption * this.compTempControl.props.lowPowerConsumptionFactor;
                }
                this.compTempControl.operatingAtHighPower = flag;
            }
        }
    }
}