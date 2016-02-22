using System;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Controller
{

    public class PreLoader : ThingDef
    {
        
        public                              PreLoader()
        {
            
            // This is a pre-start sequence to hook some deeper level functions.
            // These functions can be hooked later but it would be after the sequence
            // of operations which call them is complete.
            // This is done in the class constructor of a ThingDef override class so the
            // class PostLoad is not detoured while it's being executed for this object.

            // Log CCL version
            Version.Log();

            // Detour Verse.ThingDef.PostLoad
            MethodInfo Verse_ThingDef_PostLoad = typeof( ThingDef ).GetMethod( "PostLoad", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ThingDef_PostLoad = typeof( Detour._ThingDef ).GetMethod( "_PostLoad", BindingFlags.Static | BindingFlags.NonPublic );
            Detours.TryDetourFromTo( Verse_ThingDef_PostLoad, CCL_ThingDef_PostLoad );

        }

#if DEVELOPER
        /* protected override */            ~PreLoader()
        {

            CCL_Log.CloseStream();

        }

#endif

    }

}
