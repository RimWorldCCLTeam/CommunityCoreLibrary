using System;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{

    public struct DefStringTriplet
    {

        // Research project and last completion flag
        public Def Def;
        public string Prefix;
        public string Suffix;

        public DefStringTriplet(Def def, string prefix = "", string suffix = "")
        {
            Def = def;
            Prefix = prefix;
            Suffix = suffix;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if (Prefix != "")
            {
                s.Append(Prefix + " ");
            }
            s.Append(Def.LabelCap);
            if (Suffix != "")
            {
                s.Append(" " + Suffix);
            }
            return s.ToString();
        }
    }

}
