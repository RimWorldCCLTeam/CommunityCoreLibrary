using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class ToggleSetting_ShowLearningHelper : ToggleSetting
    {

        public override bool Value
        {
            get
            {
                return Find.PlaySettings.showLearningHelper;
            }
            set
            {
                Find.PlaySettings.showLearningHelper = value;
            }
        }

    }

}
