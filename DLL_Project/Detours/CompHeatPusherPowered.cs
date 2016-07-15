using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _CompHeatPusherPowered
    {

        #region Helper Methods

        internal static CompPowerTrader         powerComp( this CompHeatPusherPowered obj )
        {
            return obj.parent.TryGetComp<CompPowerTrader>();
        }

        internal static CompPowerLowIdleDraw    lowPowerComp( this CompHeatPusherPowered obj )
        {
            return obj.parent.TryGetComp<CompPowerLowIdleDraw>();
        }

        internal static CompFlickable           flickableComp( this CompHeatPusherPowered obj )
        {
            return obj.parent.TryGetComp<CompFlickable>();
        }

        internal static CompRefuelable          refuelableComp( this CompHeatPusherPowered obj )
        {
            return obj.parent.TryGetComp<CompRefuelable>();
        }

        internal static CompBreakdownable       breakdownableComp( this CompHeatPusherPowered obj )
        {
            return obj.parent.TryGetComp<CompBreakdownable>();
        }

        #endregion

        #region Detoured Methods

        internal static bool _ShouldPushHeatNow( this CompHeatPusherPowered obj )
        {
            var powerComp           = obj.powerComp();
            if(
                ( powerComp != null )&&
                ( !powerComp.PowerOn )
            )
            {
                return false;
            }
            var lowPowerComp        = obj.lowPowerComp();
            if(
                ( lowPowerComp != null )&&
                ( lowPowerComp.LowPowerMode )
            )
            {
                return false;
            }
            var refuelableComp      = obj.refuelableComp();
            if(
                ( refuelableComp != null )&&
                ( !refuelableComp.HasFuel )
            )
            {
                return false;
            }
            var breakdownableComp   = obj.breakdownableComp();
            if(
                ( breakdownableComp != null )&&
                ( breakdownableComp.BrokenDown )
            )
            {
                return false;
            }
            var flickableComp       = obj.flickableComp();
            return(
                ( flickableComp == null )||
                ( flickableComp.SwitchIsOn )
            );
        }

        #endregion

    }

}
