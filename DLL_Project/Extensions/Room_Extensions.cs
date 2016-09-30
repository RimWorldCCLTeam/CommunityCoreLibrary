using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class Room_Extensions
    {

        internal static FieldInfo           _cachedCellCount;
        internal static FieldInfo           _cachedOpenRoofCount;
        internal static FieldInfo           _statsAndRoleDirty;

        static                              Room_Extensions()
        {
            _cachedCellCount = typeof( Room ).GetField( "cachedCellCount", Controller.Data.UniversalBindingFlags );
            if( _cachedCellCount == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'cachedCellCount' in class 'Room'",
                    "Room_Extensions");
            }
            _cachedOpenRoofCount = typeof( Room ).GetField( "cachedOpenRoofCount", Controller.Data.UniversalBindingFlags );
            if( _cachedOpenRoofCount == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'cachedOpenRoofCount' in class 'Room'",
                    "Room_Extensions");
            }
            _statsAndRoleDirty = typeof( Room ).GetField( "statsAndRoleDirty", Controller.Data.UniversalBindingFlags );
            if( _statsAndRoleDirty == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'statsAndRoleDirty' in class 'Room'",
                    "Room_Extensions");
            }
        }

        #region Cached Cell Count

        public static int                   CachedCellCountGet( this Room room )
        {
            return (int)_cachedCellCount.GetValue( room );
        }

        public static void                  CachedCellCountSet( this Room room, int value )
        {
            _cachedCellCount.SetValue( room, value );
        }

        #endregion

        #region Cached Open Roof Count

        public static int                   CachedOpenRoofCountGet( this Room room )
        {
            return (int)_cachedOpenRoofCount.GetValue( room );
        }

        public static void                  CachedOpenRoofCountSet( this Room room, int value )
        {
            _cachedOpenRoofCount.SetValue( room, value );
        }

        #endregion

        #region Stats and Role Dirty Flag

        public static bool                  StatsAndRoleDirtyGet( this Room room )
        {
            return (bool)_statsAndRoleDirty.GetValue( room );
        }

        public static void                  StatsAndRoleDirtySet( this Room room, bool value )
        {
            _statsAndRoleDirty.SetValue( room, value );
        }

        #endregion

        #region Portal Enumeration

        public static List<Building_Door>   Portals( this Room room )
        {
            var portals = new List<Building_Door>();
            foreach( var region in room.Regions )
            {
                foreach( var neighbour in region.Neighbors.Where( n => n.portal != null ) )
                {
                    portals.AddUnique( neighbour.portal );
                }
            }
            return portals;
        }

        #endregion

    }

}