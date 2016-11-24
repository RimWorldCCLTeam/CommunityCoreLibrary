using System;

namespace CommunityCoreLibrary
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class DetourMember : SequencedInjectorAttribute
    {

        public const Type                   DefaultFromClass = null;
        public const string                 DefaultMemberName = "";
        public const InjectionSequence      DefaultSequence = InjectionSequence.MainLoad;
        public const InjectionTiming        DefaultTiming = InjectionTiming.Detours;

        public Type                         fromClass;
        public string                       fromMember;

        public                              DetourMember( Type fromClass, string fromMember, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.fromClass                  = fromClass;
            this.fromMember                 = fromMember;
        }

        public                              DetourMember( Type fromClass, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.fromClass                  = fromClass;
            this.fromMember                 = DefaultMemberName;
        }

        public                              DetourMember( string fromMember, InjectionSequence sequence = DefaultSequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.fromClass                  = DefaultFromClass;
            this.fromMember                 = fromMember;
        }

        public                              DetourMember( InjectionSequence sequence, InjectionTiming timing = DefaultTiming ) : base( sequence, timing )
        {
            this.fromClass                  = DefaultFromClass;
            this.fromMember                 = DefaultMemberName;
        }

        public                              DetourMember( InjectionTiming timing ) : base( DefaultSequence, timing )
        {
            this.fromClass                  = DefaultFromClass;
            this.fromMember                 = DefaultMemberName;
        }

        public                              DetourMember() : base( DefaultSequence, DefaultTiming )
        {
            this.fromClass                  = DefaultFromClass;
            this.fromMember                 = DefaultMemberName;
        }

    }

}
