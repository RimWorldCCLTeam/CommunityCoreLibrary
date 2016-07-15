using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_NotOnThing : PlaceWorker
    {
        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                CCL_Log.Error( "PlaceWorker_NotOnThing unable to get properties!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            foreach( Thing t in loc.GetThingList() )
            {
                if( Restrictions.RestrictedThing.Contains( t.def ) )
                {
                    return "MessagePlacementNotOn".Translate( t.def.label );
                }
            }

            return AcceptanceReport.WasAccepted;
        }

    }

}
