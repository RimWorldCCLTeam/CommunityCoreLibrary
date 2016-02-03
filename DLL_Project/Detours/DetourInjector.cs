using System;
using System.Reflection;

namespace CommunityCoreLibrary
{

    public class DetourInjector : SpecialInjector
    {

        public override void                Inject()
        {

            // Detour Verse.GenSpawn.CanPlaceBlueprintOver
            MethodInfo Verse_GenSpawn_CanPlaceBlueprintOver = typeof( Verse.GenSpawn ).GetMethod( "CanPlaceBlueprintOver", BindingFlags.Static | BindingFlags.Public );
            MethodInfo CCL_GenSpawn_CanPlaceBlueprintOver = typeof( CommunityCoreLibrary.Detour.GenSpawn ).GetMethod( "CanPlaceBlueprintOver", BindingFlags.Static | BindingFlags.Public );
            Detours.TryDetourFromTo( Verse_GenSpawn_CanPlaceBlueprintOver, CCL_GenSpawn_CanPlaceBlueprintOver );

        }

    }

}
