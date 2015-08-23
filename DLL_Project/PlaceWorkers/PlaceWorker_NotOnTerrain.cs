using Verse;

namespace CommunityCoreLibrary
{

    public class PlaceWorker_NotOnTerrain : PlaceWorker
    {
        
        public override AcceptanceReport    AllowsPlacing( BuildableDef checkingDef, IntVec3 loc, Rot4 rot )
        {
            var Restrictions = checkingDef.RestrictedPlacement_Properties();
#if DEBUG
            if( Restrictions == null )
            {
                Log.Error( "Community Core Library :: Restricted PlaceWorker :: NotOnTerrain - Unable to get properties!" );
                return AcceptanceReport.WasRejected;
            }
#endif

            TerrainDef terrainDef = loc.GetTerrain();
            for( int i = 0; i < Restrictions.RestrictedTerrain.Count; i++ )
            {
                if( Restrictions.RestrictedTerrain[ i ] == terrainDef )
                {
                    return (AcceptanceReport)( "MessagePlacementNotOn".Translate() + terrainDef.label );
                }
            }

            return AcceptanceReport.WasAccepted;
        }

    }

}
