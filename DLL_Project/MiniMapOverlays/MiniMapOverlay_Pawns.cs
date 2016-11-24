using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public abstract class MiniMapOverlay_Pawns : MiniMapOverlay
	{

		#region Constructors

		protected MiniMapOverlay_Pawns( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Methods

		public virtual void CreateMarker( Pawn pawn, bool transparentEdges = true, float edgeOpacity = .5f )
		{
            float radius = GetRadius( pawn );

			// check for valid radius
			if( radius < 1f )
			{
				throw new ArgumentOutOfRangeException( "radius must be > 1f" );
			}

			// count number of cells in full and 'inner' opaque dot
			int count = GenRadial.NumCellsInRadius( radius );
			int opaqueCount = transparentEdges && radius >= 2f ? GenRadial.NumCellsInRadius( radius - 1f ) : count;

			// get colors
			var opaqueColor = GetColor( pawn );
            var transparentColor = new Color( opaqueColor.r, opaqueColor.g, opaqueColor.b, opaqueColor.a * edgeOpacity );

			// get all cells in a circle around the pawn
			var cells = GenRadial.RadialCellsAround( pawn.Position, radius, true ).ToArray();
			for( int i = 0; i < count; i++ )
			{
				// paint it black!
				if( cells[ i ].InBounds() )
				{
					texture.SetPixel( cells[ i ].x, cells[ i ].z, transparentEdges && i > opaqueCount ? transparentColor : opaqueColor );
				}
			}
		}

		public abstract Color GetColor( Pawn pawn );

        public virtual float GetRadius( Pawn pawn ) { return 3f; }

		public abstract IEnumerable<Pawn> GetPawns();

		public override void Update()
		{
            try
            {
    			// get list of pawns
    			var pawns = GetPawns();

                // clear texture
                ClearTexture();

    			// create a marker for each pawn
    			foreach( var pawn in pawns )
    			{
    				CreateMarker( pawn );
    			}
            }
            catch( Exception e )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "An error has occured in '{0}'\nError message:\n{1}\nStack Trace:\n{2}", this.GetType().Name, e.ToString(), Environment.StackTrace ),
                    "MiniMapOverlay_Pawns.Update"
                );
            }
		}

		#endregion Methods

	}

}
