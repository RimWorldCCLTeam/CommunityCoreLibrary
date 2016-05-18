using Verse;
using MainMenuDrawerExt = CommunityCoreLibrary.MainMenuDrawer_Extensions;

namespace CommunityCoreLibrary
{

    public static class                     MainMenuDrawer_LinkOptions
    {

        public static ListableOption        FictionPrimerOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "FictionPrimer".Translate(),
                    "https://docs.google.com/document/d/1pIZyKif0bFbBWten4drrm7kfSSfvBoJPgG9-ywfN8j8/pub",
                    MainMenuDrawerExt.IconBlog
                );
            }
        }

        public static ListableOption        BlogOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "LudeonBlog".Translate(),
                    "http://ludeon.com/blog",
                    MainMenuDrawerExt.IconBlog
                );
            }
        }

        public static ListableOption        ForumsOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "Forums".Translate(),
                    "http://ludeon.com/forums",
                    MainMenuDrawerExt.IconForums
                );
            }
        }

        public static ListableOption        WikiOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "OfficialWiki".Translate(),
                    "http://rimworldwiki.com",
                    MainMenuDrawerExt.IconBlog
                );
            }
        }

        public static ListableOption        TwitterOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "TynansTwitter".Translate(),
                    "https://twitter.com/TynanSylvester",
                    MainMenuDrawerExt.IconTwitter
                );
            }
        }

        public static ListableOption        DesignBookOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "TynansDesignBook".Translate(),
                    "http://tynansylvester.com/book",
                    MainMenuDrawerExt.IconBook
                );
            }
        }

        public static ListableOption        TranslateOption
        {
            get
            {
                return new ListableOption_WebLink(
                    "HelpTranslate".Translate(),
                    "http://ludeon.com/forums/index.php?topic=2933.0",
                    MainMenuDrawerExt.IconForums
                );
            }
        }

        public static ListableOption        BuySoundTrack
        {
            get
            {
                return new ListableOption_WebLink(
                    "BuySoundtrack".Translate(),
                    "http://www.lasgameaudio.co.uk/#!store/t04fw",
                    MainMenuDrawerExt.IconSoundtrack
                );
            }
        }

        /*
        public static ListableOption        Option
        {
            get
            {
                return new ListableOption_WebLink(
                    "".Translate(),
                    "",
                    MainMenuDrawerExt.Icon
                );
            }
        }
        */

    }

}
