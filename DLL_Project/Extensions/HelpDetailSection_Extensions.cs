using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    public static class HelpDetailSection_Extensions
    {
        /// <summary>
        /// Build the display string for a HelpDetailSection
        /// </summary>
        /// <param name="hds"></param>
        /// <returns></returns>
        public static string GetString(this HelpDetailSection hds)
        {
            StringBuilder s = new StringBuilder();
            if (hds.Label != null)
            {
                s.AppendLine(hds.Label.CapitalizeFirst() + ":");
            }

            if (hds.StringDescs != null)
            {
                foreach (StringDescTriplet stringDesc in hds.StringDescs )
                {
                    s.Append(hds.InsetString);
                    s.AppendLine(stringDesc.ToString());
                }
            }

            if (hds.KeyDefs != null)
            {
                foreach (DefStringTriplet def in hds.KeyDefs)
                {
                    s.Append(hds.InsetString);
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
