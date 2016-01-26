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

        public static void                  Trace( Verbosity Severity, string content, string category = null, Def atFault = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            _Trace( null, Severity, content, category, atFault );
        }

        public static void                  TraceMod( LoadedMod mod, Verbosity Severity, string content, string category = null, Def atFault = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            var modHelperDef = Find_Extensions.ModHelperDefForMod( mod );
            _Trace( modHelperDef, Severity, content, category, atFault );
        }

        public static void                  TraceMod( ModHelperDef modHelperDef, Verbosity Severity, string content, string category = null, Def atFault = null )
        {
#if RELEASE
            if( Severity > Verbosity.Validation )
            {
                return;
            }
#endif
            _Trace( modHelperDef, Severity, content, category, atFault );
        }

        static void                         _Trace( ModHelperDef modHelperDef, Verbosity Severity, string content, string category = null, Def atFault = null )
        {
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
                    builder.Append( atFault.defName ).Append( " :: " );
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
