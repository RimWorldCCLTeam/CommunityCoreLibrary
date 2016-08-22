using CommunityCoreLibrary;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_Area : MiniMapOverlay
    {
        #region Fields

        public Area area;
        public static float opacity = .5f;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_Area( MiniMap minimap, MiniMapOverlayDef def, Area area ) : base( minimap, def )
        {
            this.area = area;

            // initial update
            //Update();
            //texture.Apply();
        }

        #endregion Constructors

        #region Properties

        public override string label => area.Label;

        #endregion Properties

        #region Methods

        public override void Update()
        {
            Color color = area.Color;
            color.a = opacity;
            ClearTexture();

            foreach ( var cell in area.ActiveCells )
            {
                texture.SetPixel( cell.x, cell.z, color );
            }
        }

        #endregion Methods
    }
}