using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_TraderKinds : IInjector
    {

        // TODO:  Alpha 13 API change
        // Obsoleted
        private static Dictionary<ModHelperDef,bool>    dictInjected;

        static                              MHD_TraderKinds()
        {
            dictInjected = new Dictionary<ModHelperDef,bool>();
        }

#if DEBUG
        public string                       InjectString
        {
            get
            {
                return "Stock Generators injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.TraderKinds.NullOrEmpty() )
            {
                return true;
            }

            bool rVal = true;

            foreach( var traderKind in def.TraderKinds )
            {
                foreach( var stockGenerator in traderKind.stockGenerators )
                {
                    stockGenerator.PostLoad();
                }
            }

            return rVal;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.TraderKinds.NullOrEmpty() )
            {
                return true;
            }

            bool injected;
            if( !dictInjected.TryGetValue( def, out injected ) )
            {
                return false;
            }

            return injected;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.TraderKinds.NullOrEmpty() )
            {
                return true;
            }

            foreach( var traderKind in def.TraderKinds )
            {
                foreach( var targetDef in traderKind.targetDefs )
                {
                    foreach( var stockGenerator in traderKind.stockGenerators )
                    {
                        targetDef.stockGenerators.Add( stockGenerator );
                    }
                }
            }

            dictInjected.Add( def, true );
            return true;
        }

    }

}
