using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjector_SpecialInjectors : SequencedInjector
    {

        private readonly List<SpecialInjector>  erroredInjectors = new List<SpecialInjector>();

        public override string              Name => "Special Injectors";

        public override bool                IsValid()
        {
            return true;
        }

        public override bool                Inject( InjectionSequence sequence, InjectionTiming timing )
        {
            var injected = true;
            foreach( var mod in Controller.Data.Mods )
            {
                foreach( var assembly in mod.assemblies.loadedAssemblies )
                {
                    var injectorsForAssembly = SpecialInjector.GetTimedInjectors( assembly, sequence, timing );
                    if( !injectorsForAssembly.NullOrEmpty() )
                    {
                        foreach( var injector in injectorsForAssembly )
                        {
                            if(
                                ( !erroredInjectors.Contains( injector ) )&&
                                ( !SpecialInjector.TryInject( injector ) )
                            )
                            {
                                erroredInjectors.Add( injector );
                                injected = false;
                            }
                        }
                    }
                }
            }
            return injected;
        }

        public override bool                ReResolveDefs( InjectionSequence sequence )
        {
            var reresolved = true;
            foreach( var mod in Controller.Data.Mods )
            {
                foreach( var assembly in mod.assemblies.loadedAssemblies )
                {
                    var injectorsForAssembly = SpecialInjector.GetTimedInjectors( assembly, sequence, InjectionTiming.All );
                    if( !injectorsForAssembly.NullOrEmpty() )
                    {
                        foreach( var injector in injectorsForAssembly )
                        {
                            if(
                                ( !erroredInjectors.Contains( injector ) )&&
                                ( !SpecialInjector.TryReResolveDefs( injector ) )
                            )
                            {
                                erroredInjectors.Add( injector );
                                reresolved = false;
                            }
                        }
                    }
                }
            }
            return reresolved;
        }

    }

}
