using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_TickerSwitcher : SequencedInjectionSet
    {

        public TickerType                   tickerType;

        public                              SequencedInjectionSet_TickerSwitcher()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.TickerSwitcher;
        }

        public override Type                defType => typeof( ThingDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Multi;

        public override bool                IsValid()
        {   // Nothing to validate for data
            // tickerType is TickerType and can't be spoofed
            return true;
        }

        public override bool                TargetIsValid( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            return true;
        }

        public override bool                Inject( Def target )
        {
            var thingDef = target as ThingDef;
#if DEBUG
            if( thingDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Def '{0}' is not a ThingDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            thingDef.tickerType = tickerType;
#if DEBUG
            return( thingDef.tickerType == tickerType );
#else
            return true;
#endif
        }

    }

}
