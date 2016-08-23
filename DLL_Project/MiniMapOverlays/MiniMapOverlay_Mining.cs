using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.MiniMap
{
    
    public class MiniMapOverlay_Mining : MiniMapOverlay, IConfigurable
    {

        private Color                   miningColor = new Color( 0.75f, 0.4f, 0.125f, 1f );
        private UI.LabeledInput_Color   miningColorInput;

        #region Constructors

        public MiniMapOverlay_Mining( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

        public override void Update()
        {
            ClearTexture();

            // get the mining designators
            var miningDesignators = Find.DesignationManager.allDesignations
                                        .Where( d => d.def == DesignationDefOf.Mine )
                                        .ToList();

            // loop over all designators
            foreach( var miningDesignator in miningDesignators )
            {
                // set pixel color
                var pos = miningDesignator.target.Cell;
                texture.SetPixel( pos.x, pos.z, miningColor );
            }
        }

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            miningColorInput.Draw( row );
            miningColor = miningColorInput.Value;

            return 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref miningColor, "miningColor" );

            if( Scribe.mode == LoadSaveMode.LoadingVars )
                UpdateInputFields();
        }

        private void CreateInputFields()
        {
            miningColorInput      = new UI.LabeledInput_Color( miningColor, "MiniMap.Mining.miningColor".Translate(), "MiniMap.Mining.miningColorTip".Translate() );
        }

        private void UpdateInputFields()
        {
            miningColorInput.Value      = miningColor;
        }

        #endregion Methods

    }

}

