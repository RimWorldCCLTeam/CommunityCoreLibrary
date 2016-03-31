using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_MapComponents : IInjector
    {

#if DEBUG
        public string                       InjectString
        {
            get
            {
                return "Map Components injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.MapComponents.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var componentType in def.MapComponents )
            {
                if(
                    ( componentType == null ) ||
                    ( !componentType.IsSubclassOf( typeof( MapComponent ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve MapComponent '{0}'", componentType );
                    isValid = false;
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.MapComponents.NullOrEmpty() )
            {
                return true;
            }

            var colonyMapComponents = Find.Map.components;

            foreach( var componentType in def.MapComponents )
            {
                if( Find_Extensions.MapComponent( componentType ) == null )
                {
                    return false;
                }
            }

            return true;
        }

        public bool                         Inject( ModHelperDef def )
        {
            if( def.MapComponents.NullOrEmpty() )
            {
                return true;
            }

            var existingComponents = Find.Map.components;

            foreach( var componentType in def.MapComponents )
            {
                if( !existingComponents.Exists( c => c.GetType() == componentType ) )
                {
                    var componentObject = (MapComponent) Activator.CreateInstance( componentType );
                    if( componentObject == null )
                    {
                        CCL_Log.Message( string.Format( "Unable to create instance of '{0}'", componentType ) );
                        return false;
                    }
                    existingComponents.Add( componentObject );
                }
            }

            return true;
        }

    }

}
