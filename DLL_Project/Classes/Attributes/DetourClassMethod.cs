using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Method)]
    public class DetourClassMethod : Attribute
    {
        public Type                         fromClass;
        public string                       fromMethod;
        public InjectionTiming              injectionTiming;

        public                              DetourClassMethod( Type fromClass, string fromMethod, InjectionTiming injectionTiming = InjectionTiming.BeforeSpecialInjectors )
        {
            this.fromClass                  = fromClass;
            this.fromMethod                 = fromMethod;
            this.injectionTiming            = injectionTiming;
        }

    }

}
