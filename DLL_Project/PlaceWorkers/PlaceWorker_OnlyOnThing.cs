using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyOnThing : PlaceWorker
    {
        
        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var thingDef = checkingDef as ThingDef;
#if DEBUG
            if( thingDef == null )
            {
                Log.Error( "Community Core Library :: Restricted PlaceWorker :: OnlyOnThing - Unable to cast BuildableDef to ThingDef!" );
                return AcceptanceReport.WasRejected;
            }
#endif

            var Restrictions = thingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                Log.Error( "Community Core Library :: Restricted PlaceWorker :: OnlyOnThing - Unable to get properties!" );
                return AcceptanceReport.WasRejected;
            }
#endif

            // Override steam-geyser restriction if required
            if( ( Restrictions.RestrictedThing.Exists( r => r == ThingDefOf.SteamGeyser ) )&&
                ( ThingDefOf.GeothermalGenerator != thingDef ) )
            {
                ThingDefOf.GeothermalGenerator = thingDef;
            }

            foreach( Thing t in loc.GetThingList() )
            {
                if( ( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )&&
                    ( t.Position == loc ) )
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return (AcceptanceReport)"MessagePlacementNotHere".Translate();
        }

    }

}
