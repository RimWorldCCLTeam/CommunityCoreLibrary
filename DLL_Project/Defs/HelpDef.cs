using System;

using Verse;

namespace CommunityCoreLibrary
{

    public class HelpDef : Def, IComparable
    {

        #region XML Data

        public HelpCategoryDef category;

        #endregion

        //[Unsaved]

        #region Process State

#if DEBUG
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (category == null)
            {
                Log.Error("Community Core Library :: Help Tab :: category resolved to null in HelpDef( " + defName + " )");
            }
        }
#endif

        public int CompareTo(object obj)
        {
            var d = obj as HelpDef;
            return d != null
                ? d.label.CompareTo(label) * -1
                : 1;
        }

        #endregion

    }

}
