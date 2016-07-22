using RimWorld;
using Verse;


namespace CommunityCoreLibrary
{
    
    public class PostLoadInjectorTest : SpecialInjector
    {
        
        public override bool Inject()
        {
            CCL_Log.Message( "PostLoadInjectorTest - Injected" );
            return true;
        }

    }

}
