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

        #region Reflected Methods

        public static List<CompGatherSpot> WorkingSpots()
        {
            if( _workingSpots == null )
            {
                _workingSpots = typeof( JoyGiver_SocialRelax ).GetField( "workingSpots", BindingFlags.Static | BindingFlags.NonPublic );
                if( _workingSpots == null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get field 'workingSpots' in 'JoyGiver_SocialRelax'",
                        "Internal Detours" );
                }
            }
            return (List<CompGatherSpot>) _workingSpots.GetValue( null );
        }

        public static int NumRadiusCells()
        {
            if( _NumRadiusCells == null )
            {
                _NumRadiusCells = typeof( JoyGiver_SocialRelax ).GetField( "NumRadiusCells", BindingFlags.Static | BindingFlags.NonPublic );
                if( _NumRadiusCells == null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get field 'NumRadiusCells' in 'JoyGiver_SocialRelax'",
                        "Internal Detours" );
                }
            }
            return (int) _NumRadiusCells.GetValue( null );
        }

        public static List<IntVec3> RadialPatternMiddleOutward()
        {
            if( _RadialPatternMiddleOutward == null )
            {
                _RadialPatternMiddleOutward = typeof( JoyGiver_SocialRelax ).GetField( "RadialPatternMiddleOutward", BindingFlags.Static | BindingFlags.NonPublic );
                if( _RadialPatternMiddleOutward == null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Unable to get field 'RadialPatternMiddleOutwards' in 'JoyGiver_SocialRelax'",
                        "Internal Detours" );
                }
            }
            return (List<IntVec3>) _RadialPatternMiddleOutward.GetValue( null );
        }

        #endregion

    }

}
