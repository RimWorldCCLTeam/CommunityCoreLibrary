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

        internal static Job _TryGiveJobInt( this JoyGiver_SocialRelax obj, Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator )
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
            CompGatherSpot compGatherSpot;
            while( GenCollection.TryRandomElement<CompGatherSpot>( _workingSpots, out compGatherSpot ) )
            {
                _workingSpots.Remove( compGatherSpot );
                if(
                    ( !( (Thing)compGatherSpot.parent ).IsForbidden( pawn ) )&&
                    ( pawn.CanReach(
                        compGatherSpot.parent,
                        PathEndMode.Touch,
                        Danger.None,
                        false ) )&&
                    ( compGatherSpot.parent.IsSociallyProper( pawn ) )&&
                    ( gatherSpotValidator == null )||
                    ( gatherSpotValidator( compGatherSpot ) )
                )
                {
                    Job job = (Job) null;
                    if( compGatherSpot.parent.def.surfaceType == SurfaceType.Eat )
                    {
                        for( int index = 0; index < 30; ++index )
                        {
                            Building sittableThing = compGatherSpot.parent.RandomAdjacentCellCardinal().GetEdifice();
                            if(
                                ( sittableThing != null )&&
                                ( sittableThing.def.building.isSittable )&&
                                ( pawn.CanReserve(
                                    (TargetInfo) ((Thing) sittableThing ),
                                    1 ) )
                            )
                            {
                                job = new Job(
                                    JobDefOf.SocialRelax,
                                    (TargetInfo) ((Thing) compGatherSpot.parent),
                                    (TargetInfo) ((Thing) sittableThing) );
                            }
                        }
                    }
                    else
                    {
                        for( int index = 0; index < _RadialPatternMiddleOutward.Count; ++index)
                        {
                            Building sittableThing = ( compGatherSpot.parent.Position + _RadialPatternMiddleOutward[ index ] ).GetEdifice();
                            if(
                                ( sittableThing != null )&&
                                ( sittableThing.def.building.isSittable )&&
                                (
                                    ( pawn.CanReserve(
                                        (TargetInfo) ((Thing) sittableThing ),
                                        1 ) )&&
                                    ( !sittableThing.IsForbidden( pawn ) )&&
                                    ( GenSight.LineOfSight(
                                        compGatherSpot.parent.Position,
                                        sittableThing.Position,
                                        true ) )
                                )
                            )
                            {
                                job = new Job(
                                    JobDefOf.SocialRelax,
                                    (TargetInfo) ((Thing) compGatherSpot.parent),
                                    (TargetInfo) ((Thing) sittableThing ) );
                                break;
                            }
                        }
                        if( job == null )
                        {
                            for( int index = 0; index < 30; ++index )
                            {
                                IntVec3 occupySpot = compGatherSpot.parent.Position + GenRadial.RadialPattern[ Rand.Range( 1, _NumRadiusCells ) ];
                                if(
                                    ( pawn.CanReserveAndReach(
                                        occupySpot,
                                        PathEndMode.OnCell,
                                        Danger.None,
                                        1 ) )&&
                                    ( occupySpot.GetEdifice() == null )&&
                                    ( GenSight.LineOfSight(
                                        compGatherSpot.parent.Position,
                                        occupySpot,
                                        true ) )
                                )
                                {
                                    job = new Job(
                                        JobDefOf.SocialRelax,
                                        (TargetInfo) ((Thing) compGatherSpot.parent ),
                                        (TargetInfo) occupySpot );
                                }
                            }
                        }
                    }
                    if( job == null )
                    {
                        return (Job) null;
                    }
                    if(
                        ( pawn.RaceProps.ToolUser )&&
                        ( pawn.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                        (
                            ( pawn.story == null )||
                            ( pawn.story.traits.DegreeOfTrait( TraitDefOf.DrugDesire ) >= 0 )
                        )
                    )
                    {
                        List<Thing> list = Find.ListerThings.AllThings.Where( t => (
                            ( t.def.IsAlcohol() )||
                            ( t is Building_AutomatedFactory )
                        ) ).ToList();
                        if( list.Count > 0 )
                        {
                            Predicate<Thing> validator = new Predicate<Thing>( JoyGiver_SocialRelax_TryUseThing.CanUseThing );
                            Thing thing = GenClosest.ClosestThing_Global_Reachable(
                                compGatherSpot.parent.Position,
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
                    }
                    return job;
                }
            }
            return (Job) null;
        }

        internal sealed class _TryUseThing
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
                        ( !FS.InteractionCell.Standable() )||
                        ( !FS.CompPowerTrader.PowerOn )||
                        ( FS.BestProduct( FoodSynthesis.IsAlcohol, FoodSynthesis.SortAlcohol ) == null )
                    )
                    {
                        return false;
                    }
                }
                return this.pawn.CanReserve( t, 1 );
            }
        }

    }

}
