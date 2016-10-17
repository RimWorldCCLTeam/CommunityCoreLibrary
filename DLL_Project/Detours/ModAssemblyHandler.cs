using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal class _ModAssemblyHandler : ModAssemblyHandler
    {

        internal static MethodInfo                  _AssemblyIsUsable;
        internal static FieldInfo                   _globalResolverIsSet;
        internal static FieldInfo                   _mod;

        public                                      _ModAssemblyHandler( ModContentPack mod ) : base( mod )
        {   // Required because C# is dumb
        }

        static                                      _ModAssemblyHandler()
        {
            _AssemblyIsUsable = typeof( ModAssemblyHandler ).GetMethod( "AssemblyIsUsable", Controller.Data.UniversalBindingFlags );
            if( _AssemblyIsUsable == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'AssemblyIsUsable' in 'ModAssemblyHandler'",
                    "Detour.ModAssemblyHandler" );
            }
            _globalResolverIsSet = typeof( ModAssemblyHandler ).GetField( "globalResolverIsSet", Controller.Data.UniversalBindingFlags );
            if( _globalResolverIsSet == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'globalResolverIsSet' in 'ModAssemblyHandler'",
                    "Detour.ModAssemblyHandler" );
            }
            _mod = typeof( ModAssemblyHandler ).GetField( "mod", Controller.Data.UniversalBindingFlags );
            if( _mod == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'mod' in 'ModAssemblyHandler'",
                    "Detour.ModAssemblyHandler" );
            }
        }

        #region Reflected Methods

        private bool                                AssemblyIsUsable( Assembly assembly )
        {
            return (bool)_AssemblyIsUsable.Invoke( this, new object[] { assembly } );
        }

        static bool                                 GlobalResolverIsSet
        {
            get
            {
                return (bool)_globalResolverIsSet.GetValue( null );
            }
            set
            {
                _globalResolverIsSet.SetValue( null, value );
            }
        }

        private ModContentPack                      ContentPack
        {
            get
            {
                return (ModContentPack)_mod.GetValue( this );
            }
            set
            {
                _mod.SetValue( this, value );
            }
        }

        #endregion

        #region Detoured Methods

        [DetourClassMethod( typeof( ModAssemblyHandler ), "ReloadAll", InjectionSequence.DLLLoad )]
        internal void                               _ReloadAll()
        {
            if( !GlobalResolverIsSet )
            {
                ResolveEventHandler @object = ( object obj, ResolveEventArgs args ) => Controller.Data.Assembly_CSharp;
                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.AssemblyResolve += @object.Invoke;
                GlobalResolverIsSet = true;
            }
            var path = Path.Combine( this.ContentPack.RootDir, "Assemblies" );
            var path2 = Path.Combine( GenFilePaths.CoreModsFolderPath, path );
            var directoryInfo = new DirectoryInfo( path2 );
            if( !directoryInfo.Exists )
            {
                return;
            }
            var files = directoryInfo.GetFiles( "*.*", SearchOption.AllDirectories );
            for( int i = 0; i < files.Length; i++ )
            {
                var fileInfo = files[ i ];
                if( !( fileInfo.Extension.ToLower() != ".dll" ) )
                {
                    Assembly assembly = null;
                    try
                    {
                        var rawAssembly = File.ReadAllBytes( fileInfo.FullName );
                        var fileName = Path.Combine( fileInfo.DirectoryName, Path.GetFileNameWithoutExtension( fileInfo.FullName ) ) + ".pdb";
                        var fileInfo2 = new FileInfo( fileName );
                        if( fileInfo2.Exists )
                        {
                            var rawSymbolStore = File.ReadAllBytes( fileInfo2.FullName );
                            assembly = AppDomain.CurrentDomain.Load( rawAssembly, rawSymbolStore );
                        }
                        else
                        {
                            assembly = AppDomain.CurrentDomain.Load( rawAssembly );
                        }
                    }
                    catch( Exception ex )
                    {
                        Log.Error( "Exception loading " + fileInfo.Name + ": " + ex.ToString() );
                        break;
                    }
                    if( assembly != null )
                    {
                        if( AssemblyIsUsable( assembly ) )
                        {
                            this.loadedAssemblies.Add( assembly );
                            // CCL added logic - Do injections on DLL Load
                            Controller.InjectionSubController.TrySequencedInjectorsOnLoad( assembly );
                        }
                    }
                }
            }
        }

        #endregion

    }

}
