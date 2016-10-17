using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class SequencedInjector_Detours : SequencedInjector
    {

        public override string              Name => "Detours";

        public override bool                IsValid()
        {
            return true;
        }

        public override bool                Inject( InjectionSequence sequence, InjectionTiming timing )
        {
            foreach( var mod in Controller.Data.Mods )
            {
                foreach( var assembly in mod.assemblies.loadedAssemblies )
                {
                    var detourPairsForAssembly = Detours.GetTimedDetours( assembly, sequence, timing );
                    if( !detourPairsForAssembly.NullOrEmpty() )
                    {
                        if( !Detours.TryDetourFromTo( detourPairsForAssembly ) )
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }

}
