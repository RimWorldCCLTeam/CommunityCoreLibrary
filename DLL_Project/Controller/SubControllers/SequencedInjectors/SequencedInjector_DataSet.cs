using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjector_DataSet : SequencedInjector
    {

        private readonly List<SequencedInjectionSet>  erroredSets = new List<SequencedInjectionSet>();

        public override string              Name => "Data Sets";

        public override bool                IsValid()
        {
            var allDefs = Controller.Data.ModHelperDefs;
            if( allDefs.NullOrEmpty() )
            {
                return true;
            }
            var lastTrace = Controller.Data.Trace_Current_Mod;
            var valid = true;
            foreach( var def in allDefs )
            {
                Controller.Data.Trace_Current_Mod = def;
                if( !def.SequencedInjectionSets.NullOrEmpty() )
                {
                    foreach( var injectionSet in def.SequencedInjectionSets )
                    {
#if DEBUG
                        if(
                            ( injectionSet.Targetting != SequencedInjectionSet.InjectorTargetting.None )&&
                            (
                                ( injectionSet.defType == null )||
                                ( !injectionSet.defType.IsSubclassOf( typeof( Def ) ) )
                            )
                        )
                        {
                            CCL_Log.Trace(
                                Verbosity.Validation,
                                "Injector targets Defs but defType is not set to a subclass of Def",
                                injectionSet.Name
                            );
                            erroredSets.Add( injectionSet );
                        }
                        if(
                            ( injectionSet.Targetting == SequencedInjectionSet.InjectorTargetting.None )&&
                            ( injectionSet.defType != null )
                        )
                        {
                            CCL_Log.Trace(
                                Verbosity.Validation,
                                "Injector does not target Defs but defType is not null",
                                injectionSet.Name
                            );
                            erroredSets.Add( injectionSet );
                        }
#endif
                        if(
                            ( !erroredSets.Contains( injectionSet ) )&&
                            ( injectionSet.RequiredModIsLoaded() )
                        )
                        {
                            injectionSet.injectorIsValid = injectionSet.IsValid();
                            if( injectionSet.injectorIsValid )
                            {
                                injectionSet.injectorIsValid &= injectionSet.AllTargetsAreValid();
                            }
                            valid &= injectionSet.injectorIsValid;
                            if( !injectionSet.injectorIsValid )
                            {
                                erroredSets.Add( injectionSet );
                            }
                            else
                            {
                                CCL_Log.Trace(
                                    Verbosity.Validation,
                                    "Validated",
                                    injectionSet.Name
                                );
                            }
                        }
                    }
                }
            }
            Controller.Data.Trace_Current_Mod = lastTrace;
            return valid;
        }

        public override bool                Inject( InjectionSequence sequence, InjectionTiming timing )
        {
            var allDefs = Controller.Data.ModHelperDefs;
            if( allDefs.NullOrEmpty() )
            {
                return true;
            }
            var injected = true;
            var lastTrace = Controller.Data.Trace_Current_Mod;
            foreach( var def in allDefs )
            {
                Controller.Data.Trace_Current_Mod = def;
                if( !def.SequencedInjectionSets.NullOrEmpty() )
                {
                    foreach( var injectionSet in def.SequencedInjectionSets )
                    {
                        if(
                            ( !erroredSets.Contains( injectionSet ) )&&
                            ( injectionSet.InjectNow( sequence, timing ) )
                        )
                        {
                            bool setInjected = injectionSet.Inject();
                            if( !setInjected )
                            {
                                erroredSets.Add( injectionSet );
                            }
                            else
                            {
                                CCL_Log.Trace(
                                    Verbosity.Injections,
                                    "Injected",
                                    injectionSet.Name
                                );
                            }
                            injected &= setInjected;
                        }
                    }
                }
            }
            Controller.Data.Trace_Current_Mod = lastTrace;
            return injected;
        }

        public override bool                ReResolveDefs( InjectionSequence sequence )
        {
            var allDefs = Controller.Data.ModHelperDefs;
            if( allDefs.NullOrEmpty() )
            {
                return true;
            }
            var lastTrace = Controller.Data.Trace_Current_Mod;
            var reresolved = true;
            foreach( var def in allDefs )
            {
                Controller.Data.Trace_Current_Mod = def;
                if( !def.SequencedInjectionSets.NullOrEmpty() )
                {
                    foreach( var injectionSet in def.SequencedInjectionSets )
                    {
                        if(
                            ( !erroredSets.Contains( injectionSet ) )&&
                            ( injectionSet.injectionSequence == sequence )&&
                            ( injectionSet.RequiredModIsLoaded() )
                        )
                        {
                            var setReresolved = injectionSet.ReResolveDefs();
                            if( !setReresolved )
                            {
                                erroredSets.Add( injectionSet );
                            }
                            reresolved &= setReresolved;
                        }
                    }
                }
            }
            Controller.Data.Trace_Current_Mod = lastTrace;
            return reresolved;
        }

    }

}
