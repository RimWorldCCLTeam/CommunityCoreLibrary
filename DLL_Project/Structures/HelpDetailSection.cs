using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityCoreLibrary.StaticClasses;
using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    public struct HelpDetailSection
    {
        // TODO: drawer with link resolver.
        public HelpDetailSection(string label, List<DefStringTriplet> keyDefs = null, string[] stringDescs = null)
        {
            this.label = label;
            KeyDefs = keyDefs;
            this.stringDescs = stringDescs;
        }

        public HelpDetailSection(string label, string[] stringDescs = null)
        {
            this.label = label;
            KeyDefs = null;
            this.stringDescs = stringDescs;
        }

        public HelpDetailSection(string label, List<DefStringTriplet> keyDefs = null)
        {
            this.label = label;
            KeyDefs = keyDefs;
            stringDescs = null;
        }

        public HelpDetailSection(string label, List<Def> keyDefs, string[] prefixes = null, string[] suffixes = null)
        {
            this.label = label;
            if (keyDefs != null)
            {
                KeyDefs = HelpDetailSectionHelper.BuildDefStringTripletList(keyDefs, prefixes, suffixes);
            }
            else KeyDefs = null;
            stringDescs = null;
        }

        public string label;

        public static string inset = "\t";

        public string[] stringDescs;

        public List<DefStringTriplet> KeyDefs;

        public string GetString
        {
            get
            {
                StringBuilder s = new StringBuilder();
                if (label != null)
                {
                    inset = "\t";
                    s.AppendLine(label.CapitalizeFirst() + ":");
                }
                else
                {
                    inset = String.Empty;
                }

                if (stringDescs != null)
                {
                    foreach (string stringDesc in stringDescs)
                    {
                        s.Append(inset);
                        s.AppendLine(stringDesc);
                    }
                }

                if (KeyDefs != null)
                {
                    foreach (DefStringTriplet def in KeyDefs)
                    {
                        s.Append(inset);
#if DEBUG
                        // show that this could potentially be a linked def.
                        s.Append("*");
#endif
                        s.AppendLine(def.ToString());
                    }
                }

                return s.ToString();
            }
        }
    }
}
