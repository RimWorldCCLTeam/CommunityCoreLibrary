using System;
using Verse;

namespace CommunityCoreLibrary
{

    public class MCMInjectionSet
    {

        public string                       label;

        // Optional:  Ability to load MCM data before anything else (default behaviour: last as per 0.13.1 and older)
        public bool                         preload = false;

        public Type                         mcmClass;

    }

}
