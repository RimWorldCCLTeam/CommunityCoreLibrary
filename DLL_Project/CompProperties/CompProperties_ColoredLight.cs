using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_ColoredLight : CompProperties
    {

        #region XML Data

        public string                       requiredResearch = "ColoredLights";
        public int                          Default;
        public List< ColorName >            color;

        #endregion

        public                              CompProperties_ColoredLight ()
        {
            compClass = typeof( CompProperties_ColoredLight );
        }

    }

}
