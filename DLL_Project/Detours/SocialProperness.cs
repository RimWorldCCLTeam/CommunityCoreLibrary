using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _SocialProperness
    {

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
            var intVec3 = !t.def.hasInteractionCell ? t.Position : t.InteractionCell;
            return( forPrisoner == intVec3.IsInPrisonCell() );
        }

    }

}
