using System;
using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class HopperHelper
    {

        /// <summary>
        /// Determines if the specified thingDef is a hopper.
        /// </summary>
        /// <returns><c>true</c> if the specified thingDef is a hopper; otherwise, <c>false</c>.</returns>
        /// <param name="thingDef">ThingDef to test</param>
        public static bool                  IsHopper( this ThingDef thingDef )
        {
            return
                ( thingDef.thingClass == typeof( Building_Hopper ) )&&
                ( thingDef.HasComp( typeof( CompHopper ) ) );
        }


        /// <summary>
        /// Determines if the specified thingDef is a hopper user.
        /// </summary>
        /// <returns><c>true</c> if the specified thingDef is a hopper user; otherwise, <c>false</c>.</returns>
        /// <param name="thingDef">ThingDef to test</param>
        public static bool                  IsHopperUser( this ThingDef thingDef )
        {
            return
                ( thingDef.building != null )&&
                ( thingDef.building.wantsHopperAdjacent )&&
                ( thingDef.HasComp( typeof( CompHopperUser ) ) );
        }

        // TODO: thingfilter stuff is now private boooo
        // Do we really want to do this anyway?
        /*public static void                  BlockDefaultAcceptanceFilters( this ThingFilter filter, StorageSettings parentSettings = null )
        {
            // Explicitly remove auto-added special filters unless they are explicitly added
            foreach( var sf in DefDatabase<SpecialThingFilterDef>.AllDefsListForReading )
            {
                if( sf.allowedByDefault )
                {
                    var blockIt = false;
                    if(
                        ( filter.specialFiltersToAllow.NullOrEmpty() )||
                        ( !filter.specialFiltersToAllow.Contains( sf.defName ) )
                    )
                    {
                        blockIt = true;
                    }
                    if(
                        ( parentSettings != null )&&
                        (
                            ( parentSettings.filter.specialFiltersToAllow.NullOrEmpty() )||
                            ( !parentSettings.filter.specialFiltersToAllow.Contains( sf.defName ) )
                        )
                    )
                    {
                        blockIt = true;
                    }
                    if( blockIt )
                    {
                        if( filter.specialFiltersToDisallow.NullOrEmpty() )
                        {
                            filter.specialFiltersToDisallow = new List<string>();
                        }
                        filter.specialFiltersToDisallow.Add( sf.defName );
                        filter.SetAllow( sf, false );
                    }
                }
            }
        }*/

    }

}
