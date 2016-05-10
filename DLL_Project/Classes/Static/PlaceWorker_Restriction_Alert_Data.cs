using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class PlaceWorker_Restriction_Alert_Data
    {

        static readonly List< Thing >       destroyedThings = new List<Thing>();
        public static List< Thing >         DestroyedThings
        {
            get
            {
                return destroyedThings;
            }
        }

        static int                          cooldownTicks;

        public static bool                  AlertPlayer
        {
            get
            {
                return destroyedThings.Count > 0;
            }
        }

        static                              PlaceWorker_Restriction_Alert_Data()
        {
        }

        public static void                  Cooldown( int ticks = 1 )
        {
            cooldownTicks -= ticks;
            if(
                ( cooldownTicks <= 0 )&&
                ( destroyedThings.Count > 0 )
            )
            {
                destroyedThings.Clear();
            }
        }

        public static void                  Add( Thing thing )
        {
            destroyedThings.Add( thing );
            cooldownTicks = 250;
        }

    }

}
