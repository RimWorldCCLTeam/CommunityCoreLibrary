using System;

using Verse;

namespace CommunityCoreLibrary
{
    
    public class CompProperties_AffectedByFacilities : RimWorld.CompProperties_AffectedByFacilities
    {

        public bool                         overrideBedOnly;

        public                              CompProperties_AffectedByFacilities() : base()
        {
            this.overrideBedOnly = false;
        }

        public                              CompProperties_AffectedByFacilities( RimWorld.CompProperties_AffectedByFacilities props )
        {
            this.linkableFacilities = props.linkableFacilities;
            this.overrideBedOnly = false;
        }

    }

}
