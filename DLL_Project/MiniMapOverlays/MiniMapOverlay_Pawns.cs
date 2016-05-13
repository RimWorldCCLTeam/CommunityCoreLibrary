using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public abstract class MiniMapOverlay_Pawns : MiniMapOverlay //, IConfigurable
	{

		#region Fields

		public float radius = 3f;

		#endregion Fields

		#region Constructors

		protected MiniMapOverlay_Pawns( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Methods

		public virtual void CreateMarker( Pawn pawn, float radius = 3f, bool transparentEdges = true, float opacity = 1f, float edgeOpacity = .5f )
		{
			// check for valid radius
			if( radius < 1f )
			{
				throw new ArgumentOutOfRangeException( "radius must be > 1f" );
			}

			// count number of cells in full and 'inner' opaque dot
			int count = GenRadial.NumCellsInRadius( radius );
			int opaqueCount = transparentEdges && radius >= 2f ? GenRadial.NumCellsInRadius( radius - 1f ) : count;

			// get colors
			var opaqueColor = GetColor( pawn, opacity );
			var transparentColor = GetColor( pawn, edgeOpacity );

			// get all cells in a circle around the pawn
			var cells = GenRadial.RadialCellsAround( pawn.Position, radius, true ).ToArray();
			for( int i = 0; i < count; i++ )
			{
				// paint it green!
				if( cells[ i ].InBounds() )
				{
					texture.SetPixel( cells[ i ].x, cells[ i ].z, transparentEdges && i > opaqueCount ? transparentColor : opaqueColor );
				}
			}
		}

		public abstract Color GetColor( Pawn pawn, float opacity = 1f );

		public abstract IEnumerable<Pawn> GetPawns();

		public override void Update()
		{
			// get list of pawns
			var pawns = GetPawns();

            // clear texture
            ClearTexture();

			// create a marker for each pawn
			foreach( var pawn in pawns )
			{
				CreateMarker( pawn, radius );
			}
		}

        public float DrawMCMRegion( Rect canvas )
        {

        }

		#endregion Methods

	}

}
