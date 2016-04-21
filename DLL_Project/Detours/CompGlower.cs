using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _CompGlower
    {

        #region Helper Methods

        internal static CompPowerTrader         powerComp( this CompGlower obj )
        {
            return obj.parent.TryGetComp<CompPowerTrader>();
        }

        internal static CompPowerLowIdleDraw    lowPowerComp( this CompGlower obj )
        {
            return obj.parent.TryGetComp<CompPowerLowIdleDraw>();
        }

        internal static CompFlickable           flickableComp( this CompGlower obj )
        {
            return obj.parent.TryGetComp<CompFlickable>();
        }

        internal static CompRefuelable          refuelableComp( this CompGlower obj )
        {
            return obj.parent.TryGetComp<CompRefuelable>();
        }

        #endregion

        #region Detoured Methods

        internal static bool _ShouldBeLitNow( this CompGlower obj )
        {
            if( !obj.parent.Spawned )
            {
                return false;
            }
            var toggleableComp      = obj as CompGlowerToggleable;
            if(
                ( toggleableComp != null )&&
                ( toggleableComp.Lit == false )
            )
            {
                return false;
            }
            var powerComp           = obj.powerComp();
            var lowPowerComp        = obj.lowPowerComp();
            var flickableComp       = obj.flickableComp();
            var refuelableComp      = obj.refuelableComp();
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
                )
            );
        }

        #endregion

    }

}
