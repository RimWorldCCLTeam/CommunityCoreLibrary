using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ResourceReadoutCategorized : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Prefs.ResourceReadoutCategorized;
            }
            set
            {
                Prefs.ResourceReadoutCategorized = value;
            }
        }

    }

}
