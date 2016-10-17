using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Class)]
    public class SequencedInjectorAttribute : Attribute
    {

        public InjectionSequence            injectionSequence = InjectionSequence.Never;
        public InjectionTiming              injectionTiming = InjectionTiming.Never;

        public                              SequencedInjectorAttribute( InjectionSequence sequence, InjectionTiming timing )
        {
            this.injectionTiming            = timing;
            this.injectionSequence          = sequence;
        }

    }

}
