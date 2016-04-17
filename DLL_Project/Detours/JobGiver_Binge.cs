using System;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _JobGiver_Binge
    {
        
        internal static Job _DrinkAlcoholJob( Pawn pawn )
        {
            var JobGiver_Binge_DrinkAlchohol = new _JobGiver_Binge._DrinkAlchohol();
            JobGiver_Binge_DrinkAlchohol.pawn = pawn;
            JobGiver_Binge_DrinkAlchohol.ignoreForbid = JobGiver_Binge_DrinkAlchohol.pawn.MentalStateDef != null;

            var validator = new Predicate<Thing>( JobGiver_Binge_DrinkAlchohol.CanBingeOn );
            var thing = GenClosest.ClosestThingReachable(
                pawn.Position,
                ThingRequest.ForUndefined(),
                PathEndMode.OnCell,
                TraverseParms.For(
                    pawn,
                    pawn.NormalMaxDanger() ),
                9999f,
                validator,
                Find.ListerThings.AllThings.Where( t => (
                    ( t.def.IsAlcohol() )||
                    ( t is Building_AutomatedFactory )
                ) ),
                -1,
                true );
            if( thing == null )
            {
                return (Job) null;
            }
            Job job = new Job( JobDefOf.Ingest, thing, thing );
            job.maxNumToCarry = Mathf.Min(
                thing.stackCount,
                thing.def.ingestible.maxNumToIngestAtOnce );
            job.ignoreForbidden = JobGiver_Binge_DrinkAlchohol.ignoreForbid;
            return job;
        }

        internal sealed class _DrinkAlchohol
        {
            internal bool                       ignoreForbid;
            internal Pawn                       pawn;

            public _DrinkAlchohol()
            {
            }

            internal bool CanBingeOn( Thing t )
            {
                if(
                    ( !this.ignoreForbid )&&
                    ( t.IsForbidden( this.pawn ) )
                )
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
                return this.pawn.CanReserve( t, 1 );
            }
        }

    }

}
