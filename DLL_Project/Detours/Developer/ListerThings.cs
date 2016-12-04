// This class is simply for logging calls.  It does nothing beyond vanilla functionality.

#if DEVELOPER
// Enable this define for a whole bunch of debug logging
//#define _I_AM_A_POTATO_
// Enable this define to target only a specific def, see MonitorForDef below
#define _I_AM_WATCHING_YOU_
// Enable this define to use log capturing to consolidate log output
#define _USE_LOG_CAPTURING_
#endif

#if _I_AM_A_POTATO_

using System;
using System.Collections.Generic;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

using CommunityCoreLibrary;

namespace CommunityCoreLibrary.Detour
{

    internal static class _ListerThings
    {

#if _I_AM_WATCHING_YOU_
        // This is the defName or partial ThingID to watch for, see _I_AM_WATCHING_YOU_ at the top
        private const string                MonitorForDef = "Synthesizer";
#endif
        
        [DetourMember( typeof( ListerThings ) )]
        internal static void                _Add( this ListerThings _this, Thing t )
        {
            bool logThisCall =
#if _I_AM_WATCHING_YOU_
            (
                ( t.def.defName == MonitorForDef )||
                ( t.ThingID.Contains( MonitorForDef ) )
            );
#else
            true;
#endif

#if _USE_LOG_CAPTURING_
            StringBuilder stringBuilder = logThisCall ? new StringBuilder() : null;
            if( logThisCall )
            {
                CCL_Log.CaptureBegin( stringBuilder );
            }
#endif
            
            if( !ListerThings.EverListable( t.def, _this.use ) )
            {
                return;
            }

            var listsByDef = _this.ListsByDef();
            var listsByGroup = _this.ListsByGroup();

            if( logThisCall )
            {
                if( listsByDef == null )
                {
                    CCL_Log.Message(
                        string.Format( "Unable to get listsByDef when examining '{0}' - '{1}'", t.def.defName, t.ThingID ),
                        "Detour.ListerThings.Add" );
                }
                if( listsByGroup == null )
                {
                    CCL_Log.Message(
                        string.Format( "Unable to get listsByGroup when examining '{0}' - '{1}'", t.def.defName, t.ThingID ),
                        "Detour.ListerThings.Add" );
                }
            }

            if(
                ( listsByDef == null )||
                ( listsByGroup == null )
            )
            {
#if _USE_LOG_CAPTURING_
                if( logThisCall )
                {
                    CCL_Log.Message(
                        string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                        "Detour.ListerThings.Add"
                    );
                    CCL_Log.CaptureEnd( stringBuilder );
                    CCL_Log.Message( stringBuilder.ToString(), "ListerThings.Add Trace" );
                }
#endif
                return;
            }

            List<Thing> thingsByDef;
            if( !listsByDef.TryGetValue( t.def, out thingsByDef ) )
            {
                thingsByDef = new List<Thing>();
                listsByDef[ t.def ] = thingsByDef;
            }
            if( logThisCall )
            {
                CCL_Log.Message(
                    string.Format( "Adding '{0}' to lists by Defs named '{1}'", t.ThingID, t.def.defName ),
                    "Detour.ListerThings.Add" );
            }
            thingsByDef.Add( t );

            for( int index = 0; index < ThingListGroupHelper.AllGroups.Length; ++index )
            {
                var group = ThingListGroupHelper.AllGroups[ index ];
                if(
                    (
                        ( _this.use != ListerThingsUse.Region )||
                        ( group.StoreInRegion() )
                    )&&
                    ( group.Includes( t.def ) )
                )
                {
                    var thingsByGroup = listsByGroup[ (int) group ];
                    if( thingsByGroup == null )
                    {
                        thingsByGroup = new List<Thing>();
                        listsByGroup[ (int) group ] = thingsByGroup;
                    }
                    if( logThisCall )
                    {
                        CCL_Log.Message(
                            string.Format( "Adding '{0}' to lists by group of '{1}'", t.ThingID, group.ToString() ),
                            "Detour.ListerThings.Add" );
                    }
                    thingsByGroup.Add( t );
                }
            }
#if _USE_LOG_CAPTURING_
            if( logThisCall )
            {
                CCL_Log.Message(
                    string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                    "Detour.ListerThings.Add"
                );
                CCL_Log.CaptureEnd( stringBuilder );
                CCL_Log.Message( stringBuilder.ToString(), "ListerThings.Add Trace" );
            }
#endif
        }

