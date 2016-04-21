using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    
    public class CompGlowerToggleable : CompGlower
    {

        private bool                        lit = true;

        public bool                         Lit
        {
            get
            {
                return lit;
            }
            set
            {
                lit = value;
                UpdateLit();
            }
        }

    }

}
