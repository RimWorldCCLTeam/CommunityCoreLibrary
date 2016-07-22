using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _PageUtility
    {
        
        internal static void _InitGameStart()
        {
            LongEventHandler.QueueLongEvent(
                () =>
            {
                Controller.Data.ResetInjectionSubController();
                MapIniter_NewGame.PrepForMapGen();
                Find.GameInitData.startedFromEntry = true;
                Find.Scenario.PreMapGenerate();
            },
                "Map",
                "GeneratingMap",
                true,
                null );
        }

    }

}
