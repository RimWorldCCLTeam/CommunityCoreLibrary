using System;

using CommunityCoreLibrary;
using RimWorld;
using Verse;

namespace CCLModTweaks
{
    
    public class WorkTableCompPowerLowIdleDrawTickerChangerQualifier : DefInjectionQualifier
    {

        public override bool Test( Def def )
        {
            var thingDef = def as ThingDef;
            if( thingDef == null )
            {
                return false;
            }
            if(
                ( thingDef.thingClass != typeof( Building_WorkTable ) )&&
                ( thingDef.thingClass != typeof( Building_WorkTable_HeatPush ) )
            )
            {
                return false;
            }
            if( !thingDef.HasComp( typeof( CompPowerTrader ) ) )
            {
                return false;
            }
            if( !thingDef.HasComp( typeof( CompPowerLowIdleDraw) ) )
            {
                return false;
            }
            return true;
       }

    }
}
