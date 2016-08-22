using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_UseWorkPriorities : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.useWorkPriorities;
            }
            set
            {
                Find.PlaySettings.useWorkPriorities = value;
            }
        }

    }

}
