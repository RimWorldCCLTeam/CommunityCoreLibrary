using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _StatWorker
    {
        
        internal static bool _ShouldShowFor( this StatWorker obj, BuildableDef eDef )
        {
            // Need some reflection to access the internals
            StatDef _stat = typeof( StatWorker ).GetField( "stat", BindingFlags.Instance | BindingFlags.NonPublic ).GetValue( obj ) as StatDef;

            if(
                ( !_stat.showIfUndefined )&&
                ( !StatUtility.StatListContains( eDef.statBases, _stat ) )
            )
            {
                return false;
            }
            ThingDef thingDef = eDef as ThingDef;
            if(
                ( thingDef != null )&&
                ( thingDef.category == ThingCategory.Pawn )&&
                (
                    ( !_stat.showOnPawns )||
                    (
                        ( !_stat.showOnHumanlikes )&&
                        ( thingDef.race.Humanlike )
                    )||
                    (
                        ( !_stat.showOnAnimals )&&
                        ( thingDef.race.Animal )
                    )||
                    (
                        ( !_stat.showOnMechanoids )&&
                        ( thingDef.race.mechanoid )
                    )
                )
            )
            {
                return false;
            }
            if(
                ( _stat.category == StatCategoryDefOf.BasicsPawn )||
                ( _stat.category == StatCategoryDefOf.PawnCombat )
            )
            {
                if( thingDef != null )
                    return ( thingDef.category == ThingCategory.Pawn );
                return false;
            }
            if(
                ( _stat.category == StatCategoryDefOf.PawnMisc )||
                ( _stat.category == StatCategoryDefOf.PawnSocial )||
                ( _stat.category == StatCategoryDefOf.PawnWork )
            )
            {
                if(
                    ( thingDef != null )&&
                    ( thingDef.category == ThingCategory.Pawn )
                )
                {
                    return thingDef.race.Humanlike;
                }
                return false;
            }
            if( _stat.category == StatCategoryDefOf.Building )
            {
                if( thingDef == null )
                {
                    return false;
                }
                if( _stat == StatDefOf.DoorOpenSpeed )
                {
                    return
                        ( thingDef.thingClass == typeof( Building_Door ) )||
                        ( thingDef.thingClass.IsSubclassOf( typeof( Building_Door ) ) );
                }
                return thingDef.category == ThingCategory.Building;
            }
            if( _stat.category == StatCategoryDefOf.Apparel )
            {
                if( thingDef == null )
                {
                    return false;
                }
                if( !thingDef.IsApparel )
                {
                    return ( thingDef.category == ThingCategory.Pawn );
                }
                return true;
            }
            if( _stat.category == StatCategoryDefOf.Weapon )
            {
                if( thingDef == null )
                {
                    return false;
                }
                if( !thingDef.IsMeleeWeapon )
                {
                    return thingDef.IsRangedWeapon;
                }
                return true;
            }
            if( _stat.category == StatCategoryDefOf.BasicsNonPawn )
            {
                if( thingDef != null )
                {
                    return ( thingDef.category != ThingCategory.Pawn );
                }
                return true;
            }
            if( _stat.category.displayAllByDefault )
            {
                return true;
            }
            object[] objArray = new object[4];
            string str1 = "Unhandled case: ";
            StatDef statDef = _stat;
            string str2 = ", ";
            BuildableDef buildableDef = eDef;
            objArray[ 0 ] = (object) str1;
            objArray[ 1 ] = (object) statDef;
            objArray[ 2 ] = (object) str2;
            objArray[ 3 ] = (object) buildableDef;
            Verse.Log.Error( string.Concat( objArray ) );
            return false;
        }

    }

}
