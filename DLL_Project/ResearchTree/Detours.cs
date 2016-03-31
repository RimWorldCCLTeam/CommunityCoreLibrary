using CommunityCoreLibrary;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using UnityEngine;
using Verse;

// ReSharper disable PossibleNullReferenceException
// reflection is dangerous - deal with it. Fluffy.

// Fixed dangerous jump across instance classes. E.

namespace CommunityCoreLibrary.ResearchTree.Detour
{
    internal static class _ResearchManager
    {
        /// <summary>
        /// Override for Verse.ResearchMananager.MakeProgress
        ///
        /// Changes default pop-up when research is complete to an inbox message, and starts the next research in the queue - if available.
        /// </summary>
        /// <param name="amount"></param>
        internal static void _MakeProgress( this ResearchManager researchManager, float amount )
        {
            // get research manager instance
            //ResearchManager researchManager = Find.ResearchManager;

            // get progress dictionary
            FieldInfo progressField = typeof( ResearchManager ).GetField( "progress", BindingFlags.Instance | BindingFlags.NonPublic );
            IDictionary<ResearchProjectDef, float> progress = progressField.GetValue( researchManager ) as IDictionary<ResearchProjectDef, float>;

            // get global progress constant
            FieldInfo globalProgressFactorField = typeof( ResearchManager ).GetField( "GlobalProgressFactor", BindingFlags.Instance | BindingFlags.NonPublic );
            float globalProgressFactor = (float)globalProgressFactorField.GetValue( researchManager );

            // make progress
            if ( researchManager.currentProj == null )
            {
                Log.Error( "Researched without having an active project." );
            }
            else
            {
                amount *= globalProgressFactor;
                if ( DebugSettings.fastResearch )
                {
                    amount *= 500f;
                }
                float curProgress = researchManager.ProgressOf( researchManager.currentProj );
                curProgress += amount;
                progress[researchManager.currentProj] = curProgress;

                // do message if finished
                if ( researchManager.currentProj.IsFinished )
                {
                    string label = "ResearchFinished".Translate( researchManager.currentProj.LabelCap );
                    string text = "ResearchFinished".Translate( researchManager.currentProj.LabelCap ) + "\n\n" + researchManager.currentProj.DescriptionDiscovered;

                    // remove from queue
                    Queue.Pop();

                    // if the completed research locks anything, notify it.
                    researchManager.currentProj.Node().Locks.ForEach( node => { node.Notify_LockedOut( true ); node.Notify_WillBeLockedOut( false ); } );

                    // if there's something on the queue start it, and push an appropriate message
                    if ( Queue.Count > 0 )
                    {
                        researchManager.currentProj = Queue.First().Research;
                        text += "\n\nNext in queue: " + researchManager.currentProj.LabelCap;
                        Find.LetterStack.ReceiveLetter( label, text, LetterType.Good );
                    }
                    else
                    {
                        researchManager.currentProj = null;
                        text += "\n\nNext in queue: none";
                        Find.LetterStack.ReceiveLetter( label, text, LetterType.BadNonUrgent );
                    }

                    // apply research mods (Why this isn't being done in a targeted way I don't know, but it's core behaviour...)
                    researchManager.ReapplyAllMods();
                }
            }
        }

        private static List<ResearchProjectDef> storeQueue = null;

        internal static void _ExposeData( this ResearchManager researchManager )
        {
            if ( !ResearchTree.Initialized )
            {
                // initialize tree
                ResearchTree.Initialize();
                
            }

            // get progress dictionary
            FieldInfo progressField = typeof( ResearchManager ).GetField( "progress", BindingFlags.Instance | BindingFlags.NonPublic );
            Dictionary<ResearchProjectDef, float> progress = progressField.GetValue( researchManager ) as Dictionary<ResearchProjectDef, float>;

            // Expose base data
            Scribe_Defs.LookDef<ResearchProjectDef>( ref researchManager.currentProj, "currentProj" );
            Scribe_Collections.LookDictionary<ResearchProjectDef, float>( ref progress, "progress", LookMode.DefReference, LookMode.Value );

            // Store research defs as these are the defining elements
            if ( Scribe.mode == LoadSaveMode.Saving )
            {
                storeQueue = Queue.ToList();
            }
            else if( storeQueue == null )
            {
                storeQueue = new List<ResearchProjectDef>();
            }

            Scribe_Collections.LookList( ref storeQueue, "Queue", LookMode.DefReference );

            if( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                Queue.FromList( storeQueue );
            }
        }

    }

}
