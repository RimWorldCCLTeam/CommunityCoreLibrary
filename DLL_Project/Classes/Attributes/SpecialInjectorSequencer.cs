using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SpecialInjectorSequencer : SequencedInjectorAttribute
    {

        public                              SpecialInjectorSequencer( InjectionSequence sequence = InjectionSequence.MainLoad, InjectionTiming timing = InjectionTiming.SpecialInjectors ) : base( sequence, timing )
        {
        }

    }

}
