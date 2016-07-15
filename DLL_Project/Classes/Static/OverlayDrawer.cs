using System;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary
{

    public static class OverlayDrawer
    {

        private static readonly Mesh mesh;
        private static readonly float BaseAlt;

        static OverlayDrawer()
        {
            mesh = MeshPool.plane08;
            BaseAlt = Altitudes.AltitudeFor( AltitudeLayer.MetaOverlays );
        }

        public static void RenderPulsingOverlay( Thing thing, Material mat, int altInd )
        {
            if( mat.NullOrBad() )
            {
                return;
            }
            var drawPos = thing.TrueCenter();
            drawPos.y = BaseAlt + 0.05f * (float) altInd;
            // A14 - Find.RealTime was removed?
            var alpha = (float) ( 0.300000011920929 + ( Math.Sin( ( (double) Time.realtimeSinceStartup + 397.0 * (double) ( thing.thingIDNumber % 571 ) ) * 4.0 ) + 1.0 ) * 0.5 * 0.699999988079071 );
            var material = FadedMaterialPool.FadedVersionOf( mat, alpha );
            Graphics.DrawMesh( mesh, drawPos, Quaternion.identity, material, 0 );
        }

    }

}
