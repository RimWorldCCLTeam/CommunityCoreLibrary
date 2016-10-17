using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{
    /// <summary>
    /// Use the SpecialInjectorSequencer class attribute to set execution timing.
    /// eg:
    /// [SpecialInjectorSequencer( InjectionSequence.MainLoad, InjectionTiming.SpecialInjectors )]
    /// </summary>
    public abstract class                   SpecialInjector
    {

        private static Dictionary<Assembly,List<Type>> assemblyInjectors = new Dictionary<Assembly, List<Type>>();

        /// <summary>
        /// Required method, performs actual injection (runs user code) at the appropriate sequence and timing
        /// </summary>
        public abstract bool                Inject();

        /// <summary>
        /// Optional def re-resolver for injectors that need it, called once at the end of the appropriate sequence
        /// </summary>
        public virtual void                 ReResolveDefs()
        {
        }

        public static bool                  TryInject( SpecialInjector injector )
        {
            try
            {
                if( !injector.Inject() )
                {
                    CCL_Log.Message( string.Format( "Error injecting '{0}'", injector.GetType().ToString() ) );
                    return false;
                }
            }
            catch( Exception e )
            {
                CCL_Log.Message( e.ToString(), string.Format( "Exception injecting '{0}'", injector.GetType().ToString() ) );
                return false;
            }
            CCL_Log.Trace(
                Verbosity.Injections,
                string.Format( "{0} :: Injected", injector.GetType().FullName ),
                "Special Injector"
            );
            return true;
        }

        public static bool                  TryInject( List<SpecialInjector> injectors )
        {
            if( injectors.NullOrEmpty() )
            {
                return true;
            }
#if DEBUG
            foreach( var injector in injectors )
            {
                if( injector == null )
                {
                    CCL_Log.Error(
                        "List of Special Injectors contains a null while trying to inject!",
                        "Special Injectors"
                    );
                    return false;
                }
            }
#endif
            foreach( var injector in injectors )
            {
                if( !TryInject( injector ) )
                {
                    return false;
                }
            }
            return true;
        }

        public static  bool                 TryTimedAssemblySpecialInjectors( Assembly assembly, InjectionSequence sequence, InjectionTiming timing )
        {
            var specialInjectors = GetTimedInjectors( assembly, sequence, timing );
            if( !specialInjectors.NullOrEmpty() )
            {
                if( !TryInject( specialInjectors ) )
                {
                    return false;
                }
            }
            return true;
        }

        public static bool                  TryReResolveDefs( SpecialInjector injector )
        {
            try
            {
                injector.ReResolveDefs();
                return true;
            }
            catch( Exception e )
            {
                CCL_Log.Error(
                    string.Format(
                        "'{0}' threw an exception while trying to re-resolve defs!\n{1}",
                        injector.GetType().FullName,
                        e.ToString() ),
                    "Special Injectors"
                );
            }
            return false;
        }

        public static List<SpecialInjector> GetTimedInjectors( Assembly toAssembly, InjectionSequence sequence, InjectionTiming timing )
        {
            // Get SpecialInjector types from assembly with matching timing
            List<Type> injectorTypes = null;

            // First try to get an already built list from the cache
            if( !assemblyInjectors.TryGetValue( toAssembly, out injectorTypes ) )
            {
                // Assembly isn't in the cache, build the list of types with special injectors
                injectorTypes = toAssembly
                    .GetTypes()
                    .Where( toType => toType.HasAttribute<SpecialInjectorSequencer>() )
                    .ToList();
                // Add the type list to the assembly cache
                assemblyInjectors.Add( toAssembly, injectorTypes );
            }

            // No special injectors, return null
            if( injectorTypes.NullOrEmpty() )
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

            // Create return list for the injectors
            var injectors = new List<SpecialInjector>();

            // Create the special injectors
            foreach( var injectorType in injectorTypes )
            {
                SpecialInjectorSequencer attribute = null;
                if( injectorType.TryGetAttribute<SpecialInjectorSequencer>( out attribute ) )
                {
                    if(
                        ( attribute.injectionSequence != sequence )||
                        (
                            ( timing != InjectionTiming.All )&&
                            ( attribute.injectionTiming != timing )
                        )
                    )
                    {   // Ignore any special injectors which timing doesn't match
                        continue;
                    }
                    var injectorObject = (SpecialInjector) Activator.CreateInstance( injectorType );
                    if( injectorObject == null )
                    {
                        CCL_Log.Message( string.Format( "Unable to create instance of '{0}'", injectorType.ToString() ) );
                        continue;
                    }
                    injectors.Add( injectorObject );
                }
            }

            // Return the list for this sequence
            return injectors;
        }

    }

}
