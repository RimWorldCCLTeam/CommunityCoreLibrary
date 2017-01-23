using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class Building_AdvancedPasteDispenser : Building_NutrientPasteDispenser
    {
        private static CompHopperUser compHopperUser;

        public override void SpawnSetup( Map map )
        {
            base.SpawnSetup( map );
            compHopperUser = this.TryGetComp<CompHopperUser>();
        }

        public override Building AdjacentReachableHopper( Pawn reacher )
        {
            var ret = base.AdjacentReachableHopper( reacher );

            // Check for generic hoppers
            if ( compHopperUser == null )
            {
                return ret;
            }

            var reachable = from hopper in compHopperUser.FindHoppers()
                            where reacher.CanReach(hopper.parent,
                                                   PathEndMode.Touch,
                                                   reacher.NormalMaxDanger(),
                                                   false)
                            where hopper.parent is Building
                            select (Building) hopper.parent;

            // Default to vanilla hoppers
            return reachable.Count() == 0? ret : reachable.RandomElement();
        }

        protected override Thing FindFeedInAnyHopper()
        {
            var ret = base.FindFeedInAnyHopper(); ;

            // Check for generic hoppers
            if ( compHopperUser == null )
            {
                return ret;
            }

            var resources = from hopper in compHopperUser.FindHoppers()
                            from resource in hopper.GetAllResources( compHopperUser.Resources )
                            where Building_NutrientPasteDispenser.IsAcceptableFeedstock( resource.def )
                            select resource;

            // Default to vanilla hoppers
            return resources.Count() == 0 ? ret : resources.First();
        }

        public override bool HasEnoughFeedstockInHoppers()
        {
            int costPerDispense = this.def.building.foodCostPerDispense;
            /* Research Project cut from A13
            if( ResearchProjectDef.Named( "NutrientResynthesis" ).IsFinished )
            {
                costPerDispense--;
            }
            */
            // Check for generic hoppers
            if( compHopperUser != null )
            {
                if( compHopperUser.EnoughResourcesInHoppers( costPerDispense ) )
                {
                    return true;
                }
            }
            // Check for vanilla hoppers
            return base.HasEnoughFeedstockInHoppers();
        }
    }
}
