using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CompAutomatedFactory : ThingComp
    {
        
        private CompProperties_AutomatedFactory _Properties = null;
        public CompProperties_AutomatedFactory  Properties
        {
            get
            {
                if( _Properties == null )
                {
                    _Properties = parent.def.GetCompProperties<CompProperties_AutomatedFactory>();
                }
                return _Properties;
            }
        }

    }

}
