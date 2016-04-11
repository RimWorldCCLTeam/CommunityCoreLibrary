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

            bool isValid = true;

            for( int index = 0; index < def.TraderKinds.Count; ++index )
            {
                var traderKind = def.TraderKinds[ index ];
                if( traderKind.targetDef.NullOrEmpty() )
                {
                    errors += string.Format( "\n\ttargetDef in TraderKinds {0} is null", index );
                    isValid = false;
                }
                else
                {
                    var target = traderKind.targetDef;
                    var traderKindDef = DefDatabase<TraderKindDef>.GetNamed( target, false );
                    if( traderKindDef == null )
                    {
                        errors += string.Format( "Unable to resolve targetDef '{0}' in TraderKinds", target );
                        isValid = false;
                    }
                }
                for( int index2 = 0; index2 < traderKind.stockGenerators.Count; ++index2 )
                {
                    if( traderKind.stockGenerators[ index2 ] == null )
                    {
                        errors += string.Format( "\n\tstockGenerator {0} in TraderKinds {1} is null", index2, index );
                        isValid = false;
                    }
                }
            }

            return isValid;
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
                var targetDef = traderKind.targetDef;
                foreach( var stockGenerator in traderKind.stockGenerators )
                {
                    var traderKindDef = DefDatabase<TraderKindDef>.GetNamed( targetDef, false );
                    traderKindDef.stockGenerators.Add( stockGenerator );
                    stockGenerator.PostLoad();
                    stockGenerator.ResolveReferences(traderKindDef);
                    CCL_Log.TraceMod(
                        def,
                        Verbosity.Injections,
                        string.Format( "Injecting {0} into {1}", stockGenerator.GetType().Name, traderKindDef.label ),
                        "TraderKinds" );
                }
            }

            dictInjected.Add( def, true );
            return true;
        }

    }

}
