using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Verse;

/*
    TODO:  Alpha 13 API change

    Can't change yet otherwise existing saves will get null errors or name clashes

namespace CommunityCoreLibrary.Controller
{
    internal class HelpController : SubController
*/

namespace CommunityCoreLibrary.Controller
{

    /// <summary>
    /// This controller generates the implied help defs
    /// </summary>
    internal class HelpSubController : SubController
    {

        public override string              Name
        {
            get
            {
                return "Help Generator";
            }
        }

        // Override sequence priorities
        public override int                 InitializationPriority
        {
            get
            {
                return -100;
            }
        }
        public override bool                Initialize()
        {
            if( !HelpBuilder.ResolveImpliedDefs() )
            {
                strReturn = "Unexpected error in HelpBuilder.ResolveImpliedDefs()";
                State = SubControllerState.InitializationError;
                return false;
            }
            strReturn = "Implied HelpDefs created";
            State = SubControllerState.Hybernating;
            return true;
        }

    }

}
