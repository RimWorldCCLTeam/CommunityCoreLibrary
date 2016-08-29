using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal class _CompHeatPusherPowered : CompHeatPusherPowered
    {

        #region Helper Methods

        internal CompPowerLowIdleDraw           lowPowerComp
        {
            get
            {
                return this.parent.TryGetComp<CompPowerLowIdleDraw>();
            }
        }

        #endregion

        #region Detoured Methods

        [DetourClassProperty( typeof( CompHeatPusherPowered ), "ShouldPushHeatNow" )]
        protected override bool ShouldPushHeatNow
        {
            get
            {
                if(
                    ( powerComp != null )&&
                    ( !powerComp.PowerOn )
                )
                {
                    return false;
                }
                if(
                    ( lowPowerComp != null )&&
                    ( lowPowerComp.LowPowerMode )
                )
                {
                    return false;
                }
                if(
                    ( refuelableComp != null )&&
                    ( !refuelableComp.HasFuel )
                )
                {
                    return false;
                }
                if(
                    ( breakdownableComp != null )&&
                    ( breakdownableComp.BrokenDown )
                )
                {
                    return false;
                }
                if(
                    ( flickableComp == null )||
                    ( !flickableComp.SwitchIsOn )
                )
                {
                    return false;
                }
                return true;
            }
        }

        #endregion

    }

}
