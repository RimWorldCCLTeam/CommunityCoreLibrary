using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    public static class StatWorker_Extensions
    {

        private static FieldInfo    _stat;

        public static StatDef Stat( this StatWorker obj )
        {
            if( _stat == null )
            {
                // Need some reflection to access the internals
                _stat = typeof( StatWorker ).GetField( "stat", BindingFlags.Instance | BindingFlags.NonPublic );
            }
            return (StatDef)_stat.GetValue( obj );
        }

    }

}
