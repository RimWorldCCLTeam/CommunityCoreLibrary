using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyUnderRoof : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null )
        {
            return this.Map.roofGrid.Roofed( loc )
                ? AcceptanceReport.WasAccepted
                    : ( AcceptanceReport )( "MessagePlacementMustBeUnderRoof".Translate() );
        }

    }

}
