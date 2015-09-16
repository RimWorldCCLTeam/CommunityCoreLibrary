using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{

    //public delegate void                    SpecialInjector();
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

        #endregion

        [Unsaved]

        #region Instance Data

        bool                                specialsInjected;

        #endregion

        #region Query State

        public bool                         IsValid
        {
            get
            {
                var isValid = true;
                var errors = "";

                try
                {
                    var modVersion = new Version( version );

                    if( modVersion < ModController.CCLVersionMin )
                    {
                        errors += "\n\tMinimum Version requirement: v" + modVersion;
                        isValid = false;
                    }

                    if ( modVersion > ModController.CCLVersion )
                    {
                        errors += "\n\tVersion requirement: v" + modVersion;
                        isValid = false;
                    }

                }
                catch
                {
                    errors += "\n\tUnable to get version from: '" + version + "'";
                    isValid = false;
                }

#if DEBUG
                if ( !MapComponents.NullOrEmpty() )
                {
                    foreach( var componentType in MapComponents )
                    {
                        //var componentType = Type.GetType( component );
                        if(
                            ( componentType == null ) ||
                            ( !componentType.IsSubclassOf( typeof( MapComponent ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve MapComponent \"" + componentType.ToString() + "\"";
                            isValid = false;
                        }
                    }
                }

                if ( !Designators.NullOrEmpty() )
                {
                    foreach ( var data in Designators )
                    {
                        var designatorType = data.designatorClass;
                        if(
                            ( designatorType == null )||
                            ( !designatorType.IsSubclassOf( typeof( Designator ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve designatorClass \"" + data.designatorClass + "\"";
                            isValid = false;
                        }
                        if(
                            ( string.IsNullOrEmpty( data.designationCategoryDef ) )||
                            ( DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false ) == null )
                        )
                        {
                            errors += "\n\tUnable to resolve designationCategoryDef \"" + data.designationCategoryDef + "\"";
                            isValid = false;
                        }
                        if(
                            ( data.designatorNextTo != null )&&
                            ( !data.designatorNextTo.IsSubclassOf( typeof( Designator ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve designatorNextTo \"" + data.designatorNextTo + "\"";
                            isValid = false;
                        }
                    }
                }

                if ( !ThingComps.NullOrEmpty() )
                {
                    foreach ( var compSet in ThingComps )
                    {
                        if ( compSet.targetDefs.NullOrEmpty() )
                        {
                            errors += "\n\tNull or no targetDefs in ThingComps";
                            isValid = false;
                        }
                        /*
                        var targDef = ThingDef.Named( comp.targetDef );
                        if ( targDef == null )
                        {
                            errors += "\n\tUnable to find ThingDef named \"" + comp.targetDef + "\" in ThingComps";
                            isValid = false;
                        }*/
                        if ( compSet.compProps == null )
                        {
                            errors += "\n\tNull compProps in ThingComps";
                            isValid = false;
                        }
                        foreach( var targetDef in compSet.targetDefs.Where( targetDef => (
                            ( string.IsNullOrEmpty( targetDef ) )||
                            ( DefDatabase< ThingDef >.GetNamed( targetDef, false ) == null )
                        ) ) )
                        {
                            errors += "\n\tUnable to resolve ThingDef \"" + targetDef + "\"";
                            isValid = false;
                        }
                    }
                }

                if ( !SpecialInjectors.NullOrEmpty() )
                {
                    foreach( var injectorType in SpecialInjectors )
                    {
                        if(
                            ( injectorType == null )||
                            ( !injectorType.IsSubclassOf( typeof( SpecialInjector ) ) )
                        )
                        {
                            errors += "\n\tUnable to resolve SpecialInjector \"" + injectorType.ToString() + "\"";
                            isValid = false;
                        }
                    }
                }
#endif

                if ( !isValid )
                {
                    CCL_Log.Error( errors, "Mod Dependency :: " + ModName );
                }

                return isValid;
            }
        }

        public bool                         MapComponentsInjected
        {
            get
            {
                if ( (MapComponents == null) ||
                    (MapComponents.Count == 0) )
                {
                    return true;
                }

                var colonyMapComponents = Find.Map.components;

                foreach ( var mapComponentType in MapComponents )
                {
                    //var mapComponentType = Type.GetType( mapComponent );
                    if ( !colonyMapComponents.Exists(c => c.GetType() == mapComponentType ) )
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
                if ( (Designators == null) ||
                    (Designators.Count == 0) )
                {
                    return true;
                }
                foreach ( var data in Designators )
                {
                    var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false );
                    if ( !designationCategory.resolvedDesignators.Exists( d => d.GetType() == data.designatorClass ) )
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool                         ThingCompsInjected
        {
            get
            {
                if ( ThingComps.NullOrEmpty() )
                {
                    return true;
                }

                foreach ( var compSet in ThingComps )
                {
                    foreach ( var targetName in compSet.targetDefs )
                    {
                        var targetDef = DefDatabase< ThingDef >.GetNamed( targetName, false );
                        if ( targetDef != null && !targetDef.comps.Exists(s => s.compClass == compSet.compProps.compClass) )
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
                if ( SpecialInjectors.NullOrEmpty() )
                {
                    return true;
                }

                return specialsInjected;
            }
        }

        #endregion

        #region Injection

        public void                         InjectMapComponents()
        {
            var colonyMapComponents = Find.Map.components;

            foreach ( var mapComponentType in MapComponents )
            {
                // Get the component type
                //var mapComponentType = Type.GetType( mapComponent );

                // Does it exist in the map?
                if ( !colonyMapComponents.Exists( c => c.GetType() == mapComponentType ) )
                {
                    // Create the new map component
                    var mapComponentObject = (MapComponent) Activator.CreateInstance( mapComponentType );

                    // Inject the component
                    colonyMapComponents.Add(mapComponentObject);
                }
            }
        }

        public void                         InjectDesignators()
        {
            foreach ( var data in Designators )
            {
                var designationCategory = DefDatabase<DesignationCategoryDef>.GetNamed( data.designationCategoryDef, false );
                if ( !designationCategory.resolvedDesignators.Exists( d => d.GetType() == data.designatorClass ) )
                {
                    // Create the new designator
                    var designatorObject = (Designator) Activator.CreateInstance( data.designatorClass );

                    if( data.designatorNextTo == null )
                    {
                        // Inject the designator at the end
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
            }

        }

        public void                         InjectThingComps()
        {
            foreach ( var compSet in ThingComps )
            {
                // Access ThingDef database
                var typeFromHandle = typeof(DefDatabase<ThingDef>);
                var defsByName = typeFromHandle.GetField("defsByName", BindingFlags.Static | BindingFlags.NonPublic);
                if ( defsByName == null )
                {
                    CCL_Log.Error("defName is null!", "ThingComp Injection");
                    return;
                }
                var dictDefsByName = defsByName.GetValue(null) as Dictionary<string, ThingDef>;
                if ( dictDefsByName == null )
                {
                    CCL_Log.Error("Cannot access private members!", "ThingComp Injection");
                    return;
                }
                
                foreach ( var targetName in compSet.targetDefs )
                {
                    var def = dictDefsByName.Values.ToList().Find(s => s.defName == targetName);
                    var compProperties = compSet.compProps;

                    def.comps.Add(compProperties);
                }
            }
        }

        public void                         InjectSpecials()
        {
            foreach( var injectorType in SpecialInjectors )
            {
                var injectorObject = (SpecialInjector) Activator.CreateInstance( injectorType );
                injectorObject.Inject();
            }
            specialsInjected = true;
        }

        #endregion

    }

}
