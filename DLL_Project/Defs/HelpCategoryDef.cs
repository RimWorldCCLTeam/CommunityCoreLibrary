using System.Collections.Generic;
using System.Linq;

using Verse;

namespace CommunityCoreLibrary
{
    
    public class HelpCategoryDef : Def
    {

        #region XML Data

        public string                       ModName;

        #endregion

        #region Instance Data
        [Unsaved]

        readonly List<HelpDef>              _cachedHelpDefs = new List<HelpDef>();

        #endregion

        #region Query State

        public List<HelpDef>                HelpDefs
        {
            get
            {
                return _cachedHelpDefs;
            }
        }

        public float                        DrawHeight
        {
            get
            {
                if( Expanded )
                {
                    return OTab_ModHelp.EntryHeight + HelpDefs.Count * OTab_ModHelp.EntryHeight;
                }
                else
                {
                    return OTab_ModHelp.EntryHeight;
                }
            }
        }

        public bool                         ShouldDraw
        {
            get
            {
                return HelpDefs.Count > 0;
            }
        }

        public bool                         Expanded { get; set; }

        #endregion

        #region Process State

        public override void                ResolveReferences()
        {
            base.ResolveReferences();
            Recache();
        }

#if DEBUG
        public override void                PostLoad()
        {
            base.PostLoad();

            if( ModName.NullOrEmpty() )
            {
                Log.Error( "Community Core Library :: Help Tab :: ModName resolved to null in HelpCategoryDef( " + defName + " )" );
            }
        }
#endif

        void                                Recache()
        {
            _cachedHelpDefs.Clear();
            foreach( var def in (
                from t in DefDatabase<HelpDef>.AllDefs
                where t.category == this
                select t ) )
            {
                _cachedHelpDefs.Add( def );
            }
            _cachedHelpDefs.Sort();
        }

        #endregion

    }

}
