#if DEVELOPER
// Enable this define for a whole bunch of debug logging
//#define _I_AM_A_POTATO_
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{

    public class DetourPair
    {
        public Type                         targetClass;
        public MethodInfo                   sourceMethod;
        public MethodInfo                   destinationMethod;

        public                              DetourPair( Type targetClass, MethodInfo sourceMethod, MethodInfo destinationMethod )
        {
            this.targetClass                  = targetClass;
            this.sourceMethod                 = sourceMethod;
            this.destinationMethod            = destinationMethod;
        }

        public bool                         TryDetour()
        {
            return Detours.TryDetourFromTo( targetClass, sourceMethod, destinationMethod );
        }

    }

    /*
        The basic implementation of the IL method 'hooks' (detours) made possible by RawCode's work (32-bit);
        https://ludeon.com/forums/index.php?topic=17143.0

        Additional implementation features (64-bit, error checking, method gathering, method validation, etc)
        are coded by and based on research done by 1000101.

        Performs detours, spits out basic logs and warns if a method is detoured multiple times.

        Remember when stealing...err...copying code to make sure the proper people get proper credit.
    */
    public static class Detours
    {

        private static Dictionary<string,string>    detouredMethods = new Dictionary<string, string>();

        private static Dictionary<Assembly,List<Type>> assemblyDetourTypes = new Dictionary<Assembly, List<Type>>();

        #region Public API

        /// <summary>
        /// Tries to detour one method to another.  Both methods must have matching return type and parameters.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="targetClass">Target class of detour</param>
        /// <param name="sourceMethod">Source method to detour from</param>
        /// <param name="destinationMethod">Destination method to detour to</param>
        public static bool                  TryDetourFromTo( Type targetClass, MethodInfo sourceMethod, MethodInfo destinationMethod )
        {
#if DEBUG
            // Error out on null arguments
            if( sourceMethod == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    "sourceMethod is null",
                    "Detour"
                );
                return false;
            }

            if( destinationMethod == null )
            {
                CCL_Log.Trace(
                    Verbosity.Injections,
                    "destinationMethod is null",
                    "Detour"
                );
                return false;
            }
#endif

            // Create strings with the fullname and address of the methods
            var sourceFullDescription      = FullMethodName( sourceMethod     , true );
            var destinationFullDescription = FullMethodName( destinationMethod, true );

#if DEBUG
            // Make sure the two methods are call compatible
            var reason = string.Empty;
            if( !MethodsAreCallCompatible( targetClass, sourceMethod, destinationMethod, out reason ) )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Methods are not call compatible when trying to detour '{0}' to '{1}' :: {2}", sourceFullDescription, destinationFullDescription, reason ),
                    "Detour"
                );
                return false;
            }
#endif

            // Warn if already detoured
            if( detouredMethods.ContainsKey( sourceFullDescription ) )
            {
                CCL_Log.Trace(
                    Verbosity.Warnings,
                    string.Format( "'{0}' has already been detoured to '{1}'", sourceFullDescription, detouredMethods[ sourceFullDescription ] ),
                    "Detour"
                );
            } 

#if DEBUG
            // Log the detour about to happen
            CCL_Log.Trace(
                Verbosity.Injections,
                string.Format( "'{0}' to '{1}'", sourceFullDescription, destinationFullDescription ),
                "Detour"
            );
