using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _RoomRoleWorker_Laboratory
    {
        
        internal static float _GetScore( this RoomRoleWorker_Laboratory obj, Room room )
        {
            int num = 0;
            List<Thing> allContainedThings = room.AllContainedThings;
            for (int index = 0; index < allContainedThings.Count; ++index)
            {
                if(
                    ( allContainedThings[ index ].def.thingClass == typeof( Building_ResearchBench ) )||
                    ( allContainedThings[ index ].def.thingClass.IsSubclassOf( typeof( Building_ResearchBench ) ) )
                )
                {
                    ++num;
                }
            }
            return 30f * (float) num;
        }
    }

}
