using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public struct CompInjectionSet
    {

        public string                       requiredMod;

        public List< string >               targetDefs;

        public CompProperties               compProps;

        public Type                         qualifier;

        public DefInjectionQualifier        qualifierInt;

    }

}
