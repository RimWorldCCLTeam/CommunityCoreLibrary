using System.Collections.Generic;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    internal static class CCL_Log
    {

#if DEVELOPER
        private static System.IO.FileStream logFile;

        static                              CCL_Log()
        {
            OpenStream();
        }

        private static void                 OpenStream()
        {
            if( logFile == null )
            {
                logFile = System.IO.File.Open( "ccl_log.txt", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read );
            }
        }

        public static void                  CloseStream()
        {
            if( logFile != null )
            {
                logFile.Close();
                logFile = null;
            }
        }

        public static void                  Write( string s )
        {
            if(
                ( s.NullOrEmpty() )||
                ( logFile == null )
            )
            {
                return;
            }

            s += "\n";
            byte[] b = new byte[ s.Length ];

            for( int i = 0; i < s.Length; ++i )
            {
                b[ i ] = (byte) s[ i ];
            }

            logFile.Write( b, 0, s.Length );
            logFile.Flush();
        }
#endif

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

#if DEVELOPER
            Write( builder.ToString() );
#endif
            Verse.Log.Message( builder.ToString() );
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

#if DEVELOPER
            Write( builder.ToString() );
#endif
            Verse.Log.Error( builder.ToString() );
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
            Verbosity TraceAt = Verbosity.Default;
            if(
                ( !Controller.Data.Mods.NullOrEmpty() )&&
                ( !Controller.Data.ModHelperDefs.NullOrEmpty() )
            )
            {
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
                if( modHelperDef != null )
                {
                    TraceAt = modHelperDef.Verbosity;
                }
                else
                {
                    TraceAt = Find_Extensions.HightestVerbosity;
                }
            }
            if( TraceAt >= Severity )
            {
                var builder = new StringBuilder();
                builder.Append( Controller.Data.UnityObjectName ).Append( " :: " );

                if(
                    ( modHelperDef != null )&&
                    ( modHelperDef != Controller.Data.cclHelperDef )
                )
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
#if DEVELOPER
                    Write( builder.ToString() );
#endif
                    Verse.Log.Error( builder.ToString() );
                }
                else if ( Severity == Verbosity.Warnings )
                {
                    // Warning
#if DEVELOPER
                    Write( builder.ToString() );
#endif
                    Verse.Log.Warning( builder.ToString() );
                }
                else
                {
                    // Wall of text
#if DEVELOPER
                    Write( builder.ToString() );
#endif
                    Verse.Log.Message( builder.ToString() );
                }
            }
        }
    }
}
