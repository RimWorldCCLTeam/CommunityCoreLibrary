using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    public static class StatWorker_Extensions
    {

        private static FieldInfo        _stat;

        static                          StatWorker_Extensions()
        {
            _stat = typeof( StatWorker ).GetField( "stat", Controller.Data.UniversalBindingFlags );
            if( _stat == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'stat' in class 'StatWorker'",
                    "StatWorker_Extensions");
            }
        }

        public static StatDef           Stat( this StatWorker obj )
        {
            return (StatDef)_stat.GetValue( obj );
        }

    }

}
