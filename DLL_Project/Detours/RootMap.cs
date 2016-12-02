using System;
using System.Reflection;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _RootMap : Verse.Root
    {

        internal static MethodInfo          _ErrorWhileLoadingMap;
        internal static MethodInfo          _ErrorWhileGeneratingMap;

        static                              _RootMap()
        {
            _ErrorWhileLoadingMap = typeof( RootMap ).GetMethod( "ErrorWhileLoadingMap", Controller.Data.UniversalBindingFlags );
            if( _ErrorWhileLoadingMap == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ErrorWhileLoadingMap' in 'RootMap'",
                    "Detour.RootMap" );
            }
            _ErrorWhileGeneratingMap = typeof( RootMap ).GetMethod( "ErrorWhileGeneratingMap", Controller.Data.UniversalBindingFlags );
            if( _ErrorWhileGeneratingMap == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'ErrorWhileGeneratingMap' in 'RootMap'",
                    "Detour.RootMap" );
            }
        }

        internal void                       ErrorWhileLoadingMap( Exception e )
        {
            _ErrorWhileLoadingMap.Invoke( this, new[] { e } );
        }

        internal void                       ErrorWhileGeneratingMap( Exception e )
        {
            _ErrorWhileGeneratingMap.Invoke( this, new[] { e } );
        }

        [DetourMember( typeof( RootMap ) )]
        internal void                       _Start()
        {
            // Do subcontroller preload
            Controller.SubControllers.PreLoad();
            // Root.Start()
            base.Start();
            // RootMap.Start()
            if(
                ( Find.GameInitData != null )&&
                ( !Find.GameInitData.mapToLoad.NullOrEmpty() )
            )
            {
                LongEventHandler.QueueLongEvent(
                    () =>
                {
                    SavedGameLoader.LoadGameFromSaveFile( Find.GameInitData.mapToLoad );
                },
                    "LoadingLongEvent",
                    true,
                    ErrorWhileLoadingMap
                );
            }
            else
            {
                LongEventHandler.QueueLongEvent(
                    MapIniter_NewGame.InitNewGeneratedMap,
                    "GeneratingMap",
                    true,
                    ErrorWhileGeneratingMap
                );
            }
            LongEventHandler.QueueLongEvent(
                () =>
            {
                ScreenFader.SetColor( Color.black );
                ScreenFader.StartFade( Color.clear, 0.5f );
            },
                null,
                false,
                null
            );
        }

    }

}
