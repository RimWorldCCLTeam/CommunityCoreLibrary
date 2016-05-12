using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

	public class MiniMapOverlay_Fog : MiniMapOverlay
	{

		#region Constructors

        public MiniMapOverlay_Fog( MiniMap minimap, MiniMapOverlayData overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Methods

		public override void Update()
		{
			// get the fog grid
			var fog = Find.Map.fogGrid.fogGrid;

			// loop over all cells
			for( int i = 0; i < MiniMap.Size.x * MiniMap.Size.z; i++ )
			{
				// set pixel color
				var pos = CellIndices.IndexToCell( i );
				texture.SetPixel( pos.x, pos.z, fog[ i ] ? Color.gray : Color.clear );
			}
		}

		#endregion Methods

	}

}
