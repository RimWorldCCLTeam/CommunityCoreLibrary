using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class CCLVersionDef : Def
    {

        #region XML Data

        public string                       ModName;

        public string                       version;

        public List< string >               MapComponents;

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

                if( ( MapComponents != null )&&
                    ( MapComponents.Count > 0 ) )
                {
                    foreach( var component in MapComponents )
                    {
                        var componentType = Type.GetType( component );
                        if( ( componentType == null )||
                            ( componentType.BaseType != typeof( MapComponent ) )||
                            ( componentType.GetConstructor( Type.EmptyTypes ) == null ) )
                        {
                            errors += "\n\tUnable to resolve MapComponent \"" + component + "\"";
                            isValid = false;
                        }
                    }
                }

                if( !isValid )
                {
                    Log.Error( errors );
                }

                return isValid;
            }
        }

        public bool                         IsInjected
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

        public void                         Inject()
        {
            var colonyMapComponents = Find.Map.components;

            foreach( var mapComponent in MapComponents )
            {
                // Get the component type
                var mapComponentType = Type.GetType( mapComponent );

                // Does it exist in the map?
                if( !colonyMapComponents.Exists( c => c.GetType() == mapComponentType ) )
                {
                    // Get the default constructor for the component
                    var mapComponentConstructor = mapComponentType.GetConstructor( Type.EmptyTypes );

                    // Create a new object for the map component
                    var mapComponentObject = mapComponentConstructor.Invoke( new object[] {} );

                    // Inject the component
                    colonyMapComponents.Add( mapComponentObject as MapComponent );
                }
            }

        }

        #endregion

    }

}
