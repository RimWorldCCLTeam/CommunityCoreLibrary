using System;

using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal class _Root_Play : Verse.Root
    {
        public MusicManagerPlay musicManagerPlay;

        internal void                       ErrorWhileLoadingMap( Exception e )
        {
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap(e);
        }

        internal void                       ErrorWhileGeneratingMap( Exception e )
        {
            GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap(e);
        }

        [DetourMember( typeof( Root_Play ) )]
        internal void                       _Start()
        {
            // changed: do subcontroller preload
            Controller.SubControllers.PreLoad();

            base.Start();
            this.musicManagerPlay = new MusicManagerPlay();
            if (
                ( Find.GameInitData != null )&&
                ( !Find.GameInitData.gameToLoad.NullOrEmpty() )
            )
            {
                LongEventHandler.QueueLongEvent(
                    () =>
                {
                    SavedGameLoader.LoadGameFromSaveFile( Find.GameInitData.gameToLoad );
                },
                    "LoadingLongEvent",
                    true,
                    ErrorWhileLoadingMap
                );
            }
            else
            {
                LongEventHandler.QueueLongEvent(
                    () =>
                {
                    if (Current.Game == null)
                    {
                        SetupForQuickTestPlay();
                    }
                    Current.Game.InitNewGame();
                },
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
        
        internal static void SetupForQuickTestPlay()
        {
            Current.ProgramState = ProgramState.Entry;
            Current.Game = new Game();
            Current.Game.InitData = new GameInitData();
            Current.Game.Scenario = ScenarioDefOf.Crashlanded.scenario;
            Find.Scenario.PreConfigure();
            Current.Game.storyteller = new Storyteller(StorytellerDefOf.Cassandra, DifficultyDefOf.Hard);
            Current.Game.World = WorldGenerator.GenerateWorld(0.05f, GenText.RandomSeedString(), OverallRainfall.Normal, OverallTemperature.Normal);
            Rand.RandomizeSeedFromTime();
            Find.Scenario.PostWorldLoad();
            Find.GameInitData.ChooseRandomStartingTile();
            Find.GameInitData.mapSize = 150;
            Find.GameInitData.PrepForMapGen();
            Find.Scenario.PreMapGenerate();
        }
    }

}
