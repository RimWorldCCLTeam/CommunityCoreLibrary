using System;
using System.Reflection;
using System.Security.Permissions;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.ResearchTree
{

    public class DetourInjector : SpecialInjector
    {

        // TODO:  Alpha 13 API change
        //public override bool Inject()
        public override void                Inject()
        {
            // Override base ResearchManager methods to expand functionality

            // Detour RimWorld.ResearchManager.MakeProgress
            MethodInfo RimWorld_ResearchManager_MakeProgress = typeof( ResearchManager ).GetMethod( "MakeProgress", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ResearchTree_ResearchManager_MakeProgress = typeof( Detour._ResearchManager ).GetMethod( "_MakeProgress", BindingFlags.Static | BindingFlags.NonPublic );
            Detours.TryDetourFromTo( RimWorld_ResearchManager_MakeProgress, CCL_ResearchTree_ResearchManager_MakeProgress );

            // Detour RimWorld.ResearchManager.ExposeData
            MethodInfo RimWorld_ResearchManager_ExposeData = typeof( ResearchManager ).GetMethod( "ExposeData", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ResearchTree_ResearchManager_ExposeData = typeof( Detour._ResearchManager ).GetMethod( "_ExposeData", BindingFlags.Static | BindingFlags.NonPublic );
            Detours.TryDetourFromTo( RimWorld_ResearchManager_ExposeData, CCL_ResearchTree_ResearchManager_ExposeData );

            // Initialize the research tree
            //ResearchTree.Initialize();

            // TODO:  Alpha 13 API change
            //return true;
        }

    }

}
