using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class MiniMap
    {
        
        #region Instance Data

        private bool            _hidden = false;

        public MiniMapDef       def;

        public List<MiniMapOverlay>   overlayWorkers;

        #endregion Instance Data

        private Texture2D       iconTexture;

        static Color[]          _clearPixelArray;

        #region Constructors

        public MiniMap( MiniMapDef miniMapDef )
        {
            this.def = miniMapDef;
            this._hidden = this.def.hiddenByDefault;

            if( !this.def.iconTex.NullOrEmpty() )
            {
                iconTexture = ContentFinder<Texture2D>.Get( this.def.iconTex, true );
            }
            overlayWorkers = new List<MiniMapOverlay>();

            for( int index = 0; index < def.overlays.Count; ++index )
            {
                var overlayData = def.overlays[ index ];
                if(
                    ( overlayData.overlayClass == null )||
                    (
                        ( overlayData.overlayClass != typeof( MiniMapOverlay ) )&&
                        ( !overlayData.overlayClass.IsSubclassOf( typeof( MiniMapOverlay ) ) )
                    )
                )
                {
                    CCL_Log.TraceMod(
                        miniMapDef,
                        Verbosity.NonFatalErrors,
                        string.Format( "Unable to resolve overlayClass for '{0}' at index {1} to 'CommunityCoreLibrary.MiniMapOverlay'", miniMapDef.defName, index )
                    );
                    return;
                }
                else
                {
                    var overlayWorker = (MiniMapOverlay) Activator.CreateInstance( overlayData.overlayClass, new System.Object[] { this, overlayData } );
                    if( overlayWorker == null )
                    {
                        CCL_Log.TraceMod(
                            miniMapDef,
                            Verbosity.NonFatalErrors,
                            string.Format( "Unable to create instance of '{0}' for '{1}'", overlayData.overlayClass.Name, miniMapDef.defName )
                        );
                        return;
                    }
                    else
                    {
                        overlayWorkers.Add( overlayWorker );
                    }
                }
            }

            // log a bit
            CCL_Log.TraceMod(
                this.def,
                Verbosity.Injections,
                string.Format( "Added overlay '{0}' at draw position {1}", this.GetType().FullName, this.def.drawOrder )
            );
        }

        #endregion Constructors

        #region Static Properties

        public static IntVec2 Size
        {
            get
            {
                return new IntVec2( Find.Map.Size.x, Find.Map.Size.z );
            }
        }

        public static Color[] GetClearPixelArray
        {
            get
            {
                if( _clearPixelArray == null )
                {
                    // create a clear pixel array for resetting textures
                    _clearPixelArray = new Color[ Find.Map.Size.x * Find.Map.Size.z ];
                    for( int i = 0; i < _clearPixelArray.Count(); i++ )
                    {
                        _clearPixelArray[ i ] = Color.clear;
                    }
                }
                return _clearPixelArray;
            }
        }

        #endregion

        #region Properties

        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                //controller.dirty = true;
                MiniMapController.dirty = true;
            }
        }

        public virtual Texture2D Icon
        {
            get
            {
                if( iconTexture.NullOrBad() )
                {
                    return TexUI.UnknownThing;
                }
                return iconTexture;
            }
        }

        public string LabelCap
        {
            get
            {
                if( def.labelKey.NullOrEmpty() )
                {
                    return def.label.CapitalizeFirst();
                }
                return def.labelKey.Translate().CapitalizeFirst();
            }
        }

        #endregion Properties

        #region Methods

        public virtual void Update()
        {
            foreach( var overlay in overlayWorkers )
            {
                overlay.Update();
                overlay.texture.Apply();
            }
        }

        public virtual void DrawOverlays( Rect inRect )
        {
            var workers = overlayWorkers.Where( worker => !worker.Hidden ).OrderByDescending( worker => worker.overlayData.drawOffset );
            if( !workers.Any() )
            {
                return;
            }
            foreach( var worker in workers )
            {
                GUI.DrawTexture( inRect, worker.texture );
            }
        }

        #endregion Methods
    }
}