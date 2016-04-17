using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class Building_SunLampHeatController : Building
    {

        CompPowerTrader                     CompPowerTrader
        {
            get
            {
                return this.TryGetComp< CompPowerTrader >();
            }
        }

        CompTempControl                     CompTempControl
        {
            get
            {
                return this.TryGetComp< CompTempControl >();
            }
        }

#if DEBUG
        public override void                SpawnSetup()
        {
            base.SpawnSetup();

            // Validate power trade
            if( CompPowerTrader == null )
            {
                CCL_Log.Error( "Building_SunLampHeatController requires CompPowerTrader!", def.defName );
                return;
            }

            // Validate temp control
            if( CompTempControl == null )
            {
                CCL_Log.Error( "Building_SunLampHeatController requires CompTempControl!", def.defName );
                return;
            }
        }
#endif

        public IEnumerable<IntVec3>         GrowableCells
        {
            get
            {
                return GenRadial.RadialCellsAround( Position, def.specialDisplayRadius, true );
            }
        }

        public override IEnumerable<Gizmo>  GetGizmos()
        {
            // Default gizmos
            foreach( Gizmo curGizmo in base.GetGizmos() )
            {
                yield return curGizmo;
            }

            // Grow zone gizmo
            var comActGrowZone = new Command_Action();
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

        void                                MakeMatchingGrowZone()
        {
            var designator = new Designator_ZoneAdd_Growing();
            designator.DesignateMultiCell(
                from tempCell in GrowableCells
                where designator.CanDesignateCell( tempCell ).Accepted
                select tempCell );
        }

        public override void                TickRare()
        {
            var PowerTrader = CompPowerTrader;
            if( PowerTrader.PowerOn )
            {
                var TempControl = CompTempControl;

                float temperature = Position.GetTemperature();
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
                float energyLimit = TempControl.Props.energyPerSecond * num * 4.16666651f;
                float num2 = GenTemperature.ControlTemperatureTempChange( Position, energyLimit, TempControl.targetTemperature );
                bool flag = !Mathf.Approximately( num2, 0f );
                if( flag )
                {
                    Position.GetRoom().Temperature += num2;
                    PowerTrader.PowerOutput = -PowerTrader.Props.basePowerConsumption;
                }
                else
                {
                    PowerTrader.PowerOutput = -PowerTrader.Props.basePowerConsumption * TempControl.Props.lowPowerConsumptionFactor;
                }
                TempControl.operatingAtHighPower = flag;
            }
        }

    }

}
