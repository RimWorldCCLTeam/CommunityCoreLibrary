using System;
using System.Linq;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal static class _IncidentWorker_TraderArrival
    {
        
        internal static bool _TryExecute( this IncidentWorker_TraderArrival obj, IncidentParms parms )
        {
            if( Find.PassingShipManager.passingShips.Count >= 5 )
            {
                return false;
            }
            TraderKindDef result;
            if( !GenCollection.TryRandomElement<TraderKindDef>( DefDatabase<TraderKindDef>.AllDefs, out result ) )
            {
                throw new InvalidOperationException();
            }
            TradeShip tradeShip = new TradeShip( result );
            if( Find.ListerBuildings.allBuildingsColonist.Any( b => (
                ( b.def.thingClass == typeof( Building_CommsConsole ) )||
                ( b.def.thingClass.IsSubclassOf( typeof( Building_CommsConsole ) ) )
            ) )
            )
            {
                LetterStack letterStack = Find.LetterStack;
                string labelCap = tradeShip.def.LabelCap;
                string key = "TraderArrival";
                string name = tradeShip.name;
                string label = tradeShip.def.label;
                object[] objArray = { name, label };
                string text = Translator.Translate( key, objArray );
                letterStack.ReceiveLetter( labelCap, text, LetterType.Good );
            }
            Find.PassingShipManager.AddShip( tradeShip );
            tradeShip.GenerateThings();
            return true;
        }

    }

}
