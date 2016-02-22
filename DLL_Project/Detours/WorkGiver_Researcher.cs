using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _WorkGiver_Researcher
    {
        
        internal static bool _HasJobOnThing( this WorkGiver_Researcher obj, Pawn pawn, Thing t )
        {
            return
                (
                    ( t.def.thingClass == typeof( Building_ResearchBench ) )||
                    ( t.def.thingClass.IsSubclassOf( typeof( Building_ResearchBench ) ) )
                )&&
                ( ReservationUtility.CanReserve( pawn, (TargetInfo) t, 1 ) );
        }

    }

}
