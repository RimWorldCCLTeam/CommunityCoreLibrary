using System.Collections.Generic;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{
    /*
    public class RefrigeratorContents : IExposable
    {
        // This is to handle the degredation of items in the refrigerator
        // It is capable of handling multiple items in multiple cells
        public Thing                        thing;
        public int                          HitPoints;
        public float                        rotProgress;
        public string                       thingID;

        public                              RefrigeratorContents()
        {
            // Default .ctor
            // Needed for Scribing
        }

        public                              RefrigeratorContents( Thing t, CompRottableRefrigerated compRottable )
        {
            thing = t;
            HitPoints = t.HitPoints;
            rotProgress = compRottable.rotProgress;
        }

        public void                         ExposeData()
        {
            //Log.Message( "\tRefrigeratorContents::ExposeData() {\n\t\tScribeMode: " + Scribe.mode );
            if( ( Scribe.mode == LoadSaveMode.Saving )&&
                ( thingID == null ) )
            {
                thingID = thing.ThingID;
            }

            Scribe_Values.LookValue( ref thingID, "rotThing" );
            Scribe_Values.LookValue( ref HitPoints, "HitPoints", forceSave: true );
            Scribe_Values.LookValue( ref rotProgress, "rotProgress", forceSave: true );

            //Log.Message( "\t} // RefrigeratorContents" );
        }

    }
    */

    public class CompRefrigerated : ThingComp
    {
        /*
        List< RefrigeratorContents >        contents = new List< RefrigeratorContents >();

        Building_Storage                    Building_Storage
        {
            get
            {
                return ( parent as Building_Storage );
            }
        }

        CompPowerTrader                     CompPowerTrader
        {
            get
            {
                return parent.TryGetComp< CompPowerTrader >();
            }
        }

        public override void                PostExposeData()
        {
            base.PostExposeData();

            //Log.Message( thisBuilding.ThingID + "::PostExposeData() {\n\tScribeMode: " + Scribe.mode );

            Scribe_Collections.LookList<RefrigeratorContents>( ref contents, "contents", LookMode.Deep );

            //Log.Message( "} // " + thisBuilding.ThingID + "::PostExposeData()" );
        }

#if DEBUG
        public override void                PostSpawnSetup()
        {
            //Log.Message( parent.def.defName + " - PostSpawnSetup()" );
            base.PostSpawnSetup();

            // Make sure the parent is the correct class
            if( Building_Storage == null )
            {
                Log.Error( "Community Core Library :: CompRefrigerated :: " + parent.def.defName + " requires parent class Building_Storage!" );
                return;
            }

            // Get the power comp
            if( CompPowerTrader == null )
            {
                Log.Error( "Community Core Library :: CompRefrigerated :: " + parent.def.defName + " requires CompPowerTrader!" );
                return;
            }

        }
#endif
        
        public override void                CompTick()
        {
            base.CompTick();

            // Only do it every 60 ticks to prevent lags
            if( !parent.IsHashIntervalTick( 60 ) )
            {
                return;
            }

            RefrigerateContents();
        }

        void                                RefrigerateContents()
        {
            // Only refrigerate if it has power
            // Don't worry about idle power though
            if( !CompPowerTrader.PowerOn )
            {
                // Clear it out
                if( contents.Count > 0 )
                {
                    contents = new List< RefrigeratorContents >();
                }
                // Now leave
                return;
            }

            // Look for things
            ScanForRottables();

            // Refrigerate the items
            foreach( RefrigeratorContents item in contents )
            {
                var compRottable = item.thing.TryGetComp< CompRottableRefrigerated >();
#if DEBUG
                if( compRottable == null )
                {
                    Log.Error( "Community Core Library :: CompRefrigerated :: " + item.thingID + " does not have CompRottable!" );
                    return;
                }
#endif
                item.thing.HitPoints = item.HitPoints;
                compRottable.rotProgress = item.rotProgress;
            }
        }

        public void                         ScanForRottables()
        {
            // Check for things removed
            for( int i = 0; i < contents.Count; ++i )
            {
                var item = contents[ i ];
                if( ( item.thing == null )&&
                    ( item.thingID != null ) )
                {
                    //Log.Message( "thingID.resolve:  '" + item.thingID + "'" );
                    item.thing = Find.ListerThings.AllThings.Find( t => t.ThingID == item.thingID );
                    //Log.Message( "thing.resolved:   '" + item.thing + "'" );
                }

                if( ( item.thing.Destroyed )||
                    ( item.thing.StoringBuilding() != Building_Storage ) )
                {
                    // Modifing list, continue scan
                    contents.Remove( item );
                    continue;
                }
            }
            // Add new things
            foreach( Thing t in Building_Storage.slotGroup.HeldThings )
            {
                CompRottableRefrigerated compRottable = t.TryGetComp< CompRottableRefrigerated >();
                if( ( compRottable != null )&&
                    ( !contents.Exists( item => item.thing == t ) ) )
                {
                    contents.Add( new RefrigeratorContents( t, compRottable ) );
                }
            }

        }
        */

    }

}
