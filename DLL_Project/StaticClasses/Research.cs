using Verse;

namespace CommunityCoreLibrary
{

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
