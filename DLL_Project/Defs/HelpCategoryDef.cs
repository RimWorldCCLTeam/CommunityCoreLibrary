using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    public class HelpCategoryDef : Def
    {
        private List<HelpDef> _cachedHelpDefs = new List<HelpDef>();

        public string ModName = null;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Recache();
        }

        public override void PostLoad()
        {
            base.PostLoad();
            if (ModName.NullOrEmpty())
                Log.Error(this.defName + " has invalid mod name. Please add a proper name");
        }

        public List<HelpDef> HelpDefs
        {
            get { return _cachedHelpDefs; }
        }

        public float DrawHeight
        {
            get
            {
                if (Expanded)
                    return OTab_ModHelp.EntryHeight + HelpDefs.Count * OTab_ModHelp.EntryHeight;
                else
                    return OTab_ModHelp.EntryHeight;
            }
        }

        public bool ShouldDraw
        {
            get { return HelpDefs.Count > 0; }
        }

        public bool Expanded { get; set; }

        private void Recache()
        {
            _cachedHelpDefs.Clear();
            foreach (var def in (from t in DefDatabase<HelpDef>.AllDefs
                                 where t.category == this
                                 select t))
            {
                _cachedHelpDefs.Add(def);
            }
            _cachedHelpDefs.Sort();
        }
    }
}
