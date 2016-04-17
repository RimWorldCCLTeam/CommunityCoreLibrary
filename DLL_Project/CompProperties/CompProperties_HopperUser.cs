using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class CompProperties_HopperUser : CompProperties
    {

        #region XML Data

        public ThingFilter                  resources;

        #endregion

        public                              CompProperties_HopperUser()
        {
            compClass = typeof( CompHopperUser );
        }

    }

}
