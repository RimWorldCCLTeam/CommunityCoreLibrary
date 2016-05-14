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
                var traderKindSet = def.TraderKinds[ index ];
                bool processThis = true;
                if( !traderKindSet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( traderKindSet.requiredMod ) != null;
                }
                if( processThis )
                {
                    if( traderKindSet.targetDef.NullOrEmpty() )
                    {
                        errors += string.Format( "\n\ttargetDef in TraderKinds {0} is null", index );
                        isValid = false;
                    }
                    else
                    {
                        var target = traderKindSet.targetDef;
                        var traderKindDef = DefDatabase<TraderKindDef>.GetNamed( target, false );
                        if( traderKindDef == null )
                        {
                            errors += string.Format( "Unable to resolve targetDef '{0}' in TraderKinds", target );
                            isValid = false;
                        }
                    }
                    for( int index2 = 0; index2 < traderKindSet.stockGenerators.Count; ++index2 )
                    {
                        if( traderKindSet.stockGenerators[ index2 ] == null )
                        {
                            errors += string.Format( "\n\tstockGenerator {0} in TraderKinds {1} is null", index2, index );
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

            foreach( var traderKindSet in def.TraderKinds )
            {
                bool processThis = true;
                if( !traderKindSet.requiredMod.NullOrEmpty() )
                {
                    processThis = Find_Extensions.ModByName( traderKindSet.requiredMod ) != null;
                }
                if( processThis )
                {
                    var targetDef = traderKindSet.targetDef;
                    foreach( var stockGenerator in traderKindSet.stockGenerators )
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
            }

            dictInjected.Add( def, true );
            return true;
        }

    }

}
