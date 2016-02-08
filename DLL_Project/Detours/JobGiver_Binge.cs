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
            JobGiver_Binge_DrinkAlchohol.ignoreForbid = JobGiver_Binge_DrinkAlchohol.pawn.BrokenStateDef != null;

            Predicate<Thing> validator = new Predicate<Thing>( JobGiver_Binge_DrinkAlchohol.CanBingeOn );
            Thing thing = GenClosest.ClosestThingReachable(
                JobGiver_Binge_DrinkAlchohol.pawn.Position,
                ThingRequest.ForUndefined(),
                PathEndMode.OnCell,
                TraverseParms.For(
                    JobGiver_Binge_DrinkAlchohol.pawn,
                    JobGiver_Binge_DrinkAlchohol.pawn.NormalMaxDanger() ),
                9999f,
                validator,
                Find.ListerThings.AllThings.Where( t => (
                    ( t.def.ingestible != null )&&
                    ( t.def.ingestible.hediffGivers != null )&&
                    ( t.def.ingestible.hediffGivers.Any( h => (
                        ( h.hediffDef == HediffDefOf.Alcohol )
                    ) ) )
                ) ),
                -1,
                true );
            if( thing == null )
            {
                return (Job) null;
            }
            Job job = new Job( JobDefOf.Ingest, (TargetInfo) thing );
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
                    ( this.ignoreForbid )||
                    ( !ForbidUtility.IsForbidden( t, this.pawn ) )
                )
                {
                    return ReservationUtility.CanReserve( this.pawn, (TargetInfo) t, 1 );
                }
                return false;
            }
        }

    }

}
