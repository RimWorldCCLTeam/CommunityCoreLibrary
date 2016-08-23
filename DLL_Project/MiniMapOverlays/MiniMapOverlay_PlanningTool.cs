using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.MiniMap
{
    
    public class MiniMapOverlay_PlanningTool : MiniMapOverlay, IConfigurable
    {

        private Color                   planningColor = Color.white;
        private UI.LabeledInput_Color   planningColorInput;

        #region Constructors

        public MiniMapOverlay_PlanningTool( MiniMap minimap, MiniMapOverlayDef overlayData ) : base( minimap, overlayData )
        {
            CreateInputFields();
        }

        #endregion Constructors

        #region Methods

        public override void Update()
        {
            ClearTexture();

            // get the planning designators
            var planningDesignators = Find.DesignationManager.allDesignations
                                          .Where( d => d.def == DesignationDefOf.Plan )
                                          .ToList();

            // loop over all cells
            foreach( var planningDesignator in planningDesignators )
            {
                // set pixel color
                var pos = planningDesignator.target.Cell;
                texture.SetPixel( pos.x, pos.z, planningColor );
            }
        }

        public float DrawMCMRegion( Rect InRect )
        {
            Rect row = InRect;
            row.height = 24f;

            planningColorInput.Draw( row );
            planningColor = planningColorInput.Value;

            return 30f;
        }

        public void ExposeData()
        {
            Scribe_Values.LookValue( ref planningColor, "planningColor" );

            if( Scribe.mode == LoadSaveMode.LoadingVars )
                UpdateInputFields();
        }

        private void CreateInputFields()
        {
            planningColorInput      = new UI.LabeledInput_Color( planningColor, "MiniMap.PlanningTool.planningColor".Translate(), "MiniMap.PlanningTool.planningColorTip".Translate() );
        }

        private void UpdateInputFields()
        {
            planningColorInput.Value      = planningColor;
        }

        #endregion Methods

    }

}

