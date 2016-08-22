using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_AutoHomeArea : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.autoHomeArea;
            }
            set
            {
                Find.PlaySettings.autoHomeArea = value;
            }
        }

    }

}
