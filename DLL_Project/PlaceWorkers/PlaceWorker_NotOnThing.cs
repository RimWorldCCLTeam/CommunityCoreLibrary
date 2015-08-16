using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_NotOnThing : PlaceWorker
    {
        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var thingDef = checkingDef as ThingDef;
#if DEBUG
            if( thingDef == null )
            {
                Log.Error( "Community Core Library :: Restricted PlaceWorker :: NotOnThing - Unable to cast BuildableDef to ThingDef!" );
                return AcceptanceReport.WasRejected;
            }
#endif

            var Restrictions = thingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                Log.Error( "Community Core Library :: Restricted PlaceWorker :: NotOnThing - Unable to get properties!" );
                return AcceptanceReport.WasRejected;
            }
#endif

            foreach( Thing t in loc.GetThingList() )
            {
                if( Restrictions.RestrictedThing.Find( r => r == t.def ) != null )
                {
                    return (AcceptanceReport)"MessagePlacementNotOn".Translate() + t.def.label;
                }
            }

            return AcceptanceReport.WasAccepted;
        }

    }

}
