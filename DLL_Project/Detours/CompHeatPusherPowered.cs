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
            var lowPowerComp        = obj.lowPowerComp();
            var flickableComp       = obj.flickableComp();
            var refuelableComp      = obj.refuelableComp();
            var breakdownableComp   = obj.breakdownableComp();
            return(
                (
                    ( powerComp == null )||
                    ( powerComp.PowerOn )
                )&&
                (
                    ( lowPowerComp == null )||
                    ( !lowPowerComp.LowPowerMode )
                )&&
                (
                    ( flickableComp == null )||
                    ( flickableComp.SwitchIsOn )
                )&&
                (
                    ( refuelableComp == null )||
                    ( refuelableComp.HasFuel )
                )&&
                (
                    ( breakdownableComp == null )||
                    ( !breakdownableComp.BrokenDown )
                )
            );
        }

        #endregion

    }

}
