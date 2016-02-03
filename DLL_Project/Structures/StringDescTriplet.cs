using System;
using System.Text;
using Verse;

namespace CommunityCoreLibrary
{
    public struct StringDescTriplet
    {
        public string StringDesc;
        public string Prefix;
        public string Suffix;

        public StringDescTriplet( string stringDesc, string prefix = null, string suffix = null )
        {
            StringDesc = stringDesc;
            Prefix = prefix;
            Suffix = suffix;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            if( Prefix != "" )
            {
                s.Append( Prefix + " " );
            }
            s.Append( StringDesc );
            if( Suffix != "" )
            {
                s.Append( " " + Suffix );
            }
            return s.ToString();
        }
    }
}