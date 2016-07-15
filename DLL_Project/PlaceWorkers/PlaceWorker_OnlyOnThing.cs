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

            foreach( Thing t in loc.GetThingList() )
            {
                if( Restrictions.RestrictedThing.Contains( t.def ) )
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return (AcceptanceReport)( "MessagePlacementNotHere".Translate() );
        }

        public override bool ForceAllowPlaceOver( BuildableDef other )
        {
            // This will allow placement on steam geysers as long as
            // the list of restricted things allows steam geysers.
            return( other == ThingDefOf.SteamGeyser );
        }

    }

}
