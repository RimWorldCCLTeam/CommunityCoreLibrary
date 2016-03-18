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
    internal class ResourceController : SubController
*/

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller handles optional resources (generic hoppers, etc)
    /// </summary>
    internal class ResourceSubController : SubController
    {

        public override string              Name
        {
            get
            {
                return "Resource Manager";
            }
        }

        // Override sequence priorities
        public override int                 InitializationPriority
        {
            get
            {
                return 50;
            }
        }

        public override bool                Initialize()
        {
            var stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            var ModHelperDefs = Controller.Data.ModHelperDefs;
            foreach( var ModHelperDef in ModHelperDefs )
            {
                // Generic hoppers
                if(
                    ( !Controller.Data.GenericHoppersEnabled )&&
                    ( ModHelperDef.UsesGenericHoppers )
                )
                {
                    if( !Resources.Buildings.Hoppers.EnableGenericHoppers() )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot enable generic hoppers" );
                        CCL_Log.CaptureEnd( stringBuilder );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.InitializationError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Generic Hoppers Enabled"
                    );
#endif
                }

                // Vanilla hoppers
                if(
                    ( !Controller.Data.VanillaHoppersDisabled )&&
                    ( ModHelperDef.HideVanillaHoppers )
                )
                {
                    if( !Resources.Buildings.Hoppers.DisableVanillaHoppers() )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot disable vanilla hoppers" );
                        CCL_Log.CaptureEnd( stringBuilder );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.InitializationError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Vanilla Hoppers Disabled"
                    );
#endif
                }
            }

            CCL_Log.CaptureEnd( stringBuilder, "Initialized" );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Hybernating;
            return true;
        }

    }

}
