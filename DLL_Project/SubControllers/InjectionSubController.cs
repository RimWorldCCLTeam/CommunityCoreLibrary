using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller handles all injections (Special Injectors, map components, designators, etc)
    /// </summary>
    internal class InjectionSubController : SubController
    {

        // Use arrays instead of lists to ensure order
        private static IInjector[]          initInjectors;
        private static IInjector[]          updateInjectors;

        static                              InjectionSubController()
        {
            initInjectors = new IInjector[]
            {
                ModHelperDef.GetInjector( typeof( MHD_SpecialInjectors ) ),
                ModHelperDef.GetInjector( typeof( MHD_ThingComps ) ),
                ModHelperDef.GetInjector( typeof( MHD_TickerSwitcher ) ),
                ModHelperDef.GetInjector( typeof( MHD_Facilities ) ),
                ModHelperDef.GetInjector( typeof( MHD_TraderKinds ) ),
                ModHelperDef.GetInjector( typeof( MHD_ThingDefAvailability ) )
            };
            updateInjectors = new IInjector[]
            {
                ModHelperDef.GetInjector( typeof( MHD_PostLoadInjectors ) ),
                ModHelperDef.GetInjector( typeof( MHD_MapComponents ) ),
                ModHelperDef.GetInjector( typeof( MHD_Designators ) )
            };
        }

        public override string              Name => "Injection Controller";

        // Override sequence priorities
        public override int                 InitializationPriority  => 100;
        public override int                 UpdatePriority          => 100;

        public override bool                Initialize()
        {
            LongEventHandler.SetCurrentEventText( "LibraryInjection".Translate() );

            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            // Initialize preload-MCMs
            if( !MCMHost.InitializeHosts( true ) )
            {
                CCL_Log.CaptureEnd( stringBuilder, "Errors initializing Mod Configuration Menus" );
                strReturn = stringBuilder.ToString();
                State = SubControllerState.InitializationError;
                return false;
            }

            foreach( var injector in initInjectors )
            {
                // Inject the group into the system
                if( !Inject( injector ) )
                {
                    CCL_Log.CaptureEnd( stringBuilder, "Errors during injection" );
                    strReturn = stringBuilder.ToString();
                    State = SubControllerState.InitializationError;
                    return false;
                }
#if DEBUG
                CCL_Log.Trace(
                    Verbosity.Injections,
                    injector.InjectString
                );
#endif
            }

            MHD_Facilities.ReResolveDefs();

            // Everything's ok for updates
            CCL_Log.CaptureEnd(
                stringBuilder,
                "Initialized"
            );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Ok;

            LongEventHandler.SetCurrentEventText( "Initializing".Translate() );

            return true;
        }

        public override bool                Update()
        {
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            foreach( var injector in updateInjectors )
            {
                // Inject the group into the system
                if( !Inject( injector ) )
                {
                    CCL_Log.CaptureEnd( stringBuilder, "Errors during injection" );
                    strReturn = stringBuilder.ToString();
                    State = SubControllerState.InitializationError;
                    return false;
                }
#if DEBUG
                CCL_Log.Trace(
                    Verbosity.Injections,
                    injector.InjectString
                );
#endif
            }

            // Post-load injections complete, stop calling this
            CCL_Log.CaptureEnd( stringBuilder, "Updated" );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Hybernating;
            return true;
        }

        public bool                         Inject( IInjector injector )
        {
            bool result = true;

            var modHelperDefs = Controller.Data.ModHelperDefs;

            foreach( var modHelperDef in modHelperDefs )
            {
                if( !injector.Injected( modHelperDef ) )
                {
                    result &= modHelperDef.Inject( injector );
                }
            }

            return result;
        }

    }

}
