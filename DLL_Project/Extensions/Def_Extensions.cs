using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public static class Def_Extensions
    {
        /// <summary>
        /// Get the helpdef associated with the current def, or null if none exists.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static HelpDef GetHelpDef(this Def def)
        {
            return DefDatabase<HelpDef>.AllDefsListForReading.FirstOrDefault(hd => hd.keyDef == def);
        }

    }
}
