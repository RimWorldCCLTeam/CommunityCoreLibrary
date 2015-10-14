using System.Collections.Generic;
using CommunityCoreLibrary.StaticClasses;
using Verse;

namespace CommunityCoreLibrary
{
    public struct HelpDetailSection
    {
        public string                   Label;
        public string[]                 StringDescs;
        public string                   InsetString;
        public float                    Inset;
        private const string            DefaultInsetString  = "\t";
        private const float             DefaultInset        = 30f;
        public List<DefStringTriplet>   KeyDefs;

        public HelpDetailSection(string label, List<DefStringTriplet> keyDefs = null, string[] stringDescs = null)
        {
            Label = label;
            KeyDefs = keyDefs;
            StringDescs = stringDescs;
            if (label != null)
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

        public HelpDetailSection(string label, string[] stringDescs = null)
        {
            Label = label;
            KeyDefs = null;
            StringDescs = stringDescs;
            if (label != null)
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

        public HelpDetailSection(string label, List<DefStringTriplet> keyDefs = null)
        {
            Label = label;
            KeyDefs = keyDefs;
            StringDescs = null;
            if (label != null)
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

        public HelpDetailSection(string label, List<Def> keyDefs, string[] prefixes = null, string[] suffixes = null)
        {
            Label = label;
            KeyDefs = keyDefs != null ? HelpDetailSectionHelper.BuildDefStringTripletList(keyDefs, prefixes, suffixes) : null;
            StringDescs = null;
            if (label != null)
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
