using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

/*
    TODO:  Alpha 13 API change

    Can't change yet otherwise existing saves will get null errors or name clashes

namespace CommunityCoreLibrary.Controller
{
    internal class InjectionController : SubController
*/

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller handles all injections (Special Injectors, map components, designators, etc)
    /// </summary>
    internal class InjectionSubController : SubController
    {

        private static IInjector[]          initInjectors;
        private static IInjector[]          updateInjectors;

        static                              InjectionSubController()
        {
            initInjectors = new IInjector[]
            {
                ModHelperDef.GetInjector( typeof( MHD_SpecialInjectors ) ),
                ModHelperDef.GetInjector( typeof( MHD_ThingComps ) ),
                ModHelperDef.GetInjector( typeof( MHD_Facilities ) )
            };
            updateInjectors = new IInjector[]
            {
                ModHelperDef.GetInjector( typeof( MHD_PostLoadInjectors ) ),
                ModHelperDef.GetInjector( typeof( MHD_MapComponents ) ),
                ModHelperDef.GetInjector( typeof( MHD_Designators ) )
            };
        }

        public override string              Name
        {
            get
            {
                return "Injection Controller";
            }
        }

        // Override sequence priorities
        public override int                 InitializationPriority
        {
            get
            {
                return 100;
            }
        }
        public override int                 UpdatePriority
        {
            get
            {
                return 100;
            }
        }

        public override bool                Initialize()
        {
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            foreach( var injector in initInjectors )
            {
                // Inject the group into the system
                if( !ModHelperDef.InjectGroup( injector ) )
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
            return true;
        }

        public override bool                Update()
        {
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            foreach( var injector in updateInjectors )
            {
                // Inject the group into the system
                if( !ModHelperDef.InjectGroup( injector ) )
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

    }

}
