using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowZones : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showZones;
            }
            set
            {
                Find.PlaySettings.showZones = value;
            }
        }

    }

}
