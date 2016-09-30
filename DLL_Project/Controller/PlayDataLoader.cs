using Verse;

namespace CommunityCoreLibrary.Controller
{
    
    internal static class PlayDataLoader
    {

        internal static void                Reload()
        {
            Controller.Data.ReloadingPlayData = true;
            LongEventHandler.QueueLongEvent(
                QueueReload,
                "",
                true,
                null
            );
        }

        private static void                 QueueReload()
        {
            Detour._PlayDataLoader.ClearAllPlayDataInt();
            if( Detour._PlayDataLoader.queueLoadAllPlayData )
            {
                Detour._PlayDataLoader.LoadAllPlayDataInt( Detour._PlayDataLoader.queueRecovering );
            }
            Controller.Data.ReloadingPlayData = false;
        }

    }

}
