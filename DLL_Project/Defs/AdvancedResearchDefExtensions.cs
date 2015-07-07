using System;
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

    public static class AdvancedResearchDefExtensions
    {
        public static bool ResearchGroupComplete( this AdvancedResearchDef arp )
        {
            // God mode, allow it
            if( Game.GodMode == true )
                return true;

            // No god mode, check it
            for( int rIndex = 0, rCountTo = arp.researchDefs.Count; rIndex < rCountTo; rIndex++ )
                if( !arp.researchDefs[ rIndex ].IsFinished )
                    return false;

            // All done
            return true;
        }

    }
}

