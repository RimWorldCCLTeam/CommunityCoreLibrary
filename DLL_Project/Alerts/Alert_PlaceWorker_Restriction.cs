using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class Alert_PlaceWorker_Restriction : Alert_Critical
    {
        public override AlertReport GetReport()
        {
            return PlaceWorker_Restriction_Alert_Data.AlertPlayer ?
                AlertReport.CulpritIs( PlaceWorker_Restriction_Alert_Data.DestroyedThings.RandomElement() ) :
                AlertReport.Inactive;
        }

        public override string GetExplanation()
        {
            var msg = new StringBuilder();
            foreach ( var t in PlaceWorker_Restriction_Alert_Data.DestroyedThings )
            {
                msg.AppendLine( "   " + t.def.defName );
            }
            return "AlertPlaceWorkerRestrictionSupportRemovedDesc".Translate( msg.ToString() );
        }

        public override void AlertActiveUpdate()
        {
            if ( PlaceWorker_Restriction_Alert_Data.AlertPlayer )
            {
                base.AlertActiveUpdate();
                PlaceWorker_Restriction_Alert_Data.Cooldown();
            }
        }

        public Alert_PlaceWorker_Restriction()
        {
            defaultLabel = "AlertPlaceWorkerRestrictionSupportRemovedLabel".Translate();
        }

    }

}
