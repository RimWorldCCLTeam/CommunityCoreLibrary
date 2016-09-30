using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Property)]
    public class DetourClassProperty : Attribute
    {
        
        public Type                         fromClass;
        public string                       fromProperty;
        public InjectionTiming              injectionTiming;

        public                              DetourClassProperty( Type fromClass, string fromProperty, InjectionTiming injectionTiming = InjectionTiming.BeforeSpecialInjectors )
        {
            this.fromClass                  = fromClass;
            this.fromProperty               = fromProperty;
            this.injectionTiming            = injectionTiming;
        }

    }

}
