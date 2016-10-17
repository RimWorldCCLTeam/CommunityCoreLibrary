using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjectionSet_StockGenerator : SequencedInjectionSet
    {

        public List<StockGenerator>         stockGenerators;

        public                              SequencedInjectionSet_StockGenerator()
        {
            injectionSequence               = InjectionSequence.MainLoad;
            injectionTiming                 = InjectionTiming.StockGenerators;
        }

        public override Type                defType => typeof( TraderKindDef );
        public override InjectorTargetting  Targetting => InjectorTargetting.Single;

        public override bool                IsValid()
        {
            var valid = true;
            if( stockGenerators.NullOrEmpty() )
            {
                CCL_Log.Trace(
                    Verbosity.Validation,
                    "stockGenerators contains no StockGenerators for injection",
                    Name
                );
                valid = false;
            }
            else
            {
                for( int index = 0; index < stockGenerators.Count; ++index )
                {
                    if( stockGenerators[ index ] == null )
                    {
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "StockGenerator at index {0} is null", index ),
                            Name
                        );
                        valid = false;
                    }
                }
            }
            return valid;
        }

        public override bool                TargetIsValid( Def target )
        {
            var traderKindDef = target as TraderKindDef;
#if DEBUG
            if( traderKindDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Validation,
                    string.Format( "Def '{0}' is not a TraderKindDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            return true;
        }

        public override bool                Inject( Def target )
        {
            var traderKindDef = target as TraderKindDef;
#if DEBUG
            if( traderKindDef == null )
            {   // Should never happen
                CCL_Log.Trace(
                    Verbosity.Injections,
                    string.Format( "Def '{0}' is not a TraderKindDef, Def is of type '{1}'", target.defName, target.GetType().FullName ),
                    Name
                );
                return false;
            }
#endif
            var injected = true;
            foreach( var stockGenerator in stockGenerators )
            {
                traderKindDef.stockGenerators.Add( stockGenerator );
#if DEBUG
                injected &= traderKindDef.stockGenerators.Contains( stockGenerator );
#endif
            }
            return injected;
        }

        public override bool ReResolveDefs()
        {
            if( stockGenerators.NullOrEmpty() )
            {
                return true;
            }
            var allTargets = AllTargets<TraderKindDef>();
            if( allTargets.NullOrEmpty() )
            {
                return true;
            }
            var resolved = true;
            foreach( var target in allTargets )
            {
                foreach( var stockGenerator in stockGenerators )
                {
                    try
                    {
                        stockGenerator.ResolveReferences( target );
                    }
                    catch( Exception e )
                    {
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "An exception was thrown while trying to re-resolve StockGenerator '{0}' on TraderKindDef '{1}'\n{2}", stockGenerator.GetType().FullName, target.defName, e.ToString() ),
                            Name
                        );
                        resolved = false;
                    }
                }
            }
            return resolved;
        }

        public override void                PostLoad()
        {
            if( stockGenerators.NullOrEmpty() )
            {
                return;
            }
            foreach( var stockGenerator in stockGenerators )
            {
                stockGenerator.PostLoad();
            }
        }

    }

}
