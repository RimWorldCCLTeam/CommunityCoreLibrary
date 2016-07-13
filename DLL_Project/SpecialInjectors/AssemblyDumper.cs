using System;
using System.IO;
using System.Reflection;
using Verse;

namespace CommunityCoreLibrary
{
#if DEBUG
    public class AssemblyDumper : SpecialInjector
    {

        public override bool                Inject()
        {
            return DumpAllModsAssemblies();
        }

        private bool                        DumpAllModsAssemblies()
        {
            var stream = CCL_Log.OpenStream( "assembly_dump.txt" );
            if( stream == null )
            {
                return false;
            }

            DumpAssembly( stream, Controller.Data.Assembly_CSharp );

            foreach( var mod in Controller.Data.Mods )
            {
                if( !mod.assemblies.loadedAssemblies.NullOrEmpty() )
                {
                    CCL_Log.Write( "Mod: " + mod.Identifier );

                    foreach( var assembly in mod.assemblies.loadedAssemblies )
                    {
                        DumpAssembly( stream, assembly );
                    }
                }
            }
            CCL_Log.CloseStream( stream );
            return true;
        }

        private void                        DumpAssembly( CCL_Log.LogStream stream, Assembly assembly )
        {
            CCL_Log.IndentStream( stream );
            {
                CCL_Log.Write( "Assembly: " + assembly.GetName(), stream );

                CCL_Log.IndentStream( stream );
                {
                    foreach( var type in Controller.Data.Assembly_CSharp.GetTypes() )
                    {
                        CCL_Log.Write( "Type: " + type.FullName, stream );
                        CCL_Log.IndentStream( stream );
                        {
#region Fields
                            var fields = type.GetFields( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                            if( !fields.NullOrEmpty() )
                            {
                                CCL_Log.Write( "Fields:", stream );
                                CCL_Log.IndentStream( stream );
                                {
                                    foreach( var entity in fields )
                                    {
                                        var str = entity.FieldType.Name;
                                        str += " " + entity.Name;
                                        if( entity.IsStatic )
                                            str += " (Static)";
                                        else
                                            str += " (Instance)";
                                        if( entity.IsPrivate ) str += " (NonPublic)";
                                        if( entity.IsPublic ) str += " (Public)";
                                        CCL_Log.Write( str, stream );
                                    }
                                }
                                CCL_Log.IndentStream( stream, -1 );
                            }
#endregion
#region Properties
                            var properties = type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                            if( !properties.NullOrEmpty() )
                            {
                                CCL_Log.Write( "Properties:", stream );
                                CCL_Log.IndentStream( stream );
                                {
                                    foreach( var entity in properties )
                                    {
                                        var str = entity.PropertyType.Name;
                                        str += " " + entity.Name;
                                        var method = entity.GetGetMethod();
                                        if( method != null )
                                        {
                                            str += " (Public Get)";
                                        }
                                        else
                                        {
                                            method = entity.GetGetMethod( true );
                                            if( method != null ) str += " (NonPublic Get)";
                                        }
                                        method = entity.GetSetMethod();
                                        if( method != null )
                                        {
                                            str += " (Public Set)";
                                        }
                                        else
                                        {
                                            method = entity.GetSetMethod( true );
                                            if( method != null ) str += " (NonPublic Set)";
                                        }
                                        CCL_Log.Write( str, stream );
                                    }
                                }
                                CCL_Log.IndentStream( stream, -1 );
                            }
#endregion
#region Methods
                            var methods = type.GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public );
                            if( !methods.NullOrEmpty() )
                            {
                                CCL_Log.Write( "Methods:", stream );
                                CCL_Log.IndentStream( stream );
                                {
                                    foreach( var entity in methods )
                                    {
                                        var str = entity.ReturnType.Name;
                                        str += " " + entity.Name;
                                        if( entity.IsStatic )
                                            str += " (Static)";
                                        else
                                            str += " (Instance)";
                                        if( entity.IsPrivate ) str += " (NonPublic)";
                                        if( entity.IsPublic ) str += " (Public)";
                                        if( !entity.GetParameters().NullOrEmpty() )
                                        {
                                            var parameters = entity.GetParameters();
                                            str += " Parameters: (";
                                            for( int i = 0; i < parameters.Length; ++i )
                                            {
                                                var pi = parameters[ i ];
                                                str += " " + pi.ParameterType.ToString();
                                                str += " " + pi.Name;
                                                if( pi.IsOut ) str += " (out)";
                                                if( pi.IsRetval ) str += " (ret)";
                                                if( i < parameters.Length - 1 )
                                                {
                                                    str += ",";
                                                }
                                            }
                                            str += " )";
                                        }
                                        CCL_Log.Write( str, stream );
                                    }
                                }
                                CCL_Log.IndentStream( stream, -1 );
                            }
#endregion
                        }
                        CCL_Log.IndentStream( stream, -1 );
                        CCL_Log.Write( "\n", stream );
                    }
                }
                CCL_Log.IndentStream( stream, -1 );
            }
            CCL_Log.IndentStream( stream, -1 );
            CCL_Log.Write( "\n", stream );
        }

    }
#endif
}
