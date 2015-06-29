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
    public class CompHeatPusherPowered : ThingComp
    {
        public override void CompTick()
        {
            base.CompTick();

            if( !Gen.IsHashIntervalTick( parent, 60 ) )
                return;

            // If it has a comp power (which it should) and it's off, abort
            CompPowerTrader compPowerTrader = (parent as Building).PowerComp as CompPowerTrader;
            if( ( compPowerTrader == null )||
                ( !compPowerTrader.PowerOn ) )
                return;
            
            // If it has a comp power idle (which it should) and it's idle, abort
            CompPowerLowIdleDraw compPowerLowIdleDraw = (parent as Building).TryGetComp<CompPowerLowIdleDraw>();
            if( ( compPowerLowIdleDraw == null )||
                ( compPowerLowIdleDraw.isItIdle == true ) )
                return;

            // If the local temp is higher than the max heat pushed, abort
            if( GridsUtility.GetTemperature( parent.Position ) >= props.heatPushMaxTemperature )
                return;

            // Push some heat
            GenTemperature.PushHeat(this.parent.Position, this.props.heatPerSecond);
        }
    }
}

