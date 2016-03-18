using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_#InjectorClass# : IInjector
    {

#if DEBUG
        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.someList.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var something in def.someList )
            {
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.someList.NullOrEmpty() )
            {
                return true;
            }

            return def.EngineLevelInjectionsComplete;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.someList.NullOrEmpty() )
            {
                return true;
            }

            foreach( var something in def.someList )
            {
            }

            return true;

        }

    }

}
