using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Alert_PlaceWorker_Restriction : Alert_Critical
    {
        
        public override AlertReport         Report
        {
            get
            {
                // Alert the player that something got destroyed
                return !PlaceWorker_Restriction_Alert_Data.AlertPlayer
                    ? AlertReport.Inactive
                    : AlertReport.CulpritIs( PlaceWorker_Restriction_Alert_Data.DestroyedThings.RandomElement() );
            }
        }

        public override string              FullExplanation
        {
            get
            {
                var msg = new StringBuilder();
                msg.AppendLine( "AlertPlaceWorkerRestrictionSupportRemovedDesc".Translate() );
                foreach( var t in PlaceWorker_Restriction_Alert_Data.DestroyedThings )
                {
                    msg.AppendLine( "   " + t.def.defName );
                }
                return msg.ToString();
            }
        }

        public override void                AlertActiveUpdate()
        {
            if( PlaceWorker_Restriction_Alert_Data.AlertPlayer )
            {
                base.AlertActiveUpdate();
                PlaceWorker_Restriction_Alert_Data.Cooldown();
            }
        }

        public                              Alert_PlaceWorker_Restriction()
        {
            baseLabel = "AlertPlaceWorkerRestrictionSupportRemovedLabel".Translate();
        }

    }

}
