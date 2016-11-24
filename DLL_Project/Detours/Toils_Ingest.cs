using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _Toils_Ingest
    {

        [DetourMember( typeof( Toils_Ingest ) )]
        internal static Toil                _TakeMealFromDispenser( TargetIndex ind, Pawn eater )
        {
            var toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Building_NutrientPasteDispenser.CollectDuration;
            toil.AddFinishAction( () =>
            {
                var pawn = toil.actor;
                var NPD = pawn.jobs.curJob.GetTarget( ind ).Thing as Building_NutrientPasteDispenser;
                var meal = NPD.TryDispenseFood();
                if( Find.Reservations.ReservedBy( NPD, pawn ) )
                {
                    Find.Reservations.Release( NPD, pawn );
                }
                if( meal == null )
                {
                    pawn.jobs.curDriver.EndJobWith( JobCondition.Incompletable );
                }
                else
                {
                    pawn.carrier.TryStartCarry( meal );
                    pawn.jobs.curJob.targetA = pawn.carrier.CarriedThing;
                }
            } );
            return toil;
        }

    }

}
