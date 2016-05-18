using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public struct ThingDefAvailability
    {

        public string                       requiredMod;

        public string                       menuHidden;
        public string                       designationCategory;
        public List< string >               researchPrerequisites;

        public List< string >               targetDefs;

    }

}
