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
        public string                       InjectString
        {
            get
            {
                return "Tickers switched";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.tickerSwitcher.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var switcher in def.tickerSwitcher )
            {
                foreach( var targetName in switcher.targetDefs )
                {
                    var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                    if( targetDef == null )
                    {
                        errors += string.Format( "Unable to resolve targetDef '{0}' in TickerSwitcher", targetName );
                        isValid = false;
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

            foreach( var switcher in def.tickerSwitcher )
            {
                foreach( var targetName in switcher.targetDefs )
                {
                    var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                    if( targetDef.tickerType != switcher.tickerType )
                    {
                        return false;
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

            foreach( var switcher in def.tickerSwitcher )
            {
                foreach( var targetName in switcher.targetDefs )
                {
                    var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                    targetDef.tickerType = switcher.tickerType;
                }
            }

            return true;

        }

    }

}
