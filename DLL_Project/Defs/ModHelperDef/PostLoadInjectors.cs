using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace CommunityCoreLibrary
{

    public class MHD_PostLoadInjectors : IInjector
    {

        private static Dictionary<string,bool>    dictInjected;

        static                              MHD_PostLoadInjectors()
        {
            dictInjected = new Dictionary<string,bool>();
        }

#if DEBUG
        public string                       InjectString
        {
            get
            {
                return "Post Loaders injected";
            }
        }

        public bool                         IsValid( ModHelperDef def, ref string errors )
        {
            if( def.PostLoadInjectors.NullOrEmpty() )
            {
                return true;
            }

            bool isValid = true;

            foreach( var injectorType in def.PostLoadInjectors )
            {
                if(
                    ( injectorType == null )||
                    ( !injectorType.IsSubclassOf( typeof( SpecialInjector ) ) )
                )
                {
                    errors += string.Format( "Unable to resolve Post Load Injector '{0}'", injectorType.ToString() );
                    isValid = false;
                }
            }

            return isValid;
        }
#endif

        public bool                         Injected( ModHelperDef def )
        {
            if( def.PostLoadInjectors.NullOrEmpty() )
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

        public bool                         Inject( ModHelperDef def )
        {
            if( def.PostLoadInjectors.NullOrEmpty() )
            {
                return true;
            }

            foreach( var injectorType in def.PostLoadInjectors )
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

        public static void                  Reset()
        {
            dictInjected.Clear();
        }

    }

}
