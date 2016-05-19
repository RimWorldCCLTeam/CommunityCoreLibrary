using System;

namespace CommunityCoreLibrary
{

    public static class Version
    {

        #region Instance Data

        private static System.Version       versionMin = new System.Version( "0.13.0" );
        private const string                versionCurrentInt = "0.13.2.1";

        private static System.Version       versionCurrent;

        #endregion

        #region Static Properties

        public static System.Version        Minimum
        {
            get
            {
                return versionMin;
            }
        }

        public static System.Version        Current
        {
            get
            {
                if( versionCurrent == null )
                {
                    // TODO:  Fix issue #30 so we can use proper assembly versioning
                    //System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    //CCLVersion = assembly.GetName().Version;
                    versionCurrent = new System.Version( versionCurrentInt );
                }
                return versionCurrent;
            }
        }

        #endregion

        #region Comparison

        public enum VersionCompare
        {
            LessThanMin,
            GreaterThanMax,
            ExactMatch,
            WithinRange,
            Invalid
        }

        public static VersionCompare        Compare( string v )
        {
            try
            {
                var modVersion = new System.Version( v );
                return Compare( modVersion );
            }
            catch
            {
                return VersionCompare.Invalid;
            }
        }

        public static VersionCompare        Compare( System.Version v )
        {
            if( v < Version.Minimum )
            {
                return VersionCompare.LessThanMin;
            }
            else if( v > Version.Current )
            {
                return VersionCompare.GreaterThanMax;
            }
            else if( v == Version.Current )
            {
                return VersionCompare.ExactMatch;
            }

            return VersionCompare.WithinRange;
        }

        #endregion

        public static void                  Log()
        {
#if DEBUG
            CCL_Log.Message( "v" + Current + " (debug)" );
#else
            CCL_Log.Message( "v" + Current );
#endif
        }

    }

}
