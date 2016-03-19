using System;
using System.Reflection;
using System.Text;

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

            bool InjectionsOk = true;
            StringBuilder stringBuilder = new StringBuilder();
            CCL_Log.CaptureBegin( stringBuilder );

            // Create system controllers
            Controller.Data.SubControllers = new SubController[]
            {
                new Controller.LibrarySubController(),
                new Controller.ResearchSubController(),
                new Controller.InjectionSubController(),
                new Controller.ResourceSubController(),
                new Controller.HelpSubController()
            };

            // Detour Verse.ThingDef.PostLoad
            MethodInfo Verse_ThingDef_PostLoad = typeof( ThingDef ).GetMethod( "PostLoad", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ThingDef_PostLoad = typeof( Detour._ThingDef ).GetMethod( "_PostLoad", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( Verse_ThingDef_PostLoad, CCL_ThingDef_PostLoad );

            // Detour RimWorld.MainMenuDrawer.MainMenuOnGUI
            MethodInfo RimWorld_MainMenuDrawer_MainMenuOnGUI = typeof( MainMenuDrawer ).GetMethod( "MainMenuOnGUI", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_MainMenuDrawer_MainMenuOnGUI = typeof( Detour._MainMenuDrawer ).GetMethod( "_MainMenuOnGUI", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( RimWorld_MainMenuDrawer_MainMenuOnGUI, CCL_MainMenuDrawer_MainMenuOnGUI );

            // Detour RimWorld.MainMenuDrawer.DoMainMenuButtons
            MethodInfo RimWorld_MainMenuDrawer_DoMainMenuButtons = typeof( MainMenuDrawer ).GetMethod( "DoMainMenuButtons", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_MainMenuDrawer_DoMainMenuButtons = typeof( Detour._MainMenuDrawer ).GetMethod( "_DoMainMenuButtons", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( RimWorld_MainMenuDrawer_DoMainMenuButtons, CCL_MainMenuDrawer_DoMainMenuButtons );

            // Detour RimWorld.MainTabWindow_Menu.RequestedTabSize
            MethodInfo RimWorld_MainTabWindow_Menu_RequestedTabSize = typeof( MainTabWindow_Menu ).GetProperty( "RequestedTabSize", BindingFlags.Instance | BindingFlags.Public ).GetGetMethod();
            MethodInfo CCL_MainTabWindow_Menu_RequestedTabSize = typeof( Detour._MainTabWindow_Menu ).GetMethod( "_RequestedTabSize", BindingFlags.Static | BindingFlags.NonPublic );
            InjectionsOk &= Detours.TryDetourFromTo( RimWorld_MainTabWindow_Menu_RequestedTabSize, CCL_MainTabWindow_Menu_RequestedTabSize );

            CCL_Log.CaptureEnd(
                stringBuilder,
                InjectionsOk ? "Initialized" : "Errors during injection"
            );
            CCL_Log.Trace(
                Verbosity.Injections,
                stringBuilder.ToString(),
                "PreLoader" );
        }

#if DEVELOPER
        /* protected override */            ~PreLoader()
        {

            CCL_Log.CloseStream();

        }

#endif

    }

}
