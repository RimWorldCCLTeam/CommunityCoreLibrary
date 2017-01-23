using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

    public class MiniMapOverlay_Fog : MiniMapOverlay
    {

        #region Constructors

        public MiniMapOverlay_Fog( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
        }

        #endregion Constructors

        #region Methods

        public override void Update()
        {
            // get the fog grid
            var fog = Find.VisibleMap.fogGrid.fogGrid;

            // loop over all cells
            for( int i = 0; i < Find.VisibleMap.cellIndices.NumGridCells; i++ )
            {
                // set pixel color
                var pos = Find.VisibleMap.cellIndices.IndexToCell( i );
                texture.SetPixel( pos.x, pos.z, fog[ i ] ? Color.gray : Color.clear );
            }
        }

        #endregion Methods

    }

}
