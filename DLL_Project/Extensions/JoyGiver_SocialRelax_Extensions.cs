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

    public static class JoyGiver_SocialRelax_Extensions
    {

        public const float _GatherRadius = 3.9f;

        private static FieldInfo _workingSpots;
        private static FieldInfo _NumRadiusCells;
        private static FieldInfo _RadialPatternMiddleOutward;
        private static FieldInfo _nurseableDrugs;

        static JoyGiver_SocialRelax_Extensions()
        {
            _workingSpots = typeof( JoyGiver_SocialRelax ).GetField( "workingSpots", Controller.Data.UniversalBindingFlags );
            if( _workingSpots == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'workingSpots' in 'JoyGiver_SocialRelax'",
                    "JoyGiver_SocialRelax_Extensions" );
            }
            _NumRadiusCells = typeof( JoyGiver_SocialRelax ).GetField( "NumRadiusCells", Controller.Data.UniversalBindingFlags );
            if( _NumRadiusCells == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'NumRadiusCells' in 'JoyGiver_SocialRelax'",
                    "JoyGiver_SocialRelax_Extensions" );
            }
            _RadialPatternMiddleOutward = typeof( JoyGiver_SocialRelax ).GetField( "RadialPatternMiddleOutward", Controller.Data.UniversalBindingFlags );
            if( _RadialPatternMiddleOutward == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'RadialPatternMiddleOutwards' in 'JoyGiver_SocialRelax'",
                    "JoyGiver_SocialRelax_Extensions" );
            }
            _nurseableDrugs = typeof( JoyGiver_SocialRelax ).GetField( "nurseableDrugs", Controller.Data.UniversalBindingFlags );
            if( _nurseableDrugs == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'nurseableDrugs' in 'JoyGiver_SocialRelax'",
                    "JoyGiver_SocialRelax_Extensions" );
            }
        }

        #region Reflected Methods

        public static List<CompGatherSpot> WorkingSpots()
        {
            return (List<CompGatherSpot>) _workingSpots.GetValue( null );
        }

        public static int NumRadiusCells()
        {
            return (int) _NumRadiusCells.GetValue( null );
        }

        public static List<IntVec3> RadialPatternMiddleOutward()
        {
            return (List<IntVec3>) _RadialPatternMiddleOutward.GetValue( null );
        }

        public static List<ThingDef> NurseableDrugs()
        {
            return (List<ThingDef>) _nurseableDrugs.GetValue( null );
        }

        #endregion

    }

}
