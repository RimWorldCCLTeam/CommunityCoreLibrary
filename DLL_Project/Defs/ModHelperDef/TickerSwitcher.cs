using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_TickerSwitcher : IInjector
    {

#if DEBUG
        public override string              InjectString => "Tickers switched";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.TickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( var index = 0; index < def.TickerSwitcher.Count; index++ )
            {
                var injectionSet = def.TickerSwitcher[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                isValid &= DefInjectionQualifier.TargetQualifierValid( injectionSet.targetDefs, injectionSet.qualifier, "TickerSwitcher", ref errors );
            }

            return isValid;
        }
#endif

        public override bool                DefIsInjected( ModHelperDef def )
        {
            if( def.TickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.TickerSwitcher.Count; index++ )
            {
                var injectionSet = def.TickerSwitcher[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
                    foreach( var thingDef in thingDefs )
                    {
                        if( thingDef.tickerType != injectionSet.tickerType )
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public override bool                InjectByDef( ModHelperDef def )
        {
            if( def.TickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.TickerSwitcher.Count; index++ )
            {
                var injectionSet = def.TickerSwitcher[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var thingDefs = DefInjectionQualifier.FilteredThingDefs( injectionSet.qualifier, ref injectionSet.qualifierInt, injectionSet.targetDefs );
                if( !thingDefs.NullOrEmpty() )
                {
#if DEBUG
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append( string.Format( "TickerSwitcher ({0}):: Qualifier returned: ", injectionSet.tickerType.ToString() ) );
#endif
                    foreach( var thingDef in thingDefs )
                    {
#if DEBUG
                        stringBuilder.Append( thingDef.defName + ", " );
#endif
                        thingDef.tickerType = injectionSet.tickerType;
                    }
#if DEBUG
                    CCL_Log.Message( stringBuilder.ToString(), def.ModName );
#endif
                }
            }

            return true;

        }

    }

}
