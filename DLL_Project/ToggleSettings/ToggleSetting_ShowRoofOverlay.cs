using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowRoofOverlay : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showRoofOverlay;
            }
            set
            {
                Find.PlaySettings.showRoofOverlay = value;
            }
        }

    }

}
