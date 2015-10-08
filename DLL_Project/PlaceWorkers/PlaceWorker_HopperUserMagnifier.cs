using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;
using RimWorld;

namespace CommunityCoreLibrary
{
    
    public class PlaceWorker_HopperUserMagnifier : PlaceWorker
    {
        
        /// <summary>
        /// Draws a target highlight on Hopper user or connection cell
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        public override void				DrawGhost(ThingDef def, IntVec3 center, Rot4 rot)
        {
			var connectionCell = center + rot.FacingCell;
			Thing hopperUser = CompHopper.FindHopperUser( connectionCell );
            if(
				( hopperUser != null )&&
				( !hopperUser.OccupiedRect().Cells.Contains( center ) )
			)
            {                
                GenDraw.DrawTargetHighlight( hopperUser );
            }
			else
			{
				GenDraw.DrawTargetHighlight( connectionCell );
			}
        }

    }

}
