using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class Verb_ShootExtended : Verb_LaunchProjectile
    {
        
        private int                         pelletCount;
        private float                       expMin;
        private float                       expMid;
        private float                       expMax;

        private bool                        gotProps;


        protected override int              ShotsPerBurst
        {
            get
            {
                return verbProps.burstShotCount;
            }
        }

        // XML data into verb
        protected virtual void              TryGetProps()
        {
            if( gotProps )
                //Already done, pass
                return;

            var props = verbProps as VerbProperties_Extended;
            if( props == null )
            {
#if DEBUG
                CCL_Log.TraceMod(
                    this.caster.def,
                    Verbosity.Warnings,
                    "Missing VerbProperties_Extended"
                );
#endif
                pelletCount = 1;
                expMin = 10;
                expMid = 50;
                expMax = 240;
            }
            else
            {
                pelletCount = props.pelletCount;
                expMin = props.experienceGain.min;
                expMid = props.experienceGain.mid;
                expMax = props.experienceGain.max;
            }
            gotProps = true;
        }

        public override void                WarmupComplete()
        {
            TryGetProps();

            base.WarmupComplete();
            if(
                ( !CasterIsPawn )||
                ( CasterPawn.skills == null )
            )
            {
                return;
            }

            var xp = expMin;
            if(
                ( currentTarget.Thing != null )&&
                ( currentTarget.Thing.def.category == ThingCategory.Pawn )
            )
            {
                xp = currentTarget.Thing.HostileTo( caster ) ? expMax : expMid;
            }
            CasterPawn.skills.Learn( SkillDefOf.Shooting, xp );
        }

        protected override bool             TryCastShot()
        {
            if( !base.TryCastShot() )
            {
                return false;
            }

            var i = 1;
            while(
                ( i < pelletCount )&&
                ( base.TryCastShot() )
            )
            {
                i++;
            }
            MoteMaker.MakeStaticMote( caster.Position, ThingDefOf.Mote_ShotFlash, 9f );
            return true;
        }

    }

}
