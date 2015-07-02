using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

    public static class Research
    {
        public static ResearchProjectDef Locker
        {
            get{
                return DefDatabase<ResearchProjectDef>.GetNamed( "CommunityCoreLibraryResearchLocker" );
            }
        }
        public const ResearchProjectDef Unlocker = null;
    }

}