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

    public static class PlaceWorker_Restriction_Alert_Data
    {
        private static List< Thing >    destroyedThings;
        public static List< Thing >     DestroyedThings
        {
            get{ return destroyedThings; }
        }

        private static int              cooldownTicks;

        public static bool              AlertPlayer
        {
            get{ return ( destroyedThings.Count > 0 ); }
        }

        static PlaceWorker_Restriction_Alert_Data()
        {
            destroyedThings = new List<Thing>();
            cooldownTicks = 0;
        }

        public static void Cooldown( int ticks = 1 )
        {
            cooldownTicks -= ticks;
            if( ( cooldownTicks <= 0 )&&
                ( destroyedThings.Count > 0 ) )
                destroyedThings.Clear();
        }

        public static void Add( Thing thing )
        {
            destroyedThings.Add( thing );
            cooldownTicks = 250;
        }

    }

    public class Alert_PlaceWorker_Restriction : Alert_Critical
    {
        public override AlertReport Report
        {
            get
            {
                // Alert the player if something got destroyed
                if( PlaceWorker_Restriction_Alert_Data.AlertPlayer == false )
                    return false;
                
                // Return the first or default instance as the culprit
                return AlertReport.CulpritIs( PlaceWorker_Restriction_Alert_Data.DestroyedThings.FirstOrDefault() );
            }
        }

        public override string FullExplanation{
            get{
                var msg = new StringBuilder();
                msg.AppendLine( "AlertPlaceWorkerRestrictionSupportRemovedDesc".Translate() );
                foreach( var t in PlaceWorker_Restriction_Alert_Data.DestroyedThings )
                    msg.AppendLine( "   " + t.def.defName );
                return msg.ToString();
            }
        }
        public override void AlertActiveUpdate()
        {
            if( PlaceWorker_Restriction_Alert_Data.AlertPlayer == true )
            {
                base.AlertActiveUpdate();
                PlaceWorker_Restriction_Alert_Data.Cooldown();
            }
        }

        public Alert_PlaceWorker_Restriction()
        {
            this.baseLabel = "AlertPlaceWorkerRestrictionSupportRemovedLabel".Translate();
        }

    }
}

