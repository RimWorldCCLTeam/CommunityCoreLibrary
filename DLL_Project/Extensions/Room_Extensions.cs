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

        #region Cached Cell Count

        public static int                   CachedCellCountGet( this Room room )
        {
            if( _cachedCellCount == null )
            {
                _cachedCellCount = typeof( Room ).GetField( "cachedCellCount", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (int)_cachedCellCount.GetValue( room );
        }

        public static void                  CachedCellCountSet( this Room room, int value )
        {
            if( _cachedCellCount == null )
            {
                _cachedCellCount = typeof( Room ).GetField( "cachedCellCount", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            _cachedCellCount.SetValue( room, value );
        }

        #endregion

        #region Cached Open Roof Count

        public static int                   CachedOpenRoofCountGet( this Room room )
        {
            if( _cachedOpenRoofCount == null )
            {
                _cachedOpenRoofCount = typeof( Room ).GetField( "cachedOpenRoofCount", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (int)_cachedOpenRoofCount.GetValue( room );
        }

        public static void                  CachedOpenRoofCountSet( this Room room, int value )
        {
            if( _cachedOpenRoofCount == null )
            {
                _cachedOpenRoofCount = typeof( Room ).GetField( "cachedOpenRoofCount", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            _cachedOpenRoofCount.SetValue( room, value );
        }

        #endregion

        #region Stats and Role Dirty Flag

        public static bool                  StatsAndRoleDirtyGet( this Room room )
        {
            if( _statsAndRoleDirty == null )
            {
                _statsAndRoleDirty = typeof( Room ).GetField( "statsAndRoleDirty", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool)_statsAndRoleDirty.GetValue( room );
        }

        public static void                  StatsAndRoleDirtySet( this Room room, bool value )
        {
            if( _statsAndRoleDirty == null )
            {
                _statsAndRoleDirty = typeof( Room ).GetField( "statsAndRoleDirty", BindingFlags.Instance | BindingFlags.NonPublic );
            }
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