using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CommunityCoreLibrary
{
    public class BackstoryDef : Def
    {

        #region XML Data

        public string                               baseDescription;
        public BodyType                             bodyTypeGlobal      = BodyType.Undefined;
        public BodyType                             bodyTypeMale        = BodyType.Male;
        public BodyType                             bodyTypeFemale      = BodyType.Female;
        public string                               title;
        public string                               titleShort;
        public BackstorySlot                        slot                = BackstorySlot.Adulthood;
        public bool                                 shuffleable         = true;
        public bool                                 addToDatabase       = true;
        public List<WorkTags>                       workAllows          = new List<WorkTags>();
        public List<WorkTags>                       workDisables        = new List<WorkTags>();
        public List<BackstoryDefSkillListItem>      skillGains          = new List<BackstoryDefSkillListItem>();
        public List<string>                         spawnCategories     = new List<string>();
        public List<TraitEntry>                     forcedTraits        = new List<TraitEntry>();
        public List<TraitEntry>                     disallowedTraits    = new List<TraitEntry>();
        public string                               saveKeyIdentifier;

        #endregion

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
                CCL_Log.Error(defName + " backstory has empty title. Skipping...", "Backstories");
                return;
            }
            if (!titleShort.NullOrEmpty())
                b.titleShort = titleShort;
            else
                b.titleShort = b.title;

            if (!baseDescription.NullOrEmpty())
                b.baseDesc = baseDescription;
            else
            {
                CCL_Log.Message(defName + " backstory has empty description.", "Backstories");
                b.baseDesc = "Empty.";
            }

            b.bodyTypeGlobal        = bodyTypeGlobal;
            b.bodyTypeMale          = bodyTypeMale;
            b.bodyTypeFemale        = bodyTypeFemale;

            b.slot = slot;

            b.shuffleable = shuffleable;
            if (spawnCategories.NullOrEmpty())
            {
                CCL_Log.Error(defName + " backstory doesn't have any spawn categories defined. Skipping...", "Backstories");
                return;
            }
            else
                b.spawnCategories = spawnCategories;

            if (workAllows.Count > 0)
            {
                foreach (WorkTags current in Enum.GetValues(typeof(WorkTags)))
                {
                    if (!workAllows.Contains(current))
                    {
                        b.workDisables |= current;
                    }
                }
            }
            else if (workDisables.Count > 0)
            {
                foreach (var tag in workDisables)
                {
                    b.workDisables |= tag;
                }
            }
            else
            {
                b.workDisables = WorkTags.None;
            }
            b.skillGains = skillGains.ToDictionary(i => i.defName, i => i.amount);

            if( forcedTraits.Count > 0 )
            {
                b.forcedTraits = new List<TraitEntry>();
                foreach( var trait in forcedTraits )
                {
                    var newTrait = new TraitEntry( trait.def, trait.degree );
                    b.forcedTraits.Add( newTrait );
                }
            }

            if( disallowedTraits.Count > 0 )
            {
                b.disallowedTraits = new List<TraitEntry>();
                foreach( var trait in disallowedTraits )
                {
                    var newTrait = new TraitEntry( trait.def, trait.degree );
                    b.disallowedTraits.Add( newTrait );
                }
            }

            b.ResolveReferences();
            b.PostLoad();
            b.uniqueSaveKey = this.UniqueSaveKey();

            bool flag = false;
            foreach (var s in b.ConfigErrors(false))
            {
                if (!flag)
                {
                    flag = true;
                    CCL_Log.Error("Errors in custom backstory with defName: " + defName + ", backstory will be skipped.", "Backstories");
                }
                CCL_Log.Error(defName + " error: " + s, "Backstories");
            }
            if (!flag)
            {
                BackstoryDatabase.AddBackstory(b);
                //CCL_Log.Message("Added " + this.UniqueSaveKey() + " backstory", "Backstories");
            }

        }
    }
}
