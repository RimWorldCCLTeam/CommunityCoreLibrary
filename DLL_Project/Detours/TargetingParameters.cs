using System;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _TargetingParameters
    {

        // This method is to handle targetting custom doors and fire
        // which will fail for any class inheritting Building_Door or is not ThingDefOf.Fire
        internal static bool _CanTarget( this TargetingParameters obj, TargetInfo targ )
        {
            if(
                ( obj.validator != null )&&
                ( !obj.validator( targ ) )
            )
            {
                return false;
            }
            if( targ.Thing == null )
            {
                return obj.canTargetLocations;
            }
            if(
                ( obj.neverTargetDoors )&&
                (
                    ( targ.Thing.def.thingClass == typeof( Building_Door ) )||
                    ( targ.Thing.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                )||
                ( obj.onlyTargetDamagedThings )&&
                ( targ.Thing.HitPoints == targ.Thing.MaxHitPoints )||
                (
                    ( obj.onlyTargetFlammables )&&
                    ( !targ.Thing.FlammableNow )||
                    ( obj.mustBeSelectable )&&
                    ( !ThingSelectionUtility.SelectableNow( targ.Thing ) )
                )
            )
            {
                return false;
            }
            if(
                ( obj.targetSpecificThing != null )&&
                ( targ.Thing == obj.targetSpecificThing )||
                ( obj.canTargetFires )&&
                (
                    ( targ.Thing.def.thingClass == typeof( Fire ) )||
                    ( targ.Thing.def.thingClass.IsSubclassOf( typeof( Fire ) ) )
                )
            )
            {
                return true;
            }
            if(
                ( obj.canTargetPawns )&&
                ( targ.Thing.def.category == ThingCategory.Pawn )
            )
            {
                if( ( (Pawn) targ.Thing ).Downed )
                {
                    if( obj.neverTargetIncapacitated )
                    {
                        return false;
                    }
                }
                else if( obj.onlyTargetIncapacitatedPawns )
                {
                    return false;
                }
                return
                    ( obj.onlyTargetFactions == null )||
                    ( obj.onlyTargetFactions.Contains( targ.Thing.Faction ) );
            }
            if(
                ( obj.canTargetBuildings )&&
                ( targ.Thing.def.category == ThingCategory.Building )
            )
            {
                return
                    (
                        ( !obj.onlyTargetBarriers )||
                        ( targ.Thing.def.regionBarrier )
                    )&&
                    (
                        ( obj.onlyTargetFactions == null )||
                        ( obj.onlyTargetFactions.Contains( targ.Thing.Faction ) )
                    );
            }
            return
                ( obj.canTargetItems )&&
                (
                    ( !obj.worldObjectTargetsMustBeAutoAttackable )||
                    ( targ.Thing.def.isAutoAttackableWorldObject )
                );
        }

    }

}
