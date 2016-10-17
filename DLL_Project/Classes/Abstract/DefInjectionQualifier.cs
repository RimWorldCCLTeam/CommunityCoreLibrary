using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public abstract class DefInjectionQualifier
    {

        public abstract bool                DefIsUsable( Def def );

    }

}

