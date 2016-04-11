using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace CommunityCoreLibrary
{
    public static class Texture2D_Extensions
    {
        // NOTE: This entire method will not work with core assets, which rather defeats the purpose.
        // Core assets are not marked as readable (not kept around in memory - only GPU).
        // Alternatives such as GetRawTextureData() are not yet implemented in the dated version of Unity Tynan uses.
        //      - Fluffy

        /// <summary>
        /// Returns a new texture with as much blank space as possible stripped, while respecting (to an extent) original proportions.
        /// WARNING: Does not cache! Textures are not automatically garbage collected, 
        /// so make sure to destroy and/or cache textures where appropriate.
        /// Failing to do so will lead to a memory leak.
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static Texture2D Crop( this Texture2D tex )
        {
            // see note above
            return tex;

            /* Commented out unreachable code - 1000101
            int xMin = 0, xMax = 0, yMin = 0, yMax = 0;
            float proportion = (float)tex.width / (float)tex.height;

            // loop over rows of pixels to determine first and last row with any opacity.
            for (int x = 0; x < tex.height; x++ )
            {
                var row = tex.GetPixels( x, 0, tex.width, 1 );
                if (row.Any( c => c.a > 1e-4 ) )
                {
                    xMin = x;
                    break;
                }
                if ( x == tex.width - 1 )
                {
                    // only need to check this once, if a single pixel has alpha it will inevitably pop up.
                    CCL_Log.Error( "Whole texture will be cropped." );
                    return tex;
                }
            }
            // and from bottom up for the max
            for( int x = tex.height - 1; x >= 0; x-- )
            {
                var row = tex.GetPixels( x, 0, tex.width, 1 );
                if( row.Any( c => c.a > 1e-4 ) )
                {
                    xMax = x;
                    break;
                }
            }
            // columns, left to right
            for( int y = 0; y < tex.width; y++ )
            {
                var column = tex.GetPixels( 0, y, 1, tex.height );
                if( column.Any( c => c.a > 1e-4 ) )
                {
                    yMin = y;
                    break;
                }
                return tex;
            }
            // right to left
            for( int y = tex.width - 1; y >= 0; y-- )
            {
                var column = tex.GetPixels( 0, y, 1, tex.height );
                if( column.Any( c => c.a > 1e-4 ) )
                {
                    yMax = y;
                    break;
                }
            }

            // respect proportions, upscale x or y range when necessary
            int croppedWidth = xMax - xMin + 1;
            int croppedHeight = yMax - yMin + 1;
            if ( croppedHeight <= 0 ||
                 croppedWidth <= 0 )
            {
                CCL_Log.Error( "Whole texture will be cropped." );
                return tex;
            }
            if ( (float)croppedWidth / (float)croppedHeight < proportion )
            {
                xMin -= (int)( proportion * croppedHeight - croppedWidth ) / 2;
                xMax += (int)( proportion * croppedHeight - croppedWidth ) / 2;
            }
            else if ( (float)croppedWidth / (float)croppedHeight > proportion )
            {
                yMin -= (int)( croppedWidth / proportion - croppedHeight ) / 2;
                yMax += (int)( croppedWidth / proportion - croppedHeight ) / 2;
            }

            // if respecting proportions means we go out of bounds, screw proportions
            xMin = Mathf.Clamp( xMin, 0, tex.width - 1 );
            xMax = Mathf.Clamp( xMax, 0, tex.width - 1 );
            yMin = Mathf.Clamp( yMin, 0, tex.height - 1 );
            yMax = Mathf.Clamp( yMax, 0, tex.height - 1 );

            // return a new texture as a pixel slice from the old texture
            Texture2D cropped = new Texture2D( xMax - xMin, yMax - yMin );
            cropped.SetPixels( tex.GetPixels( xMin, yMin, xMax - xMin, yMax - yMin ) );
            cropped.Apply();
            return cropped;
            */
        }

        public static void DrawFittedIn( this Texture2D tex, Rect rect )
        {
            float rectProportion = (float)rect.width / (float)rect.height;
            float texProportion = (float)tex.width / (float)tex.height;

            if ( texProportion > rectProportion )
            {
                Rect wider = new Rect(rect.xMin, 0f, rect.width, rect.width / texProportion ).CenteredOnYIn(rect).CenteredOnXIn(rect);
                GUI.DrawTexture( wider, tex );
                return;
            }
            else if ( texProportion < rectProportion )
            {
                Rect taller = new Rect(0f, rect.yMin, rect.height * texProportion, rect.height ).CenteredOnXIn(rect).CenteredOnXIn(rect);
                GUI.DrawTexture( taller, tex );
                return;
            }
            GUI.DrawTexture( rect, tex );
        }
    }
}
