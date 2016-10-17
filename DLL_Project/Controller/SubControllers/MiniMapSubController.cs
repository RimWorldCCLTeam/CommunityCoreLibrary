using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace CommunityCoreLibrary.Controller
{

    internal class MiniMapSubController : SubController
    {

        // Controller name
        public override string              Name => "Minimap Controller";

        // Get sequence priorities
        public override int                 ValidationPriority      => 10;
        public override int                 InitializationPriority  => 40;

        // Entry with controller state Uninitialized
        // Exit with ValidationError (false) on error
        // Validated (true) if ready for initialization
        public override bool                Validate()
        {
            var errors = false;
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            var miniMapDefs = DefDatabase<MiniMap.MiniMapDef>.AllDefsListForReading;
            foreach( var miniMapDef in miniMapDefs )
            {
                if(
                    ( miniMapDef.miniMapClass == null )||
                    (
                        ( miniMapDef.miniMapClass != typeof( MiniMap.MiniMap ) )&&
                        ( !miniMapDef.miniMapClass.IsSubclassOf( typeof( MiniMap.MiniMap ) ) )
                    )
                )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "Unable to resolve miniMapClass for '{0}' to 'CommunityCoreLibrary.MiniMap'", miniMapDef.defName )
                    );
                    errors = true;
                }
                else
                {
                    // Make sure the minimap def has a list of overlay defs
                    if( miniMapDef.overlays == null )
                    {
                        miniMapDef.overlays = new List<MiniMap.MiniMapOverlayDef>();
                    }
                    // Fetch any overlays which may want to add-in
                    var overlayDefs =
                        DefDatabase<MiniMap.MiniMapOverlayDef>
                            .AllDefs
                            .Where( overlayDef => (
                                ( overlayDef.miniMapDef != null )&&
                                ( overlayDef.miniMapDef == miniMapDef )
                            ) );
                    if( overlayDefs.Count() > 0 )
                    {   // Add-in the overlay defs
                        foreach( var overlayDef in overlayDefs )
                        {
                            miniMapDef.overlays.AddUnique( overlayDef );
                        }
                    }
                    if( miniMapDef.overlays.NullOrEmpty() && !miniMapDef.dynamicOverlays )
                    {
                        CCL_Log.Trace(
                            Verbosity.Validation,
                            string.Format( "MiniMap '{0}' has no overlays", miniMapDef.defName )
                        );
                        errors = true;
                    }
                }
            }

            CCL_Log.CaptureEnd( stringBuilder, !errors ? "Validated" : "Errors during validation" );
            strReturn = stringBuilder.ToString();

            State = errors ? SubControllerState.ValidationError : SubControllerState.Validated;
            return !errors;
        }

        // Entry with controller state Validated or in a running state after a game load
        // Exit with InitializationError (false) on error
        // Ok (true) if ready for game play
        // Hybernating (true) if system is ok and no frame updates are required
        public override bool                Initialize()
        {
            // Default class method for sub-classes which don't require initialization
            var errors = false;
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            var miniMapDefs = DefDatabase<MiniMap.MiniMapDef>.AllDefsListForReading;
            foreach( var miniMapDef in miniMapDefs )
            {
                var miniMapWorker = (MiniMap.MiniMap) Activator.CreateInstance( miniMapDef.miniMapClass, new System.Object[] { miniMapDef } );
                if( miniMapWorker == null )
                {
                    CCL_Log.Trace(
                        Verbosity.NonFatalErrors,
                        string.Format( "Unable to create instance of '{0}' for '{1}'", miniMapDef.miniMapClass.Name, miniMapDef.defName )
                    );
                    errors = true;
                }
                else
                {
                    Controller.Data.MiniMaps.Add( miniMapWorker );
                }
            }

            CCL_Log.CaptureEnd( stringBuilder, !errors ? "Initialized" : "Errors during intialization" );
            strReturn = stringBuilder.ToString();

            State = errors ? SubControllerState.InitializationError : SubControllerState.Ok;
            return !errors;
        }

    }

}
