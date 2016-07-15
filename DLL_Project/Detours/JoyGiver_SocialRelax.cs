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

        #region Detoured Methods

        internal static Job _TryGiveJobInt( this JoyGiver_SocialRelax obj, Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator )
        {
            if( GatherSpotLister.activeSpots.NullOrEmpty() )
            {
                return (Job)null;
            }

            var workingSpots = JoyGiver_SocialRelax_Extensions.WorkingSpots();
            var NumRadiusCells = JoyGiver_SocialRelax_Extensions.NumRadiusCells();
            var RadialPatternMiddleOutward = JoyGiver_SocialRelax_Extensions.RadialPatternMiddleOutward();

            workingSpots.Clear();
            for( int index = 0; index < GatherSpotLister.activeSpots.Count; ++index )
            {
                workingSpots.Add( GatherSpotLister.activeSpots[ index ] );
            }

            CompGatherSpot compGatherSpot;
            while( GenCollection.TryRandomElement<CompGatherSpot>( workingSpots, out compGatherSpot ) )
            {
                workingSpots.Remove( compGatherSpot );
                if(
                    ( !compGatherSpot.parent.IsForbidden( pawn ) )&&
                    ( pawn.CanReach(
                        compGatherSpot.parent,
                        PathEndMode.Touch,
                        Danger.None,
                        false ) )&&
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
                        for( int index = 0; index < 30; ++index )
                        {
                            Building sittableThing = compGatherSpot.parent.RandomAdjacentCellCardinal().GetEdifice();
                            if(
                                ( sittableThing != null )&&
                                ( sittableThing.def.building.isSittable )&&
                                ( pawn.CanReserve(
                                    (TargetInfo)( (Thing)sittableThing ),
                                    1 ) )
                            )
                            {
                                job = new Job(
                                    JobDefOf.SocialRelax,
                                    (TargetInfo)( (Thing)compGatherSpot.parent ),
                                    (TargetInfo)( (Thing)sittableThing ) );
                            }
                        }
                    }
                    else
                    {
                        for( int index = 0; index < RadialPatternMiddleOutward.Count; ++index )
                        {
                            Building sittableThing = ( compGatherSpot.parent.Position + RadialPatternMiddleOutward[ index ] ).GetEdifice();
                            if(
                                ( sittableThing != null )&&
                                ( sittableThing.def.building.isSittable )&&
                                (
                                    ( pawn.CanReserve(
                                        (TargetInfo)( (Thing)sittableThing ),
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
                                    (TargetInfo)( (Thing)compGatherSpot.parent ),
                                    (TargetInfo)( (Thing)sittableThing ) );
                                break;
                            }
                        }
                        if( job == null )
                        {
                            for( int index = 0; index < 30; ++index )
                            {
                                IntVec3 occupySpot = compGatherSpot.parent.Position + GenRadial.RadialPattern[ Rand.Range( 1, NumRadiusCells ) ];
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
                                        (TargetInfo)( (Thing)compGatherSpot.parent ),
                                        (TargetInfo)occupySpot );
                                }
                            }
                        }
                    }
                    if( job == null )
                    {
                        return (Job)null;
                    }
                    if(
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
                            Thing thing = GenClosest.ClosestThing_Global_Reachable(
                                compGatherSpot.parent.Position,
                                list,
                                PathEndMode.OnCell,
                                TraverseParms.For(
                                    pawn,
                                    pawn.NormalMaxDanger() ),
                                40f,
                                ( t ) =>
                            {
                                if( t.IsForbidden( pawn ) )
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
                                return pawn.CanReserve( t, 1 );
                            } );
                            if( thing != null )
                            {
                                job.targetC = (TargetInfo)thing;
                                job.maxNumToCarry = Mathf.Min( thing.stackCount, thing.def.ingestible.maxNumToIngestAtOnce );
                            }
                        }
                    }
                    return job;
                }
            }
            return (Job)null;
        }

        #endregion

    }

}
