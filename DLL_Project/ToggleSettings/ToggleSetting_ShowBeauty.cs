using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowBeauty : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showBeauty;
            }
            set
            {
                Find.PlaySettings.showBeauty = value;
            }
        }

    }

}
