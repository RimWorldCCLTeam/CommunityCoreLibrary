using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public static class Def_Extensions
    {
        /// <summary>
        /// hold a cached list of def -> helpdef links
        /// </summary>
        private static Dictionary<Def, HelpDef> _cachedDefHelpDefLinks = new Dictionary<Def, HelpDef>(); 

        /// <summary>
        /// Get the helpdef associated with the current def, or null if none exists.
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static HelpDef GetHelpDef(this Def def)
        {
            if ( _cachedDefHelpDefLinks.ContainsKey( def ) )
            {
                return _cachedDefHelpDefLinks[def];
            }
            _cachedDefHelpDefLinks.Add( def, DefDatabase<HelpDef>.AllDefsListForReading.FirstOrDefault( hd => hd.keyDef == def ) );
            return _cachedDefHelpDefLinks[def];
        }

        /// <summary>
        /// Get the label, capitalized and given appropriate styling ( bold if def has a helpdef, italic if def has no helpdef but does have description. )
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string LabelStyled( this Def def )
        {
            if ( def.label.NullOrEmpty() )
            {
                return string.Empty;
            }
            if ( def.GetHelpDef() != null )
            {
                return "<b>" + def.LabelCap + "</b>";
            }
            if ( !def.description.NullOrEmpty() )
            {
                return "<i>" + def.LabelCap + "</i>";
            }
            return def.LabelCap;
        }
    }
}
