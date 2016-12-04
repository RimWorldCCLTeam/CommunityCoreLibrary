#if DEVELOPER
using RimWorld;
using Verse;


namespace CommunityCoreLibrary
{
    
    [SpecialInjectorSequencer( InjectionSequence.GameLoad )]
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
