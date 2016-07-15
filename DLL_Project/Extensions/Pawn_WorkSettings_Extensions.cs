using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public static class Pawn_WorkSettings_Extensions
    {

        private static MethodInfo           _ConfirmInitializedDebug;
        private static FieldInfo            _priorities;
        private static FieldInfo            _pawn;
        private static FieldInfo            _workGiversDirty;

        public static void                  ConfirmInitializedDebug( this Pawn_WorkSettings p )
        {
            if( _ConfirmInitializedDebug == null )
            {
                _ConfirmInitializedDebug = typeof( Pawn_WorkSettings ).GetMethod( "ConfirmInitializedDebug", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            _ConfirmInitializedDebug.Invoke( p, null );
        }

        public static DefMap<WorkTypeDef, int> Priorities( this Pawn_WorkSettings p )
        {
            if( _priorities == null )
            {
                _priorities = typeof( Pawn_WorkSettings ).GetField( "priorities", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (DefMap<WorkTypeDef, int>)_priorities.GetValue( p );
        }

        public static Pawn                  Pawn( this Pawn_WorkSettings p )
        {
            if( _pawn == null )
            {
                _pawn = typeof( Pawn_WorkSettings ).GetField( "pawn", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (Pawn)_pawn.GetValue( p );
        }

        public static bool                  WorkGiversDirtyGet( this Pawn_WorkSettings p )
        {
            if( _workGiversDirty == null )
            {
                _workGiversDirty = typeof( Pawn_WorkSettings ).GetField( "workGiversDirty", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (bool)_workGiversDirty.GetValue( p );
        }

        public static void                  WorkGiversDirtySet( this Pawn_WorkSettings p, bool value )
        {
            if( _workGiversDirty == null )
            {
                _workGiversDirty = typeof( Pawn_WorkSettings ).GetField( "workGiversDirty", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            _workGiversDirty.SetValue( p, value );
        }

        public static void                  ForcePriority( this Pawn_WorkSettings p, WorkTypeDef w, int priority )
        {
            p.ConfirmInitializedDebug();
            if(
                ( priority < 0 )||
                ( priority > 4 )
            )
            {
                Log.Message( "Trying to set work to invalid priority " + (object) priority );
            }
            var priorities = p.Priorities();
            priorities[ w ] = priority;
            if( priority == 0 )
            {
                p.Pawn().mindState.Notify_WorkPriorityDisabled( w );
            }
            p.WorkGiversDirtySet( true );
        }

    }
}

