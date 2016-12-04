using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    [SpecialInjectorSequencer]
    public class DetourBuildSpecificMethods : SpecialInjector
    {

        public override bool                Inject()
        {

            // Detour RimWorld.MainTabWindow_Research.DrawLeftRect "NotFinished" predicate function
            // Use build number to get the correct predicate function
            var RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = string.Empty;
            var RimWorld_Build = RimWorld.VersionControl.CurrentBuild;
            switch( RimWorld_Build )
            {
            case 1279:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__495";
                break;
            case 1284:
                RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name = "<DrawLeftRect>m__496";
                break;
            default:
                CCL_Log.Trace(
                    Verbosity.Warnings,
                    "CCL needs updating for RimWorld build " + RimWorld_Build.ToString() );
                break;
            }
            if( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name != string.Empty )
            {
                MethodInfo RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished = typeof( RimWorld.MainTabWindow_Research ).GetMethod( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished_Name, Controller.Data.UniversalBindingFlags );
                MethodInfo CCL_MainTabWindow_Research_DrawLeftRect_NotFinishedNotLockedOut = typeof( Detour._MainTabWindow_Research ).GetMethod( "_NotFinishedNotLockedOut", Controller.Data.UniversalBindingFlags );
                if( !Detours.TryDetourFromTo( RimWorld_MainTabWindow_Research_DrawLeftRect_NotFinished, CCL_MainTabWindow_Research_DrawLeftRect_NotFinishedNotLockedOut ) )
                {
                    return false;
                }
            }

            /*
            // Detour 
            MethodInfo foo = typeof( foo_class ).GetMethod( "foo_method", Controller.Data.UniversalBindingFlags );
            MethodInfo CCL_bar = typeof( Detour._bar ).GetMethod( "_bar_method", Controller.Data.UniversalBindingFlags );
            if( !Detours.TryDetourFromTo( foo, CCL_bar ) )
                return false;

            */

            return true;
        }

    }

}
