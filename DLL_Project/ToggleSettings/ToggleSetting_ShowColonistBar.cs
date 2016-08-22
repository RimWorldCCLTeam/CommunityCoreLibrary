using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowColonistBar : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showColonistBar;
            }
            set
            {
                Find.PlaySettings.showColonistBar = value;
            }
        }

    }

}
