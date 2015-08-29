using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Verse;

namespace CommunityCoreLibrary
{
    public class ModHelperDef : Def
    {

        #region XML Data

        public string                       ModName;

        public string                       version;

        public List< Type >                 MapComponents;

        public List< DesignatorData >       Designators;

        public List< CompInjectionSet >     ThingComps;

        #endregion

        //[Unsaved]

        #region Instance Data

        #endregion

        #region Query State

        public bool                         IsValid
        {
            get
            {
                var isValid = true;
                var errors = "Community Core Library :: Mod Dependency :: " + ModName;

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
                if ( (MapComponents != null) &&
                    (MapComponents.Count > 0) )
                {
                    foreach ( var componentType in MapComponents )
                    {
                        //var componentType = Type.GetType( component );
                        if ( (componentType == null) ||
                            (componentType.BaseType != typeof(MapComponent)) )
                        {
                            errors += "\n\tUnable to resolve MapComponent \"" + componentType.ToString() + "\"";
                            isValid = false;
                        }
                    }
                }

                if ( (Designators != null) &&
                    (Designators.Count > 0) )
                {
                    foreach ( var data in Designators )
                    {
                        var designatorType = data.designatorClass;
                        if ( (designatorType == null)//||
                        //    ( designatorType.BaseType != typeof( Designator ) )
                        )
                        {
                            errors += "\n\tUnable to resolve designatorClass \"" + data.designatorClass + "\"";
                            isValid = false;
                        }
                        if ( (string.IsNullOrEmpty(data.designationCategoryDef)) ||
                            (DefDatabase<DesignationCategoryDef>.GetNamed(data.designationCategoryDef, false) == null) )
                        {
                            errors += "\n\tUnable to resolve designationCategoryDef \"" + data.designationCategoryDef + "\"";
                            isValid = false;
                        }
                    }
                }

                if ( (ThingComps != null) &&
                    (ThingComps.Count > 0 ) )
                {
                    foreach ( var comp in ThingComps )
                    {
                        if ( comp.targetDefs.NullOrEmpty() )
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
                        if ( comp.compProps == null )
                        {
                            errors += "\n\tNull compProps in ThingComps";
                            isValid = false;
                        }
                        foreach ( var targetDef in comp.targetDefs.Where( targetDef => string.IsNullOrEmpty(targetDef) ||
                                                                                       DefDatabase<ThingDef>.GetNamed(targetDef, false) == null ) )
                        {
                            errors += "\n\tUnable to resolve ThingDef \"" + targetDef + "\"";
                            isValid = false;
                        }
                    }
                }
#endif

                if ( !isValid )
                {
                    Log.Error(errors);
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
                    if ( !colonyMapComponents.Exists(c => c.GetType() == mapComponentType) )
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
                    if ( !designationCategory.resolvedDesignators.Exists(d => d.GetType() == data.designatorClass) )
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
                if ( !colonyMapComponents.Exists(c => c.GetType() == mapComponentType) )
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
                if ( !designationCategory.resolvedDesignators.Exists(d => d.GetType() == data.designatorClass) )
                {
                    // Create the new designator
                    var designatorObject = (Designator) Activator.CreateInstance( data.designatorClass );

                    // Inject the designator
                    designationCategory.resolvedDesignators.Add(designatorObject);
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

        #endregion

    }

}
