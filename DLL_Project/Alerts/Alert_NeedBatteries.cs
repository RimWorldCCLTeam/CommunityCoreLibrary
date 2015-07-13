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

    public class Alert_NeedBatteries : RimWorld.Alert_NeedBatteries
    {
        private bool CheckBuilding( Building b )
        {
            // Only check for power traders
            var p = b.TryGetComp<CompPowerTrader>();
            if( p == null )
                return false;

            // That are connected to a power network
            if( p.PowerNet == null )
                return false;

            // Which aren't powered on
            if( p.PowerOn == true )
                return false;

            // Which want to be powered on
            if( p.DesirePowerOn == false )
                return false;

            /*
            // Batteries on network, don't worry about it for now
            if( p.PowerNet.CurrentStoredEnergy() >= 1 )
                return false;

            // Where the network power is too low
            var netEnergy = p.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick;
            if( netEnergy > -p.EnergyOutputPerTick )
                return false;
            */

            // And return this building is under powered
            return true;
        }

        public override AlertReport Report
        {
            get
            {
                // Check for individual power trader which is low
                var powerTraders = Find.ListerBuildings.allBuildingsColonist.FindAll( b => CheckBuilding( b ) == true );
                if( ( powerTraders != null )&&
                    ( powerTraders.Count > 0 ) )
                    return AlertReport.CulpritIs( powerTraders.RandomElement() );

                // All trader's good
                return (AlertReport) false;
            }
        }

    }
}

