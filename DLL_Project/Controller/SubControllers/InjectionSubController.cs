using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static SequencedInjector[]          SequencedInjectors;
        /*
        private static SequencedInjector            detourInjector;
        private static SequencedInjector            specialInjector;
        private static SequencedInjector[]          initInjectors;
        private static SequencedInjector[]          updateInjectors;
        */

        static                              InjectionSubController()
        {
            // Add the injectors to the order-specific array
            // These injectors will be validated in order
            SequencedInjectors = new SequencedInjector[]
            {
                new SequencedInjector_Detours(),
                new SequencedInjector_SpecialInjectors(),
                new SequencedInjector_DataSet(),
                new SequencedInjector_MapComponents()
            };
        }

        public override string              Name => "Injection Controller";

        // Override sequence priorities
        public override int                 ValidationPriority      =>  90;
        public override int                 InitializationPriority  => 100;
        public override int                 UpdatePriority          => 100;

        public override bool                Validate()
        {
            var valid = true;

            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            foreach( var injector in SequencedInjectors )
            {
                try
                {
                    valid &= injector.IsValid();
                }
                catch( Exception e )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        string.Format( "An exception has occured while validating '{0}':\n{1}", injector.Name, e.ToString() ),
                        Name
                    );
                }
            }

            CCL_Log.CaptureEnd( stringBuilder, valid ? "Validated" : "Errors during validation" );
            strReturn = stringBuilder.ToString();

            State = valid ? SubControllerState.ValidationError : SubControllerState.Validated;
            return valid;
        }

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

            // Execute all the main load sequence injections
            if( !TrySequencedInjectors( InjectionSequence.MainLoad ) )
            {
                CCL_Log.CaptureEnd(
                    stringBuilder,
                    "Error"
                );
                strReturn = stringBuilder.ToString();
                State = SubControllerState.InitializationError;
            }

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

            // Execute all the game load sequence injections
            if( !TrySequencedInjectors( InjectionSequence.GameLoad ) )
            {
                CCL_Log.CaptureEnd(
                    stringBuilder,
                    "Error"
                );
                strReturn = stringBuilder.ToString();
                State = SubControllerState.RuntimeError;
            }

            // Post-load injections complete, stop calling this
            CCL_Log.CaptureEnd( stringBuilder, "Updated" );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Hybernating;
            return true;
        }

        internal static bool                TrySequencedInjectors( InjectionSequence sequence )
        {
            var injected = true;
            for( var timing = InjectionTiming.Priority_25; timing >= InjectionTiming.Priority_0; timing-- )
            {
                foreach( var injector in SequencedInjectors )
                {
                    //Log.Message( string.Format( "\n\n\tInjector: {0}\n\tSequence: {1}\n\tTiming: {2}", injector.Name, sequence.ToString(), timing.ToString() ) );
                    try
                    {
                        injected &= injector.Inject( sequence, timing );
                    }
                    catch( Exception e )
                    {
                        CCL_Log.Trace(
                            Verbosity.FatalErrors,
                            string.Format(
                                "Exception during sequenced injection ({0}, {1}, {2})\n\t{3}",
                                injector.Name,
                                sequence,
                                timing,
                                e.ToString() )
                        );
                        injected = false;
                    }
                }
            }

            // Do all def resolutions
            foreach( var injector in SequencedInjectors )
            {
                if( !injector.ReResolveDefs( sequence ) )
                {
                    injected = false;
                }
            }

            return injected;
        }

        internal static bool                TrySequencedInjectorsOnLoad( Assembly assembly )
        {   // Get all injectors for this assembly scheduled on DLL load
            var injected = true;
            for( var timing = InjectionTiming.Priority_25; timing >= InjectionTiming.Priority_0; timing-- )
            {
                injected &= Detours.TryTimedAssemblyDetours( assembly, InjectionSequence.DLLLoad, timing );
                injected &= SpecialInjector.TryTimedAssemblySpecialInjectors( assembly, InjectionSequence.DLLLoad, timing );
            }
            return injected;
        }

    }

}
