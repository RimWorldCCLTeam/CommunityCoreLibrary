using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    
    public abstract class MiniMapOverlay
    {
        
        #region Instance Data

        // TODO:  Make overlays individually selectable, currently fixed at def default
        private bool                _hidden = false;

        public Texture2D            texture;
        private MiniMap             minimap;
        public MiniMapOverlayDef    overlayDef;

        #endregion Instance Data

        #region Properties

        public bool                 Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                if( !_hidden )
                {
                    minimap.Hidden = value;
                }
                MiniMapController.dirty = true;
            }
        }

        public string               label
        {
            get
            {
                if( overlayDef.labelKey.NullOrEmpty() )
                {
                    return overlayDef.label;
                }
                return overlayDef.labelKey.Translate();
            }
        }

        public string               LabelCap
        {
            get
            {
                return label.CapitalizeFirst();
            }
        }

        #endregion

        #region Constructors

        public                      MiniMapOverlay( MiniMap minimap, MiniMapOverlayDef overlayDef )
        {
            this.minimap = minimap;
            this.overlayDef = overlayDef;
            _hidden = overlayDef.hiddenByDefault;

            // create texture
            texture = new Texture2D( MiniMap.Size.x, MiniMap.Size.z );
            if( texture == null )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Unable to create texture for '{0}'", this.GetType().Name )
                );
                return;
            }

            // transparent.... for now!
            texture.SetPixels( MiniMap.GetClearPixelArray );

            // apply changes
            texture.Apply();
        }

        #endregion Constructors

        #region Methods

        // Required, update your texture, caller (MiniMap.Update()) will re-Apply()
        public abstract void        Update();

        // Optional float menu option for this overlay
        public virtual FloatMenuOption  GetFloatMenuOption()
        {
            return null;
        }

        // Optional mod configuration menu content region, inRect is only valid for x, y, width; return final height (inRect.height can be ignored)
        public virtual float        DoMCMRegion( Rect inRect )
        {
            return 0f;
        }

        #endregion Methods
    }
}