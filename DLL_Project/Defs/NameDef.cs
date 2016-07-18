using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class NameDef : Def
    {

        #region XML Data

        public string                       first;
        public string                       nick                = "";
        public string                       last;
        public GenderPossibility            genderPossibility   = GenderPossibility.Either;

        #endregion

        public NameTriple Name
        {
            get
            {
                return new NameTriple(first, nick, last);
            }
        }

        public override void PostLoad()
        {
            base.PostLoad();

            if (first.NullOrEmpty())
            {
                CCL_Log.Error("Custom name with defName: " + defName + " has no defined first name. It will not be added.", "Backstories");
                return;
            }
            if (last.NullOrEmpty())
            {
                CCL_Log.Error("Custom name with defName: " + defName + " has no defined last name. It will not be added.", "Backstories");
                return;
            }

            PawnNameDatabaseSolid.AddPlayerContentName(Name, genderPossibility);

        }

    }
}
