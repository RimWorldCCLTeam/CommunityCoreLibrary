using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _SocialProperness
    {

        // Fixes social properness for prison cells by checking that the thing/interaction
        // cells location is a prison cell instead of the same room
        internal static bool _IsSociallyProper( this Thing t, Pawn p, bool forPrisoner, bool animalsCare = false )
        {
            if(
                ( !animalsCare )&&
                ( !p.RaceProps.Humanlike )||
                ( !t.def.socialPropernessMatters )
            )
            {
                return true;
            }
            var thingPos = !t.def.hasInteractionCell ? t.Position : t.InteractionCell;
            return( forPrisoner == thingPos.IsInPrisonCell() );
        }

    }

}
