using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    public static class Detours
    {
        private static List<string> detoured = new List<string>();
        private static List<string> destinations = new List<string>();

        /**
            This is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
            https://ludeon.com/forums/index.php?topic=17143.0

            Performs detours, spits out basic logs and warns if a method is detoured multiple times.
        **/
        public static unsafe bool TryDetourFromTo ( MethodInfo source, MethodInfo destination )
        {
            if( source == null || destination == null )
                return false;

            // keep track of detours and spit out some messaging
            string sourceString = source.DeclaringType.FullName + source.Name;
            string destinationString = destination.DeclaringType.FullName + destination.Name;

            if ( detoured.Contains( sourceString ) )
            {
                CCL_Log.Error( sourceString + " already detoured to " + destinations[detoured.IndexOf( sourceString )] + ". This detour will be overwritten.", "Detours" );
            } 
            CCL_Log.Message( "Detouring " + sourceString + " to " + destinationString, "Detours");
            detoured.Add( sourceString );
            destinations.Add( destinationString );

            // get pointers
            int Source_Base         = source     .MethodHandle.GetFunctionPointer ().ToInt32();
            int Destination_Base    = destination.MethodHandle.GetFunctionPointer ().ToInt32();

            // get offset
            int offset_raw = Destination_Base - Source_Base;
            uint* Pointer_Raw_Source = (uint*)Source_Base;

            // insert jump to destination into source
            *( Pointer_Raw_Source + 0 ) = 0xE9909090;
            *( Pointer_Raw_Source + 1 ) = (uint)( offset_raw - 8 );
            
            // done!
            return true;
        }
    }
}
