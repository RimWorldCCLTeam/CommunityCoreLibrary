using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public abstract class SequencedInjectionSet
    {

        public enum InjectorTargetting
        {
            None,
            Single,
            Multi
        }

        // Injection timing
        public InjectionSequence            injectionSequence = InjectionSequence.Never;
        public InjectionTiming              injectionTiming = InjectionTiming.Never;

        // Mod requirement
        public string                       requiredMod;

        // Single target injectors
        public string                       targetDef;

        // Multi-target injectors
        public List< string >               targetDefs;
        public Type                         qualifier;
        [Unsaved]
        private DefInjectionQualifier       qualifierInt;

        // Internal flag
        [Unsaved]
        internal bool                       injectorIsValid = false;

        // Type of Defs for targetting
        public abstract Type                defType{ get; }
        public abstract InjectorTargetting  Targetting { get; }

        public DefInjectionQualifier        Qualifier
        {
            get
            {
                if( qualifier == null )
                {
                    /*  Not really an error, return null if 'qualifier' is null
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Tried to create a qualifier instance when no qualifier set",
                        Name );
                    */
                    return null;
                }
                if( qualifierInt == null )
                {
                    qualifierInt = (DefInjectionQualifier) Activator.CreateInstance( qualifier );
                    if( qualifierInt == null )
                    {
                        CCL_Log.Trace(
                            Verbosity.FatalErrors,
                            string.Format( "Unable to create DefInjectionQualifier instance of '{0}'", qualifier.FullName ),
                            Name );
                        return null;
                    }
                }
                return qualifierInt;
            }
        }

        public bool                         RequiredModIsLoaded()
        {
            if( string.IsNullOrEmpty( requiredMod ) )
            {
                return true;
            }
            return( Find_Extensions.ModByName( requiredMod ) != null );
        }

        public bool                         InjectNow( InjectionSequence sequence, InjectionTiming timing )
        {
            return(
                ( injectorIsValid )&&
                ( injectionSequence == sequence )&&
                ( injectionTiming == timing )&&
                ( RequiredModIsLoaded() )
            );
        }

        public List<T>                      AllTargets<T>() where T : Def
        {
            return AllTargets( defType ).ConvertAll( def => def as T );
        }

        public List<Def>                    AllTargets( Type defType )
        {
            List<Def> targets = null;
            Def target;
            switch( Targetting )
            {
            case InjectorTargetting.None:
                throw new NotImplementedException();
            
            case InjectorTargetting.Single:
                if( string.IsNullOrEmpty( targetDef ) )
                {
                    return null;
                }
                targets = new List<Def>();
                target = GenDefDatabase.GetDef( defType, targetDef, false );
                if( target != null )
                {
                    targets.Add( target );
                }
                break;
                
            case InjectorTargetting.Multi:
                if( !targetDefs.NullOrEmpty() )
                {
                    targets = new List<Def>();
                    foreach( var targetDefName in targetDefs )
                    {
                        target = GenDefDatabase.GetDef( defType, targetDefName, false );
                        if( target != null )
                        {
                            targets.Add( target );
                        }
                    }
                }
                if( Qualifier != null )
                {
                    var allDefsOfType = GenDefDatabase_Extensions.AllDefsListForReading( defType );
                    if( !allDefsOfType.NullOrEmpty() )
                    {
                        targets = new List<Def>();
                        for( var index = 0; index < allDefsOfType.Count; index++ )
                        {
                            target = allDefsOfType[ index ];
                            if( Qualifier.DefIsUsable( target ) )
                            {
                                targets.Add( target );
                            }
                        }
                    }
                }
                break;
            }
            return targets;
        }

        public virtual string               Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Checks the validity of all the injector specific data for injection.
        /// </summary>
        /// <returns>All the data is valid.</returns>
        public abstract bool                IsValid();

        /// <summary>
        /// Checks the validity of each target that will be injected into.
        /// Required if Targetting != InjectorTargetting.None
        /// </summary>
        /// <returns>All the data is valid.</returns>
        public virtual bool                 TargetIsValid( Def target )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks the validity of all the targets for injection.
        /// </summary>
        /// <returns>All the targets are valid.</returns>
        public bool                         AllTargetsAreValid()
        {
            var valid = true;
            switch( Targetting )
            {
            case InjectorTargetting.None:
                if( !string.IsNullOrEmpty( targetDef ) )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        "Has targetDef but is not a targetted injector",
                        Name
                    );
                    valid = false;
                }
                if( !targetDefs.NullOrEmpty() )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        "Has targetDefs but is not a targetted injector",
                        Name
                    );
                    valid = false;
                }
                if( qualifier != null )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        "Has a qualifier but is not a targetted injector",
                        Name
                    );
                    valid = false;
                }
                break;
                
            case InjectorTargetting.Single:
                if( string.IsNullOrEmpty( targetDef ) )
                {
                    CCL_Log.Trace(
                        Verbosity.Validation,
                        "Is missing targetDef",
                        Name
                    );
                    valid = false;
                }
                else
                {
                    var target = GenDefDatabase.GetDef( defType, targetDef, false );
                    if( target == null )
                    {
                        CCL_Log.Trace(
                            Verbosity.FatalErrors,
                            string.Format( "Unable to resolve targetDef '{0}'", targetDef ),
                            Name
                        );
                        valid = false;
                    }
                    else
                    {
                        valid &= TargetIsValid( target );
                    }
                }
                if( !targetDefs.NullOrEmpty() )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Has targetDefs but is a single target injector",
                        Name
                    );
                    valid = false;
                }
                if( qualifier != null )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Has a qualifier but is a single target injector",
                        Name
                    );
                    valid = false;
                }
                break;
            
            case InjectorTargetting.Multi:
                if( !string.IsNullOrEmpty( targetDef ) )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Has targetDef but is a multi-target injector",
                        Name
                    );
                    valid = false;
                }
                if(
                    ( targetDefs.NullOrEmpty() )&&
                    ( qualifier == null )
                )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Both targetDefs and qualifier are null",
                        Name
                    );
                    valid = false;
                }
                if(
                    ( !targetDefs.NullOrEmpty() )&&
                    ( qualifier != null )
                )
                {
                    CCL_Log.Trace(
                        Verbosity.FatalErrors,
                        "Both targetDefs and qualifier are set",
                        Name
                    );
                    valid = false;
                }
                if( valid )
                {
                    if( !targetDefs.NullOrEmpty() )
                    {
                        foreach( var targetName in targetDefs )
                        {
                            var target = GenDefDatabase.GetDef( defType, targetName, false );
                            if( target == null )
                            {
                                CCL_Log.Trace(
                                    Verbosity.FatalErrors,
                                    string.Format( "Unable to resolve target '{0}'", targetName ),
                                    Name
                                );
                                valid = false;
                            }
                        }
                    }
                    if(
                        ( qualifier != null )&&
                        ( !qualifier.IsSubclassOf( typeof( DefInjectionQualifier ) ) )
                    )
                    {
                        CCL_Log.Trace(
                            Verbosity.FatalErrors,
                            string.Format( "Unable to resolve qualifier '{0}'", qualifier.FullName ),
                            Name
                        );
                        valid = false;
                    }
                }
                break;
                
            }
            if( valid )
            {
                var targets = AllTargets( defType );
                if( !targets.NullOrEmpty() )
                {
                    foreach( var thingDef in targets )
                    {
                        valid &= TargetIsValid( thingDef );
                    }
                }
            }
            return valid;
        }

        /// <summary>
        /// Inject this instance.
        /// This method must be implemented if Targets == InjectorTargetting.None
        /// For targetted injectors this will method will call Inject( target ) for each usable target.
        /// </summary>
        /// <returns>true if successful</returns>
        public virtual bool                 Inject()
        {
#if DEBUG
            if( Targetting == InjectorTargetting.None )
            {
                throw new NotImplementedException();
            }
#endif
            var injected = true;
            var targets = AllTargets( defType );
            if( !targets.NullOrEmpty() )
            {
#if DEBUG
                var stringBuilder = new StringBuilder();
                stringBuilder.Append( "Qualifier returned: " );
#endif
                foreach( var target in targets )
                {
                    if( TargetIsValid( target ) )
                    {
#if DEBUG
                        stringBuilder.Append( target.defName + ", " );
#endif
                        injected &= Inject( target );
                    }
                }
#if DEBUG
                CCL_Log.Trace(
                    Verbosity.Injections,
                    stringBuilder.ToString(),
                    Name
                );
#endif
            }
            return injected;
        }

        /// <summary>
        /// Inject this target.
        /// This method must be implemented if Targets != InjectorTargetting.None
        /// This will be called once per individual target def.
        /// </summary>
        /// <returns>true if successful</returns>
        public virtual bool                 Inject( Def target )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method provides the injector the chance to do
        /// any final def resolutions which may need to be done.
        /// This method is called once per InjectionSequence per InjectionSet.
        /// This method itself must handle enumerating all of
        /// it's targets (see the AllTargets).
        /// It is not required to implement this method.
        /// </summary>
        /// <returns>true if successful</returns>
        public virtual bool                 ReResolveDefs()
        {
            return true;
        }

        /// <summary>
        /// This method provides the injector the chance to do injector data PostLoads()
        /// This method is called once when the ModHelperDef is loader per InjectionSet.
        /// This method itself must handle enumerating all of it's targets (see the AllTargets).
        /// It is not required to implement this method.
        /// </summary>
        public virtual void                 PostLoad()
        {
        }

    }

}
