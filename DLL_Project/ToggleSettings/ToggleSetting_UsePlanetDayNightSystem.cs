using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_UsePlanetDayNightSystem : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.usePlanetDayNightSystem;
            }
            set
            {
                Find.PlaySettings.usePlanetDayNightSystem = value;
            }
        }

    }

}
