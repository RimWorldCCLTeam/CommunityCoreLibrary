using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{
    
    public static class GenDefDatabase_Extensions
    {
        
        public static List<T> AllDefsListForReading<T>() where T : Def
        {
            return (List<T>) typeof( DefDatabase<> ).MakeGenericType( typeof( T ) ).GetField( "defsList", Controller.Data.UniversalBindingFlags ).GetValue( null );
        }

        public static List<Def> AllDefsListForReading( Type defType )
        {
            var list = new List<Def>();
            var defsList = typeof( DefDatabase<> ).MakeGenericType( defType ).GetField( "defsList", Controller.Data.UniversalBindingFlags ).GetValue( null );
            foreach( var obj in (IEnumerable) defsList )
            {
                list.Add( obj as Def );
            }
            return list;
        }

    }

}
