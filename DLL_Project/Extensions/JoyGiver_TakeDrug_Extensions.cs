using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    public static class JoyGiver_TakeDrug_Extensions
    {

        private static FieldInfo _takeableDrugs;

        static JoyGiver_TakeDrug_Extensions()
        {
            _takeableDrugs = typeof( JoyGiver_TakeDrug ).GetField( "takeableDrugs", Controller.Data.UniversalBindingFlags );
            if( _takeableDrugs == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'takeableDrugs' in 'JoyGiver_TakeDrug'",
                    "JoyGiver_TakeDrug_Extensions" );
            }
        }

        #region Reflected Methods

        public static List<ThingDef> TakeableDrugs()
        {
            return (List<ThingDef>) _takeableDrugs.GetValue( null );
        }

        #endregion

    }

}
