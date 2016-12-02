using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using RimWorld.Planet;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _Game : Game
    {

        internal static MethodInfo          _ExposeSmallComponents;
        internal static FieldInfo           _mapInt;

        static                              _Game()
        {
            _ExposeSmallComponents = typeof( Game ).GetMethod( "ExposeSmallComponents", Controller.Data.UniversalBindingFlags );
            if( _ExposeSmallComponents == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'ExposeSmallComponents' in 'Game'",
                    "Detour.Game" );
            }
            _mapInt = typeof( Game ).GetField( "mapInt", Controller.Data.UniversalBindingFlags );
            if( _mapInt == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get method 'mapInt' in 'Game'",
                    "Detour.Game" );
            }
        }

        #region Reflected Methods

        internal void                       ExposeSmallComponents()
        {
            _ExposeSmallComponents.Invoke( this, null );
        }

        internal Map                        mapInt
        {
            get
            {
                return (Map)_mapInt.GetValue( this );
            }
            set
            {
                _mapInt.SetValue( this, value );
            }
        }

        #endregion

        #region Detoured Methods

        [DetourMember]
        internal void                       _LoadData()
        {
            ExposeSmallComponents();

            LongEventHandler.SetCurrentEventText( "LoadingWorld".Translate() );
            Scribe.EnterNode( "world" );
            {
                Current.Game.World = new World();
                Find.World.ExposeData();
            }
            Scribe.ExitNode();

            LongEventHandler.SetCurrentEventText( "LoadingMap".Translate() );
            mapInt = new Map();

            List<Thing> nonCompressedThings = null;
            MapFileCompressor mapFileCompressor = null;
            Scribe.EnterNode( "map" );
            {
                Scribe_Deep.LookDeep<MapInfo>( ref Find.Map.info, "mapInfo", new object[ 0 ] );

                MapIniterUtility.ReinitStaticMapComponents_PreConstruct();
                Find.Map.ConstructComponents();
                MapIniterUtility.ReinitStaticMapComponents_PostConstruct();
                Find.Map.ExposeComponents();

                // Now call the subcontrollers that needs to do stuff between the basic game data loaded but before things are loaded
                Controller.SubControllers.PreThingLoad();

                DeepProfiler.Start( "Load compressed things" );
                mapFileCompressor = new MapFileCompressor();
                mapFileCompressor.ExposeData();
                DeepProfiler.End();

                DeepProfiler.Start( "Load non-compressed things" );
                Scribe_Collections.LookList<Thing>( ref nonCompressedThings, "things", LookMode.Deep, new object[ 0 ] );
                DeepProfiler.End();
            }
            Scribe.ExitNode();

            Scribe.FinalizeLoading();

            LongEventHandler.SetCurrentEventText( "InitializingMap".Translate() );

            DeepProfiler.Start( "ResolveAllCrossReferences" );
            CrossRefResolver.ResolveAllCrossReferences();
            DeepProfiler.End();

            DeepProfiler.Start( "DoAllPostLoadInits" );
            PostLoadInitter.DoAllPostLoadInits();
            DeepProfiler.End();

            LongEventHandler.SetCurrentEventText( "SpawningAllThings".Translate() );

            var thingsToSpawnAfterLoad = mapFileCompressor.ThingsToSpawnAfterLoad().ToList<Thing>();

            DeepProfiler.Start( "Merge compressed and non-compressed thing lists" );
            var allThings = new List<Thing>( nonCompressedThings.Count + thingsToSpawnAfterLoad.Count );
            foreach( var thing in nonCompressedThings.Concat( thingsToSpawnAfterLoad ) )
            {
                allThings.Add( thing );
            }
            DeepProfiler.End();
            DeepProfiler.Start( "Spawn everything into the map" );

            foreach( var thing in allThings )
            {
                try
                {
                    GenSpawn.Spawn( thing, thing.Position, thing.Rotation );
                }
                catch( Exception ex )
                {
                    Log.Error( string.Format( "Exception spawning loaded thing {0}: {1}", thing, ex ) );
                }
            }
            DeepProfiler.End();
            MapIniterUtility.FinalizeMapInit();

            // Finally, call the subcontrollers that need to do stuff after a game load
            Controller.SubControllers.PostLoad();
        }

        #endregion

    }

}
