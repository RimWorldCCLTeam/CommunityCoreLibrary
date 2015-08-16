using Verse;

namespace CommunityCoreLibrary
{

    public struct ColorName
    {
        
        public string                       name;
        public ColorInt                     value;

        public                              ColorName ( string n, ColorInt v )
        {
            name = n;
            value = v;
        }

    }

}
