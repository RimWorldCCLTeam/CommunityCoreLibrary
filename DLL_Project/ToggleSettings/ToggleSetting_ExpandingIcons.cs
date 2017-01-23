using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ExpandingIcons : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.expandingIcons;
            }
            set
            {
                Find.PlaySettings.expandingIcons = value;
            }
        }

    }

}
