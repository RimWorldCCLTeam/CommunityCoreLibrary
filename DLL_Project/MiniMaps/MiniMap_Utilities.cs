using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{

	public static class MiniMap_Utilities
    {

        static Color[]                  _clearPixelArray;
                
        public static Color[] GetClearPixelArray
        {
            get
            {
                if ( _clearPixelArray == null )
                {
                    // create a clear pixel array for resetting textures
                    _clearPixelArray = new Color[Find.Map.Size.x * Find.Map.Size.z];
                    for ( int i = 0; i < _clearPixelArray.Count(); i++ )
                    {
                        _clearPixelArray[i] = Color.clear;
                    }
                }
                return _clearPixelArray;
            }
        }

        #region Rendering

        public static void DrawThing( Texture2D texture, Thing thing, Color color )
		{
#if DEVELOPER
            CCL_Log.Message( "Painting cells for " + thing.LabelCap + thing.Position + color );
#endif

			// check if this makes sense
            if( texture == null )
            {
                CCL_Log.Error( "Tried to draw to NULL texture" );
                return;
            }
			if( thing == null )
			{
				CCL_Log.Error( "Tried to get occupied rect for NULL thing" );
				return;
			}
			if(
				( thing.OccupiedRect().Cells == null ) ||
				( thing.OccupiedRect().Cells.Count() == 0 )
			)
			{
				CCL_Log.Error( "Tried to get occupier rect for " + thing.LabelCap + " but it is NULL or empty" );
				return;
			}

			// paint all cells occupied by thing in 'color'.
			foreach( var cell in thing.OccupiedRect().Cells )
			{
				if( cell.InBounds() )
				{
					texture.SetPixel( cell.x, cell.z, color );
				}
			}
		}

		#endregion Methods

	}

}