#endif

            // Add the detour to the list of already detoured methods
            detouredMethods[ sourceFullDescription ] = destinationFullDescription;

            TryDetourFromToInt( sourceMethod, destinationMethod );

            // Method is now detoured, we are doneski!
            return true;
        }

        /// <summary>
        /// Takes a list and processes an entire list of detour as a batch.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="detours">List of DetourPairs to process</param>
        public static bool                  TryDetourFromTo( List<DetourPair> detours )
        {
            if( detours.NullOrEmpty() )
            {
                return true;
            }
            foreach( var detour in detours )
            {
                if(
                    ( detour.sourceMethod == null )||
                    ( detour.destinationMethod == null )
                )
                {
                    return false;
                }
            }
            var rVal = true;
            foreach( var detour in detours )
            {
                rVal &= detour.TryDetour();
            }
            return rVal;
        }

        /// <summary>
        /// Detour methods in an assembly with matching sequence and timing parameters.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="assembly">Assembly to get detours from</param>
        /// <param name="sequence">Injection sequence</param>
        /// <param name="timing">Injection timing</param>
        public static bool                  TryTimedAssemblyDetours( Assembly assembly, InjectionSequence sequence, InjectionTiming timing )
        {
            var rVal = true;
            var detourPairs = GetTimedDetours( assembly, sequence, timing );
            if( !detourPairs.NullOrEmpty() )
            {
                rVal &= TryDetourFromTo( detourPairs );
            }
            return rVal;
        }

        /// <summary>
        /// Gets the list detour methods in an assembly with matching sequence and timing parameters.
        /// </summary>
        /// <returns>The list of detours with matching sequence and timing.</returns>
        /// <param name="assembly">Assembly to get detours from</param>
        /// <param name="sequence">Injection sequence</param>
        /// <param name="timing">Injection timing</param>
        public static List<DetourPair>      GetTimedDetours( Assembly assembly, InjectionSequence sequence, InjectionTiming timing )
        {
            // Get only types which have methods and/or properties marked with either of the detour attributes
            List<Type> toTypes = null;

            // First try to get an already built list from the cache
            if( !assemblyDetourTypes.TryGetValue( assembly, out toTypes ) )
            {
                // Assembly isn't in the cache, build the list of types with detours
                toTypes = assembly
                    .GetTypes()
                    .Where( toType => (
                        ( toType.GetMethods(    Controller.Data.UniversalBindingFlags ).Any( toMethod   => toMethod  .HasAttribute<DetourMember>() ) )||
                        ( toType.GetProperties( Controller.Data.UniversalBindingFlags ).Any( toProperty => toProperty.HasAttribute<DetourMember>() ) )
                    ) )
                    .ToList();
                // Add the type list to the assembly cache
                assemblyDetourTypes.Add( assembly, toTypes );
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

        #endregion

        #region Actual detour method

        private static unsafe void          TryDetourFromToInt( MethodInfo sourceMethod, MethodInfo destinationMethod )
        {
            // Now the meat!  Do the machine-word size appropriate detour (32/64-bit)
            if( IntPtr.Size == sizeof( Int64 ) )
            {
                // 64-bit systems use 64-bit absolute address and jumps
                // 12 byte destructive

                // Get function pointers
                long sourceMethodPtr      = sourceMethod     .MethodHandle.GetFunctionPointer().ToInt64();
                long destinationMethodPtr = destinationMethod.MethodHandle.GetFunctionPointer().ToInt64();

                // Native source address
                byte* sourceMethodRawPtr = (byte*)sourceMethodPtr;

                // Pointer to insert jump address into native code
                long* jumpInstructionAddressPtr = (long*)( sourceMethodRawPtr + 0x02 );

                // Insert 64-bit absolute jump into native code (address in rax)
                // mov rax, immediate64
                // jmp [rax]
                *( sourceMethodRawPtr + 0x00 ) = 0x48;
                *( sourceMethodRawPtr + 0x01 ) = 0xB8;
                *jumpInstructionAddressPtr  = destinationMethodPtr; // ( sourceMethodRawPtr + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                *( sourceMethodRawPtr + 0x0A ) = 0xFF;
                *( sourceMethodRawPtr + 0x0B ) = 0xE0;

            }
            else
            {
                // 32-bit systems use 32-bit relative offset and jump
                // 5 byte destructive

                // Get function pointers
                int sourceMethodPtr      = sourceMethod     .MethodHandle.GetFunctionPointer().ToInt32();
                int destinationMethodPtr = destinationMethod.MethodHandle.GetFunctionPointer().ToInt32();

                // Native source address
                byte* sourceMethodRawPtr = (byte*)sourceMethodPtr;

                // Pointer to insert jump address into native code
                int* jumpInstructionAddressPtr = (int*)( sourceMethodRawPtr + 1 );

                // Jump offset (less instruction size)
                int relativeJumpOffset = ( destinationMethodPtr - sourceMethodPtr ) - 5;

                // Insert 32-bit relative jump into native code
                *sourceMethodRawPtr = 0xE9;
                *jumpInstructionAddressPtr = relativeJumpOffset;
            }
        }

        #endregion

        #region Internal class helper methods

        private enum MethodType
        {
            Invalid,
            Instance,
            Extension,
            Static
        }

        private static string               FullMethodName( MethodInfo methodInfo, bool withAddress = false )
        {
            var rVal = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
            if( withAddress )
            {
                rVal += " @ 0x" + methodInfo.MethodHandle.GetFunctionPointer().ToString( "X" + ( IntPtr.Size *  2 ).ToString() );
            }
            return rVal;
        }

        private static MethodType           GetMethodType( MethodInfo methodInfo )
        {
            if( !methodInfo.IsStatic )
            {
                return MethodType.Instance;
            }
            if( methodInfo.IsDefined( typeof( ExtensionAttribute ), false ) )
            {
                return ( !methodInfo.GetParameters().NullOrEmpty() )
                    ? MethodType.Extension
                    : MethodType.Invalid;
            }
            return MethodType.Static;
        }

        private static Type                 GetMethodTargetClass( MethodInfo info )
        {
#if _I_AM_A_POTATO_
            var fullMethodName = FullMethodName( info );
#endif

            var methodType = GetMethodType( info );
            DetourMember attribute = null;

            if( info.TryGetAttribute( out attribute ) )
            {
                if( attribute.targetClass != DetourMember.DefaultTargetClass )
                {
#if _I_AM_A_POTATO_
                    CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: TargetClass = '{1}'", fullMethodName, FullNameOfType( attribute.targetClass ) ), "Detour" );
#endif
                    return attribute.targetClass;
                }
                if(
                    ( methodType == MethodType.Instance )||
                    ( methodType == MethodType.Static )
                )
                {
#if _I_AM_A_POTATO_
                    CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: DeclaringType.BaseType = '{1}'", fullMethodName, FullNameOfType( info.DeclaringType.BaseType ) ), "Detour" );
#endif
                    return info.DeclaringType.BaseType;
                }
                if( methodType == MethodType.Extension )
                {
#if _I_AM_A_POTATO_
                    CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: Parameters[ 0 ].ParameterType = '{1}'", fullMethodName, FullNameOfType( info.GetParameters()[ 0 ].ParameterType ) ), "Detour" );
#endif
                    return info.GetParameters()[ 0 ].ParameterType;
                }
            }

            if(
                ( methodType == MethodType.Instance )||
                ( methodType == MethodType.Static )
            )
            {
#if _I_AM_A_POTATO_
                CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: DeclaringType = '{1}'", fullMethodName, FullNameOfType( info.DeclaringType ) ), "Detour" );
#endif
                return info.DeclaringType;
            }
            if( methodType == MethodType.Extension )
            {
#if _I_AM_A_POTATO_
                CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: Parameters[ 0 ].ParameterType = '{1}'", fullMethodName, FullNameOfType( info.GetParameters()[ 0 ].ParameterType ) ), "Detour" );
#endif
                return info.GetParameters()[ 0 ].ParameterType;
            }
#if _I_AM_A_POTATO_
            CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) = null", fullMethodName ), "Detour" );
#endif
            return null;
        }

        private static Type                 GetPropertyTargetClass( PropertyInfo info )
        {
            DetourMember attribute = null;
            if( info.TryGetAttribute( out attribute ) )
            {
                if( attribute.targetClass != DetourMember.DefaultTargetClass )
                {
                    return attribute.targetClass;
                }
                return info.DeclaringType.BaseType;
            }

            return info.DeclaringType;
        }

        private static string               GetFixedMemberName( string memberName )
        {
            if( memberName[ 0 ] == "_"[ 0 ] )
            {
                return memberName.Substring( 1, memberName.Length - 1 );
            }
            return memberName;
        }

        private static MethodInfo           GetTimedDetouredMethodInt( Type targetClass, string sourceMember, MethodInfo destinationMethod )
        {
            if( sourceMember == DetourMember.DefaultTargetMemberName )
            {
                sourceMember = destinationMethod.Name;
            }
            MethodInfo sourceMethod = null;
            try
            {   // Try to get method direct
                sourceMethod = targetClass.GetMethod( sourceMember, Controller.Data.UniversalBindingFlags );
            }
            catch
            {   // May be ambiguous, try from parameter types (thanks Zhentar for the change from count to types)
                sourceMethod = targetClass.GetMethods( Controller.Data.UniversalBindingFlags )
                                          .FirstOrDefault( checkMethod => (
                                              ( checkMethod.Name == sourceMember )&&
                                              ( checkMethod.ReturnType == destinationMethod.ReturnType )&&
                                              ( checkMethod.GetParameters().Select( checkParameter => checkParameter.ParameterType ).SequenceEqual( destinationMethod.GetParameters().Select( destinationParameter => destinationParameter.ParameterType ) ) )
                                             ) );
            }
            var fixedName = GetFixedMemberName( sourceMember );
            if(
                ( sourceMethod == null )&&
                ( sourceMember != fixedName )
            )
            {
                return GetTimedDetouredMethodInt( targetClass, fixedName, destinationMethod );
            }
            return sourceMethod;
        }

        private static PropertyInfo         GetTimedDetouredPropertyInt( Type targetClass, string sourceMember, PropertyInfo destinationProperty )
        {
            if( sourceMember == DetourMember.DefaultTargetMemberName )
            {
                sourceMember = destinationProperty.Name;
            }
            var sourceProperty = targetClass.GetProperty( sourceMember, Controller.Data.UniversalBindingFlags );
            var fixedName = GetFixedMemberName( sourceMember );
            if(
                ( sourceProperty == null )&&
                ( sourceMember != fixedName )
            )
            {
                return GetTimedDetouredPropertyInt( targetClass, fixedName, destinationProperty );
            }
            return sourceProperty;
        }

        private static void                 GetTimedDetouredMethods( ref List<DetourPair> detours, Type destinationType, InjectionSequence sequence, InjectionTiming timing )
        {
            var destinationMethods = destinationType
                .GetMethods( Controller.Data.UniversalBindingFlags )
                .Where( destinationMethod => destinationMethod.HasAttribute<DetourMember>() )
                .ToList();
            if( destinationMethods.NullOrEmpty() )
            {   // No methods to detour
                return;
            }
            foreach( var destinationMethod in destinationMethods )
            {
                DetourMember attribute = null;
                if( destinationMethod.TryGetAttribute( out attribute ) )
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
                    var targetClass = GetMethodTargetClass( destinationMethod );
                    if( targetClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, FullNameOfType( attribute.targetClass ) ),
                            "Detour"
                        );
                        continue;
                    }
                    var sourceMethod = GetTimedDetouredMethodInt( targetClass, attribute.targetMember, destinationMethod );
                    if( sourceMethod == null )
                    {   // Report and ignore any missing methods
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, targetClass.FullName, attribute.targetMember ),
                            "Detour"
                        );
                        continue;
                    }
                    // Add detour for method
                    detours.Add( new DetourPair( targetClass, sourceMethod, destinationMethod ) );
                }
            }
        }

        private static void                 GetTimedDetouredProperties( ref List<DetourPair> detours, Type destinationType, InjectionSequence sequence, InjectionTiming timing  )
        {
            var destinationProperties = destinationType
                .GetProperties( Controller.Data.UniversalBindingFlags )
                .Where( destinationProperty => destinationProperty.HasAttribute<DetourMember>() )
                .ToList();
            if( destinationProperties.NullOrEmpty() )
            {   // No properties to detour
                return;
            }
            foreach( var destinationProperty in destinationProperties )
            {
                DetourMember attribute = null;
                if( destinationProperty.TryGetAttribute( out attribute ) )
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
                    var targetClass = GetPropertyTargetClass( destinationProperty );
                    if( targetClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, FullNameOfType( attribute.targetClass ) ),
                            "Detour"
                        );
                        continue;
                    }
                    var sourceProperty = GetTimedDetouredPropertyInt( targetClass, attribute.targetMember, destinationProperty );
                    if( sourceProperty == null )
                    {   // Report and ignore any missing properties
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, targetClass.FullName, attribute.targetMember ),
                            "Detour"
                        );
                        continue;
                    }
                    var destinationMethod = destinationProperty.GetGetMethod( true );
                    if( destinationMethod != null )
                    {   // Check for get method detour
                        var sourceMethod = sourceProperty.GetGetMethod( true );
                        if( sourceMethod == null )
                        {   // Report and ignore missing get method
                            CCL_Log.Trace(
                                Verbosity.Injections,
                                string.Format( "TargetMember '{2}.{3}' has no get method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, targetClass.FullName, attribute.targetMember ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for get method
                            detours.Add( new DetourPair( targetClass, sourceMethod, destinationMethod ) );
                        }
                    }
                    destinationMethod = destinationProperty.GetSetMethod( true );
                    if( destinationMethod != null )
                    {   // Check for set method detour
                        var sourceMethod = sourceProperty.GetSetMethod( true );
                        if( sourceMethod == null )
                        {   // Report and ignore missing set method
                            CCL_Log.Trace(
                                Verbosity.Injections,
                                string.Format( "TargetMember '{2}.{3}' has no set method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, targetClass.FullName, attribute.targetMember ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for set method
                            detours.Add( new DetourPair( targetClass, sourceMethod, destinationMethod ) );
                        }
                    }
                }
            }
        }

        private static string               FullNameOfType( Type type )
        {
            return type == null ? "null" : type.FullName;
        }

        #endregion

        #region Debug Methods

#if DEBUG
        private static bool                 DetourTargetsAreValid( Type targetA, MethodType typeA, string nameA, Type targetB, MethodType typeB, string nameB, out string reason )
        {
            if(
                ( typeA == MethodType.Instance )||
                ( typeA == MethodType.Extension )
            )
            {
                if( typeB == MethodType.Static )
                {
                    reason = string.Format(
                        "'{0}' is static but not an extension method",
                        nameB
                    );
                    return false;
                }
                else if( targetA != targetB )
                {
                    reason = string.Format(
                        "Target classes do not match :: '{0}' target is '{1}'; '{2}' target is '{3}'",
                        nameA,
                        FullNameOfType( targetA ),
                        nameB,
                        FullNameOfType( targetB )
                    );
                    return false;
                }
            }
            reason = string.Empty;
            return true;
        }

        private static bool                 MethodsAreCallCompatible( Type targetClass, MethodInfo sourceMethod, MethodInfo destinationMethod, out string reason )
        {
            reason = string.Empty;
            if( sourceMethod.ReturnType != destinationMethod.ReturnType )
            {   // Return types don't match
                reason = string.Format(
                    "Return type mismatch :: Source={1}, Destination={0}",
                    sourceMethod.ReturnType.Name,
                    destinationMethod.ReturnType.Name
                );
                return false;
            }

            // Get the method types
            var sourceMethodType      = GetMethodType( sourceMethod );
            var destinationMethodType = GetMethodType( destinationMethod );

            // Do basic parameter matching
            if(
                ( sourceMethodType == destinationMethodType )&&
                ( sourceMethod.GetParameters().Select( sourceParameter => sourceParameter.ParameterType ).SequenceEqual( destinationMethod.GetParameters().Select( destinationParameter => destinationParameter.ParameterType ) ) )
            )
            {   // This will catch 99% of detours
                return true;
            }

            // The other 1% needs to do some deeper analysis

            // Make sure neither method is invalid
            if( sourceMethodType == MethodType.Invalid )
            {
                reason = "Source method is not an instance, valid extension, or static method";
                return false;
            }
            if( destinationMethodType == MethodType.Invalid )
            {
                reason = "Destination method is not an instance, valid extension, or static method";
                return false;
            }

            // Get the actual target classes
            var sourceTargetClass      = GetMethodTargetClass( sourceMethod );
            var destinationTargetClass = GetMethodTargetClass( destinationMethod );

            // Check validity of target classes
            if( !DetourTargetsAreValid(
                    sourceTargetClass     , sourceMethodType     , FullMethodName( sourceMethod ),
                    destinationTargetClass, destinationMethodType, FullMethodName( destinationMethod ),
                    out reason ) )
            {
                return false;
            }

            // Method types and targets are all valid, now check the parameter lists
            var sourceParamsBase      = sourceMethod.GetParameters();
            var destinationParamsBase = destinationMethod.GetParameters();

            // Get the first parameter index that isn't "this"
            var sourceParamBaseIndex      = sourceMethodType      == MethodType.Extension ? 1 : 0;
            var destinationParamBaseIndex = destinationMethodType == MethodType.Extension ? 1 : 0;

            // Parameter counts less "this"
            var sourceParamCount      = sourceParamsBase     .Length - sourceParamBaseIndex;
            var destinationParamCount = destinationParamsBase.Length - destinationParamBaseIndex;

            // Easy check that they have the same number of parameters
            if( sourceParamCount != destinationParamCount )
            {
                reason = "Parameter count mismatch";
                return false;
            }

            // Pick smaller parameter count (to skip "this")
            var paramCount = sourceParamCount > destinationParamCount ? destinationParamCount : sourceParamCount;

            // Now examine parameter-for-parameter
            if( paramCount > 0 )
            {
                for( var offset = 0; offset < paramCount; offset++ )
                {
                    // Get parameter
                    var sourceParam      = sourceParamsBase     [ sourceParamBaseIndex      + offset ];
                    var destinationParam = destinationParamsBase[ destinationParamBaseIndex + offset ];

                    // Parameter types and attributes are all we care about
                    if(
                        ( sourceParam.ParameterType != destinationParam.ParameterType )||
                        ( sourceParam.Attributes    != destinationParam.Attributes )
                    )
                    {   // Parameter type mismatch
                        reason = string.Format(
                            "Parameter type mismatch at index {6} :: Source='{0}', type='{1}', attributes='{2}'; Destination='{3}', type='{4}', attributes='{5}'",
                            sourceParam     .Name, sourceParam     .ParameterType.FullName, sourceParam     .Attributes,
                            destinationParam.Name, destinationParam.ParameterType.FullName, destinationParam.Attributes,
                            offset
                        );
                        return false;
                    }
                }
            }

            // Methods are call compatible!
            return true;
        }
#endif

        #endregion

    }

}
