using Verse;

namespace CommunityCoreLibrary
{

    public delegate void                     AdvancedResearchMod( bool Enable );

    public static class Research
    {

        public static ResearchProjectDef    Locker
        {
            get
            {
                return DefDatabase< ResearchProjectDef >.GetNamed( "CommunityCoreLibraryResearchLocker" );
            }
        }

        public const ResearchProjectDef     Unlocker = null;
    }

}
