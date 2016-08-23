using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public struct                           FacilityInjectionSet
    {

        public string                       requiredMod;

        public string                       facility;

        public List<string>                 targetDefs;

        public Type                         qualifier;

        public DefInjectionQualifier        qualifierInt;

    }

}
