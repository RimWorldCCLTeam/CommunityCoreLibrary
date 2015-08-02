using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    public class HelpDef : Def, IComparable
    {
        public HelpCategoryDef category;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (category == null)
                Log.Warning(defName + " has null help category. This means it won't be displayed in the help menu");
        }

        public int CompareTo(object obj)
        {
            HelpDef d = obj as HelpDef;
            if (d != null)
            {
                return d.label.CompareTo(label);
            }
            return 1;
        }
    }
}
