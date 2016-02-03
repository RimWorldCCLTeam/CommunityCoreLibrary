using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CommunityCoreLibrary
{
    public struct HelpDetailSection
    {
        public string                   Label;
        public string                   InsetString;
        public float                    Inset;
        private const string            DefaultInsetString  = "\t";
        private const float             DefaultInset        = 30f;
        public List<DefStringTriplet>   KeyDefs;
        public List<StringDescTriplet>  StringDescs;
        public bool                     Align;
        
        public HelpDetailSection(string label, 
                                 string[] stringDescs,
                                 string[] prefixes,
                                 string[] suffixes,
                                 bool align = true )
        {
            Label = label;
            KeyDefs = null;
            StringDescs = stringDescs != null ? HelpDetailSectionHelper.BuildStringDescTripletList( stringDescs, prefixes, suffixes ) : null;
            Align = align;
            if( label != null )
            {
                InsetString = DefaultInsetString;
                Inset = DefaultInset;
            }
            else
            {
                InsetString = string.Empty;
                Inset = 0f;
            }
        }

        public HelpDetailSection( string label,
                                  List<Def> keyDefs,
                                  string[] defPrefixes,
                                  string[] defSuffixes,
                                  string[] stringDescs,
                                  string[] descPrefixes,
                                  string[] descSuffixes,
                                  bool align = true)
        {
            Label = label;
            KeyDefs = keyDefs != null ? HelpDetailSectionHelper.BuildDefStringTripletList( keyDefs, defPrefixes, defSuffixes ) : null;
            StringDescs = stringDescs != null ? HelpDetailSectionHelper.BuildStringDescTripletList( stringDescs, descPrefixes, descSuffixes ) : null;
            Align = align;
            if( label != null )
            {
                InsetString = DefaultInsetString;
                Inset = DefaultInset;
            }
            else
            {
                InsetString = string.Empty;
                Inset = 0f;
            }
        }

        public HelpDetailSection(string label, List<DefStringTriplet> defStringTriplets, List<StringDescTriplet> stringDescTriplets, bool align = true )
        {
            Label = label;
            KeyDefs = defStringTriplets;
            StringDescs = stringDescTriplets;
            Align = align;
            if( label != null)
            {
                InsetString = DefaultInsetString;
                Inset = DefaultInset;
            }
            else
            {
                InsetString = string.Empty;
                Inset = 0f;
            }
        }

        public HelpDetailSection(string label, List<Def> keyDefs, string[] prefixes = null, string[] suffixes = null, bool align = true)
        {
            Label = label;
            KeyDefs = keyDefs != null ? HelpDetailSectionHelper.BuildDefStringTripletList(keyDefs, prefixes, suffixes) : null;
            StringDescs = null;
            Align = align;
            if( label != null)
            {
                InsetString = DefaultInsetString;
                Inset = DefaultInset;
            }
            else
            {
                InsetString = string.Empty;
                Inset = 0f;
            }
        }
    }
}
