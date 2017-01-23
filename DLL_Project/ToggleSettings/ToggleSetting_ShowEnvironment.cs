using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowEnvironment : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showEnvironment;
            }
            set
            {
                Find.PlaySettings.showEnvironment = value;
            }
        }

    }

}
