using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Method)]
    public class DetourClassMethod : Attribute
    {
        public Type                         targetClass;
        public string                       method;
        public DetourInjectionTiming        injectionTiming;

        public                              DetourClassMethod( Type targetClass, string method, DetourInjectionTiming injectionTiming = DetourInjectionTiming.PostSpecialInjector )
        {
            this.targetClass        = targetClass;
            this.method             = method;
            this.injectionTiming    = injectionTiming;
        }

    }

}
