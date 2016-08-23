using System;
using System.Collections.Generic;

namespace CommunityCoreLibrary
{
    
    public struct ITabInjectionSet
    {

        public string                       requiredMod;

        public Type                         newITab;
        public Type                         replaceITab;

        public List< string >               targetDefs;

        public Type                         qualifier;

        public DefInjectionQualifier        qualifierInt;

    }

}

