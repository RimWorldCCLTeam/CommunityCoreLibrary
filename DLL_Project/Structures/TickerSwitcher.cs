using System;
using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public struct TickerSwitcher
    {

        public string                       requiredMod;

        public TickerType                   tickerType;

        public List< string >               targetDefs;

        public Type                         qualifier;

        public DefInjectionQualifier        qualifierInt;

    }

}
