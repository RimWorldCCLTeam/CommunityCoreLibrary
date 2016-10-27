using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{

    [SpecialInjectorSequencer( InjectionSequence.MainLoad, InjectionTiming.SpecialInjectors )]
    public class DetourInjector : SpecialInjector
    {

        public override bool                Inject()
        {
            // Change CompGlower into CompGlowerToggleable
            FixGlowers();

            // Change Building_NutrientPasteDispenser into Building_AdvancedPasteDispenser
            UpgradeNutrientPasteDispensers();

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
                    return false;
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

        private void                        FixGlowers()
        {
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => (
                ( def != null )&&
                ( def.comps != null )&&
                ( def.HasComp( typeof( CompGlower ) ) )
            ) ) )
            {
                var compGlower = def.GetCompProperties<CompProperties_Glower>();
                compGlower.compClass = typeof( CompGlowerToggleable );
            }
        }

        private void                        UpgradeNutrientPasteDispensers()
        {
            foreach( var def in DefDatabase<ThingDef>.AllDefs.Where( def => def.thingClass == typeof( Building_NutrientPasteDispenser ) ) )
            {
                def.thingClass = typeof( Building_AdvancedPasteDispenser );
            }
        }

    }

}
