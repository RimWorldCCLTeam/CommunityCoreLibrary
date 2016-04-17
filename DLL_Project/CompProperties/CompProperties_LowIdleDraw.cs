using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_LowIdleDraw : CompProperties
    {

        #region XML Data

        public float                        idlePowerFactor;
        public LowIdleDrawMode              operationalMode = LowIdleDrawMode.InUse;
        public int                          cycleLowTicks = -1;
        public int                          cycleHighTicks = -1;

        #endregion

        public                              CompProperties_LowIdleDraw()
        {
            compClass = typeof( CompPowerLowIdleDraw );
        }

    }

}
