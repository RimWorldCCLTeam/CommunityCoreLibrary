/*
    The basic implementation of the IL method 'hooks' (detours) made possible by RawCode's work (32-bit);
    https://ludeon.com/forums/index.php?topic=17143.0

    Additional implementation features (64-bit, error checking, method gathering, method validation, etc)
    are coded by and based on research done by 1000101.

    Method parameter list matching for initial gathering purposes supplied by Zhentar.

    Performs detours, spits out basic logs and warns if a method is detoured multiple times.

    Remember when stealing...err...copying free code to make sure the proper people get proper credit.
*/

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

    /// <summary>
    /// This is a helper class for batch processing of detours.
    /// </summary>
    public class DetourPair
    {
        public Type                         sourceMethodTarget;
        public MethodInfo                   sourceMethod;
        public Type                         destinationMethodTarget;
        public MethodInfo                   destinationMethod;

        public                              DetourPair( Type sourceMethodTarget, MethodInfo sourceMethod, Type destinationMethodTarget, MethodInfo destinationMethod )
        {
            this.sourceMethodTarget         = sourceMethodTarget;
            this.sourceMethod               = sourceMethod;
            this.destinationMethodTarget    = destinationMethodTarget;
            this.destinationMethod          = destinationMethod;
        }

    }

    public static class Detours
    {

        private static Dictionary<string,string>    detouredMethods = new Dictionary<string, string>();

        private static Dictionary<Assembly,List<Type>> assemblyDetourTypes = new Dictionary<Assembly, List<Type>>();

        #region Public API

        /// <summary>
        /// Tries to detour one method to another.  Both methods must have matching return type and parameters.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="sourceMethod">Source method to detour from</param>
        /// <param name="destinationMethod">Destination method to detour to</param>
        public static bool                  TryDetourFromTo( MethodInfo sourceMethod, MethodInfo destinationMethod )
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

            // Used for deeper method checks to return failure string
            var reason = string.Empty;

            // Make sure the class containing the detour doesn't contain instance fields
            if( !DetourContainerClassIsFieldSafe( destinationMethod.DeclaringType ) )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format(
                        "'{0}' contains fields which are not static!  Detours can not be defined in classes which have instance fields!",
                        FullNameOfType( destinationMethod.DeclaringType )
                    ),
                    "Detour"
                );
                return false;
            }

            // Make sure the two methods are call compatible
            if( !MethodsAreCallCompatible( GetMethodTargetClass( sourceMethod ), sourceMethod, GetMethodTargetClass( destinationMethod ), destinationMethod, out reason ) )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Methods are not call compatible when trying to detour '{0}' to '{1}' :: {2}", FullMethodName( sourceMethod ), FullMethodName( destinationMethod ), reason ),
                    "Detour"
                );
                return false;
            }
