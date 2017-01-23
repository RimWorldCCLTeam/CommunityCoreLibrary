using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyOnSurface : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null )
        {
            foreach( Thing curThing in loc.GetThingList( this.Map ) )
            {
                if( curThing.def.surfaceType != SurfaceType.None )
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return (AcceptanceReport)( "MessagePlacementItemSurface".Translate() );
        }

    }

}
