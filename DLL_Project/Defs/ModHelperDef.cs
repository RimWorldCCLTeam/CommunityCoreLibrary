using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
	public struct CompInjectionSet
	{
		public Type                         compClass;
		public ThingDef                     targetDef;
	}

    public class ModHelperDef : Def
    {

        #region XML Data

        public string                       ModName;

        public string                       version;

        public List< string >               MapComponents;

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

                var modVersion = new Version( version );

                if( modVersion > ModController.CCLVersion )
                {
                    errors += "\n\tVersion requirement: v" + modVersion;
                    isValid = false;
                }

#if DEBUG
                if( ( MapComponents != null )&&
                    ( MapComponents.Count > 0 ) )
                {
                    foreach( var component in MapComponents )
                    {
                        var componentType = Type.GetType( component );
                        if( ( componentType == null )||
                            ( componentType.BaseType != typeof( MapComponent ) ) )
                        {
                            errors += "\n\tUnable to resolve MapComponent \"" + component + "\"";
                            isValid = false;
                        }
                    }
                }

                if( ( Designators != null )&&
                    ( Designators.Count > 0 ) )
                {
                    foreach( var data in Designators )
                    {
                        var designatorType = Type.GetType( data.designatorClass );
                        if( ( designatorType == null )||
                            ( designatorType.BaseType != typeof( Designator ) ) )
                        {
                            errors += "\n\tUnable to resolve designatorClass \"" + data.designatorClass + "\"";
                            isValid = false;
                        }
                        if( DefDatabase< DesignationCategoryDef >.GetNamed( data.designationCategoryDef, false ) == null )
                        {
                            errors += "\n\tUnable to resolve designationCategoryDef \"" + data.designationCategoryDef + "\"";
                            isValid = false;
                        }
                    }
                }
#endif
                
                if( !isValid )
                {
                    Log.Error( errors );
                }

                return isValid;
            }
        }

        public bool                         MapComponentsInjected
        {
            get
            {
                if( ( MapComponents == null )||
                    ( MapComponents.Count == 0 ) )
                {
                    return true;
                }

                var colonyMapComponents = Find.Map.components;

                foreach( var mapComponent in MapComponents )
                {
                    var mapComponentType = Type.GetType( mapComponent );
                    if( !colonyMapComponents.Exists( c => c.GetType() == mapComponentType ) )
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
                if( ( Designators == null )||
                    ( Designators.Count == 0 ) )
                {
                    return true;
                }
                foreach( var data in Designators )
                {
                    var designatorType = Type.GetType( data.designatorClass );
                    var designatorCategoryDef = DefDatabase< DesignationCategoryDef >.GetNamed( data.designationCategoryDef, false );
                    if( !designatorCategoryDef.resolvedDesignators.Exists( d => d.GetType() == designatorType ) )
                    {
                        return false;
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

            foreach( var mapComponent in MapComponents )
            {
                // Get the component type
                var mapComponentType = Type.GetType( mapComponent );

                // Does it exist in the map?
                if( !colonyMapComponents.Exists( c => c.GetType() == mapComponentType ) )
                {
                    // Create the new map component
                    var mapComponentObject = (MapComponent) Activator.CreateInstance( mapComponentType );

                    // Inject the component
                    colonyMapComponents.Add( mapComponentObject );
                }
            }
        }

        public void                         InjectDesignators()
        {
            foreach( var data in Designators )
            {
                var designatorType = Type.GetType( data.designatorClass );
                var designatorCategoryDef = DefDatabase< DesignationCategoryDef >.GetNamed( data.designationCategoryDef, false );
                if( !designatorCategoryDef.resolvedDesignators.Exists( d => d.GetType() == designatorType ) )
                {
                    // Create the new designator
                    var designatorObject = (Designator) Activator.CreateInstance( designatorType );

                    // Inject the designator
                    designatorCategoryDef.resolvedDesignators.Add( designatorObject );
                }
            }
        }

	    public void                         InjectThingComps()
	    {
		    foreach (var comp in ThingComps)
		    {
			    //
		    }
	    }

        #endregion

    }

}
