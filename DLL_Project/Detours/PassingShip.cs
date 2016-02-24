using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _PassingShip
    {
        
        internal static void _Depart( this PassingShip obj )
        {
            if( Find.ListerBuildings.allBuildingsColonist.Any( b => (
                ( b.def.thingClass == typeof( Building_CommsConsole ) )||
                ( b.def.thingClass.IsSubclassOf( typeof( Building_CommsConsole ) ) )
                ) )
            )
            {
                string key = "MessageShipHasLeftCommsRange";
                string fullTitle = obj.FullTitle;
                object[] objArray = { (object) fullTitle };
                Messages.Message( Translator.Translate( key, objArray ), MessageSound.Silent );
            }
            Find.PassingShipManager.RemoveShip( obj );
        }

    }

}
