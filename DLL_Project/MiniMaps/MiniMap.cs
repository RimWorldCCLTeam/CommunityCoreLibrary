using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.MiniMap
{
    
    public class MiniMap
    {
        
        #region Instance Data

        private bool                    _hidden = false;

        public MiniMapDef               def;

        public List<MiniMapOverlay>     overlayWorkers;

        private Texture2D               _iconTexture;

        #endregion Instance Data

        #region Constructors

        public                          MiniMap( MiniMapDef miniMapDef )
        {
            this.def = miniMapDef;
            this._hidden = this.def.hiddenByDefault;

            if( !this.def.iconTex.NullOrEmpty() )
            {
                _iconTexture = ContentFinder<Texture2D>.Get( this.def.iconTex, true );
            }
            overlayWorkers = new List<MiniMapOverlay>();

            for( int index = 0; index < this.def.overlays.Count; ++index )
            {
                var overlayData = this.def.overlays[ index ];
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
                this.def,
                Verbosity.Injections,
                string.Format( "Added overlay '{0}' at draw position {1}", this.GetType().FullName, this.def.drawOrder )
            );
        }

        #endregion Constructors

        #region Properties

        public List<MiniMapOverlay>     VisibleOverlays
        {
            get
            {
                return overlayWorkers.Where( worker => !worker.Hidden ).OrderByDescending( worker => worker.def.drawOffset ).ToList();
            }
        }
        
        public virtual bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;

                // give it an update since it's not updated while hidden
                Update();

                // mark the controller dirty so overlays get re-ordered.
                MiniMapController.dirty = true;
            }
        }

        public virtual Texture2D Icon
        {
            get
            {
                if( _iconTexture.NullOrBad() )
                {
                    return TexUI.UnknownThing;
                }
                return _iconTexture;
            }
        }

        public virtual string label
        {
            get
            {
                if( def.labelKey.NullOrEmpty() )
                {
                    return def.label;
                }
                return def.labelKey.Translate();
            }
        }

        public virtual string LabelCap
        {
            get
            {
                return label.CapitalizeFirst();
            }
        }

        #endregion Properties

        #region Methods

        public void ClearTextures( bool apply = false )
        {
            foreach ( var overlay in overlayWorkers )
            {
                overlay.ClearTexture( apply );
                overlay.texture.Apply();
            }
        }

        public virtual void Update()
        {
            var workers = VisibleOverlays;
            if ( workers.Any() )
            {
                foreach ( var worker in workers )
                {
                    worker.Update();
                    worker.texture.Apply();
                }
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
            List<FloatMenuOption> options = overlayWorkers.SelectMany( worker => worker.GetFloatMenuOptions() ).ToList();
            
            if( def.mcmWorker != null )
            {
                options.Add( new FloatMenuOption(
                    "MiniMap.ShowMCMOption".Translate( label ),
                    () =>
                {
                    // TODO:  Open MCM Window to this worker
                    return;
                } ) );
            }
            return options;
        }

        public virtual string   ToolTip
        {
            get
            {
                // Get tool tip (w/ description)
                // Use core translations for "Off" and "On"
                var tipString = string.Empty;
                if( !def.description.NullOrEmpty() )
                {
                    tipString = def.description + "\n\n";
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