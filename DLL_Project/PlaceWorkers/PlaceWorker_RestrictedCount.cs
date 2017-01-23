using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_RestrictedCount : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null ){
                CCL_Log.Error( "PlaceWorker_RestrictedCount unable to get properties!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            var thingDef = checkingDef as ThingDef;
#if DEBUG
            if( thingDef == null )
            {
                CCL_Log.Error( "PlaceWorker_RestrictedCount unable to get cast BuildableDef to ThingDef!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            // Get the current count of instances and blueprints of
            int count = this.Map.listerThings.ThingsOfDef( thingDef ).Count
                + this.Map.listerThings.ThingsOfDef( thingDef.blueprintDef ).Count;

            return count < Restrictions.MaxCount
                ? AcceptanceReport.WasAccepted
                    : "MessagePlacementCountRestricted".Translate( Restrictions.MaxCount );

        }

    }

}
