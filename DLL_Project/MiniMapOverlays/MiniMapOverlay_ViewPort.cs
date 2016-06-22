using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_ViewPort : MiniMapOverlay, IConfigurable
    {
        #region Fields

        private Color color = Color.white;
        private UI.LabeledInput_Color colorInput;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_ViewPort( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
            colorInput = new UI.LabeledInput_Color( color, "MiniMap.ViewPort.Color".Translate(), "MiniMap.ViewPort.ColorTip".Translate() );
        }

        #endregion Constructors

        #region Methods

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            colorInput.Draw( row );
            color = colorInput.Value;

            return 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref color, "color" );

            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
                colorInput.Value = color;
        }

        public override void Update()
        {
            // clear texture
            ClearTexture();

            // draw square
            var edges = Find.CameraDriver.CurrentViewRect.EdgeCells;
            foreach ( var edge in edges )
            {
                if ( edge.InBounds() )
                {
                    texture.SetPixel( edge.x, edge.z, color );
                }
            }
        }

        #endregion Methods
    }
}