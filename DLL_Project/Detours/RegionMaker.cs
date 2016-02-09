using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _RegionMaker
    {

        // Need some reflected properties to access the internals
        internal static Region               _newReg
        {
            get
            {
                return typeof( RegionMaker ).GetField( "newReg", BindingFlags.Static | BindingFlags.NonPublic ).GetValue( null ) as Region;
            }
        }

        internal static MethodInfo          _TryMakePortalSpan;

        // This method is to fix region building for custom doors
        // which will fail for any class inheritting Building_Door
        internal static bool _CheckRegionableAndProcessNeighbor( IntVec3 c, Rot4 processingDirection )
        {
            if( !GenGrid.InBounds( c ) )
            {
                _RegionMaker._newReg.touchesMapEdge = true;
                return false;
            }
            if( !GenGrid.Walkable( c ) )
            {
                return false;
            }
            Thing regionBarrier = GridsUtility.GetRegionBarrier( c );
            if( regionBarrier == null )
            {
                return true;
            }
            if(
                ( regionBarrier.def.thingClass == typeof( Building_Door ) )||
                ( regionBarrier.def.thingClass.IsSubclassOf( typeof( Building_Door ) ) )
            )
            {
                _TryMakePortalSpan.Invoke( null, new System.Object[] { c, processingDirection } );
            }
            return false;
        }

    }

}
