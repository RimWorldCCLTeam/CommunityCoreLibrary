using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    
    public abstract class MiniMapOverlay // : IConfigurable
    {
        
        #region Fields

        public MiniMapOverlayDef        overlayDef;
        private bool                    _hidden = false;

        private Texture2D               _texture;
        private MiniMap                 minimap;

        public bool                     dirty;

        #endregion Fields

        #region Constructors

        protected                       MiniMapOverlay( MiniMap minimap, MiniMapOverlayDef overlayDef )
        {
            this.minimap = minimap;
            this.overlayDef = overlayDef;
            _hidden = overlayDef.hiddenByDefault;
            dirty = true;
        }

        #endregion Constructors

        #region Properties

        public virtual bool             Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;

                // make entire minimap visible if any overlay is made visible
                if( !_hidden )
                {
                    minimap.Hidden = value;
                    // Mark as dirty for immediate update
                    dirty = true;
                }

                // mark the controller dirty so everything is properly sorted
                MiniMapController.dirty = true;
            }
        }

        public string                   SaveKey
        {
            get
            {
                if( minimap.miniMapDef.dynamicOverlays )
                {
                    return MiniMapController.regEx.Replace( overlayDef.label, "" );
                }
                return overlayDef.defName;
            }
        }

        public virtual string           label
        {
            get
            {
                if ( overlayDef.labelKey.NullOrEmpty() )
                {
                    return overlayDef.label;
                }
                return overlayDef.labelKey.Translate();
            }
        }

        public virtual string           LabelCap
        {
            get
            {
                return label.CapitalizeFirst();
            }
        }

        public Texture2D                texture
        {
            get
            {
                if ( _texture == null )
                {
                    _texture = new Texture2D( Find.Map.Size.x, Find.Map.Size.z );

                    // not sure clearing pixels is strictly necessary, but can't hurt much
                    _texture.SetPixels( MiniMap_Utilities.GetClearPixelArray );
                    _texture.Apply();
                }
                return _texture;
            }
        }

        #endregion Properties

        #region Methods

        public void                     ClearTexture( bool apply = false )
        {
            texture.SetPixels( MiniMap_Utilities.GetClearPixelArray );
            if( apply )
            {
                texture.Apply();
            }
        }

        // Optional float menu option for this overlay
        public virtual List<FloatMenuOption> GetFloatMenuOptions()
        {
            // create empty list of options
            var options = new List<FloatMenuOption>();

            // add a toggle option for overlays that are not always on, and only if there's more than one overlay on this minimap 'mode'
            if( !minimap.miniMapDef.alwaysVisible && minimap.overlayWorkers.Count > 1 )
            {
                options.Add( new FloatMenuOption( "MiniMap.ToggleX".Translate( Hidden ? "Off".Translate().ToUpper() : "On".Translate().ToUpper(), LabelCap ), delegate
                {
                    Hidden = !Hidden;
                } ) );
            }

            // done!
            return options;
        }

        // Required, update your texture, caller (MiniMap.Update()) will re-Apply()
        public abstract void            Update();

        #endregion Methods
    }
}