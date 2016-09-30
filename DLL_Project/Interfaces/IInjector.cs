using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public enum IInjectorType
    {
        ByDef,
        ByTiming
    }


    public abstract class IInjector
    {

        public virtual IInjectorType        InjectorType
        {
            get
            {
                return IInjectorType.ByDef;
            }
        }

#if DEBUG
        public abstract string              InjectString { get; }

        public virtual bool                 IsValid( ModHelperDef def, ref string errors )
        {
            return true;
        }
#endif

        public virtual bool                 DefIsInjected( ModHelperDef def )
        {
            return true;
        }
        public virtual bool                 TimingIsInjected( InjectionTiming priority )
        {
            return true;
        }

        public virtual bool                 InjectByDef( ModHelperDef def )
        {
            return true;
        }
        public virtual bool                 InjectByTiming( InjectionTiming priority )
        {
            return true;
        }

    }

}
