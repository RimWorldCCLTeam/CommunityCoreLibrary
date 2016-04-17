using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_ColoredLight : CompProperties
    {

        #region XML Data

        public string                       requiredResearch    = "ColoredLights";
        public int                          Default;
        public List< ColorName >            color;
        public bool                         useColorPicker      = false;
        public bool                         hideGizmos          = false;

        #endregion

        public                              CompProperties_ColoredLight()
        {
            compClass = typeof( CompColoredLight );
        }

    }

}
