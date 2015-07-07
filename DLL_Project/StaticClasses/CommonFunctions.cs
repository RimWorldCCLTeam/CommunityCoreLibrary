using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CommunityCoreLibrary
{
    public static class Common
    {
        public static bool ThingHasThingCategoryDef( Thing t, ThingCategoryDef c )
        {
            if( t == null ) return false;
            if( t.def.thingCategories == null ) return false;
            foreach( ThingCategoryDef curCategory in t.def.thingCategories )
            {
                if( curCategory == c )
                    return true;
            }
            return false;
        }

        public static void ChangeThingDefDesignationCategoryOfTo( ThingDef t, string d )
        {
            // Set the designation category on the thing
            t.designationCategory = d;
            t.menuHidden = ( d == "None" );
        }

        public static void ClearBuildingRecipeCache( ThingDef building )
        {
            //Log.Message( building.LabelCap + " - Recaching" );
            typeof(ThingDef).GetField( "allRecipesCached", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( building, null );
        }

        public static void ResolveDesignationCategoryDefs()
        {
            // Resolve all designation category defs
            //Log.Message( "Redesignating" );
            DefDatabase< DesignationCategoryDef >.ClearCachedData();
            DefDatabase< DesignationCategoryDef >.ResolveAllReferences();
        }

        public static void SpawnThingDefOfCountAt( ThingDef of, int count, IntVec3 at )
        {
            while( count > 0 )
            {
                Thing newStack = ThingMaker.MakeThing( of, null );
                newStack.stackCount = Math.Min( count, of.stackLimit );
                GenSpawn.Spawn( newStack, at );
                count -= newStack.stackCount;
            }
        }

        public static void ReplaceThingWithThingDefOfCount( Thing oldThing, ThingDef of, int count )
        {
            IntVec3 at = oldThing.Position;
            oldThing.Destroy( DestroyMode.Vanish );
            while( count > 0 )
            {
                Thing newStack = ThingMaker.MakeThing( of, null );
                newStack.stackCount = Math.Min( count, of.stackLimit );
                GenSpawn.Spawn( newStack, at );
                count -= newStack.stackCount;
            }
        }

        public static void RemoveDesignationDefOfAt( DesignationDef of, IntVec3 at )
        {
            Designation designation = Find.DesignationManager.DesignationAt( at, of );
            if( designation != null )
            {
                Find.DesignationManager.RemoveDesignation( designation );
            }
        }

        public static void RemoveDesignationDefOfOn( DesignationDef of, Thing on )
        {
            Designation designation = Find.DesignationManager.DesignationOn( on, of );
            if( designation != null )
            {
                Find.DesignationManager.RemoveDesignation( designation );
            }
        }

    }
}