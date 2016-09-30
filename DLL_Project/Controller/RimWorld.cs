using System;
using System.Diagnostics;

using Verse;

namespace CommunityCoreLibrary.Controller
{
    
    internal static class RimWorld
    {

        internal static void                Restart()
        {
            var args = Environment.GetCommandLineArgs();
            var commandLine = "\"" + args[ 0 ] + "\"";
            var arguements = string.Empty;
            for( int index = 1; index < args.GetLength( 0 ); ++index )
            {
                if( index > 1 )
                {
                    arguements += " ";
                }
                arguements += "\"" + args[ index ] + "\"";
            }
#if DEVELOPER
            Log.Message( "Restarting RimWorld:\n" + commandLine + " " + arguements );
#endif
            Process.Start( commandLine, arguements );
            Root.Shutdown();
        }

    }

}