#endif

            TryDetourFromToInt( sourceMethod, destinationMethod );

            // Method is now detoured, we are doneski!
            return true;
        }

        /// <summary>
        /// Process a single detour pair.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="pair">DetourPair to process</param>
        public static bool                  TryDetourFromTo( DetourPair pair )
        {
#if DEBUG
            // Make sure the two methods are call compatible
            var reason = string.Empty;
            if( !PairIsCallCompatible( pair, out reason ) )
            {
                CCL_Log.Trace(
                    Verbosity.NonFatalErrors,
                    string.Format( "Methods are not call compatible when trying to detour '{0}' to '{1}' :: {2}", FullMethodName( pair.sourceMethod ), FullMethodName( pair.destinationMethod ), reason ),
                    "Detour"
                );
                return false;
            }
#endif

            TryDetourFromToInt( pair.sourceMethod, pair.destinationMethod );

            // Method is now detoured, we are doneski!
            return true;
        }

        /// <summary>
        /// Takes and processes an entire list of detours as a batch.
        /// </summary>
        /// <returns>True on success; False on failure (with log message)</returns>
        /// <param name="detours">List of DetourPairs to process</param>
        public static bool                  TryDetourFromTo( List<DetourPair> detours )
        {
#if DEBUG
            // These checks should never cause failure.  If it does they the
            // gather method is broken as it should never add a null to the list.
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
#endif
            var rVal = true;
            foreach( var detour in detours )
            {
                rVal &= TryDetourFromTo( detour );
            }
            return rVal;
        }

        /// <summary>
        /// Try to detour methods in an assembly with matching sequence and timing parameters.
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
        /// Gets a list detours in an assembly with matching sequence and timing parameters.
        /// </summary>
        /// <returns>The list of detours with matching sequence and timing</returns>
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

        /// <summary>
        /// This method performs the actual detour.  This method is destructive (5-byte on
        /// 32-bit systems, 12-byte on 64-bit systems) to the sourceMethod as it overwrites
        /// the machine code with new machine code.
        /// USE THE PUBLIC API WHICH WILL VALIDATE DETOURS BEFORE EXECUTING THEM!
        /// DO NOT CALL THIS METHOD DIRECTLY!
        /// DO NOT DETOUR METHODS IF YOU DON'T FULLY UNDERSTAND EVERY ASPECT OF WHAT IS HAPPENING, WHY, HOW, AND CONSEQUENCES OF DOING SO!
        /// THIS IS NOT FOR SCRUBS/NEWBS/BEGINNERS!
        /// THIS IS FOR ADVANCED CODERS ONLY!
        /// </summary>
        /// <param name="sourceMethod">Source method to overwrite with a jump to the destination method</param>
        /// <param name="destinationMethod">Destination method to target for the jump from the source method</param>
        private static unsafe void          TryDetourFromToInt( MethodInfo sourceMethod, MethodInfo destinationMethod )
        {
            // Create strings with the fullname and address of the methods
            var sourceFullDescription      = FullMethodName( sourceMethod     , true );
            var destinationFullDescription = FullMethodName( destinationMethod, true );

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

        /// <summary>
        /// This classification is used to validate detours and to get the appropriate
        /// operating context of methods.
        /// A method should never be classed as "invalid", this classification only happens
        /// when a method has the "ExtensionAttribute" but no parameters and therefore no
        /// "this" parameter.
        /// </summary>
        private enum MethodType
        {
            Invalid,
            Instance,
            Extension,
            Static
        }

        /// <summary>
        /// Return the full class and method name with optional method address
        /// </summary>
        /// <returns>Full class and name</returns>
        /// <param name="methodInfo">MethodInfo of method</param>
        /// <param name="withAddress">Optional bool flag to add the address</param>
        private static string               FullMethodName( MethodInfo methodInfo, bool withAddress = false )
        {
            var rVal = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
            if( withAddress )
            {
                rVal += " @ 0x" + methodInfo.MethodHandle.GetFunctionPointer().ToString( "X" + ( IntPtr.Size *  2 ).ToString() );
            }
            return rVal;
        }

        /// <summary>
        /// null safe method to get the full name of a type for debugging
        /// </summary>
        /// <returns>The name of type or "null"</returns>
        /// <param name="type">Type to get the full name of</param>
        private static string               FullNameOfType( Type type )
        {
            return type == null ? "null" : type.FullName;
        }

        /// <summary>
        /// Trims underscores one at a time from the begining of a member name
        /// </summary>
        /// <returns>The fixed member name</returns>
        /// <param name="memberName">Member name</param>
        private static string               GetFixedMemberName( string memberName )
        {
            if( memberName[ 0 ] == "_"[ 0 ] )
            {
                return memberName.Substring( 1, memberName.Length - 1 );
            }
            return memberName;
        }

        /// <summary>
        /// Return the type of method from the MethodInfo
        /// </summary>
        /// <returns>MethodType of method</returns>
        /// <param name="methodInfo">MethodInfo of method</param>
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

        /// <summary>
        /// Gets the class that the method will operate in the context of.
        /// This is NOT necessarily the class that the method exists in.
        /// For extension methods the "this" parameter (first) will be returned
        /// regardless of whether it is a detour or not, pure static methods
        /// will return null (again, regardless of being a detour) as they
        /// don't operate in the context of class.  Instance methods will
        /// return the defining class for non-detours and the class being
        /// injected into for detours.
        /// </summary>
        /// <returns>The method target class</returns>
        /// <param name="info">MethodInfo of the method to check</param>
        /// <param name="attribute">DetourMember attribute</param>
        private static Type                 GetMethodTargetClass( MethodInfo info, DetourMember attribute = null )
        {
#if _I_AM_A_POTATO_
            var fullMethodName = FullMethodName( info );
#endif

            var methodType = GetMethodType( info );

            if( methodType == MethodType.Static )
            {   // Pure static methods don't have a target class
#if _I_AM_A_POTATO_
                CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: Static = null", fullMethodName ), "Detour" );
#endif
                return null;
            }
            if( methodType == MethodType.Extension )
            {   // Regardless of whether this is the detour method or the method to be detoured, for extension methods we take the target class from the first parameter
#if _I_AM_A_POTATO_
                CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: Parameters[ 0 ].ParameterType = '{1}'", fullMethodName, FullNameOfType( info.GetParameters()[ 0 ].ParameterType ) ), "Detour" );
#endif
                return info.GetParameters()[ 0 ].ParameterType;
            }

            if( attribute == null )
            {
                info.TryGetAttribute( out attribute );
            }
            if( attribute != null )
            {
                if( attribute.targetClass != DetourMember.DefaultTargetClass )
                {
#if _I_AM_A_POTATO_
                    CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: TargetClass = '{1}'", fullMethodName, FullNameOfType( attribute.targetClass ) ), "Detour" );
#endif
                    return attribute.targetClass;
                }
#if _I_AM_A_POTATO_
                CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: DeclaringType.BaseType = '{1}'", fullMethodName, FullNameOfType( info.DeclaringType.BaseType ) ), "Detour" );
#endif
                return info.DeclaringType.BaseType;
            }

#if _I_AM_A_POTATO_
            CCL_Log.Message( string.Format( "GetMethodTargetClass( {0} ) :: DeclaringType = '{1}'", fullMethodName, FullNameOfType( info.DeclaringType ) ), "Detour" );
#endif
            return info.DeclaringType;
        }

        /// <summary>
        /// Gets the class that a detour will be injected into.
        /// </summary>
        /// <returns>The detour target class</returns>
        /// <param name="info">MemberInfo of the member to check</param>
        /// <param name="attribute">DetourMember attribute</param>
        private static Type                 GetDetourTargetClass( MemberInfo info, DetourMember attribute )
        {
            if( attribute != null )
            {
                if( attribute.targetClass != DetourMember.DefaultTargetClass )
                {
                    return attribute.targetClass;
                }
                var methodInfo = info as MethodInfo;
                if(
                    ( info.DeclaringType.BaseType == typeof( System.Object ) )&&
                    ( methodInfo != null )&&
                    ( GetMethodType( methodInfo ) == MethodType.Extension )
                )
                {
                    return methodInfo.GetParameters()[ 0 ].ParameterType;
                }
                return info.DeclaringType.BaseType;
            }

            return info.DeclaringType;
        }

        /// <summary>
        /// Returns a list of timed detour methods with matching sequence and timing from a class.
        /// </summary>
        /// <param name="detours">List to add the detours from this class to</param>
        /// <param name="destinationType">The class to check for detour methods</param>
        /// <param name="sequence">Injection sequence</param>
        /// <param name="timing">Injection timing</param>
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
                    var memberClass = GetDetourTargetClass( destinationMethod, attribute );
                    if( memberClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "MemberClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, FullNameOfType( attribute.targetClass ) ),
                            "Detour"
                        );
                        continue;
                    }
                    var sourceMethod = GetTimedDetouredMethodInt( memberClass, attribute.targetMember, destinationMethod );
                    if( sourceMethod == null )
                    {   // Report and ignore any missing methods
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationMethod.Name, memberClass.FullName, attribute.targetMember ),
                            "Detour"
                        );
                        continue;
                    }
                    // Add detour for method
                    detours.Add( new DetourPair( GetMethodTargetClass( sourceMethod, null ), sourceMethod, GetMethodTargetClass( destinationMethod, attribute ), destinationMethod ) );
                }
            }
        }

        /// <summary>
        /// Returns a list of timed detour property methods (get/set) with matching sequence and timing from a class.
        /// </summary>
        /// <param name="detours">List to add the detours from this class to</param>
        /// <param name="destinationType">The class to check for detour properties</param>
        /// <param name="sequence">Injection sequence</param>
        /// <param name="timing">Injection timing</param>
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
                    var memberClass = GetDetourTargetClass( destinationProperty, attribute );
                    if( memberClass == null )
                    {   // Report and ignore any missing classes
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "MemberClass '{2}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, FullNameOfType( attribute.targetClass ) ),
                            "Detour"
                        );
                        continue;
                    }
                    var sourceProperty = GetTimedDetouredPropertyInt( memberClass, attribute.targetMember, destinationProperty );
                    if( sourceProperty == null )
                    {   // Report and ignore any missing properties
                        CCL_Log.Trace(
                            Verbosity.Injections,
                            string.Format( "TargetMember '{2}.{3}' resolved to null for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember ),
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
                                string.Format( "TargetMember '{2}.{3}' has no get method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for get method
                            detours.Add( new DetourPair( GetMethodTargetClass( sourceMethod, null ), sourceMethod, GetMethodTargetClass( destinationMethod, attribute ), destinationMethod ) );
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
                                string.Format( "TargetMember '{2}.{3}' has no set method for '{0}.{1}'", destinationType.FullName, destinationProperty.Name, memberClass.FullName, attribute.targetMember ),
                                "Detour"
                            );
                        }
                        else
                        {   // Add detour for set method
                            detours.Add( new DetourPair( GetMethodTargetClass( sourceMethod, null ), sourceMethod, GetMethodTargetClass( destinationMethod, attribute ), destinationMethod ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the specific method that is detoured.
        /// </summary>
        /// <returns>The MethodInfo of the method to be detoured or null on failure</returns>
        /// <param name="sourceClass">Class that contains the expected method to be detoured</param>
        /// <param name="sourceMember">Name of the expected method to be detoured</param>
        /// <param name="destinationMethod">MethodInfo of the detour</param>
        private static MethodInfo           GetTimedDetouredMethodInt( Type sourceClass, string sourceMember, MethodInfo destinationMethod )
        {
            if( sourceMember == DetourMember.DefaultTargetMemberName )
            {
                sourceMember = destinationMethod.Name;
            }
            MethodInfo sourceMethod = null;
            try
            {   // Try to get method direct
                sourceMethod = sourceClass.GetMethod( sourceMember, Controller.Data.UniversalBindingFlags );
            }
            catch
            {   // May be ambiguous, try from parameter types (thanks Zhentar for the change from count to types)
                sourceMethod = sourceClass.GetMethods( Controller.Data.UniversalBindingFlags )
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
                return GetTimedDetouredMethodInt( sourceClass, fixedName, destinationMethod );
            }
            return sourceMethod;
        }

        /// <summary>
        /// Gets the specific property that is detoured.
        /// </summary>
        /// <returns>The PropertyInfo of the method to be detoured or null on failure</returns>
        /// <param name="sourceClass">Class that contains the expected property to be detoured</param>
        /// <param name="sourceMember">Name of the expected property to be detoured</param>
        /// <param name="destinationProperty">PropertyInfo of the detour</param>
        private static PropertyInfo         GetTimedDetouredPropertyInt( Type sourceClass, string sourceMember, PropertyInfo destinationProperty )
        {
            if( sourceMember == DetourMember.DefaultTargetMemberName )
            {
                sourceMember = destinationProperty.Name;
            }
            var sourceProperty = sourceClass.GetProperty( sourceMember, Controller.Data.UniversalBindingFlags );
            var fixedName = GetFixedMemberName( sourceMember );
            if(
                ( sourceProperty == null )&&
                ( sourceMember != fixedName )
            )
            {
                return GetTimedDetouredPropertyInt( sourceClass, fixedName, destinationProperty );
            }
            return sourceProperty;
        }

        #endregion

        #region Debug Methods

#if DEBUG
        /// <summary>
        /// Checks that the class containing a detour does not contain instance fields.
        /// </summary>
        /// <returns>True if there are no instance fields; False if any instance field is contained in the class</returns>
        /// <param name="detourContainerClass">Detour container class</param>
        private static bool                 DetourContainerClassIsFieldSafe( Type detourContainerClass )
        {
            var fields = detourContainerClass.GetFields( Controller.Data.UniversalBindingFlags );
            if( fields.NullOrEmpty() )
            {   // No fields, no worries
                return true;
            }

            // Check that each field is static
            foreach( var field in fields )
            {
                if( !field.IsStatic )
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks that B is a valid detour for A based on method types and class context (targets)
        /// </summary>
        /// <returns>True if B is a valid detour for A; False and reason string set otherwise</returns>
        /// <param name="targetA">Target class of A</param>
        /// <param name="typeA">MethodType of A</param>
        /// <param name="nameA">Method name of A</param>
        /// <param name="targetB">Target class of B</param>
        /// <param name="typeB">MethodType of B</param>
        /// <param name="nameB">Method name of B</param>
        /// <param name="reason">Return string with reason for failure</param>
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

        /// <summary>
        /// Validates that a DetourPair is call compatible
        /// </summary>
        /// <returns>True if the pair is call compatible; False and reason string set otherwise</returns>
        /// <param name="pair">DetourPair to check</param>
        /// <param name="reason">Return string with reason for failure</param>
        private static bool                 PairIsCallCompatible( DetourPair pair, out string reason )
        {
            return MethodsAreCallCompatible(
                pair.sourceMethodTarget,
                pair.sourceMethod,
                pair.destinationMethodTarget,
                pair.destinationMethod,
                out reason );
        }

        /// <summary>
        /// Validates that two methods are call compatible
        /// </summary>
        /// <returns>True if the methods are call compatible; False and reason string set otherwise</returns>
        /// <param name="sourceTargetClass">Source method target class</param>
        /// <param name="sourceMethod">Source method</param>
        /// <param name="destinationTargetClass">Destination method target class</param>
        /// <param name="destinationMethod">Destination method</param>
        /// <param name="reason">Return string with reason for failure</param>
        private static bool                 MethodsAreCallCompatible( Type sourceTargetClass, MethodInfo sourceMethod, Type destinationTargetClass, MethodInfo destinationMethod, out string reason )
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
