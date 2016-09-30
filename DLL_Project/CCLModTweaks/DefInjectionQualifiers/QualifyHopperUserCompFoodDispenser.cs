using System;

using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace CCLModTweaks
{
    
    public class QualifyHopperUserCompFoodDispenser : DefInjectionQualifier
    {

        public override bool DefIsUsable( Def def )
        {
            var thingDef = def as ThingDef;
            if( thingDef == null )
            {
                return false;
            }
            if( !thingDef.IsFoodDispenser )
            {
                return false;
            }
            if( thingDef.HasComp( typeof( CompHopperUser ) ) )
            {
                return false;
            }
            return true;
       }

    }
}
