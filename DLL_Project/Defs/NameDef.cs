using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class NameDef : Def
    {
        public string first;
        public string nick = "";
        public string last;
        public GenderPossibility genderPossibility = GenderPossibility.Either;

        public override void PostLoad()
        {
            base.PostLoad();

            if (first.NullOrEmpty())
            {
                Log.Error("Custom name with defName: " + defName + " has no defined first name. It will not be added.");
                return;
            }
            if (last.NullOrEmpty())
            {
                Log.Error("Custom name with defName: " + defName + " has no defined last name. It will not be added.");
                return;
            }

            PawnNameDatabaseSolid.AddPlayerContentName(Name, this.genderPossibility);

            //Log.Message(name.ToString());
        }

        public NameTriple Name
        {
            get
            {
                return new NameTriple(first, nick, last);
            }
        }
    }
}
