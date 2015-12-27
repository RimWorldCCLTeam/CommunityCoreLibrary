using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CommunityCoreLibrary
{
    public static class Detours
    {
        public static unsafe bool TryDetourFromTo ( MethodInfo source, MethodInfo destination )
        {
            if( source == null || destination == null )
                return false;

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
