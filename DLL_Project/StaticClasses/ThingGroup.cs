using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{

    public static class ThingGroup
    {
        // Is this position in a room, not on a door and not touching the edge of the map?
        public static bool IsInRoom( this IntVec3 position, bool MustBeRoofed = false )
        {
            var room = position.GetRoom();
            var things = position.GetThingList();
            var door = things.Find( t => ( ( t as Building_Door ) != null ) );
            return
                ( door == null )&&
                ( room != null )&&
                ( ( MustBeRoofed == false )||( ( MustBeRoofed == true )&&( !room.HasOpenRoofSection ) ) )&&
                ( !room.TouchesMapEdge );
        }

        // Returns if this thing has another thing in the same room with the same def
        public static bool HasRoommateByDef( this Thing parent )
        {
            // Get room cells
            List< IntVec3 > roomCells = parent.GetRoom().Cells.ToList();

            // Scan cells
            foreach( IntVec3 curCell in roomCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( curCell )
                    .Where( thing => thing.def == parent.def )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Returns if this thing has another thing in the same room with the same linker flag
        public static bool HasRoommateByLinker( this Thing parent )
        {
            // Get adjacent cells
            List< IntVec3 > roomCells = parent.GetRoom().Cells.ToList();

            // Scan cells
            foreach( IntVec3 curCell in roomCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( curCell )
                    .Where( thing =>
                        ( thing != null )&&
                        ( thing.def.graphic != null )&&
                        ( thing.def.graphic.data != null )&&
                        ( thing.def.graphic.data.linkFlags == parent.def.graphic.data.linkFlags ) )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Returns if this thing has another thing in the same room with the same def
        public static bool HasRoommateByThingComp( this Thing parent, Type RequiredComp )
        {
            // Get adjacent cells
            List< IntVec3 > roomCells = parent.GetRoom().Cells.ToList();

            // Scan cells
            foreach( IntVec3 curCell in roomCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( curCell )
                    .Where( thing => ( (thing as ThingWithComps) != null)&&( ((ThingWithComps)thing).AllComps.Where( tc => tc.GetType() == RequiredComp ).ToList().Count > 0 ) )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Returns if this thing has another thing touching it with the same def
        public static bool HasTouchingByDef( this Thing parent )
        {
            // Get adjacent cells
            List< IntVec3 > adjCells = GenAdj.CellsAdjacentCardinal( parent )
                .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing => thing.def.defName == parent.def.defName )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Returns if this thing has another thing touching it with the same linker flag
        public static bool HasTouchingByLinker( this Thing parent )
        {
            // Get adjacent cells
            List< IntVec3 > adjCells = GenAdj.CellsAdjacentCardinal( parent )
                .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match parent linker
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing =>
                        ( thing != null )&&
                        ( thing.def.graphic != null )&&
                        ( thing.def.graphic.data != null )&&
                        ( thing.def.graphic.data.linkFlags == parent.def.graphic.data.linkFlags ) )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Returns if this thing has another thing touching it with the same thing comp
        public static bool HasTouchingByThingComp( this Thing parent, Type RequiredComp )
        {
            // Get adjacent cells
            List< IntVec3 > adjCells = GenAdj.CellsAdjacentCardinal( parent )
                .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match required thing comp
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing => ( (thing as ThingWithComps) != null)&&( ((ThingWithComps)thing).AllComps.Where( tc => tc.GetType() == RequiredComp ).ToList().Count > 0 ) )
                    .ToList<Thing>();
                if( ( things != null )&&( things.Count > 0 ) )
                    return true;
            }

            // Nothing found
            return false;
        }

        // Get's a group of things in room by def
        // optionally room bounds
        public static List< Thing > GetGroupOfByDefInRoom( this Thing parent )
        {
            // List for things
            List< Thing > list = new List< Thing >();

            // Get room cells
            List< IntVec3 > roomCells = parent.GetRoom().Cells.ToList();

            // Scan cells
            foreach( IntVec3 curCell in roomCells ){
                // Add things that match parent def
                list.AddRange(
                    Find.ThingGrid.ThingsAt( curCell )
                    .Where( thing => thing.def == parent.def )
                    .ToList<Thing>() );
            }

            // Return list or null if empty
            return list.Count == 0 ? null : list;
        }

        // Get's a group of touching things by thing comp
        // optionally room bounds
        public static List< Thing > GetGroupOfByThingCompInRoom( this Thing parent, Type RequiredComp )
        {
            // List for things
            List< Thing > list = new List< Thing >();

            // Get room cells
            List< IntVec3 > roomCells = parent.GetRoom().Cells.ToList();

            // Scan cells
            foreach( IntVec3 curCell in roomCells ){
                // Add things that match parent def
                list.AddRange(
                    Find.ThingGrid.ThingsAt( curCell )
                    .Where( thing => ( (thing as ThingWithComps) != null)&&( ((ThingWithComps)thing).AllComps.Where( tc => tc.GetType() == RequiredComp ).ToList().Count > 0 ) )
                    .ToList<Thing>() );
            }

            // Return list or null if empty
            return list.Count == 0 ? null : list;
        }

        // Get's a group of touching things by def
        // optionally room bounds
        public static List< Thing > GetGroupOfTouchingByDef( this Thing parent, bool RoomBound = false, List< Thing > cache = null )
        {
            // First call, cache should be null
            if( cache == null ){
                // Create cache
                cache = new List< Thing >();
            }
            // Already in list
            if( cache.Contains( parent ) )
                return cache;

            // Add this one too
            cache.Add( parent );

            // Get adjacent cells
            List< IntVec3 > adjCells;
            if( RoomBound )
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .Where( c => c.GetRoom() == parent.GetRoom() )
                    .ToList< IntVec3 >();
            else
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing => thing.def.defName == parent.def.defName )
                    .ToList<Thing>();
                // Scan things
                foreach( Thing thing in things ){
                    // Add it and scan it
                    cache = thing.GetGroupOfTouchingByDef( RoomBound, cache );
                }
            }
            return cache;
        }

        // Get's a group of touching things by linker
        // optionally room bounds
        public static List< Thing > GetGroupOfTouchingByLinker( this Thing parent, bool RoomBound = false, List< Thing > cache = null )
        {
            // First call, cache should be null
            if( cache == null ){
                // Create cache
                cache = new List< Thing >();
            }
            // Already in list
            if( cache.Contains( parent ) )
                return cache;
            // Add this one too
            cache.Add( parent );

            // Get adjacent cells
            List< IntVec3 > adjCells;
            if( RoomBound )
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .Where( c => c.GetRoom() == parent.GetRoom() )
                    .ToList< IntVec3 >();
            else
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing =>
                        ( thing != null )&&
                        ( thing.def.graphic != null )&&
                        ( thing.def.graphic.data != null )&&
                        ( thing.def.graphic.data.linkFlags == parent.def.graphic.data.linkFlags ) )
                    .ToList<Thing>();
                // Scan things
                foreach( Thing thing in things ){
                    // Add it and scan it
                    cache = thing.GetGroupOfTouchingByLinker( RoomBound, cache );
                }
            }
            return cache;
        }

        // Get's a group of touching things by thing comp
        // optionally room bounds
        public static List< Thing > GetGroupOfTouchingByThingComp( this Thing parent, Type RequiredComp, bool RoomBound = false, List< Thing > cache = null )
        {
            // First call, cache should be null
            if( cache == null ){
                // Create cache
                cache = new List< Thing >();
            }
            // Already in list
            if( cache.Contains( parent ) )
                return cache;
            // Add this one too
            cache.Add( parent );

            // Get adjacent cells
            List< IntVec3 > adjCells;
            if( RoomBound )
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .Where( c => c.GetRoom() == parent.GetRoom() )
                    .ToList< IntVec3 >();
            else
                adjCells = GenAdj.CellsAdjacentCardinal( parent )
                    .ToList< IntVec3 >();

            // Scan cells
            foreach( IntVec3 adjCell in adjCells ){
                // Things in cell which match parent def
                var things = Find.ThingGrid.ThingsAt( adjCell )
                    .Where( thing => ( (thing as ThingWithComps) != null)&&( ((ThingWithComps)thing).AllComps.Where( tc => tc.GetType() == RequiredComp ).ToList().Count > 0 ) )
                    .ToList<Thing>();
                // Scan things
                foreach( ThingWithComps thing in things ){
                    // Add it and scan it
                    cache = thing.GetGroupOfTouchingByThingComp( RequiredComp, RoomBound, cache );
                }
            }
            return cache;
        }

    }
}

