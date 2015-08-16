using Verse;

namespace CommunityCoreLibrary
{

    public struct ResearchCompletePair
    {
        
        // Research project and last completion flag
        public ResearchProjectDef           researchProject;
        public bool                         wasComplete;

        public                              ResearchCompletePair( ResearchProjectDef r )
        {
            researchProject = r;
            wasComplete = false;
        }

    }

}
