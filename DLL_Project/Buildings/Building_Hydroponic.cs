using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public class Building_Hydroponic : Building_PlantGrower
    {

        CompPowerTrader                     CompPowerTrader
        {
            get
            {
                return this.TryGetComp< CompPowerTrader >();
            }
        }

        public override void                TickRare()
        {
            // Building_PlantGrower does not call base.TickRare() so do it here
            for( int i = 0; i < AllComps.Count; ++i )
            {
                AllComps[ i ].CompTickRare();
            }

            if( ( CompPowerTrader == null )||
                ( CompPowerTrader.PowerOn ) )
            {
                return;
            }

            foreach( var plant in PlantsOnMe )
            {
                plant.TakeDamage( new DamageInfo( DamageDefOf.Rotting, 4, (Thing) null, new BodyPartDamageInfo?(), (ThingDef) null ) );
            }
        }

    }

}
