using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DetourMember : SequencedInjectorAttribute
    {

        public const Type                   DefaultTargetClass = null;
        public const string                 DefaultTargetMemberName = "";
        public const InjectionSequence      DefaultSequence = InjectionSequence.MainLoad;
        public const InjectionTiming        DefaultTiming = InjectionTiming.Detours;

        public Type                         targetClass;
        public string                       targetMember;

        public                              DetourMember( Type targetClass, string targetMember, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.targetClass                = targetClass;
            this.targetMember               = targetMember;
        }

        public                              DetourMember( Type targetClass, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.targetClass                = targetClass;
            this.targetMember               = DefaultTargetMemberName;
        }

        public                              DetourMember( string targetMember, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.targetClass                = DefaultTargetClass;
            this.targetMember               = targetMember;
        }

        public                              DetourMember( InjectionSequence sequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.targetClass                = DefaultTargetClass;
            this.targetMember               = DefaultTargetMemberName;
        }

        public                              DetourMember( InjectionTiming timing ) : base( DefaultSequence, timing )
        {
            this.targetClass                = DefaultTargetClass;
            this.targetMember               = DefaultTargetMemberName;
        }

        public                              DetourMember() : base( DefaultSequence, DefaultTiming )
        {
            this.targetClass                = DefaultTargetClass;
            this.targetMember               = DefaultTargetMemberName;
        }

    }

}
