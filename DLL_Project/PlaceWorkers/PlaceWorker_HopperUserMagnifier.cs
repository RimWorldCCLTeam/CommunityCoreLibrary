using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Verse;

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
        public override void                DrawGhost( ThingDef def, IntVec3 center, Rot4 rot )
        {
            var connectionCell = center + rot.FacingCell;
            var hopperUser = CompHopper.FindHopperUser( connectionCell );
            var thingUser = hopperUser == null ? (Thing) null : hopperUser.parent;
            if(
                ( thingUser != null )&&
                ( !thingUser.OccupiedRect().Cells.Contains( center ) )
            )
            {                
                GenDraw.DrawFieldEdges( thingUser.OccupiedRect().Cells.ToList() );
                GenDraw.DrawTargetHighlight( thingUser );
            }
            else
            {
                GenDraw.DrawTargetHighlight( connectionCell );
            }
        }

    }

}
