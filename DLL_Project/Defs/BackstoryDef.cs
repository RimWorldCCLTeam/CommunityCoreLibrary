using RimWorld;
using System;
using System.Linq;
using System.Collections.Generic;
using Verse;

namespace CommunityCoreLibrary
{
    public class BackstoryDef : Def
    {
        public string baseDescription;
        public BodyType bodyTypeGlobal = BodyType.Undefined;
        public BodyType bodyTypeMale = BodyType.Male;
        public BodyType bodyTypeFemale = BodyType.Female;
        public string title;
        public string titleShort;
        public BackstorySlot slot = BackstorySlot.Adulthood;
        public bool shuffleable = true;
        public bool addToDatabase = true;
        public List<WorkTags> workAllows = new List<WorkTags>();
        public List<WorkTags> workDisables = new List<WorkTags>();
        public List<BackstoryDefSkillListItem> skillGains = new List<BackstoryDefSkillListItem>();
        public List<string> spawnCategories = new List<string>();
        public string saveKeyIdentifier;

        public static BackstoryDef Named(string defName)
        {
            return DefDatabase<BackstoryDef>.GetNamed(defName);
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            if (!this.addToDatabase) return;
            if (BackstoryDatabase.allBackstories.ContainsKey(this.UniqueSaveKey())) return;

            Backstory b = new Backstory();
            if (!this.title.NullOrEmpty())
                b.title = this.title;
            else
            {
                Log.Error(this.defName + " backstory has empty title. Skipping...");
                return;
            }
            if (!this.titleShort.NullOrEmpty())
                b.titleShort = this.titleShort;
            else
                b.titleShort = b.title;

            if (!baseDescription.NullOrEmpty())
                b.baseDesc = this.baseDescription;
            else
            {
                Log.Warning(defName + " backstory has empty description.");
                b.baseDesc = "Empty.";
            }

            b.bodyTypeGlobal = this.bodyTypeGlobal;
            b.bodyTypeMale = this.bodyTypeMale;
            b.bodyTypeFemale = this.bodyTypeFemale;

            b.slot = this.slot;

            b.shuffleable = this.shuffleable;
            if (spawnCategories.NullOrEmpty())
            {
                Log.Error(defName + " backstory doesn't have any spawn categories defined. Skipping...");
                return;
            }
            else
                b.spawnCategories = this.spawnCategories;

            if (this.workAllows.Count > 0)
            {
                foreach (WorkTags current in Enum.GetValues(typeof(WorkTags)))
                {
                    if (!this.workAllows.Contains(current))
                    {
                        b.workDisables |= current;
                    }
                }
            }
            else if (this.workDisables.Count > 0)
            {
                foreach (var tag in this.workDisables)
                {
                    b.workDisables |= tag;
                }
            }
            else
            {
                b.workDisables = WorkTags.None;
            }
            b.skillGains = this.skillGains.ToDictionary(i => i.defName, i => i.amount);
            b.ResolveReferences();
            b.PostLoad();
            b.uniqueSaveKey = this.UniqueSaveKey();
            bool flag = false;
            foreach (var s in b.ConfigErrors(false))
            {
                if (!flag)
                {
                    flag = true;
                    Log.Error("Errors in custom backstory with defName: " + this.defName + ", backstory will be skipped.");
                }
                Log.Error(this.defName + " error: " + s);
            }
            if (!flag)
            {
                BackstoryDatabase.AddBackstory(b);
                //Log.Message("Added " + this.UniqueSaveKeyFor() + " backstory");
            }

        }
    }
}
