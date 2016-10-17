using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Property)]
    public class DetourClassProperty : SequencedInjectorAttribute
    {
        
        public Type                         fromClass;
        public string                       fromProperty;

        public                              DetourClassProperty( Type fromClass, string fromProperty, InjectionSequence sequence = InjectionSequence.MainLoad, InjectionTiming timing = InjectionTiming.Detours ) : base( sequence, timing )
        {
            this.fromClass                  = fromClass;
            this.fromProperty               = fromProperty;
        }

    }

}
