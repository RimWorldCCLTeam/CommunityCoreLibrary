using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_Detours : IInjector
    {

        private Dictionary<InjectionTiming,bool> InjectionTimingComplete = new Dictionary<InjectionTiming, bool>();

        public override IInjectorType       InjectorType => IInjectorType.ByTiming;

#if DEBUG
        public override string              InjectString => "Detours injected";
#endif

        public override bool                TimingIsInjected( InjectionTiming priority )
        {
            bool result;
            if( !InjectionTimingComplete.TryGetValue( priority, out result ) )
            {
                return result;
            }
            return false;
        }

        public override bool                InjectByTiming( InjectionTiming priority )
        {
            foreach( var mod in Controller.Data.Mods )
            {
                foreach( var assembly in mod.assemblies.loadedAssemblies )
                {
                    var detourPairsForAssembly = Detours.GetDetourPairs( assembly, priority );
                    if( !detourPairsForAssembly.NullOrEmpty() )
                    {
                        if( !Detours.TryDetourFromTo( detourPairsForAssembly ) )
                        {
                            return false;
                        }
                    }
                }
            }
            InjectionTimingComplete.Add( priority, true );
            return true;
        }

    }

}
