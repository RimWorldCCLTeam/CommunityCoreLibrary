using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public struct                           StockGeneratorInjectionSet
    {

        public string                       requiredMod;

        public string                       targetDef;

        public List<StockGenerator>         stockGenerators;

    }

}
