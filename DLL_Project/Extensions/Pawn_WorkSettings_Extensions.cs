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

        static                              Pawn_WorkSettings_Extensions()
        {
            _ConfirmInitializedDebug = typeof( Pawn_WorkSettings ).GetMethod( "ConfirmInitializedDebug", Controller.Data.UniversalBindingFlags );
            if( _ConfirmInitializedDebug == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ConfirmInitializedDebug' in class 'Pawn_WorkSettings'",
                    "Pawn_WorkSettings_Extensions");
            }
            _priorities = typeof( Pawn_WorkSettings ).GetField( "priorities", Controller.Data.UniversalBindingFlags );
            if( _priorities == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'priorities' in class 'Pawn_WorkSettings'",
                    "Pawn_WorkSettings_Extensions");
            }
            _pawn = typeof( Pawn_WorkSettings ).GetField( "pawn", Controller.Data.UniversalBindingFlags );
            if( _pawn == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'pawn' in class 'Pawn_WorkSettings'",
                    "Pawn_WorkSettings_Extensions");
            }
            _workGiversDirty = typeof( Pawn_WorkSettings ).GetField( "workGiversDirty", Controller.Data.UniversalBindingFlags );
            if( _workGiversDirty == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'workGiversDirty' in class 'Pawn_WorkSettings'",
                    "Pawn_WorkSettings_Extensions");
            }
        }

        public static void                  ConfirmInitializedDebug( this Pawn_WorkSettings p )
        {
            _ConfirmInitializedDebug.Invoke( p, null );
        }

        public static DefMap<WorkTypeDef, int> Priorities( this Pawn_WorkSettings p )
        {
            return (DefMap<WorkTypeDef, int>)_priorities.GetValue( p );
        }

        public static Pawn                  Pawn( this Pawn_WorkSettings p )
        {
            return (Pawn)_pawn.GetValue( p );
        }

        public static bool                  WorkGiversDirtyGet( this Pawn_WorkSettings p )
        {
            return (bool)_workGiversDirty.GetValue( p );
        }

        public static void                  WorkGiversDirtySet( this Pawn_WorkSettings p, bool value )
        {
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

