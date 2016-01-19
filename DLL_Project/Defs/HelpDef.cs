using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{

    public class HelpDef : Def, IComparable
    {

        #region XML Data

        public HelpCategoryDef      category;

        #endregion

        [Unsaved]

        #region Instance Data

        public Def                   keyDef;

        #endregion

        #region Process State

#if DEBUG
        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if( category == null )
            {
                CCL_Log.Error( "Category resolved to null", "HelpDef :: " + defName );
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

        #region Log Dump

#if DEBUG
        // Guard against using this method in release builds!
        public string LogDump()
        {
            return 
                "HelpDef: " + defName +
                "\n\t" + keyDef +
                "\n\t" + category.LabelCap +
                "\n\t" + LabelCap +
                "\n------\n" +
                description +
                "\n------\n";
        }
#endif
        
        #endregion

        #region Help details

        public List< HelpDetailSection > HelpDetailSections = new List<HelpDetailSection>();

        public string Description
        {
            get
            {
                StringBuilder s = new StringBuilder();
                s.AppendLine(description);
                foreach (HelpDetailSection section in HelpDetailSections)
                {
                    s.AppendLine(section.GetString());
                }
                return s.ToString();
            }
        }

        #endregion

        #region Filter

        public bool ShouldDraw { get; set; }

        public void Filter(string filter, bool force = false)
        {
            ShouldDraw = force || MatchesFilter(filter);
        }

        public bool MatchesFilter(string filter)
        {
            return filter == "" || LabelCap.ToUpper().Contains(filter.ToUpper());

        }

        #endregion

    }

}
