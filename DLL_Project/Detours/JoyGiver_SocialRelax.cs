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

    internal static class _JoyGiver_SocialRelax
    {

        private const float _GatherRadius = 3.9f;
        private static List<CompGatherSpot> _workingSpots;
        private static readonly int _NumRadiusCells;
        private static readonly List<IntVec3> _RadialPatternMiddleOutward;

        static _JoyGiver_SocialRelax()
        {
            _workingSpots = typeof( JoyGiver_SocialRelax ).GetField( "workingSpots", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null ) as List<CompGatherSpot>;
            _NumRadiusCells = GenRadial.NumCellsInRadius( _GatherRadius );
            _RadialPatternMiddleOutward = typeof( JoyGiver_SocialRelax ).GetField( "RadialPatternMiddleOutward", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null ) as List<IntVec3>;
        }

        // TODO: see other todos
        /*internal static Job _TryGiveJob( this JoyGiver_SocialRelax obj, Pawn pawn)
        {
            var JoyGiver_SocialRelax_TryUseThing = new _JoyGiver_SocialRelax._TryUseThing();
            JoyGiver_SocialRelax_TryUseThing.pawn = pawn;

            if( GatherSpotLister.activeSpots.Count == 0 )
            {
                return (Job) null;
            }

            _workingSpots.Clear();
            for( int index = 0; index < GatherSpotLister.activeSpots.Count; ++index )
            {
                _workingSpots.Add( GatherSpotLister.activeSpots[ index ] );
            }
            CompGatherSpot result;
            while( GenCollection.TryRandomElement<CompGatherSpot>( _workingSpots, out result ) )
            {
                _workingSpots.Remove( result );
                if(
                    ( !ForbidUtility.IsForbidden(
                        (Thing) result.parent,
                        JoyGiver_SocialRelax_TryUseThing.pawn ) )&&
                    ( Reachability.CanReach(
                        JoyGiver_SocialRelax_TryUseThing.pawn,
                        (TargetInfo) ((Thing) result.parent ),
                        PathEndMode.Touch,
                        Danger.None,
                        false ) )&&
                    ( SocialProperness.IsSociallyProper(
                        (Thing) result.parent,
                        JoyGiver_SocialRelax_TryUseThing.pawn ) )
                )
                {
                    Job job = (Job) null;
                    if( result.parent.def.surfaceType == SurfaceType.Eat )
                    {
                        for( int index = 0; index < 30; ++index )
                        {
                            Building edifice = GridsUtility.GetEdifice( GenAdj.RandomAdjacentCellCardinal( (Thing) result.parent ) );
                            if(
                                ( edifice != null )&&
                                ( edifice.def.building.isSittable )&&
                                ( ReservationUtility.CanReserve(
                                    JoyGiver_SocialRelax_TryUseThing.pawn,
                                    (TargetInfo) ((Thing) edifice ),
                                    1) )
                            )
                            {
                                job = new Job(
                                    JobDefOf.SocialRelax,
                                    (TargetInfo) ((Thing) result.parent),
                                    (TargetInfo) ((Thing) edifice) );
                            }
                        }
                    }
                    else
                    {
                        for( int index = 0; index < _RadialPatternMiddleOutward.Count; ++index)
                        {
                            Building edifice = GridsUtility.GetEdifice( result.parent.Position + _RadialPatternMiddleOutward[ index ] );
                            if(
                                ( edifice != null )&&
                                ( edifice.def.building.isSittable )&&
                                (
                                    ( ReservationUtility.CanReserve(
                                        JoyGiver_SocialRelax_TryUseThing.pawn,
                                        (TargetInfo) ((Thing) edifice ),
                                        1 ) )&&
                                    ( !ForbidUtility.IsForbidden(
                                        (Thing) edifice,
                                        JoyGiver_SocialRelax_TryUseThing.pawn ) )&&
                                    ( GenSight.LineOfSight(
                                        result.parent.Position,
                                        edifice.Position,
                                        true ) )
                                )
                            )
                            {
                                job = new Job(
                                    JobDefOf.SocialRelax,
                                    (TargetInfo) ((Thing) result.parent),
                                    (TargetInfo) ((Thing) edifice ) );
                                break;
                            }
                        }
                        if( job == null )
                        {
                            for( int index = 0; index < 30; ++index )
                            {
                                IntVec3 intVec3 = result.parent.Position + GenRadial.RadialPattern[ Rand.Range( 1, _NumRadiusCells ) ];
                                if(
                                    ( ReservationUtility.CanReserveAndReach(
                                        JoyGiver_SocialRelax_TryUseThing.pawn,
                                        (TargetInfo) intVec3,
                                        PathEndMode.OnCell,
                                        Danger.None,
                                        1 ) )&&
                                    ( GridsUtility.GetEdifice( intVec3 ) == null )&&
                                    ( GenSight.LineOfSight(
                                        result.parent.Position,
                                        intVec3,
                                        true ) )
                                )
                                {
                                    job = new Job(
                                        JobDefOf.SocialRelax,
                                        (TargetInfo) ((Thing) result.parent ),
                                        (TargetInfo) intVec3 );
                                }
                            }
                        }
                    }
                    if( job == null )
                    {
                        return (Job) null;
                    }
                    List<Thing> list = Find.ListerThings.AllThings.Where( t => (
                        ( t.def.IsAlcohol() )||
                        ( t is Building_AutomatedFactory )
                    ) ).ToList();
                    if( list.Count > 0 )
                    {
                        Predicate<Thing> validator = new Predicate<Thing>( JoyGiver_SocialRelax_TryUseThing.CanUseThing );
                        Thing thing = GenClosest.ClosestThing_Global_Reachable(
                            result.parent.Position,
                            list,
                            PathEndMode.OnCell,
                            TraverseParms.For(
                                JoyGiver_SocialRelax_TryUseThing.pawn,
                                JoyGiver_SocialRelax_TryUseThing.pawn.NormalMaxDanger() ),
                            40f,
                            validator );
                        if( thing != null )
                        {
                            job.targetC = (TargetInfo) thing;
                            job.maxNumToCarry = Mathf.Min( thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce );
                        }
                    }
                    return job;
                }
            }
            return (Job) null;
        }*/

        // TODO:see other todos
        /*internal sealed class _TryUseThing
        {
            internal Pawn pawn;

            public _TryUseThing()
            {
            }

            internal bool CanUseThing( Thing t )
            {
                if( ForbidUtility.IsForbidden( t, this.pawn ) )
                {
                    return false;
                }

                if( t is Building_AutomatedFactory )
                {
                    var FS = t as Building_AutomatedFactory;
                    if(
                        ( !GenGrid.Standable( FS.InteractionCell ) )||
                        ( !FS.CompPowerTrader.PowerOn )||
                        ( FS.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol ) == null )
                    )
                    {
                        return false;
                    }
                }
                return ReservationUtility.CanReserve( this.pawn, (TargetInfo) t, 1 );
            }
        }*/

    }

}
