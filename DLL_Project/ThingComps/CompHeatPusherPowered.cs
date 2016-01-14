using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompHeatPusherPowered : ThingComp
    {

        CompPowerTrader                     CompPowerTrader
        {
            get
            {
                return parent.TryGetComp< CompPowerTrader >();
            }
        }

        CompPowerLowIdleDraw                CompPowerLowIdleDraw
        {
            get
            {
                return parent.TryGetComp< CompPowerLowIdleDraw >();
            }
        }

#if DEBUG
        public override void                PostSpawnSetup()
        {
            base.PostSpawnSetup();

            // Check power comp
            if( CompPowerTrader == null )
            {
                CCL_Log.TraceMod(
                    Find_Extensions.ModByDefOfType<ThingDef>( parent.def.defName ),
                    Verbosity.FatalErrors,
                    "Missing CompPowerTrader",
                    this.GetType().ToString(),
                    parent.def
                );
                return;
            }

            // Check idle power comp
            if( CompPowerLowIdleDraw == null )
            {
                CCL_Log.TraceMod(
                    Find_Extensions.ModByDefOfType<ThingDef>( parent.def.defName ),
                    Verbosity.FatalErrors,
                    "Missing CompPowerLowIdleDraw",
                    this.GetType().ToString(),
                    parent.def
                );
                return;
            }

        }
#endif

        public override void                CompTick()
        {
            base.CompTick();

            if( !parent.IsHashIntervalTick( 60 ) )
            {
                return;
            }

            // If power is off, abort
            if( !CompPowerTrader.PowerOn )
            {
                return;
            }

            // If it's in low power mode, abort
            if( CompPowerLowIdleDraw.LowPowerMode )
            {
                return;
            }

            // If the local temp is higher than the max heat pushed, abort
            if( parent.Position.GetTemperature() >= props.heatPushMaxTemperature )
            {
                return;
            }

            // Push some heat
            GenTemperature.PushHeat( parent.Position, props.heatPerSecond );
        }

    }

}
