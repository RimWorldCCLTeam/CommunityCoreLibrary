using System;
using System.Linq;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _MentalStateWorker_Binging
    {
        // TODO: Is this still needed?
        //internal static bool _StateCanOccur( this MentalStateWorker_Binging obj, Pawn pawn )
        //{
        //    if(
        //        ( pawn.Faction != Faction.OfPlayer )||
        //        ( pawn.GetPosture() != PawnPosture.Standing )
        //    )
        //    {
        //        return false;
        //    }
        //    return
        //        Find.ListerThings.AllThings.Any( t => (
        //            ( t.def.ingestible?.hediffGivers != null )&&
        //            ( t.def.ingestible.hediffGivers.Any( h => (
        //                ( h.hediffDef == HediffDefOf.Alcohol )
        //            ) ) )
        //        ) );
        //}
    }

}
