using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
    public class RefrigeratorContents : IExposable
    {
        // This is to handle the degredation of items in the refrigerator
        // It is capable of handling multiple items in multiple cells
        public Thing                    thing;
        public int                      HitPoints;
        public float                    rotProgress;
        public string                   thingID;

        public RefrigeratorContents()
        {
            // Default .ctor
            // Needed for Scribing
        }

        public RefrigeratorContents( Thing t, CompRottable compRottable )
        {
            thing = t;
            HitPoints = t.HitPoints;
            rotProgress = compRottable.rotProgress;
        }

        public void ExposeData()
        {
            //Log.Message( "\tRefrigeratorContents::ExposeData() {\n\t\tScribeMode: " + Scribe.mode );
            if( ( Scribe.mode == LoadSaveMode.Saving )&&
                ( thingID == null ) ){
                thingID = thing.ThingID;
            }

            Scribe_Values.LookValue( ref this.thingID, "rotThing" );
            Scribe_Values.LookValue( ref this.HitPoints, "HitPoints", forceSave: true );
            Scribe_Values.LookValue( ref this.rotProgress, "rotProgress", forceSave: true );

            //Log.Message( "\t} // RefrigeratorContents" );
        }

    }

    public class CompRefrigerated : ThingComp
    {
        private List< RefrigeratorContents >    contents = new List< RefrigeratorContents >();

        private CompPowerTrader         compPower{
            get{ return parent.TryGetComp<CompPowerTrader>(); }
        }

        private Building_Storage        thisBuilding{
            get{ return (parent as Building_Storage); }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            //Log.Message( thisBuilding.ThingID + "::PostExposeData() {\n\tScribeMode: " + Scribe.mode );

            Scribe_Collections.LookList<RefrigeratorContents>( ref this.contents, "contents", LookMode.Deep );

            //Log.Message( "} // " + thisBuilding.ThingID + "::PostExposeData()" );
        }

        public override void CompTick()
        {
            base.CompTick();

            // Only do it every 60 ticks to prevent lags
            if( !Gen.IsHashIntervalTick( parent, 60 ) )
                return;

            RefrigerateContents();
        }

        private void RefrigerateContents()
        {
            // Only refrigerate if it has power
            // Don't worry about idle power though
            if( compPower.PowerOn == false )
            {
                // Clear it out
                if( contents.Count > 0 )
                    contents = new List<RefrigeratorContents>();
                // Now leave
                return;
            }

            // Look for things
            ScanForRottables();

            // Refrigerate the items
            foreach( RefrigeratorContents item in contents )
            {
                CompRottable compRottable = item.thing.TryGetComp<CompRottable>();
                item.thing.HitPoints = item.HitPoints;
                compRottable.rotProgress = item.rotProgress;
            }
        }

        public void ScanForRottables()
        {
            // Check for things removed
            for( int i = 0; i < contents.Count; ++i )
            {
                var item = contents[ i ];
                if( ( item.thing == null )&&
                    ( item.thingID != null ) ){
                    //Log.Message( "thingID.resolve:  '" + item.thingID + "'" );
                    item.thing = Find.ListerThings.AllThings.Find( t => t.ThingID == item.thingID );
                    //Log.Message( "thing.resolved:   '" + item.thing + "'" );
                }

                if( ( item.thing.Destroyed )||
                    ( item.thing.StoringBuilding() != thisBuilding ) )
                {
                    // Modifing list, continue scan
                    contents.Remove( item );
                    continue;
                }
            }
            // Add new things
            foreach( Thing t in thisBuilding.slotGroup.HeldThings )
            {
                CompRottable compRottable = t.TryGetComp<CompRottable>();
                if( ( compRottable != null )&&
                    ( contents.Find( item => item.thing == t ) == null ) )
                    contents.Add( new RefrigeratorContents( t, compRottable ) );
            }

        }

    }
}