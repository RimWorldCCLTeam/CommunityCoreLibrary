using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.Detour
{

    internal static class _DropCellFinder
    {

        internal static MethodInfo          _DropCellFinder_IsGoodDropSpot;
        internal static MethodInfo          DropCellFinder_IsGoodDropSpot
        {
            get
            {
                if( _DropCellFinder_IsGoodDropSpot == null )
                {
                    _DropCellFinder_IsGoodDropSpot = typeof( DropCellFinder ).GetMethod( "IsGoodDropSpot", BindingFlags.Static | BindingFlags.NonPublic );
                }
                return DropCellFinder_IsGoodDropSpot;
            }
        }

        internal static Predicate<IntVec3>  _IsGoodDropSpot;
        internal static Predicate<IntVec3>  IsGoodDropSpot
        {
            get
            {
                if( _IsGoodDropSpot == null )
                {
                    _IsGoodDropSpot = new Predicate<IntVec3>( c => (bool)DropCellFinder_IsGoodDropSpot.Invoke( null, new System.Object[] { c } ) );
                }
                return _IsGoodDropSpot;
            }
        }

        internal static MethodInfo          _DropCellFinder_AnyAdjacentGoodDropSpot;
        internal static MethodInfo          DropCellFinder_AnyAdjacentGoodDropSpot
        {
            get
            {
                if( _DropCellFinder_AnyAdjacentGoodDropSpot == null )
                {
                    _DropCellFinder_AnyAdjacentGoodDropSpot = typeof( DropCellFinder ).GetMethod( "AnyAdjacentGoodDropSpot", BindingFlags.Static | BindingFlags.NonPublic );
                }
                return _DropCellFinder_AnyAdjacentGoodDropSpot;
            }
        }

        internal static Func<Building, bool> _ValidAdjacentCell;
        internal static Func<Building, bool> ValidAdjacentCell
        {
            get
            {
                if( _ValidAdjacentCell == null )
                {
                    _ValidAdjacentCell = new Func<Building, bool>( delegate( Building b ) {
                        if( !Find.RoofGrid.Roofed( b.Position ) )
                        {
                            return (bool)DropCellFinder_AnyAdjacentGoodDropSpot.Invoke( null, new System.Object[] { b.Position, false, false } );
                        }
                        return false;
                    } );
                }
                return _ValidAdjacentCell;
            }
        }

        internal static bool                PoweredAndOn( Thing t )
        {
            CompPowerTrader comp = t.TryGetComp<CompPowerTrader>();
            if( comp != null )
            {
                return comp.PowerOn;
            }
            return true;
        }

        internal static IntVec3             _TradeDropSpot()
        {
            IEnumerable<Building> beacons = Find.ListerBuildings.allBuildingsColonist.Where( b => (
                (
                    ( b.def.thingClass == typeof( Building_OrbitalTradeBeacon ) )||
                    ( b.def.thingClass.IsSubclassOf( typeof( Building_OrbitalTradeBeacon ) ) )
                )&&
                ( PoweredAndOn( b ) )
            ) );
            Building building = Enumerable.FirstOrDefault<Building>( beacons, ValidAdjacentCell );
            if( building != null )
            {
                IntVec3 position = building.Position;
                IntVec3 result;
                if( !DropCellFinder.TryFindDropSpotNear( position, out result, false, false ) )
                {
                    Log.Error( "Could find no good TradeDropSpot near dropCenter " + (object) position + ". Using a random standable unfogged cell." );
                    result = CellFinderLoose.RandomCellWith( c => ( GenGrid.Standable( c ) && !GridsUtility.Fogged( c ) ) );
                }
                return result;
            }
            List<Building> beaconAndComms = beacons.ToList();
            beaconAndComms.AddRange(
                Find.ListerBuildings.allBuildingsColonist.Where( b => (
                    (
                        ( b.def.thingClass == typeof( Building_CommsConsole ) )||
                        ( b.def.thingClass.IsSubclassOf( typeof( Building_CommsConsole ) ) )
                    )&&
                    ( PoweredAndOn( b ) )
                ) ) );
            int squareRadius = 8;
            do
            {
                for( int index = 0; index < beaconAndComms.Count; ++index )
                {
                    IntVec3 result;
                    if( CellFinder.TryFindRandomCellNear(
                        beaconAndComms[ index ].Position,
                        squareRadius,
                        IsGoodDropSpot,
                        out result ) )
                    {
                        return result;
                    }
                }
                squareRadius = Mathf.RoundToInt( (float) squareRadius * 1.1f );
            }
            while( squareRadius <= Find.Map.Size.x );
            Log.Error( "Failed to generate trade drop center. Giving random." );
            return CellFinderLoose.RandomCellWith( IsGoodDropSpot );
        }

    }

}
