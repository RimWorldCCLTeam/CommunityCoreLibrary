using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;
using RimWorld;

namespace CommunityCoreLibrary
{
    
    public class PlaceWorker_HopperMagnifier : PlaceWorker
    {

        /// <summary>
        /// Draws a target highlight on all connectable Hoppers around target
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        public override void				DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
            List<CompHopper> hoppers = CompHopperUser.FindHoppers( center, rot, def.Size );
            foreach( var hopper in hoppers )
            {
                GenDraw.DrawTargetHighlight( hopper.parent );
            }
        }

    }

}
