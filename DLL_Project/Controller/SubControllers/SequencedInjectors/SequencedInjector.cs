using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public abstract class SequencedInjector
    {

        public string                       Errors = string.Empty;

        public virtual string               Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public abstract bool                IsValid();

        public abstract bool                Inject( InjectionSequence sequence, InjectionTiming timing );

        public virtual bool                 ReResolveDefs( InjectionSequence sequence )
        {
            return true;
        }

    }

}