        [DetourMember( typeof( ListerThings ) )]
        internal static void                _Remove( this ListerThings _this, Thing t )
        {
            bool logThisCall =
#if _I_AM_WATCHING_YOU_
            (
                ( t.def.defName == MonitorForDef )||
                ( t.ThingID.Contains( MonitorForDef ) )
            );
#else
            true;
#endif

#if _USE_LOG_CAPTURING_
            StringBuilder stringBuilder = logThisCall ? new StringBuilder() : null;
            if( logThisCall )
            {
                CCL_Log.CaptureBegin( stringBuilder );
            }
#endif

            if( !ListerThings.EverListable( t.def, _this.use ) )
            {
#if _USE_LOG_CAPTURING_
                if( logThisCall )
                {
                    CCL_Log.Message(
                        string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                        "Detour.ListerThings.Remove"
                    );
                    CCL_Log.CaptureEnd( stringBuilder );
                    CCL_Log.Message( stringBuilder.ToString(), "ListerThings.Remove Trace" );
                }
#endif
                return;
            }

            var listsByDef = _this.ListsByDef();
            var listsByGroup = _this.ListsByGroup();

            if( logThisCall )
            {
                if( listsByDef == null )
                {
                    CCL_Log.Message(
                        string.Format( "Unable to get listsByDef when examining '{0}' - '{1}'", t.def.defName, t.ThingID ),
                        "Detour.ListerThings.Remove" );
                }
                if( listsByGroup == null )
                {
                    CCL_Log.Message(
                        string.Format( "Unable to get listsByGroup when examining '{0}' - '{1}'", t.def.defName, t.ThingID ),
                        "Detour.ListerThings.Remove" );
                }
            }

            if(
                ( listsByDef == null )||
                ( listsByGroup == null )
            )
            {
#if _USE_LOG_CAPTURING_
                if( logThisCall )
                {
                    CCL_Log.Message(
                        string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                        "Detour.ListerThings.Remove"
                    );
                    CCL_Log.CaptureEnd( stringBuilder );
                    CCL_Log.Message( stringBuilder.ToString(), "ListerThings.Remove Trace" );
                }
#endif
                return;
            }

            if( logThisCall )
            {
                CCL_Log.Message(
                    string.Format( "Removing '{0}' from lists by Defs named '{1}'", t.ThingID, t.def.defName ),
                    "Detour.ListerThings.Remove" );
            }
            listsByDef[ t.def ].Remove( t );

            for( int index = 0; index < ThingListGroupHelper.AllGroups.Length; ++index )
            {
                var group = ThingListGroupHelper.AllGroups[ index ];
                if(
                    (
                        ( _this.use != ListerThingsUse.Region )||
                        ( group.StoreInRegion() )
                    )&&
                    ( group.Includes( t.def ) )
                )
                {
                    if( logThisCall )
                    {
                        CCL_Log.Message(
                            string.Format( "Removing '{0}' from lists by group of '{1}'", t.ThingID, group.ToString() ),
                            "Detour.ListerThings.Remove" );
                    }
                    listsByGroup[ index ].Remove( t );
                }
            }
#if _USE_LOG_CAPTURING_
            if( logThisCall )
            {
                CCL_Log.Message(
                    string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                    "Detour.ListerThings.Remove"
                );
                CCL_Log.CaptureEnd( stringBuilder );
                CCL_Log.Message( stringBuilder.ToString(), "ListerThings.Remove Trace" );
            }
#endif
        }

    }

}

#endif
