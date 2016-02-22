using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    // TODO:  Alpha 13 API change
    /*
    public abstract class                   SpecialInjector
    {

        public abstract bool                Inject();

    }
    */
    public class                            SpecialInjector
    {

        public                              SpecialInjector ()
        {
        }

        public virtual void                 Inject()
        {
        }

    }

    public class ModHelperDef : Def
    {

        #region XML Data

        public string                       ModName;

        public string                       version;

        public List< Type >                 MapComponents;

        public List< DesignatorData >       Designators;

        public List< CompInjectionSet >     ThingComps;

        public List< Type >                 SpecialInjectors;

        public List< Type >                 PostLoadInjectors;

        public bool                         UsesGenericHoppers = false;
        public bool                         HideVanillaHoppers = false;

        public Verbosity                    Verbosity = Verbosity.Default;

        #endregion

        [Unsaved]

        #region Instance Data

        // Used to flag xml defined (false) and auto-generated (true) for logging
        public bool                         dummy = false;

        public LoadedMod                    mod;

        bool                                specialsInjected;
        bool                                postLoadersInjected;

        #endregion

        #region Validation

        public bool                         IsValid
        {
            get
            {
                var isValid = true;
                var errors = "";

                if( ModName.NullOrEmpty() )
                {
                    errors += "\n\tMissing ModName";
                    isValid = false;
                }

                if( version.NullOrEmpty() )
                {
                    errors += "\n\tNull or empty CCL version requirement";
                    isValid = false;
                }
                else
                {
                    var vc = Version.Compare( version );
                    switch( vc )
                    {
                    case Version.VersionCompare.LessThanMin:
                        errors += "\n\tUnsupported CCL version requirement (v" + version + ") minimum supported version is v" + Version.Minimum;
                        isValid = false;
                        break;
                    case Version.VersionCompare.GreaterThanMax:
                        errors += "\n\tUnsupported CCL version requirement (v" + version + ") maximum supported version is v" + Version.Current;
                        isValid = false;
                        break;
                    case Version.VersionCompare.Invalid:
                        errors += "\n\tUnable to get version from '" + version + "'";
                        isValid = false;
                        break;
                    }
                }

#if DEBUG
                if( !MapComponents.NullOrEmpty() )
                {
                    foreach( var componentType in MapComponents )
                    {
                        //var componentType = Type.GetType( component );
                        if(
                            ( componentType == null ) ||
                            ( !componentType.IsSubclassOf( typeof( MapComponent ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve MapComponent '" + componentType.ToString() + "'";
                            isValid = false;
                        }
                    }
                }

                if( !Designators.NullOrEmpty() )
                {
                    foreach( var data in Designators )
                    {
                        var designatorType = data.designatorClass;
                        if(
                            ( designatorType == null )||
                            ( !designatorType.IsSubclassOf( typeof( Designator ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve designatorClass '" + data.designatorClass + "'";
                            isValid = false;
                        }
                        if(
                            ( string.IsNullOrEmpty( data.designationCategoryDef ) )||
                            ( DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false ) == null )
                        )
                        {
                            errors += "\n\tUnable to resolve designationCategoryDef '" + data.designationCategoryDef + "'";
                            isValid = false;
                        }
                        if(
                            ( data.designatorNextTo != null )&&
                            ( !data.designatorNextTo.IsSubclassOf( typeof( Designator ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve designatorNextTo '" + data.designatorNextTo + "'";
                            isValid = false;
                        }
                    }
                }

                if( !ThingComps.NullOrEmpty() )
                {
                    foreach( var compSet in ThingComps )
                    {
                        if( compSet.targetDefs.NullOrEmpty() )
                        {
                            errors += "\n\tNull or no targetDefs in ThingComps";
                            isValid = false;
                        }
                        /*
                        var targDef = ThingDef.Named( comp.targetDef );
                        if( targDef == null )
                        {
                            errors += "\n\tUnable to find ThingDef named '" + comp.targetDef + "\" in ThingComps";
                            isValid = false;
                        }*/
                        if( compSet.compProps == null )
                        {
                            errors += "\n\tNull compProps in ThingComps";
                            isValid = false;
                        }
                        foreach( var targetDef in compSet.targetDefs.Where( targetDef => (
                            ( string.IsNullOrEmpty( targetDef ) )||
                            ( DefDatabase< ThingDef >.GetNamed( targetDef, false ) == null )
                        ) ) )
                        {
                            errors += "\n\tUnable to resolve ThingDef '" + targetDef + "'";
                            isValid = false;
                        }
                    }
                }

                if( !SpecialInjectors.NullOrEmpty() )
                {
                    foreach( var injectorType in SpecialInjectors )
                    {
                        if(
                            ( injectorType == null )||
                            ( !injectorType.IsSubclassOf( typeof( SpecialInjector ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve SpecialInjector '" + injectorType.ToString() + "'";
                            isValid = false;
                        }
                    }
                }

                if( !PostLoadInjectors.NullOrEmpty() )
                {
                    foreach( var injectorType in PostLoadInjectors )
                    {
                        if(
                            ( injectorType == null )||
                            ( !injectorType.IsSubclassOf( typeof( SpecialInjector ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve PostLoadInjector '" + injectorType.ToString() + "'";
                            isValid = false;
                        }
                    }
                }
#endif

                if( !isValid )
                {
                    var builder = new StringBuilder();
                    builder.Append( "ModHelperDef :: " ).Append( defName );
                    if( !ModName.NullOrEmpty() )
                    {
                        builder.Append( " :: " ).Append( ModName );
                    }
                    CCL_Log.Error( errors, builder.ToString() );
                }

                return isValid;
            }
        }

        #endregion

        #region Query State

        public bool                         MapComponentsInjected
        {
            get
            {
                if( MapComponents.NullOrEmpty() )
                {
                    return true;
                }

                var colonyMapComponents = Find.Map.components;

                foreach( var mapComponentType in MapComponents )
                {
                    //var mapComponentType = Type.GetType( mapComponent );
                    if( !colonyMapComponents.Exists(c => c.GetType() == mapComponentType ) )
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool                         DesignatorsInjected
        {
            get
            {
                if( Designators.NullOrEmpty() )
                {
                    // No designators to inject
                    return true;
                }

                foreach( var data in Designators )
                {
                    var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false );
                    if( !designationCategory.resolvedDesignators.Exists( d => d.GetType() == data.designatorClass ) )
                    {
                        // This designator hasn't been injected yet
                        return false;
                    }
                }

                // All designators injected
                return true;
            }
        }

        public bool                         ThingCompsInjected
        {
            get
            {
                if( ThingComps.NullOrEmpty() )
                {
                    return true;
                }

                foreach( var compSet in ThingComps )
                {
                    foreach( var targetName in compSet.targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        if( targetDef != null && !targetDef.comps.Exists(s => s.compClass == compSet.compProps.compClass) )
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public bool                         SpecialsInjected
        {
            get
            {
                if( SpecialInjectors.NullOrEmpty() )
                {
                    return true;
                }

                return specialsInjected;
            }
        }

        public bool                         PostLoadersInjected
        {
            get
            {
                if( PostLoadInjectors.NullOrEmpty() )
                {
                    return true;
                }

                return postLoadersInjected;
            }
        }

        #endregion

        #region Injection

        // TODO:  Alpha 13 API change
        //public bool                         InjectMapComponents()
        public void                         InjectMapComponents()
        {
            var colonyMapComponents = Find.Map.components;

            foreach( var mapComponentType in MapComponents )
            {
                // Get the component type
                //var mapComponentType = Type.GetType( mapComponent );

                // Does it exist in the map?
                if( !colonyMapComponents.Exists( c => c.GetType() == mapComponentType ) )
                {
                    // Create the new map component
                    var mapComponentObject = (MapComponent) Activator.CreateInstance( mapComponentType );

                    // Inject the component
                    colonyMapComponents.Add(mapComponentObject);
                }
            }

            // TODO:  Alpha 13 API change
            //return MapComponentsInjected;
        }

        // TODO:  Alpha 13 API change
        //public bool                         InjectDesignators()
        public void                         InjectDesignators()
        {
            // Instatiate designators and add them to the resolved list, also add the
            // the designator class to the list of designator classes as a saftey net
            // in case another mod resolves the designation categories which would
            // remove the instatiated designators.
            foreach( var data in Designators )
            {
                // Get the category
                var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false );

                // First instatiate and inject the designator into the list of resolved designators
                if( !designationCategory.resolvedDesignators.Exists( d => d.GetType() == data.designatorClass ) )
                {
                    // Create the new designator
                    var designatorObject = (Designator) Activator.CreateInstance( data.designatorClass );

                    if( data.designatorNextTo == null )
                    {
                        // Inject the designator
                        designationCategory.resolvedDesignators.Add( designatorObject );
                    }
                    else
                    {
                        // Prefers to be beside a specific designator
                        var designatorIndex = designationCategory.resolvedDesignators.FindIndex( d => (
                            ( d.GetType() == data.designatorNextTo )
                        ) );

                        if( designatorIndex < 0 )
                        {
                            // Other designator doesn't exist (yet?)
                            // Inject the designator at the end
                            designationCategory.resolvedDesignators.Add( designatorObject );
                        }
                        else
                        {
                            // Inject beside desired designator
                            designationCategory.resolvedDesignators.Insert( designatorIndex + 1, designatorObject );
                        }
                    }
                }

                // Now inject the designator class into the list of classes as a saftey net for another mod resolving the category
                if( !designationCategory.specialDesignatorClasses.Exists( s => s == data.designatorClass ) )
                {
                    if( data.designatorNextTo == null )
                    {
                        // Inject the designator class at the end of the list
                        designationCategory.specialDesignatorClasses.Add( data.designatorClass );
                    }
                    else
                    {
                        // Prefers to be beside a specific designator
                        var designatorIndex = designationCategory.specialDesignatorClasses.FindIndex( s => s == data.designatorNextTo );

                        if( designatorIndex < 0 )
                        {
                            // Can't find desired designator class
                            // Inject the designator at the end
                            designationCategory.specialDesignatorClasses.Add( data.designatorClass );
                        }
                        else
                        {
                            // Inject beside desired designator class
                            designationCategory.specialDesignatorClasses.Insert( designatorIndex + 1, data.designatorClass );
                        }
                    }
                }
            }

            // TODO:  Alpha 13 API change
            //return DesignatorsInjected;
        }

        // TODO:  Alpha 13 API change
        //public bool                         InjectThingComps()
        public void                         InjectThingComps()
        {
            foreach( var compSet in ThingComps )
            {
                var defsByName = DefDatabase<ThingDef>.AllDefs;

                foreach( var targetName in compSet.targetDefs )
                {
                    var def = defsByName.ToList().Find( s => s.defName == targetName );
                    var compProperties = compSet.compProps;
                    def.comps.Add( compProperties );
                }
            }

            // TODO:  Alpha 13 API change
            //return ThingCompsInjected;
        }

        // TODO:  Alpha 13 API change
        //public bool                         InjectSpecials()
        public void                         InjectSpecials()
        {
            foreach( var injectorType in SpecialInjectors )
            {
                var injectorObject = (SpecialInjector) Activator.CreateInstance( injectorType );
                injectorObject.Inject();
            }
            specialsInjected = true;

            // TODO:  Alpha 13 API change
            //return SpecialsInjected;
        }

        // TODO:  Alpha 13 API change
        //public bool                         InjectPostLoaders()
        public void                         InjectPostLoaders()
        {
            foreach( var injectorType in PostLoadInjectors )
            {
                var injectorObject = (SpecialInjector) Activator.CreateInstance( injectorType );
                injectorObject.Inject();
            }
            postLoadersInjected = true;

            // TODO:  Alpha 13 API change
            //return postLoadersInjected;
        }

        #endregion

    }

}
