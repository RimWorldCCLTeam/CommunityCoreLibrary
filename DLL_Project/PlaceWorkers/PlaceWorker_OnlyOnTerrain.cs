using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyOnTerrain : PlaceWorker
    {

        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                CCL_Log.Error( "PlaceWorker_OnlyOnTerrain unable to get properties!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            TerrainDef terrainDef = loc.GetTerrain( this.Map );
            if( Restrictions.RestrictedTerrain.Contains( terrainDef ) )
            {
                return AcceptanceReport.WasAccepted;
            }

            return "MessagePlacementNotOn".Translate( terrainDef.label );
        }

    }

}
