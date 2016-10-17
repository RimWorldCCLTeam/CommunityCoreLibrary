using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Method)]
    public class DetourClassMethod : SequencedInjectorAttribute
    {
        
        public Type                         fromClass;
        public string                       fromMethod;

        public                              DetourClassMethod( Type fromClass, string fromMethod, InjectionSequence sequence = InjectionSequence.MainLoad, InjectionTiming timing = InjectionTiming.Detours ) : base( sequence, timing )
        {
            this.fromClass                  = fromClass;
            this.fromMethod                 = fromMethod;
        }

    }

}
