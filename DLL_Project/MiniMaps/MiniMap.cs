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

        private bool                    _hidden = false;

        public MiniMapDef               miniMapDef;

        public List<MiniMapOverlay>     overlayWorkers;

        private Texture2D               iconTexture;

        #endregion Instance Data

        #region Static Data

        static Color[]                  _clearPixelArray;

        #endregion

        #region Constructors

        public                          MiniMap( MiniMapDef miniMapDef )
        {
            this.miniMapDef = miniMapDef;
            this._hidden = this.miniMapDef.hiddenByDefault;

            if( !this.miniMapDef.iconTex.NullOrEmpty() )
            {
                iconTexture = ContentFinder<Texture2D>.Get( this.miniMapDef.iconTex, true );
            }
            overlayWorkers = new List<MiniMapOverlay>();

            for( int index = 0; index < this.miniMapDef.overlays.Count; ++index )
            {
                var overlayData = this.miniMapDef.overlays[ index ];
                if(
                    ( overlayData.overlayClass == null )||
                    (
                        ( overlayData.overlayClass != typeof(MiniMapOverlay ) )&&
                        ( !overlayData.overlayClass.IsSubclassOf( typeof(MiniMapOverlay ) ) )
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
                    var overlayWorker = (MiniMapOverlay)Activator.CreateInstance( overlayData.overlayClass, new System.Object[] { this, overlayData } );
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
                this.miniMapDef,
                Verbosity.Injections,
                string.Format( "Added overlay '{0}' at draw position {1}", this.GetType().FullName, this.miniMapDef.drawOrder )
            );
        }

        #endregion Constructors

        #region Properties

        public List<MiniMapOverlay>     VisibleOverlays
        {
            get
            {
                return overlayWorkers.Where( worker => !worker.Hidden ).OrderByDescending( worker => worker.overlayDef.drawOffset ).ToList();
            }
        }

        #endregion

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

        public string label
        {
            get
            {
                if( miniMapDef.labelKey.NullOrEmpty() )
                {
                    return miniMapDef.label;
                }
                return miniMapDef.labelKey.Translate();
            }
        }

        public string LabelCap
        {
            get
            {
                return label.CapitalizeFirst();
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
            var workers = VisibleOverlays;
            if( workers.Any() )
            {
                foreach( var worker in workers )
                {
                    GUI.DrawTexture( inRect, worker.texture );
                }
            }
        }

        public virtual List<FloatMenuOption>  GetFloatMenuOptions()
        {
            var list = new List<FloatMenuOption>();
            foreach( var overlay in overlayWorkers )
            {
                var option = overlay.GetFloatMenuOption();
                if( option != null )
                {
                    list.Add( option );
                }
            }
            if( miniMapDef.mcmWorker != null )
            {
                list.Add( new FloatMenuOption(
                    "MiniMap.ShowMCMOption".Translate( label ),
                    () =>
                {
                    // TODO:  Open MCM Window to this worker
                    return;
                } ) );
            }
            return list.NullOrEmpty() ? null : list;
        }

        public virtual string   ToolTip
        {
            get
            {
                // Get tool tip (w/ description)
                // Use core translations for "Off" and "On"
                var tipString = string.Empty;
                if( !miniMapDef.description.NullOrEmpty() )
                {
                    tipString = miniMapDef.description + "\n\n";
                }
                tipString += "MiniMap.OverlayIconTip".Translate( LabelCap, Hidden ? "Off".Translate() : "On".Translate() );
                tipString += "\n\n";
                if( overlayWorkers.Count > 1 )
                {
                    for( int index = 0; index < overlayWorkers.Count; ++index )
                    {
                        var worker = overlayWorkers[ index ];
                        tipString += "MiniMap.OverlayIconTip".Translate( worker.LabelCap, worker.Hidden ? "Off".Translate() : "On".Translate() );
                        tipString += "\n";
                    }
                    tipString += "\n";
                }
                tipString += "MiniMap.Toggle".Translate();
                return tipString;
            }
        }

        #endregion Methods
    }
}