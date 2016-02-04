using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyOnThing : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                CCL_Log.Error( "PlaceWorker_OnlyOnThing unable to get properties!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            var thingDef = checkingDef as ThingDef;
#if DEBUG
            if( thingDef == null )
            {
                CCL_Log.Error( "PlaceWorker_OnlyOnTerrain unable to cast BuildableDef to ThingDef!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            // Override steam-geyser restriction if required
            // Obsoleted with detouring
            //if(
            //    ( Restrictions.RestrictedThing.Exists( r => r == ThingDefOf.SteamGeyser ) )&&
            //    ( ThingDefOf.GeothermalGenerator != thingDef )
            //)
            //{
            //    ThingDefOf.GeothermalGenerator = thingDef;
            //}

            foreach( Thing t in loc.GetThingList() )
            {
                if(
                    ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                    ( t.Position == loc )
                )
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return (AcceptanceReport)( "MessagePlacementNotHere".Translate() );
        }

    }

}
