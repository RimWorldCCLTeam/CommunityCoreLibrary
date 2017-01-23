using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class IntVec3_Extensions
    {

        // Is this cell in an enclosed room?
        // (not on a door and not touching the edge of the map)
        public static bool                  IsInRoom( this IntVec3 cell, Map map, bool MustBeRoofed = false )
        {
            var room = cell.GetRoom( map );
            var things = cell.GetThingList( map );
            var door = things.Find( t => ( ( t as Building_Door ) != null ) );
            return
                ( door == null )&&
                ( room != null )&&
                ( !MustBeRoofed || ( MustBeRoofed && room.OpenRoofCount > 0 ) ) &&
                ( !room.TouchesMapEdge );
        }

    }

}
