using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    [StaticConstructorOnStartup]
    public static class Data
    {

        #region Instance Data

        public static readonly string       UnityObjectName = "Community Core Library";
        public static GameObject            UnityObject;

        public static Assembly              Assembly_CSharp;

        public static MainMonoBehaviour     cclMonoBehaviour;

        public static string                cclModIdentifier = string.Empty;
        public static ModContentPack        cclMod;
        public static ModHelperDef          cclHelperDef;

        private static Dictionary< ModContentPack, ModHelperDef > dictModHelperDefs;
        private static List< ModContentPack >    mods;
        private static List< ModHelperDef > modHelperDefs;
        private static List< AdvancedResearchDef > advancedResearchDefs;

        private static List<MCMHost>        mcmHosts;

        private static List<MiniMap.MiniMap> miniMaps;

        internal static SubController[]     SubControllers;

        // Some settings or windows require a game restart
        public static bool                  RequireRestart = false;
        public static bool                  ReloadingPlayData = false;
        public static bool                  PlayWithoutRestart = false;
        public static bool                  RestartWarningIsOpen = false;
        public static bool                  WarnedAboutRestart = false;

        // For tracing in global functions
        private static ModHelperDef         _Trace_Current_Mod = null;

        #endregion

        #region Consructors
        static                              Data()
        {
            Assembly_CSharp = Assembly.Load( "Assembly-CSharp.dll" );
#if DEBUG
            if( Assembly_CSharp == null )
            {
                CCL_Log.Error( "Unable to load 'Assembly-CSharp'" );
                return;
            }
#endif
        }

        #endregion

        #region Static Properties

        public static ModHelperDef          Trace_Current_Mod
        {
            get
            {
                if( _Trace_Current_Mod == null )
                {
                    return cclHelperDef;
                }
                return _Trace_Current_Mod;
            }
            set
            {
                _Trace_Current_Mod = value;
            }
        }

        // All controllers will use these lists when working on the global set.
        // These lists are properly maintained for the game state based on varing
        // factors.  The DefDatabase should only be used as a list of all potential
        // defs where as these lists are the working set used by CCL.

        public static List< ModContentPack >  Mods
        {
            get
            {
                if( mods == null )
                {
                    // Get the initial raw set of mods
                    mods = new List<ModContentPack>();
                }
                return mods;
            }
        }

        public static List< ModHelperDef >  ModHelperDefs
        {
            get
            {
                if( modHelperDefs == null )
                {
                    // Get the initial raw set of mods
                    modHelperDefs = new List<ModHelperDef>();
                }
                return modHelperDefs;
            }
        }

        public static Dictionary< ModContentPack, ModHelperDef > DictModHelperDefs
        {
            get
            {
                if( dictModHelperDefs == null )
                {
                    // Get the initial raw set of mods
                    dictModHelperDefs = new Dictionary<ModContentPack, ModHelperDef>();
                }
                return dictModHelperDefs;
            }
        }

        public static List< AdvancedResearchDef > AdvancedResearchDefs
        {
            get
            {
                if( advancedResearchDefs == null )
                {
                    // Get the initial ordered raw set of advanced research
                    advancedResearchDefs = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();
                }
                return advancedResearchDefs;
            }
        }

        public static List< MCMHost >   MCMHosts
        {
            get
            {
                if( mcmHosts == null )
                {
                    mcmHosts = new List<MCMHost>();
                }
                return mcmHosts;
            }
        }

        public static List<MiniMap.MiniMap> MiniMaps
        {
            get
            {
                if( miniMaps == null )
                {
                    miniMaps = new List<MiniMap.MiniMap>();
                }
                return miniMaps;
            }
        }

        #endregion

    }

}
