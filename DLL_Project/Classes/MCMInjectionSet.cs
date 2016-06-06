using System;
using Verse;

namespace CommunityCoreLibrary
{

    public class MCMInjectionSet
    {

        public string                       label;

        // Optional:  Use this string as the save key instead of the label, will help with labels which contain strange characters that need special parsing (such as ampersands, quotes, etc)
        public string                       saveKey;

        // Optional:  Ability to load MCM data before anything else (default behaviour: last as per 0.13.1 and older)
        public bool                         preload = false;

        public Type                         mcmClass;

    }

}
