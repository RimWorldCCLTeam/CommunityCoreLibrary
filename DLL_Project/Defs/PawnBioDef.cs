using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public class PawnBioDef : Def
    {
        public BackstoryDef childhoodDef;
        public BackstoryDef adulthoodDef;
        public string firstName;
        public string nickName = "";
        public string lastName;
        public GenderPossibility gender;

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            PawnBio bio = new PawnBio();
            bio.gender = this.gender;

            if (!firstName.NullOrEmpty() && !lastName.NullOrEmpty())
            {
                bio.name = new NameTriple(firstName, nickName, lastName);
            }
            else
            {
                Log.Error("PawnBio with defName: " + defName + " has empty first or last name. It will not be added.");
                return;
            }

            Backstory childhood = BackstoryDatabase.GetWithKey(this.childhoodDef.UniqueSaveKey());
            if (childhood != null)
            {
                bio.childhood = childhood;
                bio.childhood.shuffleable = false;
                bio.childhood.slot = BackstorySlot.Childhood;
            }
            else
            {
                Log.Error("PawnBio with defName: " + defName + " has null childhood. It will not be added.");
                return;
            }

            Backstory adulthood = BackstoryDatabase.GetWithKey(this.adulthoodDef.UniqueSaveKey());
            if (adulthood != null)
            {
                bio.adulthood = adulthood;
                bio.adulthood.shuffleable = false;
                bio.adulthood.slot = BackstorySlot.Adulthood;
            }
            else
            {
                Log.Error("PawnBio with defName: " + defName + " has null adulthood. It will not be added.");
                return;
            }

            bio.name.ResolveMissingPieces();
            //bio.PostLoad();

            bool flag = false;
            foreach (var error in bio.ConfigErrors())
            {
                if (!flag)
                {
                    flag = true;
                    Log.Error("Config error(s) in PawnBioDef " + this.defName + ". Skipping...");
                }
                Log.Error(error);
            }
            if (flag)
            {
                return;
            }

            if (!SolidBioDatabase.allBios.Contains(bio))
                SolidBioDatabase.allBios.Add(bio);
        }
    }
}
