using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
	public class CompInjectionSet
	{
		public string                       targetDef;
		//public Type                         compClass;
											            //Default props
		public CompProperties               compProps = new CompProperties();
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

	            if ((ThingComps != null) &&
	                (ThingComps.Count > 0))
	            {
		            foreach (var comp in ThingComps)
		            {
			            if (ThingDef.Named(comp.targetDef) == null)
						{
							errors += "\n\tUnable to find ThingDef named \"" + comp.targetDef + "\"";
						}
		            }
	            }
#endif
                
				if ( !isValid )
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

        public bool                         ThingCompsInjected
		{
			get
			{
				if (ThingComps == null || ThingComps.Count == 0)
				{
					return true;
				}

				foreach (var current in ThingComps)
				{
					var targDef = ThingDef.Named(current.targetDef);
					if (current.targetDef == null)
					{
						CCL_Log.Error("Unknown ThingDef named \"" + current.targetDef + "\"", "ThingComps Injection");
						return true;
					}
					if (targDef.comps == null)
					{
						targDef.comps = new List<CompProperties>();
					}
					if (targDef.comps.Exists(s => s.compClass == current.compProps.compClass))
					{
						return true;
					}
				}
				return false;
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

	    public void InjectDesignators()
	    {
		    foreach (var data in Designators)
		    {
			    var designatorType = Type.GetType(data.designatorClass);
			    var designatorCategoryDef = DefDatabase<DesignationCategoryDef>.GetNamed(data.designationCategoryDef, false);
			    if (!designatorCategoryDef.resolvedDesignators.Exists(d => d.GetType() == designatorType))
			    {
				    // Create the new designator
				    var designatorObject = (Designator) Activator.CreateInstance(designatorType);

				    // Inject the designator
				    designatorCategoryDef.resolvedDesignators.Add(designatorObject);
			    }
		    }

	    }

	    public void InjectThingComps()
	    {
		    foreach (var comp in ThingComps)
			{
				// Access ThingDef database
				var typeFromHandle = typeof(DefDatabase<ThingDef>);
				var defsByName = typeFromHandle.GetField("defsByName", BindingFlags.Static | BindingFlags.NonPublic);
				if (defsByName == null)
				{
					CCL_Log.Error("defName is null!", "ThingComp Injection");
					return;
				}
				var dictDefsByName = defsByName.GetValue(null) as Dictionary<string, ThingDef>;
				if (dictDefsByName == null)
				{
					CCL_Log.Error("Cannot access private members!", "ThingComp Injection");
					return;
				}
				var def = dictDefsByName.Values.ToList().Find(s => s.defName == comp.targetDef);

				var compProperties = comp.compProps;

				def.comps.Add(compProperties);
				CCL_Log.MessageVerbose("Injected " + comp.compProps.compClass.Name + " to " + def.defName, "ThingComp Injection");
			}
		}

        #endregion

    }

}
