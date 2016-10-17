using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller generates the implied help defs
    /// </summary>
    internal class HelpSubController : SubController
    {

        public override string              Name => "Help Generator";

        // Override sequence priorities
        public override int                 InitializationPriority => 80;

        public override bool                Initialize()
        {
            var stringBuilder = new StringBuilder();
            string finalMessage = string.Empty;
            CCL_Log.CaptureBegin( stringBuilder );

            // Don't auto-gen help if "quicktest" or "nohelp" command line switches are used
            var buildHelp = true;
            if(
                ( GenCommandLine.CommandLineArgPassed( "quicktest" ) )||
                ( GenCommandLine.CommandLineArgPassed( "nohelp" ) )
            )
            {
                buildHelp = false;
                finalMessage = "Skipping auto-gen";
            }

            if( buildHelp )
            {
                LongEventHandler.SetCurrentEventText( "LibraryHelpGen".Translate() );

                var startTime = DateTime.Now;

                if( !HelpBuilder.ResolveImpliedDefs() )
                {
                    strReturn = "Unexpected error in HelpBuilder.ResolveImpliedDefs()";
                    State = SubControllerState.InitializationError;
                    return false;
                }

                var finishTime = DateTime.Now;
                var finalTime = finishTime - startTime;
                finalMessage = string.Format( "Completed in {0}", finalTime.ToString() );

                LongEventHandler.SetCurrentEventText( "Initializing".Translate() );
            }

            CCL_Log.CaptureEnd( stringBuilder, finalMessage );
            CCL_Log.Message( stringBuilder.ToString(), "Help System" );
            strReturn = "Initialized";
            State = SubControllerState.Hybernating;
            return true;
        }

    }

}
