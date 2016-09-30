using System;

using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace CCLModTweaks
{
    
    public class QualifyGrowerCompPlantGrower : DefInjectionQualifier
    {

        public override bool DefIsUsable( Def def )
        {
            var thingDef = def as ThingDef;
            if( thingDef == null )
            {
                return false;
            }
            if( !typeof( Building_PlantGrower ).IsAssignableFrom( thingDef.thingClass ) )
            {
                return false;
            }
            if( thingDef.HasComp( typeof( CompNeighbourlyGrower ) ) )
            {
                return false;
            }
            return true;
       }

    }
}
