using System.Collections.Generic;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _PostLoadInitter
    {

        internal static FieldInfo           _saveablesToPostLoad;

        internal static HashSet<IExposable> SaveablesToPostLoad()
        {
            if( _saveablesToPostLoad == null )
            {
                _saveablesToPostLoad = typeof( PostLoadInitter ).GetField( "saveablesToPostLoad", BindingFlags.Static | BindingFlags.NonPublic );
            }
            return (HashSet<IExposable>)_saveablesToPostLoad.GetValue( null );
        }

        internal static void _DoAllPostLoadInits()
        {
            Scribe.mode = LoadSaveMode.PostLoadInit;
            var hashSet = SaveablesToPostLoad();
            var enumerator = hashSet.GetEnumerator();
            var listSet = new List<IExposable>();
            while( enumerator.MoveNext() )
            {
                listSet.Add( enumerator.Current );
            }
            listSet.Sort( (x,y)=>
            {
                return ( x is Thing ) ? -1 : 1;
            } );
            foreach( var exposable in listSet )
            {
                exposable.ExposeData();
            }
            PostLoadInitter.Clear();
            Scribe.mode = LoadSaveMode.Inactive;
        }

    }
}
