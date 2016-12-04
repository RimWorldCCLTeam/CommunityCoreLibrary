// This class is simply for logging calls.  It does nothing beyond vanilla functionality.

#if DEVELOPER
// Enable this define to detour MapPawns for a whole bunch of debug logging
//#define _I_AM_A_POTATO_
// Enable this define to use log capturing to consolidate log output
#define _USE_LOG_CAPTURING_
#endif

#if _I_AM_A_POTATO_

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary.Detour
{

    internal class _MapPawns
    {

        internal static FieldInfo           _pawnsSpawned;

        static                              _MapPawns()
        {
            _pawnsSpawned = typeof( MapPawns ).GetField( "pawnsSpawned", Controller.Data.UniversalBindingFlags );
            if( _pawnsSpawned == null )
            {
                CCL_Log.Trace(
                    Verbosity.FatalErrors,
                    "Unable to get field 'pawnsSpawned' in 'MapPawns'",
                    "Detour.MapPawns" );
            }
        }

        #region Reflected Methods

        internal List<Pawn>                 PawnsSpawned
        {
            get
            {   // Is this a valid call?  "this" should be correctly pointing to MapPawns
                // as the only usage of this property is from within a detour
                return (List<Pawn>) _pawnsSpawned.GetValue( this );
            }
        }

        #endregion

        #region Detoured Methods

        [DetourMember( typeof( MapPawns ) )]
        public IEnumerable<Pawn>            _AllPawnsUnspawned
        {
            get
            {
#if _USE_LOG_CAPTURING_
                var stringBuilder = new StringBuilder();
                CCL_Log.CaptureBegin( stringBuilder );
#endif
                CCL_Log.Message(
                    "Checking for pawns carried by pawns",
                    "Detour.MapPawns.AllPawnsUnspawned"
                );
                var pawnsSpawned = PawnsSpawned;
                for( int index = 0; index < pawnsSpawned.Count; index++ )
                {
                    var innerPawn = pawnsSpawned[ index ].carrier.CarriedThing as Pawn;
                    if( innerPawn != null )
                    {
                        CCL_Log.Message(
                            string.Format( "Pawn '{0}' is carried by pawn '{1}'", innerPawn.LabelShort, pawnsSpawned[ index ].LabelShort ),
                            "Detour.MapPawns.AllPawnsUnspawned"
                        );
                        yield return innerPawn;
                    }
                }

                CCL_Log.Message(
                    "Checking for pawns stored in containers",
                    "Detour.MapPawns.AllPawnsUnspawned"
                );
                var containersList = Find.ListerThings.ThingsInGroup( ThingRequestGroup.Container );
                for( int index = 0; index < containersList.Count; index++ )
                {
                    var containerThing = containersList[ index ];
                    var iThingContainerOwner = containerThing as IThingContainerOwner;
                    var container = iThingContainerOwner.GetContainer();
                    for( int contentIndex = 0; contentIndex < container.Count; contentIndex++ )
                    {
                        var innerPawn = container[ contentIndex ] as Pawn;
                        if( innerPawn != null )
                        {
                            CCL_Log.Message(
                                string.Format( "Pawn '{0}' is in container '{1}'", innerPawn.LabelShort, containerThing.ThingID ),
                                "Detour.MapPawns.AllPawnsUnspawned"
                            );
                            yield return innerPawn;
                        }
                    }
                }

                CCL_Log.Message(
                    "Checking for pawns arriving in drop pods",
                    "Detour.MapPawns.AllPawnsUnspawned"
                );
                var dropPodsList = Find.ListerThings.ThingsInGroup( ThingRequestGroup.DropPod );
                for( int index = 0; index < dropPodsList.Count; index++ )
                {
                    var thing = dropPodsList[ index ];
                    CCL_Log.Message(
                        string.Format( "Examining thing '{0}' as DropPod, DropPod = '{1}', DropPodIncoming = '{2}'", thing.ThingID, thing is DropPod, thing is DropPodIncoming ),
                        "Detour.MapPawns.AllPawnsUnspawned"
                    );
                    var dropPod = thing as DropPod;
                    if( dropPod == null )
                    {
                        CCL_Log.Error(
                            string.Format( "Thing '{0}' is not a DropPod!", thing.ThingID ),
                            "Detour.MapPawns.AllPawnsUnspawned"
                        );
                    }
                    else
                    {
                        var dropPodIncoming = thing as DropPodIncoming;
                        var contents = dropPodIncoming != null
                            ? dropPodIncoming.contents
                            : dropPod.info;
                        
                        for( int contentIndex = 0; contentIndex < contents.containedThings.Count; contentIndex++ )
                        {
                            var innerPawn = contents.containedThings[ contentIndex ] as Pawn;
                            if( innerPawn != null )
                            {
                                CCL_Log.Message(
                                    string.Format( "Pawn '{0}' is arriving in drop pod '{1}'", innerPawn.LabelShort, dropPod.ThingID ),
                                    "Detour.MapPawns.AllPawnsUnspawned"
                                );
                                yield return innerPawn;
                            }
                        }
                    }
                }

                CCL_Log.Message(
                    "Checking for pawns in passing ships",
                    "Detour.MapPawns.AllPawnsUnspawned"
                );
                var shipList = Find.PassingShipManager.passingShips;
                for( int index = 0; index < shipList.Count; index++ )
                {
                    var tradeShip = shipList[ index ] as TradeShip;
                    if( tradeShip != null )
                    {
                        foreach( Thing thing in tradeShip.Goods )
                        {
                            var innerPawn = thing as Pawn;
                            if( innerPawn != null )
                            {
                                CCL_Log.Message(
                                    string.Format( "Pawn '{0}' is arriving in passing ship '{1}'", innerPawn.LabelShort, tradeShip.FullTitle ),
                                    "Detour.MapPawns.AllPawnsUnspawned"
                                );
                                yield return innerPawn;
                            }
                        }
                    }
                }
#if _USE_LOG_CAPTURING_
                CCL_Log.Message(
                    string.Format( "Stack trace:\n{0}", Environment.StackTrace ),
                    "Detour.MapPawns.AllPawnsUnspawned"
                );
                CCL_Log.CaptureEnd( stringBuilder );
                CCL_Log.Message( stringBuilder.ToString(), "MapPawns.AllPawnsUnspawned Trace" );
#endif
            }
        }

        #endregion

    }

}

#endif
