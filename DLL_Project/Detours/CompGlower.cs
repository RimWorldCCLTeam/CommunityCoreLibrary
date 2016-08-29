using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal class _CompGlower : CompGlower
    {

        #region Helper Methods

        internal CompPowerTrader                powerComp
        {
            get
            {
                return this.parent.TryGetComp<CompPowerTrader>();
            }
        }

        internal CompPowerLowIdleDraw           lowPowerComp
        {
            get
            {
                return this.parent.TryGetComp<CompPowerLowIdleDraw>();
            }
        }

        internal CompFlickable                  flickableComp
        {
            get
            {
                return this.parent.TryGetComp<CompFlickable>();
            }
        }

        internal CompRefuelable                 refuelableComp
        {
            get
            {
                return this.parent.TryGetComp<CompRefuelable>();
            }
        }

        internal static CompGlowerToggleable    ToggleableComp( CompGlower baseComp )
        {
            return baseComp as CompGlowerToggleable;
        }

        #endregion

        #region Detoured Methods

        [DetourClassProperty( typeof( CompGlower ), "ShouldBeLitNow" )]
        internal bool _ShouldBeLitNow
        {
            get
            {
                if( !this.parent.Spawned )
                {
                    return false;
                }
                var toggleableComp      = ToggleableComp( this );
                if(
                    ( toggleableComp != null )&&
                    ( !toggleableComp.Lit )
                )
                {
                    return false;
                }
                if(
                    ( powerComp != null )&&
                    ( !powerComp.PowerOn )
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
                    ( lowPowerComp != null )&&
                    ( lowPowerComp.LowPowerMode )
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
