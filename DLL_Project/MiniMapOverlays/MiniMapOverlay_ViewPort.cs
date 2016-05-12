using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

	public class MiniMapOverlay_ViewPort : MiniMapOverlay
	{

		#region Constructors

		public MiniMapOverlay_ViewPort( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
		{
		}

		#endregion Constructors

		#region Methods

		public override void Update()
		{
			// clear texture
			texture.SetPixels( MiniMap.GetClearPixelArray );

			// draw square
			var edges = Find.CameraMap.CurrentViewRect.EdgeCells;
			foreach( var edge in edges )
			{
				if( edge.InBounds() )
				{
					texture.SetPixel( edge.x, edge.z, Color.white );
				}
			}
		}

		#endregion Methods

	}

}
