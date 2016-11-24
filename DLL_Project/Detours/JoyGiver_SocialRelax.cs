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

    internal class _JoyGiver_SocialRelax : JoyGiver_SocialRelax
    {

        internal static MethodInfo          _TryFindChairBesideTable;
        internal static MethodInfo          _TryFindChairNear;
        internal static MethodInfo          _TryFindSitSpotOnGroundNear;

        static                              _JoyGiver_SocialRelax()
        {
            _TryFindChairBesideTable = typeof( JoyGiver_SocialRelax ).GetMethod( "TryFindChairBesideTable", Controller.Data.UniversalBindingFlags );
            if( _TryFindChairBesideTable == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'TryFindChairBesideTable' in 'JoyGiver_SocialRelax'",
                    "Detour.JoyGiver_SocialRelax" );
            }
            _TryFindChairNear = typeof( JoyGiver_SocialRelax ).GetMethod( "TryFindChairNear", Controller.Data.UniversalBindingFlags );
            if( _TryFindChairNear == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'TryFindChairNear' in 'JoyGiver_SocialRelax'",
                    "Detour.JoyGiver_SocialRelax" );
            }
            _TryFindSitSpotOnGroundNear = typeof( JoyGiver_SocialRelax ).GetMethod( "TryFindSitSpotOnGroundNear", Controller.Data.UniversalBindingFlags );
            if( _TryFindSitSpotOnGroundNear == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'TryFindSitSpotOnGroundNear' in 'JoyGiver_SocialRelax'",
                    "Detour.JoyGiver_SocialRelax" );
            }
        }

        #region Reflected Methods

        internal static bool                TryFindChairBesideTable( Thing table, Pawn sitter, out Thing chair )
        {
            chair = null;
            var args = new object[] { table, sitter, chair };
            if( (bool)_TryFindChairBesideTable.Invoke( null, args ) )
            {
                chair = (Thing) args[2];
                return true;
            }
            return false;
        }

        internal static bool                TryFindChairNear( IntVec3 center, Pawn sitter, out Thing chair )
        {
            chair = null;
            var args = new object[] { center, sitter, chair };
            if( (bool)_TryFindChairNear.Invoke( null, args ) )
            {
                chair = (Thing) args[2];
                return true;
            }
            return false;
        }

        internal static bool                TryFindSitSpotOnGroundNear( IntVec3 center, Pawn sitter, out IntVec3 result )
        {
            result = IntVec3.Invalid;
            var args = new object[] { center, sitter, result };
            if( (bool)_TryFindChairNear.Invoke( null, args ) )
            {
                result = (IntVec3) args[2];
                return true;
            }
            return false;
        }

        #endregion

        #region Detoured Methods

        [DetourMember]
        internal Job                        _TryGiveJobInt( Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator )
        {
            if( GatherSpotLister.activeSpots.NullOrEmpty() )
            {
                return null;
            }

            var workingSpots = JoyGiver_SocialRelax_Extensions.WorkingSpots();
            var NumRadiusCells = JoyGiver_SocialRelax_Extensions.NumRadiusCells();
            var RadialPatternMiddleOutward = JoyGiver_SocialRelax_Extensions.RadialPatternMiddleOutward();

            workingSpots.Clear();
            for( int index = 0; index < GatherSpotLister.activeSpots.Count; index++ )
            {
                workingSpots.Add( GatherSpotLister.activeSpots[ index ] );
            }

            CompGatherSpot compGatherSpot;
            while( workingSpots.TryRandomElement( out compGatherSpot ) )
            {
                workingSpots.Remove( compGatherSpot );
                if(
                    ( !compGatherSpot.parent.IsForbidden( pawn ) )&&
                    ( pawn.CanReach(
                        compGatherSpot.parent,
                        PathEndMode.Touch,
                        Danger.None,
                        false ) ) &&
                    ( compGatherSpot.parent.IsSociallyProper( pawn ) )&&
                    (
                        ( gatherSpotValidator == null )||
                        ( gatherSpotValidator( compGatherSpot ) )
                    )
                )
                {
                    var job = (Job)null;
                    if( compGatherSpot.parent.def.surfaceType == SurfaceType.Eat )
                    {
                        Thing chair;
                        if( !TryFindChairBesideTable( compGatherSpot.parent, pawn, out chair ) )
                        {
                            return null;
                        }
                        job = new Job( JobDefOf.SocialRelax, compGatherSpot.parent, chair );
                    }
                    else
                    {
                        Thing chair;
                        if( TryFindChairNear( compGatherSpot.parent.Position, pawn, out chair ) )
                        {
                            job = new Job( JobDefOf.SocialRelax, compGatherSpot.parent, chair );
                        }
                        else
                        {
                            IntVec3 sitSpot;
                            if( !TryFindSitSpotOnGroundNear(  compGatherSpot.parent.Position, pawn, out sitSpot ) )
                            {
                                return null;
                            }
                            job = new Job( JobDefOf.SocialRelax, compGatherSpot.parent, sitSpot );
                        }
                    }
                    if( pawn.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )
                    {
                        Thing drugSource;
                        ThingDef drugDef;
                        if( DrugUtility.TryFindJoyDrug( compGatherSpot.parent.Position, pawn, 40f, true, JoyGiver_SocialRelax_Extensions.NurseableDrugs(), out drugSource, out drugDef ) )
                        {
                            job.targetC = drugSource;
                            var synthesizer = drugSource as Building_AutomatedFactory;
                            if( synthesizer != null )
                            {
                                if( !synthesizer.ReserveForUseBy( pawn, drugDef ) )
                                {   // Couldn't reserve the synthesizer for production
                                    return null;
                                }
                                job.maxNumToCarry = 1;
                            }
                            else
                            {
                                job.maxNumToCarry = Mathf.Min( drugSource.stackCount, drugSource.def.ingestible.maxNumToIngestAtOnce );
                            }

                        }
                    }
                    return job;
                }
            }
            return null;
        }

        #endregion

    }

}
