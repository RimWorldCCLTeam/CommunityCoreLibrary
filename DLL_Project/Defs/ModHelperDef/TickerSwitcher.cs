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

            foreach( var switcherSet in def.tickerSwitcher )
            {
                bool processThis = true;
                if( !switcherSet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( switcherSet.requiredMod ) != null;
                }
                if( processThis )
                {
                    foreach( var targetName in switcherSet.targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        if( targetDef == null )
                        {
                            errors += string.Format( "Unable to resolve targetDef '{0}' in TickerSwitcher", targetName );
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

            foreach( var switcherSet in def.tickerSwitcher )
            {
                bool processThis = true;
                if( !switcherSet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( switcherSet.requiredMod ) != null;
                }
                if( processThis )
                {
                    foreach( var targetName in switcherSet.targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        if( targetDef.tickerType != switcherSet.tickerType )
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

            foreach( var switcherSet in def.tickerSwitcher )
            {
                bool processThis = true;
                if( !switcherSet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( switcherSet.requiredMod ) != null;
                }
                if( processThis )
                {
                    foreach( var targetName in switcherSet.targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        targetDef.tickerType = switcherSet.tickerType;
                    }
                }
            }

            return true;

        }

    }

}
