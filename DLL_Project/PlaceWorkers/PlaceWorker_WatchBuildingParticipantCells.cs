using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_WatchBuildingParticipantCells : PlaceWorker
    {

        /// <summary>
        /// Draws a ghost around all cells which are participatible from
        /// </summary>
        /// <param name="def"></param>
        /// <param name="center"></param>
        /// <param name="rot"></param>
        public override void                DrawGhost( ThingDef def, IntVec3 center, Rot4 rot )
        {
            var cells = def.GetParticipantCells( center, rot, false );
            GenDraw.DrawFieldEdges( cells, Color.white );
            cells = def.GetParticipantCells( center, rot, true );
            GenDraw.DrawFieldEdges( cells, Color.yellow );
        }

    }

}
