using System;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompProperties_MilkableRenameable : CompProperties
    {

        public string                       growthLabel = "";

        public CompProperties_MilkableRenameable()
        {
            this.compClass = typeof( CompMilkableRenameable );
        }

    }

}

