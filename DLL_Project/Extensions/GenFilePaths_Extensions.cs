using System;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{
    
    public static class GenFilePaths_Extensions
    {

        private static PropertyInfo       _ConfigFolderPath;

        static GenFilePaths_Extensions()
        {
            _ConfigFolderPath = typeof( GenFilePaths ).GetProperty( "ConfigFolderPath", Controller.Data.UniversalBindingFlags );
            if( _ConfigFolderPath == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ConfigFolderPath' in 'GenFilePaths'",
                    "GenFilePaths_Extensions" );
            }
        }

        public static string            ConfigFolderPath
        {
            get
            {
                return (string)_ConfigFolderPath.GetValue( null, null );
            }
        }

    }

}
