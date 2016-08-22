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
        public string                       InjectString => "Tickers switched";

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.tickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( var index = 0; index < def.tickerSwitcher.Count; index++ )
            {
                var qualifierValid = true;
                var injectionSet = def.tickerSwitcher[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                if(
                    ( injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier == null )
                )
                {
                    errors += "targetDefs and qualifier are both null, one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if(
                    ( !injectionSet.targetDefs.NullOrEmpty() )&&
                    ( injectionSet.qualifier != null )
                )
                {
                    errors += "targetDefs and qualifier are both supplied, only one or the other must be supplied";
                    isValid = false;
                    qualifierValid = false;
                }
                if( qualifierValid )
                {
                    if( !injectionSet.targetDefs.NullOrEmpty() )
                    {
                        foreach( var targetName in injectionSet.targetDefs )
                        {
                            var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                            if( targetDef == null )
                            {
                                errors += string.Format( "Unable to resolve targetDef '{0}' in TickerSwitcher", targetName );
                                isValid = false;
                            }
                        }
                    }
                    if( injectionSet.qualifier != null )
                    {
                        if( !injectionSet.qualifier.IsSubclassOf( typeof( DefInjectionQualifier ) ) )
                        {
                            errors += string.Format( "Unable to resolve qualifier '{0}'", injectionSet.qualifier );
                            isValid = false;
                        }
                    }
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.tickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.tickerSwitcher.Count; index++ )
            {
                var injectionSet = def.tickerSwitcher[ index ];
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

        public bool                         Inject( ModHelperDef def )
        {
            if( def.tickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            for( var index = 0; index < def.tickerSwitcher.Count; index++ )
            {
                var injectionSet = def.tickerSwitcher[ index ];
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
                    stringBuilder.Append( "TickerSwitcher :: Qualifier returned: " );
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
