using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_StockGenerators : IInjector
    {

        private static Dictionary<ModHelperDef,bool>    dictInjected;

        static                              MHD_StockGenerators()
        {
            dictInjected = new Dictionary<ModHelperDef,bool>();
        }

#if DEBUG
        public string                       InjectString => "Stock Generators injected";

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.TraderKinds.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            for( int index = 0; index < def.TraderKinds.Count; ++index )
            {
                var injectionSet = def.TraderKinds[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                for( int index2 = 0; index2 < injectionSet.stockGenerators.Count; ++index2 )
                {
                    if( injectionSet.stockGenerators[ index2 ] == null )
                    {
                        errors += string.Format( "\n\tstockGenerator {0} in TraderKinds {1} is null", index2, index );
                        isValid = false;
                    }
                }
                if( injectionSet.targetDef.NullOrEmpty() )
                {
                    errors += "targetDef is null";
                    isValid = false;
                }
                else
                {
                    var traderKindDef = DefDatabase<TraderKindDef>.GetNamed( injectionSet.targetDef, false );
                    if( traderKindDef == null )
                    {
                        errors += string.Format( "Unable to resolve targetDef '{0}' in TraderKinds", injectionSet.targetDef );
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

            for( var index = 0; index < def.TraderKinds.Count; index++ )
            {
                var injectionSet = def.TraderKinds[ index ];
                if(
                    ( !injectionSet.requiredMod.NullOrEmpty() )&&
                    ( Find_Extensions.ModByName( injectionSet.requiredMod ) == null )
                )
                {
                    continue;
                }
                var traderKindDef = DefDatabase<TraderKindDef>.GetNamed( injectionSet.targetDef );
                foreach( var stockGenerator in injectionSet.stockGenerators )
                {
                    traderKindDef.stockGenerators.Add( stockGenerator );
                    stockGenerator.PostLoad();
                    stockGenerator.ResolveReferences( traderKindDef );
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
