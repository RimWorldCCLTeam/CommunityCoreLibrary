using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{

    public struct DetourPair
    {
        public MethodInfo                   fromMethod;
        public MethodInfo                   toMethod;

        public                              DetourPair( MethodInfo fromMethod, MethodInfo toMethod )
        {
            this.fromMethod                 = fromMethod;
            this.toMethod                   = toMethod;
        }

    }

    public static class Detours
    {
        
        private static List<string>         detouredFromMethods = new List<string>();
        private static List<string>         detouredToMethods = new List<string>();

        private static Dictionary<Assembly,List<Type>> assemblyDetourTypes = new Dictionary<Assembly, List<Type>>();

        /**
            This is a basic first implementation of the IL method 'hooks' (detours) made possible by RawCode's work;
            https://ludeon.com/forums/index.php?topic=17143.0

            Performs detours, spits out basic logs and warns if a method is detoured multiple times.
        **/
        public static unsafe bool           TryDetourFromTo ( MethodInfo fromMethod, MethodInfo toMethod )
        {
            // error out on null arguments
            if( fromMethod == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    "fromMethod is null",
                    "Detour"
                );
                return false;
            }

            if( toMethod == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    "toMethod is null",
                    "Detour"
                );
                return false;
            }

            // keep track of detours and spit out some messaging
            string fromString       = fromMethod.DeclaringType.FullName + "." + fromMethod.Name + " @ 0x" + fromMethod.MethodHandle.GetFunctionPointer().ToString( "X" + ( IntPtr.Size *  2 ).ToString() );
            string toString         = toMethod.DeclaringType.FullName   + "." + toMethod.Name   + " @ 0x" + toMethod.MethodHandle.GetFunctionPointer().ToString( "X" + ( IntPtr.Size *  2 ).ToString() );

            if( detouredFromMethods.Contains( fromString ) )
            {
                CCL_Log.Trace(
                    Verbosity.Warnings,
                    string.Format( "'{0}' has already been detoured to '{1}'", fromString, detouredToMethods[ detouredFromMethods.IndexOf( fromString ) ] ),
                    "Detour"
                );
            } 
            CCL_Log.Trace(
                Verbosity.Injections,
                string.Format( "'{0}' to '{1}'", fromString, toString ),
                "Detour"
            );

            detouredFromMethods.Add( fromString );
            detouredToMethods.Add( toString );

            if( IntPtr.Size == sizeof( Int64 ) )
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                long fromMethodPtr = fromMethod.MethodHandle.GetFunctionPointer().ToInt64();
                long toMethodPtr   = toMethod  .MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* fromMethodRawPtr = (byte*)fromMethodPtr;

                // Pointer to insert jump address into native code
                long* jumpInstructionAddressPtr = (long*)( fromMethodRawPtr + 0x02 );

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *( fromMethodRawPtr + 0x00 ) = 0x48;
                *( fromMethodRawPtr + 0x01 ) = 0xB8;
                *jumpInstructionAddressPtr  = toMethodPtr; // ( fromMethodRawPtr + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *( fromMethodRawPtr + 0x0A ) = 0xFF;
                *( fromMethodRawPtr + 0x0B ) = 0xE0;

            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                int fromMethodPtr = fromMethod.MethodHandle.GetFunctionPointer().ToInt32();
                int toMethodPtr   = toMethod  .MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                byte* fromMethodRawPtr = (byte*)fromMethodPtr;

                // Pointer to insert jump address into native code
                int* jumpInstructionAddressPtr = (int*)( fromMethodRawPtr + 1 );

                // Jump offset (less instruction size)
                int relativeJumpOffset = ( toMethodPtr - fromMethodPtr ) - 5;

                // Insert 32-bit relative jump into native code
                *fromMethodRawPtr = 0xE9;
                *jumpInstructionAddressPtr = relativeJumpOffset;
            }

            // done!
            return true;
        }

        public static bool                  TryDetourFromTo( List<DetourPair> detours )
        {
            if( detours.NullOrEmpty() )
            {
                return true;
            }
            foreach( var detour in detours )
            {
                if(
                    ( detour.fromMethod == null )||
                    ( detour.toMethod == null )
                )
                {
                    return false;
                }
            }
            foreach( var detour in detours )
            {
                if( !TryDetourFromTo( detour.fromMethod, detour.toMethod ) )
                {
                    return false;
                }
            }
            return true;
        }

        public static bool                  TryTimedAssemblyDetours( Assembly assembly, InjectionSequence sequence, InjectionTiming timing )
        {
            var detourPairs = GetTimedDetours( assembly, sequence, timing );
            if( !detourPairs.NullOrEmpty() )
            {
                if( !TryDetourFromTo( detourPairs ) )
                {
                    return false;
                }
            }
            return true;
        }

        public static List<DetourPair>      GetTimedDetours( Assembly toAssembly, InjectionSequence sequence, InjectionTiming timing )
        {
            // Get only types which have methods and/or properties marked with either of the detour attributes
            List<Type> toTypes = null;

            // First try to get an already built list from the cache
            if( !assemblyDetourTypes.TryGetValue( toAssembly, out toTypes ) )
            {
                // Assembly isn't in the cache, build the list of types with detours
                toTypes = toAssembly
                    .GetTypes()
                    .Where( toType => (
                        ( toType.GetMethods(    Controller.Data.UniversalBindingFlags ).Any( toMethod   => toMethod  .HasAttribute<DetourClassMethod>()   ) )||
                        ( toType.GetProperties( Controller.Data.UniversalBindingFlags ).Any( toProperty => toProperty.HasAttribute<DetourClassProperty>() ) )
                    ) )
                    .ToList();
                // Add the type list to the assembly cache
                assemblyDetourTypes.Add( toAssembly, toTypes );
            }

            // No types are detouring, return null
            if( toTypes.NullOrEmpty() )
            {
                return null;
            }

            // Invalid timing
            if(
                ( sequence == InjectionSequence.Never )||
                ( timing == InjectionTiming.Never )
            )
            {
                return null;
            }

            // Create return list for the detours
            var detours = new List<DetourPair>();

            // Process the types and fetch their detours
            foreach( var toType in toTypes )
            {
                // Get the raw methods
                GetTimedDetouredMethods(    ref detours, toType, sequence, timing );
                // Get the cloaked methods (properties)
                GetTimedDetouredProperties( ref detours, toType, sequence, timing );
            }

            // Return the list for this sequence and timing
            return detours;
        }

        private static void                 GetTimedDetouredMethods( ref List<DetourPair> detours, Type toType, InjectionSequence sequence, InjectionTiming timing )
        {
            var toMethods = toType
                .GetMethods( Controller.Data.UniversalBindingFlags )
                .Where( toMethod => toMethod.HasAttribute<DetourClassMethod>() )
                .ToList();
            if( toMethods.NullOrEmpty() )
            {   // No methods to detour
                return;
            }
            foreach( var toMethod in toMethods )
            {
                DetourClassMethod attribute = null;
                if( toMethod.TryGetAttribute<DetourClassMethod>( out attribute ) )
                {
                    if(
                        ( attribute.injectionSequence != sequence )||
                        (
                            ( timing != InjectionTiming.All )&&
                            ( attribute.injectionTiming != timing )
                        )
                    )
                    {   // Ignore any detours which timing doesn't match
                        continue;
                    }
                    if( attribute.fromClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "fromClass is null for '{0}.{1}'", toType.FullName, toMethod.Name ),
                            "Detour"
                        );
                        continue;
                    }
                    MethodInfo fromMethod;
                    try
                    {   // Try to get method direct
                        fromMethod = attribute.fromClass.GetMethod( attribute.fromMethod, Controller.Data.UniversalBindingFlags );
                    }
                    catch
                    {   // May be ambiguous, try from parameter count
                        fromMethod = attribute.fromClass.GetMethods( Controller.Data.UniversalBindingFlags )
                                                  .FirstOrDefault( checkMethod =>
                                                                  (
                                                                      ( checkMethod.Name == attribute.fromMethod )&&
                                                                      ( checkMethod.ReturnType == toMethod.ReturnType )&&
                                                                      ( checkMethod.GetParameters().Count() == toMethod.GetParameters().Count() )
                                                                     ) );
                    }
                    if( fromMethod == null )
                    {   // Report and ignore any missing methods
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "fromMethod is null for '{0}.{1}'", toType.FullName, toMethod.Name ),
                            "Detour"
                        );
                        continue;
                    }
                    // Add detour for method
                    detours.Add( new DetourPair( fromMethod, toMethod ) );
                }
            }
        }

        private static void                 GetTimedDetouredProperties( ref List<DetourPair> detours, Type toType, InjectionSequence sequence, InjectionTiming timing  )
        {
            var toProperties = toType
                .GetProperties( Controller.Data.UniversalBindingFlags )
                .Where( toProperty => toProperty.HasAttribute<DetourClassProperty>() )
                .ToList();
            if( toProperties.NullOrEmpty() )
            {   // No properties to detour
                return;
            }
            foreach( var toProperty in toProperties )
            {
                DetourClassProperty attribute = null;
                if( toProperty.TryGetAttribute<DetourClassProperty>( out attribute ) )
                {
                    if(
                        ( attribute.injectionSequence != sequence )||
                        (
                            ( timing != InjectionTiming.All )&&
                            ( attribute.injectionTiming != timing )
                        )
                    )
                    {   // Ignore any detours which timing doesn't match
                        continue;
                    }
                    if( attribute.fromClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "fromClass is null for '{0}.{1}'", toType.FullName, toProperty.Name ),
                            "Detour"
                        );
                        continue;
                    }
                    var fromProperty = attribute.fromClass.GetProperty( attribute.fromProperty, Controller.Data.UniversalBindingFlags );
                    if( fromProperty == null )
                    {   // Report and ignore any missing properties
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "fromProperty is null for '{0}.{1}'", toType.FullName, toProperty.Name ),
                            "Detour"
                        );
                        continue;
                    }
                    var toMethod = toProperty.GetGetMethod( true );
                    if( toMethod != null )
                    {   // Check for get method detour
                        var fromMethod = fromProperty.GetGetMethod( true );
                        if( fromMethod == null )
                        {   // Report and ignore missing get method
                            CCL_Log.Trace(
                                Verbosity.Injections,
                                string.Format( "fromProperty has no get method for '{0}.{1}'", toType.FullName, toProperty.Name ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for get method
                            detours.Add( new DetourPair( fromMethod, toMethod ) );
                        }
                    }
                    toMethod = toProperty.GetSetMethod( true );
                    if( toMethod != null )
                    {   // Check for set method detour
                        var fromMethod = fromProperty.GetSetMethod( true );
                        if( fromMethod == null )
                        {   // Report and ignore missing set method
                            CCL_Log.Trace(
                                Verbosity.Injections,
                                string.Format( "fromProperty has no set method for '{0}.{1}'", toType.FullName, toProperty.Name ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for set method
                            detours.Add( new DetourPair( fromMethod, toMethod ) );
                        }
                    }
                }
            }
        }

    }

}
