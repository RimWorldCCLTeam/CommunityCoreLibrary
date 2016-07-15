using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    public class Building_AdvancedPasteDispenser : Building_NutrientPasteDispenser
    {

        private static CompHopperUser   compHopperUser;

        public override void            SpawnSetup()
        {
            base.SpawnSetup();
            compHopperUser = this.TryGetComp<CompHopperUser>();
        }


        public override Building        AdjacentReachableHopper( Pawn reacher )
        {
            // Check for generic hoppers
            if( compHopperUser != null )
            {
                var hoppers = compHopperUser.FindHoppers();
                if( !hoppers.NullOrEmpty() )
                {
                    foreach( var hopper in hoppers )
                    {
                        if(
                            reacher.CanReach(
                                ( TargetInfo )( ( Thing )hopper.parent ),
                                PathEndMode.Touch,
                                reacher.NormalMaxDanger(),
                                false )
                        )
                        {
                            return (Building) hopper.parent;
                        }
                    }
                }
            }
            // Check for vanilla hoppers
            return base.AdjacentReachableHopper( reacher );
        }

        protected override Thing           FindFeedInAnyHopper()
        {
            // Check for generic hoppers
            if( compHopperUser != null )
            {
                var hoppers = compHopperUser.FindHoppers();
                if( !hoppers.NullOrEmpty() )
                {
                    foreach( var hopper in hoppers )
                    {
                        var resources = hopper.GetAllResources( compHopperUser.Resources );
                        if( !resources.NullOrEmpty() )
                        {
                            foreach( var resource in resources )
                            {
                                // This check shouldn't be needed, but we'll do it as a fail-safe
                                if( Building_NutrientPasteDispenser.IsAcceptableFeedstock( resource.def ) )
                                {
                                    return resource;
                                }
                            }
                        }
                    }
                }
            }
            // Check for vanilla hoppers
            return base.FindFeedInAnyHopper();
        }

        public override bool            HasEnoughFeedstockInHoppers()
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
