using System;
using System.IO;
using System.Reflection;

using Verse;

namespace CommunityCoreLibrary
{

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

        private void                        DumpAssembly( FileStream stream, Assembly assembly )
        {
            CCL_Log.WriteIndent();
            {
                CCL_Log.Write( "Assembly: " + assembly.GetName() );

                CCL_Log.WriteIndent();
                {
                    foreach( var type in Controller.Data.Assembly_CSharp.GetTypes() )
                    {
                        CCL_Log.Write( "Type: " + type.FullName );
                        CCL_Log.WriteIndent();
                        {
                            #region Fields
                            CCL_Log.Write( "Fields:" );
                            CCL_Log.WriteIndent();
                            {
                                foreach( var entity in type.GetFields( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                                {
                                    var str = entity.Name;
                                    if( entity.IsStatic )
                                        str += " (Static)";
                                    else
                                        str += " (Instance)";
                                    if( entity.IsPrivate ) str += " (NonPublic)";
                                    if( entity.IsPublic ) str += " (Public)";
                                    CCL_Log.Write( str );
                                }
                            }
                            CCL_Log.WriteIndent( -1 );
                            #endregion
                            #region Properties
                            CCL_Log.Write( "Properties:" );
                            CCL_Log.WriteIndent();
                            {
                                foreach( var entity in type.GetProperties( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                                {
                                    var str = entity.Name;
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
                                    CCL_Log.Write( str );
                                }
                            }
                            CCL_Log.WriteIndent( -1 );
                            #endregion
                            #region Methods
                            CCL_Log.Write( "Methods:" );
                            CCL_Log.WriteIndent();
                            {
                                foreach( var entity in type.GetMethods( BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public ) )
                                {
                                    var str = entity.Name;
                                    if( entity.IsStatic )
                                        str += " (Static)";
                                    else
                                        str += " (Instance)";
                                    if( entity.IsPrivate ) str += " (NonPublic)";
                                    if( entity.IsPublic ) str += " (Public)";
                                    if( entity.GetParameters() != null )
                                    {
                                        str += " Parameters:";
                                        foreach( var pi in entity.GetParameters() )
                                        {
                                            str += " " + pi.ParameterType.ToString();
                                            if( pi.IsOut ) str += " (out)";
                                            if( pi.IsRetval ) str += " (ret)";
                                        }
                                    }
                                    CCL_Log.Write( str );
                                }
                            }
                            CCL_Log.WriteIndent( -1 );
                            #endregion
                        }
                        CCL_Log.WriteIndent( -1 );
                    }
                }
                CCL_Log.WriteIndent( -1 );
            }
            CCL_Log.WriteIndent( -1 );
            CCL_Log.Write( "\n", stream );
        }

    }

}
