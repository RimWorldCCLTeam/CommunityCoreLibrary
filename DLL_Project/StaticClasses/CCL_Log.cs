using System.Collections.Generic;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    internal static class CCL_Log
    {
        /// <summary>
        /// Write a log => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Message( string content, string category = null )
        {
            var builder = new StringBuilder();
            builder.Append( Controller.Data.UnityObjectName ).Append( " :: " );

            if( category != null )
            {
                builder.Append( category ).Append( " :: " );
            }

            builder.Append( content );

            Log.Message( builder.ToString() );
        }

        /// <summary>
        /// Write an error => Community Core Library :: category(nullable) :: content
        /// </summary>
        public static void                  Error( string content, string category = null )
        {
            var builder = new StringBuilder();
            builder.Append( Controller.Data.UnityObjectName ).Append( " :: " );

            if( category != null )
            {
                builder.Append( category ).Append( " :: " );
            }

            builder.Append( content );

            Log.Error( builder.ToString() );
        }

        public static void                  Trace( Verbosity Severity, string content, string category = null )
        {
            _Trace( Controller.Data.Trace_Current_Mod, Severity, content, null, category );
        }

        public static void                  TraceMod( Def atFault, Verbosity Severity, string content, string category = null )
        {
            var mod = Find_Extensions.ModByDef( atFault );
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            _Trace( modHelperDef, Severity, content, atFault, category );
        }

        public static void                  TraceMod( LoadedMod mod, Verbosity Severity, string content, string category = null )
        {
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            _Trace( modHelperDef, Severity, content, null, category );
        }

        public static void                  TraceMod( ModHelperDef modHelperDef, Verbosity Severity, string content, string category = null )
        {
            _Trace( modHelperDef, Severity, content, null, category );
        }

        private static void                 _Trace( ModHelperDef modHelperDef, Verbosity Severity, string content, Def atFault = null, string category = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            if(
                ( modHelperDef == null )&&
                ( atFault != null )
            )
            {
                // Try to find the mod associated with this def

                var mod = Find_Extensions.ModByDef( atFault );

                if( mod != null )
                {
                    modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
                }
            }
            if(
                (
                    ( modHelperDef != null )&&
                    ( modHelperDef.Verbosity >= Severity )
                )||
                (
                    ( modHelperDef == null )&&
                    ( Find_Extensions.HightestVerbosity >= Severity )
                )
            )
            {
                var builder = new StringBuilder();
                builder.Append( Controller.Data.UnityObjectName ).Append( " :: " );

                if( modHelperDef != null )
                {
                    builder.Append( modHelperDef.ModName ).Append( " :: " );
                }
                if( category != null )
                {
                    builder.Append( category ).Append( " :: " );
                }
                if( atFault != null )
                {
                    // Name of class
                    var defType = atFault.GetType().ToString();
                    builder.Append( defType ).Append( " :: " ).Append( atFault.defName ).Append( " :: " );
                }

                builder.Append( content );

                if( Severity <= Verbosity.NonFatalErrors )
                {
                    // Error
                    Log.Error( builder.ToString() );
                }
                else if ( Severity == Verbosity.Warnings )
                {
                    // Warning
                    Log.Warning( builder.ToString() );
                }
                else
                {
                    // Wall of text
                    Log.Message( builder.ToString() );
                }
            }
        }
    }
}
