using System;

using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace CCLModTweaks
{
    
    public class QualifyLowPowerCompDeepDrill : DefInjectionQualifier
    {

        public override bool DefIsUsable( Def def )
        {
            var thingDef = def as ThingDef;
            if( thingDef == null )
            {
                return false;
            }
            if( !thingDef.hasInteractionCell )
            {
                return false;
            }
            if( !thingDef.HasComp( typeof( CompDeepDrill ) ) )
            {
                return false;
            }
            if( !thingDef.HasComp( typeof( CompPowerTrader ) ) )
            {
                return false;
            }
            if( thingDef.HasComp( typeof( CompPowerLowIdleDraw ) ) )
            {
                return false;
            }
            return true;
       }

    }
}
