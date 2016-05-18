using System.Collections.Generic;

using Verse;

namespace CommunityCoreLibrary
{

    public static class Light
    {

        public static List< ColorName >     Color = new List< ColorName > {
            new ColorName( "colorWhite".Translate() , new ColorInt( 217, 217, 217,   0 ) ),
            new ColorName( "colorRed".Translate()   , new ColorInt( 217,   0,   0,   0 ) ),
            new ColorName( "colorGreen".Translate() , new ColorInt(   0, 217,   0,   0 ) ),
            new ColorName( "colorBlue".Translate()  , new ColorInt(   0,   0, 217,   0 ) ),
            new ColorName( "colorYellow".Translate(), new ColorInt( 217, 217,  43,   0 ) ),
            new ColorName( "colorOrange".Translate(), new ColorInt( 255, 132,   0,   0 ) ),
            new ColorName( "colorPurple".Translate(), new ColorInt( 185,  61, 205,   0 ) ),
        };

    }

}
