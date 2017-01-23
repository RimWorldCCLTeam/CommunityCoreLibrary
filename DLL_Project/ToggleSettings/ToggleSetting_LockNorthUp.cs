using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_LockNorthUp : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.lockNorthUp;
            }
            set
            {
                Find.PlaySettings.lockNorthUp = value;
            }
        }

    }

}
