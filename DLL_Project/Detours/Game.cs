using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _Game : Game
    {

        internal static MethodInfo          _ExposeSmallComponents;
        internal static FieldInfo           _maps;

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
            _maps = typeof( Game ).GetField("maps", Controller.Data.UniversalBindingFlags );
            if( _maps == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'maps' in 'Game'",
                    "Detour.Game" );
            }
        }

        #region Reflected Methods

        internal void                       ExposeSmallComponents()
        {
            _ExposeSmallComponents.Invoke( this, null );
        }

        internal List<Map>                  maps
        {
            get
            {
                return (List<Map>)_maps.GetValue( this );
            }
            set
            {
                _maps.SetValue( this, value );
            }
        }

        #endregion

        // HARMONY CANDIDATE: postfix on World.EsposeData() and FinalizeInit(); 
        #region Detoured Methods
        [DetourMember]
        internal void                       _LoadGame()
        {
            if( this.maps.Any<Map>() )
            {
                Log.Error( "Called LoadGame() but there already is a map. There should be 0 maps..." );
                return;
            }
            MemoryUtility.UnloadUnusedUnityAssets();
            Current.ProgramState = ProgramState.MapInitializing;
            this.ExposeSmallComponents();
            LongEventHandler.SetCurrentEventText( "LoadingWorld".Translate() );
            if( Scribe.EnterNode( "world" ) )
            {
                this.World = new World();
                this.World.ExposeData();

                // changed: call the subcontrollers that needs to do stuff between the basic game data loaded but before things are loaded
                // TODO: I do not expect this work, but I think this is roughly where this goes;
                //       alternatively, it may belong in Map.FinalizeLoading()
                Controller.SubControllers.PreThingLoad();

                Scribe.ExitNode();
                this.World.FinalizeInit();
                LongEventHandler.SetCurrentEventText( "LoadingMap".Translate() );

                List<Map> list = this.maps;
                Scribe_Collections.LookList<Map>( ref list, "maps", LookMode.Deep, new object[ 0 ] );
                this.maps = list;

                int num = -1;
                Scribe_Values.LookValue<int>( ref num, "visibleMapIndex", -1, false );

                if( num < 0 && this.maps.Any<Map>() )
                {
                    Log.Error( "Visible map is null after loading but there are maps available. Setting visible map to [0]." );
                    num = 0;
                }
                if( num >= this.maps.Count )
                {
                    Log.Error( "Visible map index out of bounds after loading." );
                    if( this.maps.Any<Map>() )
                    {
                        num = 0;
                    }
                    else
                    {
                        num = -1;
                    }
                }
                this.visibleMapIndex = -128;
                this.VisibleMap = ( ( num < 0 ) ? null : this.maps[ num ] );
                LongEventHandler.SetCurrentEventText( "InitializingGame".Translate() );
                Find.CameraDriver.Expose();
                Scribe.FinalizeLoading();
                DeepProfiler.Start( "ResolveAllCrossReferences" );
                CrossRefResolver.ResolveAllCrossReferences();
                DeepProfiler.End();
                DeepProfiler.Start( "DoAllPostLoadInits" );
                PostLoadInitter.DoAllPostLoadInits();
                DeepProfiler.End();
                LongEventHandler.SetCurrentEventText( "SpawningAllThings".Translate() );
                for( int i = 0; i < this.maps.Count; i++ )
                {
                    this.maps[ i ].FinalizeLoading();
                }
                this.FinalizeInit();

                // changed: call the subcontrollers that need to do stuff after a game load
                Controller.SubControllers.PostLoad();

                if( Prefs.PauseOnLoad )
                {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        Find.TickManager.DoSingleTick();
                        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
                    } );
                }
                return;
            }
            Log.Error( "Could not find world XML node." );
        }

        
        /* NuOfBelthasar: this was replaced with LoadGame;
         *                I'll leave it here in case I did the update wrong

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

                // changed: call the subcontrollers that needs to do stuff between the basic game data loaded but before things are loaded
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
                    // changed
                    Log.Error( string.Format( "Exception spawning loaded thing {0}: {1}", thing, ex ) );
                }
            }
            DeepProfiler.End();
            MapIniterUtility.FinalizeMapInit();

            // changed: call the subcontrollers that need to do stuff after a game load
            Controller.SubControllers.PostLoad();
        }*/
        // Verse.Game

        #endregion

    }

}
