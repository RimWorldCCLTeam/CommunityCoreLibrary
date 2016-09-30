using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_SpecialInjectors : IInjector
    {

        private static Dictionary<string,bool>    dictInjected;

        static                              MHD_SpecialInjectors()
        {
            dictInjected = new Dictionary<string,bool>();
        }

#if DEBUG
        public override string              InjectString => "Special Injectors injected";

        public override bool                IsValid( ModHelperDef def, ref string errors )
        {
            if( def.SpecialInjectors.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var injectorType in def.SpecialInjectors )
            {
                if(
                    ( injectorType == null )||
                    ( !injectorType.IsSubclassOf( typeof( SpecialInjector ) ) )
                )
                {
                    errors += string.Format( "\n\tUnable to resolve Special Injector '{0}'", injectorType.ToString() );
                    isValid = false;
                }
            }

            return isValid;
        }
#endif

        public override bool                DefIsInjected( ModHelperDef def )
        {
            if( def.SpecialInjectors.NullOrEmpty() )
            {
                return true;
            }

            bool injected;
            if( !dictInjected.TryGetValue( def.defName, out injected ) )
            {
                return false;
            }

            return injected;
        }

        public override bool                InjectByDef( ModHelperDef def )
        {
            if( def.SpecialInjectors.NullOrEmpty() )
            {
                return true;
            }

            foreach( var injectorType in def.SpecialInjectors )
            {
                try
                {
                    var injectorObject = (SpecialInjector) Activator.CreateInstance( injectorType );
                    if( injectorObject == null )
                    {
                        CCL_Log.Message( string.Format( "Unable to create instance of '{0}'", injectorType.ToString() ) );
                        return false;
                    }
                    if( !injectorObject.Inject() )
                    {
                        CCL_Log.Message( string.Format( "Error injecting '{0}'", injectorType.ToString() ) );
                        return false;
                    }
                }
                catch( Exception e )
                {
                    CCL_Log.Message( e.ToString(), string.Format( "Error injecting '{0}'", injectorType.ToString() ) );
                    return false;
                }
            }

            dictInjected.Add( def.defName, true );
            return true;
        }

    }

}
