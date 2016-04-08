using System;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    // TODO:  A13 Check for obsolecense
    public class CompProperties_ShearableRenameable : CompProperties
    {

        #region XML Data

        public string                       growthLabel = "";

        #endregion

        public                              CompProperties_ShearableRenameable()
        {
            this.compClass = typeof( CompShearableRenameable );
        }

    }

}
