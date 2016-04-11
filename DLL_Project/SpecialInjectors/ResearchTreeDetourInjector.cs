using System;
using System.Reflection;
using System.Security.Permissions;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.ResearchTree
{

    public class DetourInjector : SpecialInjector
    {

        public override bool                Inject()
        {
            // Override base ResearchManager methods to expand functionality

            // Detour RimWorld.ResearchManager.MakeProgress
            MethodInfo RimWorld_ResearchManager_MakeProgress = typeof( ResearchManager ).GetMethod( "MakeProgress", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ResearchTree_ResearchManager_MakeProgress = typeof( Detour._ResearchManager ).GetMethod( "_MakeProgress", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_ResearchManager_MakeProgress, CCL_ResearchTree_ResearchManager_MakeProgress ) )
                return false;

            // Detour RimWorld.ResearchManager.ExposeData
            MethodInfo RimWorld_ResearchManager_ExposeData = typeof( ResearchManager ).GetMethod( "ExposeData", BindingFlags.Instance | BindingFlags.Public );
            MethodInfo CCL_ResearchTree_ResearchManager_ExposeData = typeof( Detour._ResearchManager ).GetMethod( "_ExposeData", BindingFlags.Static | BindingFlags.NonPublic );
            if( !Detours.TryDetourFromTo( RimWorld_ResearchManager_ExposeData, CCL_ResearchTree_ResearchManager_ExposeData ) )
                return false;

            // Initialize the research tree
            //ResearchTree.Initialize();

            return true;
        }

    }

}
