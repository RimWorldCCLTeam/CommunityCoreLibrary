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
            stringBuilder.AppendLine( "Initialization" );
            CCL_Log.CaptureBegin( stringBuilder );

            // Inject the special injectors into the system
            var ModHelperDefs = Controller.Data.ModHelperDefs;
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.SpecialsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectSpecials() )

                    Controller.Data.Trace_Current_Mod = ModHelperDef;
                    ModHelperDef.InjectSpecials();
                    Controller.Data.Trace_Current_Mod = null;
                    if( !ModHelperDef.SpecialsInjected )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot inject Special Injectors" );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.InitializationError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Special Injectors injected"
                    );
#endif
                }
            }

            // Inject the thing comps into defs
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.ThingCompsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectThingComps() )

                    ModHelperDef.InjectThingComps();
                    if( !ModHelperDef.ThingCompsInjected )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot inject ThingComps" );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.InitializationError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "ThingComps injected"
                    );
#endif
                }
            }

            // Everything's ok for post load injections
            CCL_Log.CaptureEnd( stringBuilder );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Ok;
            return true;
        }

        public override bool                Update()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( "Update" );
            CCL_Log.CaptureBegin( stringBuilder );

            // Inject the post load injectors into the game
            var ModHelperDefs = Controller.Data.ModHelperDefs;
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.PostLoadersInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectPostLoaders() )

                    ModHelperDef.InjectPostLoaders();
                    if( !ModHelperDef.PostLoadersInjected )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot inject Post Load Injectors" );
                        CCL_Log.CaptureEnd( stringBuilder );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.RuntimeError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Post Loaders injected"
                    );
#endif
                }
            }

            // Inject the map components into the game
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.MapComponentsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectMapComponents() )

                    ModHelperDef.InjectMapComponents();
                    if( !ModHelperDef.MapComponentsInjected )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot inject MapComponents" );
                        CCL_Log.CaptureEnd( stringBuilder );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.RuntimeError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "MapComponents injected"
                    );
#endif
                }
            }

            // Inject the designators into their categories
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.DesignatorsInjected )
                {
                    // TODO:  Alpha 13 API change
                    //if( ModHelperDef.InjectDesignators() )

                    ModHelperDef.InjectDesignators();
                    if( !ModHelperDef.DesignatorsInjected )
                    {
                        CCL_Log.TraceMod(
                            ModHelperDef,
                            Verbosity.NonFatalErrors,
                            "Cannot inject Designators" );
                        CCL_Log.CaptureEnd( stringBuilder );
                        strReturn = stringBuilder.ToString();
                        State = SubControllerState.RuntimeError;
                        return false;
                    }
#if DEBUG
                    CCL_Log.TraceMod(
                        ModHelperDef,
                        Verbosity.Injections,
                        "Designators injected"
                    );
#endif
                }
            }

            // Post-load injections complete, stop calling this
            CCL_Log.CaptureEnd( stringBuilder );
            strReturn = stringBuilder.ToString();
            State = SubControllerState.Hybernating;
            return true;
        }

    }

}
