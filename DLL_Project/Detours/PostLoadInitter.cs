using System.Collections.Generic;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _PostLoadInitter
    {
        // Unused, do we really want to do this anyway? 1000101 - 04/09/2016
        /*
        internal static FieldInfo           _saveablesToPostLoad;

        static                              _PostLoadInitter()
        {
            _saveablesToPostLoad = typeof( PostLoadInitter ).GetField( "saveablesToPostLoad", Controller.Data.UniversalBindingFlags );
            if( _saveablesToPostLoad == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'saveablesToPostLoad' in 'PostloadInitter'",
                    "Detour.PostLoadInitter" );
            }
        }

        internal static HashSet<IExposable> SaveablesToPostLoad()
        {
            return (HashSet<IExposable>)_saveablesToPostLoad.GetValue( null );
        }

        [DetourClassMethod( typeof( PostLoadInitter ), "DoAllPostLoadInits" )]
        internal static void                _DoAllPostLoadInits()
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
        */

    }
}
