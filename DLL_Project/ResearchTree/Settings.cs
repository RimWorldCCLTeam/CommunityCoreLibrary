using UnityEngine;

namespace CommunityCoreLibrary.ResearchTree
{
    public static class Settings
    {
        #region tuning parameters

        public static int     LineMaxLengthNodes = 10;
        public static int     MinTrunkSize       = 2;

        #endregion tuning parameters

        #region UI elements

        public static float   HubSize            = 16f;
        public static Vector2 IconSize           = new Vector2( 18f, 18f );
        public static Vector2 NodeMargins        = new Vector2( 50f, 10f );
        public static Vector2 NodeSize           = new Vector2( 200f, 50f );
        public static int     TipID              = 24 * 1271;

        #endregion UI elements
    }
}