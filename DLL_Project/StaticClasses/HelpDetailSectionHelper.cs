using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using UnityEngine;

namespace CommunityCoreLibrary.StaticClasses
{
    class HelpDetailSectionHelper
    {

        #region DefStringTriplet list builder

        public static List<DefStringTriplet> BuildDefStringTripletList(List<Def> defs, string[] prefixes = null, string[] suffixes = null)
        {
            bool pre = false, suf = false;
            if (prefixes != null)
            {
                if (prefixes.Length != defs.Count)
                {
                    throw new Exception("Prefix array length does not match Def list length.");
                }
                pre = true;
            }

            if (suffixes != null)
            {
                if (suffixes.Length != defs.Count)
                {
                    throw new Exception("Suffix array length does not match Def list length.");
                }
                suf = true;
            }

            List<DefStringTriplet> ret = new List<DefStringTriplet>();
            for (int i = 0; i < defs.Count; i++)
            {
                ret.Add(new DefStringTriplet(defs[i], pre ? prefixes[i] : null, suf ? suffixes[i] : null));
            }

            return ret;
        }

        #endregion
    }
}
