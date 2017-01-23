using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    public class MiniMapOverlay_Colonists : MiniMapOverlay_Pawns, IConfigurable
    {
        #region Fields

        private Color color = Color.green;
        private UI.LabeledInput_Color colorInput;
        private float radius = 3f;

        private UI.LabeledInput_Float radiusInput;

        #endregion Fields

        #region Constructors

        public MiniMapOverlay_Colonists( MiniMap minimap, MiniMapOverlayDef overlayDef ) : base( minimap, overlayDef )
        {
            radiusInput = new UI.LabeledInput_Float( radius, "MiniMap.CP.Radius".Translate(), "MiniMap.CP.RadiusTip".Translate() );
            colorInput = new UI.LabeledInput_Color( color, "MiniMap.CP.Color".Translate(), "MiniMap.CP.ColorTip".Translate() );
        }

        #endregion Constructors

        #region Methods

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            colorInput.Draw( row );
            color = colorInput.Value;
            row.y += 30f;

            radiusInput.Draw( row );
            radius = radiusInput.Value;
            row.y += 30f;

            return 2 * 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref color, "color" );
            Scribe_Values.LookValue( ref radius, "radius" );

            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                radiusInput.Value = radius;
                colorInput.Value = color;
            }
        }

        public override Color GetColor( Pawn pawn )
        {
            return color;
        }

        public override IEnumerable<Pawn> GetPawns()
        {
            return Find.VisibleMap.mapPawns.FreeColonists;
        }

        public override float GetRadius( Pawn pawn )
        {
            return radius;
        }

        #endregion Methods
    }
}