using System;
using System.Linq;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MentalStateWorker_BingingAlcohol
    {

        internal static bool _BaseStateCanOccur( this MentalStateWorker obj, Pawn pawn )
        {   // Can't call base.StateCanOccur in detoured method otherwise it will call the
            // override method which is the detoured method (infinite recursion -> stack overflow)
            return
                (
                    ( obj.def.prisonersCanDo )||
                    ( pawn.HostFaction == null )
                ) &&
                (
                    ( !obj.def.colonistsOnly )||
                    ( pawn.Faction == Faction.OfPlayer )
                );
        }

        // Enable any alcohol and not just beer
        internal static bool _StateCanOccur( this MentalStateWorker_BingingAlcohol obj, Pawn pawn )
        {
            if(
                ( !obj._BaseStateCanOccur( pawn ) )||
                ( pawn.GetPosture() != PawnPosture.Standing )
            )
            {
                return false;
            }
            return Find.ListerThings.AllThings.Any( t => t.def.IsAlcohol() );
        }

    }

}
