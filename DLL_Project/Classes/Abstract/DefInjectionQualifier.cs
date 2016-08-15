using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public abstract class DefInjectionQualifier
    {
        public abstract bool                Test( Def def );

        public static List<ThingDef>        FilteredThingDefs( Type qualifier, ref DefInjectionQualifier qualifierInt, List<string> targetDefs )
        {
            if( !targetDefs.NullOrEmpty() )
            {
                return DefDatabase<ThingDef>.AllDefs.Where( def => targetDefs.Contains( def.defName ) ).ToList();
            }
            if( qualifierInt == null )
            {
                qualifierInt = (DefInjectionQualifier) Activator.CreateInstance( qualifier );
                if( qualifierInt == null )
                {
                    return null;
                }
            }
            return DefDatabase<ThingDef>.AllDefs.Where( qualifierInt.Test ).ToList();
        }

    }

}

