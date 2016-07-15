using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_WallAttachment : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            IntVec3 c = loc - rot.FacingCell;

            Building support = c.GetEdifice();
            if( support == null )
            {
                return (AcceptanceReport)( "MessagePlacementAgainstSupport".Translate() );
            }

            if(
                ( support.def == null )||
                ( support.def.graphicData == null )
            )
            {
                return (AcceptanceReport)( "MessagePlacementAgainstSupport".Translate() );
            }

            return ( support.def.graphicData.linkFlags & ( LinkFlags.Rock | LinkFlags.Wall ) ) != 0
                ? AcceptanceReport.WasAccepted
                    : (AcceptanceReport)( "MessagePlacementAgainstSupport".Translate() );
        }

    }

}
