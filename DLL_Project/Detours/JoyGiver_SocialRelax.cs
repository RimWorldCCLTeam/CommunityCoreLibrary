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

        /* TODO:  Investigate and expand drug system to use factories

        #region Helper Methods

        internal static bool                        TryFindChairBesideTable( Thing table, Pawn sitter, out Thing chair )
        {
            for( int index = 0; index < 30; ++index )
            {
                var edifice = table.RandomAdjacentCellCardinal().GetEdifice();
                if(
                    ( edifice != null ) &&
                    ( edifice.def.building.isSittable ) &&
                    ( sitter.CanReserve( edifice, 1 ) )
                )
                {
                    chair = edifice;
                    return true;
                }
            }
            chair = null;
            return false;
        }

        internal static bool                        TryFindChairNear( IntVec3 center, Pawn sitter, out Thing chair )
        {
            var RadialPatternMiddleOutward = JoyGiver_SocialRelax_Extensions.RadialPatternMiddleOutward();
            for( int index = 0; index < RadialPatternMiddleOutward.Count; ++index )
            {
                var edifice = ( center + RadialPatternMiddleOutward[ index ] ).GetEdifice();
                if(
                    ( edifice != null )&&
                    ( edifice.def.building.isSittable )&&
                    (
                        ( sitter.CanReserve( edifice, 1 ) )&&
                        ( !edifice.IsForbidden( sitter ) )
                    )&&
                    ( GenSight.LineOfSight( center, edifice.Position, true ) )
                )
                {
                    chair = edifice;
                    return true;
                }
            }
            chair = null;
            return false;
        }

        internal static bool                        TryFindSitSpotOnGroundNear( IntVec3 center, Pawn sitter, out IntVec3 result )
        {
            var NumRadiusCells = JoyGiver_SocialRelax_Extensions.NumRadiusCells();
            for( int index = 0; index < 30; ++index )
            {
                IntVec3 intVec3 = center + GenRadial.RadialPattern[ Rand.Range( 1, NumRadiusCells ) ];
                if(
                    ( sitter.CanReserveAndReach( intVec3, PathEndMode.OnCell, Danger.None, 1 ) )&&
                    ( intVec3.GetEdifice() == null )&&
                    ( GenSight.LineOfSight( center, intVec3, true ) )
                )
                {
                    result = intVec3;
                    return true;
                }
            }
            result = IntVec3.Invalid;
            return false;
        }

        internal static bool                        TryFindIngestibleToNurse( IntVec3 center, Pawn ingester, out Thing ingestible )
        {
            if(
                ( ingester.story != null )&&
                ( ingester.story.traits.DegreeOfTrait( TraitDefOf.DrugDesire ) < 0 )
            )
            {
                ingestible = (Thing) null;
                return false;
            }
            if( ingester.drugs == null )
            {
                ingestible = (Thing) null;
                return false;
            }
            var nurseableDrugs = JoyGiver_SocialRelax_Extensions.NurseableDrugs();
            nurseableDrugs.Clear();
            var currentPolicy = ingester.drugs.CurrentPolicy;
            for( int index = 0; index < currentPolicy.Count; ++index )
            {
                if(
                    ( currentPolicy[ index ].allowedForJoy )&&
                    ( currentPolicy[ index ].drug.ingestible.nurseable )
                )
                {
                    nurseableDrugs.Add( currentPolicy[ index ].drug );
                }
            }
            nurseableDrugs.Shuffle();
            for( int index = 0; index < nurseableDrugs.Count; ++index )
            {
                var listOfDrugs = Find.ListerThings.ThingsOfDef( nurseableDrugs[ index ] );
                // TODO:  Add checks for synthesizers that can produce drugs
                if( listOfDrugs.Count > 0 )
                {
                    ingestible = GenClosest.ClosestThing_Global_Reachable(
                        center,
                        listOfDrugs,
                        PathEndMode.OnCell,
                        TraverseParms.For(
                            ingester,
                            Danger.Deadly,
                            TraverseMode.ByPawn,
                            false ),
                        40f,
                        (drug) =>
                    {
                        if( ingester.CanReserve( drug, 1 ) )
                        {
                            return !drug.IsForbidden( ingester );
                        }
                        return false;
                    },
                        null );
                    if( ingestible != null )
                    {
                        return true;
                    }
                }
            }
            ingestible = null;
            return false;
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( JoyGiver_SocialRelax ), "TryGiveJobInt" )]
        internal Job                                TryGiveJobInt( Pawn pawn, Predicate<CompGatherSpot> gatherSpotValidator )
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
                    ( !compGatherSpot.parent.IsForbidden( pawn ) ) &&
                    ( pawn.CanReach(
                        compGatherSpot.parent,
                        PathEndMode.Touch,
                        Danger.None,
                        false ) ) &&
                    ( compGatherSpot.parent.IsSociallyProper( pawn ) ) &&
                    (
                        ( gatherSpotValidator == null ) ||
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
                    if(
                        ( pawn.health.capacities.CapableOf( PawnCapacityDefOf.Manipulation ) )&&
                        (
                            ( pawn.story == null ) ||
                            ( pawn.story.traits.DegreeOfTrait( TraitDefOf.DrugDesire ) >= 0 )
                        )
                    )
                    {
                        List<Thing> list = Find.ListerThings.AllThings.Where( t => (
                            ( t.def.IsDrug ) ||
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
                                        ( !FS.InteractionCell.Standable() ) ||
                                        ( !FS.CompPowerTrader.PowerOn ) ||
                                        ( FS.BestProduct( FoodSynthesis.IsDrug, FoodSynthesis.SortDrug ) == null )
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

        */

    }

}
