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

        public RefrigeratorContents()
        {
               // Default .ctor
        }

        public RefrigeratorContents( Thing t, CompRottable compRottable )
        {
            thing = t;
            HitPoints = t.HitPoints;
            rotProgress = compRottable.rotProgress;
        }

        public void ExposeData()
        {
            Scribe_References.LookReference( ref thing, "thing" );
            Scribe_Values.LookValue( ref HitPoints, "HitPoints" );
            Scribe_Values.LookValue( ref rotProgress, "rotProgress" );
        }

    }

    public class CompRefrigerated : ThingComp
    {
        private CompPowerTrader         compPower;
        private Building_Storage        thisBuilding;
        private List< RefrigeratorContents >    contents = new List< RefrigeratorContents >();

        private bool                    okToProcess = false;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Collections.LookList<RefrigeratorContents>( ref contents, "contents", LookMode.Deep );
        }

        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            okToProcess = false;

            // Get this building
            thisBuilding = (parent as Building_Storage);
            if( thisBuilding == null ) {
                Log.Message( "Community Core Library :: CompRefrigerated :: Unable to cast '" + parent.def.defName + "' to Building" );
                return;
            }

            // Get the power comp
            compPower = parent.GetComp<CompPowerTrader>();
            if( compPower == null )
            {
                Log.Message( "Community Core Library :: CompRefrigerated :: '" + parent.def.defName + "' needs compPowerTrader!" );
                return;
            }

            // Everything seems ok
            okToProcess = true;

        }

        public void ScanForRottables()
        {
            // Check for things removed
            bool restartScan;
            do
            {
                restartScan = false;
                foreach( RefrigeratorContents item in contents )
                {
                    if( ( item.thing.Destroyed )||
                        ( item.thing.StoringBuilding() != thisBuilding ) )
                    {
                        // Modifing list, restart scan
                        contents.Remove( item );
                        restartScan = true;
                        break;
                    }
                }
            }while( restartScan == true );

            // Add new things
            foreach( Thing t in thisBuilding.slotGroup.HeldThings )
            {
                CompRottable compRottable = t.TryGetComp<CompRottable>();
                if( ( compRottable != null )&&
                    ( contents.Find( item => item.thing == t ) == null ) )
                    contents.Add( new RefrigeratorContents( t, compRottable ) );
            }

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
            if( !okToProcess ) return;

            // Only refrigerate if it has power
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
    }
}