using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{
    
    internal static class _PageUtility
    {
     
        [DetourMember( typeof( PageUtility ) )]
        internal static void                _InitGameStart()
        {
            LongEventHandler.QueueLongEvent(
                () =>
            {
                // TODO:  Detour different sequence for GameLoad injectors
                // Controller.Data.ResetInjectionSubController();
                Find.GameInitData.PrepForMapGen();
                Find.GameInitData.startedFromEntry = true;
                Find.Scenario.PreMapGenerate();
            },
                "Play",
                "GeneratingMap",
                true,
                null );
        }

    }

}
