using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowRoomStats : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showRoomStats;
            }
            set
            {
                Find.PlaySettings.showRoomStats = value;
            }
        }

    }

}
