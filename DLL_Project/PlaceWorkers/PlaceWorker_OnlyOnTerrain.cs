using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_OnlyOnTerrain : PlaceWorker
    {
        
        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                CCL_Log.Error( "PlaceWorker_OnlyOnTerrain unable to get properties!", checkingDef.defName );
                return AcceptanceReport.WasRejected;
            }
#endif

            TerrainDef terrainDef = loc.GetTerrain();
            for( int i = 0; i < Restrictions.RestrictedTerrain.Count; i++ )
            {
                if( Restrictions.RestrictedTerrain[ i ] == terrainDef )
                {
                    return AcceptanceReport.WasAccepted;
                }
            }

            return "MessagePlacementNotOn".Translate(terrainDef.label);
        }

    }

}
