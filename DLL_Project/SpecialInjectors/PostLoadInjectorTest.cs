#if DEVELOPER
using RimWorld;
using Verse;


namespace CommunityCoreLibrary
{
    
    [SpecialInjectorSequencer( InjectionSequence.GameLoad, InjectionTiming.SpecialInjectors )]
    public class PostLoadInjectorTest : SpecialInjector
    {
        
        public override bool Inject()
        {
            CCL_Log.Message( "PostLoadInjectorTest - Injected" );
            return true;
        }

    }

}
#endif
