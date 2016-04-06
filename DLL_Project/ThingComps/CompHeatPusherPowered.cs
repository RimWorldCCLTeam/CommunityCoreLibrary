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

        CompHeatPusher HeatPusher
        {
            get
            {
                return parent.TryGetComp<CompHeatPusher>();
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
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompPowerTrader"
                );
                return;
            }

            // Check idle power comp
            if( CompPowerLowIdleDraw == null )
            {
                CCL_Log.TraceMod(
                    parent.def,
                    Verbosity.FatalErrors,
                    "Missing CompPowerLowIdleDraw"
                );
                return;
            }

        }
#endif

        // TODO: see other todos
        /*public override void                CompTick()
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
            if( parent.Position.GetTemperature() >= HeatPusher.Props.heatPushMaxTemperature )
            {
                return;
            }

            // Push some heat
            GenTemperature.PushHeat( parent.Position, HeatPusher.Props.heatPerSecond );
        }*/

    }

}
