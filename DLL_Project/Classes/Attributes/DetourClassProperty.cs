using System;
namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Property)]
    public class DetourClassProperty : Attribute
    {
        
        public Type                         targetClass;
        public string                       property;
        public DetourInjectionTiming        injectionTiming;

        public                              DetourClassProperty( Type targetClass, string property, DetourInjectionTiming injectionTiming = DetourInjectionTiming.PostSpecialInjector )
        {
            this.targetClass        = targetClass;
            this.property           = property;
            this.injectionTiming    = injectionTiming;
        }

    }

}
