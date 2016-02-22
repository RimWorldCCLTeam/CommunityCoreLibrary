using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary.Detour
{

    internal static class _GenPlace
    {

        internal enum _PlaceSpotQuality : byte
        {
            Unusable,
            Awful,
            Bad,
            Okay,
            Perfect,
        }

        internal static _GenPlace._PlaceSpotQuality _PlaceSpotQualityAt( IntVec3 c, Thing thing, IntVec3 center )
        {
            if(
                ( !GenGrid.InBounds( c ) )||
                ( !GenGrid.Walkable( c ) )
            )
            {
                return _GenPlace._PlaceSpotQuality.Unusable;
            }
            List<Thing> list = Find.ThingGrid.ThingsListAt(c);
            for( int index = 0; index < list.Count; ++index )
            {
                Thing thing1 = list[ index ];
                if(
                    ( thing.def.saveCompressible )&&
                    ( thing1.def.saveCompressible )
                )
                {
                    return _GenPlace._PlaceSpotQuality.Unusable;
                }
                if( thing1.def.category == ThingCategory.Item )
                {
                    return
                        ( thing1.def == thing.def )&&
                        ( thing1.stackCount < thing.def.stackLimit )
                        ? _GenPlace._PlaceSpotQuality.Perfect
                        : _GenPlace._PlaceSpotQuality.Unusable;
                }
            }
            if( GridsUtility.GetRoom( c ) != GridsUtility.GetRoom( center ) )
            {
                return
                    ( !Reachability.CanReach( center, (TargetInfo) c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly ) )
                    ? _GenPlace._PlaceSpotQuality.Awful
                    : _GenPlace._PlaceSpotQuality.Bad;
            }
            _GenPlace._PlaceSpotQuality placeSpotQuality = _GenPlace._PlaceSpotQuality.Perfect;
            for( int index = 0; index < list.Count; ++index )
            {
                Thing thing1 = list[ index ];
                if(
                    ( thing1.def.thingClass == typeof( Building_Door ) )||
                    ( thing1.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
                )
                {
                    return _GenPlace._PlaceSpotQuality.Bad;
                }
                Pawn pawn = thing1 as Pawn;
                if( pawn != null )
                {
                    if( pawn.Downed )
                    {
                        return _GenPlace._PlaceSpotQuality.Bad;
                    }
                    if( placeSpotQuality > _GenPlace._PlaceSpotQuality.Okay )
                    {
                        placeSpotQuality = _GenPlace._PlaceSpotQuality.Okay;
                    }
                }
                if(
                    ( thing1.def.category == ThingCategory.Plant )&&
                    ( thing1.def.selectable )&&
                    ( placeSpotQuality > _GenPlace._PlaceSpotQuality.Okay )
                )
                {
                    placeSpotQuality = _GenPlace._PlaceSpotQuality.Okay;
                }
            }
            return placeSpotQuality;
        }

    }

}
