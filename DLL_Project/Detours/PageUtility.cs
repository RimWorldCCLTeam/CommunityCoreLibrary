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
                //Controller.Data.ResetInjectionSubController();
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
