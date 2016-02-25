using CommunityCoreLibrary;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

// ReSharper disable PossibleNullReferenceException
// reflection is dangerous - deal with it. Fluffy.

namespace CommunityCoreLibrary.ResearchTree
{
    public class Queue : MapComponent
    {
        #region Fields

        internal static readonly Texture2D      CircleFill = ContentFinder<Texture2D>.Get("UI/ResearchTree/circle-fill");
        private static readonly List<Node>      _queue     = new List<Node>();
        private static List<ResearchProjectDef> _saveableQueue;

        #endregion Fields

        #region Methods

        public static void Dequeue( Node node )
        {
            _queue.Remove( node );
            List<Node> followUps = _queue.Where( n => n.GetMissingRequiredRecursive().Contains( node ) ).ToList();
            foreach ( Node followUp in followUps )
            {
                _queue.Remove( followUp );
            }
        }

        public static void DrawLabels()
        {
            int i = 1;
            foreach ( Node node in _queue )
            {
                // draw coloured tag
                GUI.color = node.Tree.MediumColor;
                GUI.DrawTexture( node.QueueRect, CircleFill );

                // if this is not first in line, grey out centre of tag
                if ( i > 1 )
                {
                    GUI.color = node.Tree.GreyedColor;
                    GUI.DrawTexture( node.QueueRect.ContractedBy( 2f ), CircleFill );
                }

                // draw queue number
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label( node.QueueRect, i++.ToString() );
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        public static void Enqueue( Node node, bool add )
        {
            // if we're not adding, clear the current queue and current research project
            if ( !add )
            {
                NotifyAll();
                _queue.Clear();
                Find.ResearchManager.currentProj = null;
            }
            else
            {
                foreach ( Node queuedNode in _queue )
                {
                    if ( queuedNode.Locks.Contains( node ) )
                    {
                        Messages.Message( "Fluffy.ResearchTree.CannotQueueXLocksY".Translate(
                                "Fluffy.ResearchTree.QueuedNode".Translate() + " " + queuedNode.Research.LabelCap,
                                node.Research.LabelCap ) + " " +
                                "Fluffy.ResearchTree.CannotQueueDequeue".Translate(),
                            MessageSound.RejectInput );
                        return;
                    }
                    if ( node.Locks.Contains( queuedNode ) && !_queue.Contains( node ) )
                    {
                        Messages.Message( "Fluffy.ResearchTree.CannotQueueXLocksY".Translate(
                                node.Research.LabelCap,
                                "Fluffy.ResearchTree.QueuedNode".Translate() + " " + queuedNode.Research.LabelCap ) + " " +
                                "Fluffy.ResearchTree.CannotQueueDequeue".Translate(),
                            MessageSound.RejectInput );
                        return;
                    }
                }
            }

            // add to the queue if not already in it
            if ( !_queue.Contains( node ) )
            {
                node.Locks.ForEach( n => n.Notify_WillBeLockedOut( true ) );
                _queue.Add( node );
            }

            // try set the first research in the queue to be the current project.
            Node next = _queue.First();
            Find.ResearchManager.currentProj = next?.Research; // null if next is null.
        }

        public static void EnqueueRange( IEnumerable<Node> nodes, bool add )
        {
            // clear current Queue if not adding
            if ( !add )
            {
                NotifyAll();
                _queue.Clear();
                Find.ResearchManager.currentProj = null;
            }
            else
            {
                foreach ( Node queuedNode in _queue )
                {
                    foreach ( Node newNode in nodes )
                    {
                        if ( queuedNode.Locks.Contains( newNode ) )
                        {
                            Messages.Message( "Fluffy.ResearchTree.CannotQueueXLocksY".Translate(
                                    "Fluffy.ResearchTree.QueuedNode".Translate() + " " + queuedNode.Research.LabelCap,
                                    newNode.Research.LabelCap ) + " " +
                                    "Fluffy.ResearchTree.CannotQueueDequeue".Translate(),
                                MessageSound.RejectInput );
                            return;
                        }
                        if ( newNode.Locks.Contains( queuedNode ) && !_queue.Contains( newNode ) )
                        {
                            Messages.Message( "Fluffy.ResearchTree.CannotQueueXLocksY".Translate(
                                    newNode.Research.LabelCap,
                                    "Fluffy.ResearchTree.QueuedNode".Translate() + " " + queuedNode.Research.LabelCap ) + " " +
                                    "Fluffy.ResearchTree.CannotQueueDequeue".Translate(),
                                MessageSound.RejectInput );
                            return;
                        }
                    }
                }
            }

            // sorting by depth ensures prereqs are met - cost is just a bonus thingy.
            foreach ( Node node in nodes.OrderBy( node => node.Depth ).ThenBy( node => node.Research.totalCost ) )
            {
                Enqueue( node, true );
            }
            Node first = _queue.First();
            Find.ResearchManager.currentProj = first?.Research;
        }

        public static bool IsQueued( Node node )
        {
            return _queue.Contains( node );
        }

        public static void NotifyAll()
        {
            foreach ( Node node in _queue )
            {
                node.Locks.ForEach( n => n.Notify_WillBeLockedOut( false ) );
            }
        }

        /// <summary>
        /// Removes and returns the first node in the queue.
        /// </summary>
        /// <returns></returns>
        public static Node Pop()
        {
            if ( _queue != null && _queue.Count > 0 )
            {
                Node node = _queue[0];
                _queue.RemoveAt( 0 );
                return node;
            }
            return null;
        }

        public override void ExposeData()
        {
            // store research defs as these are the defining elements
            if ( Scribe.mode == LoadSaveMode.Saving )
            {
                _saveableQueue = _queue.Select( node => node.Research ).ToList();
            }

            Scribe_Collections.LookList( ref _saveableQueue, "Queue", LookMode.DefReference );

            if ( Scribe.mode == LoadSaveMode.PostLoadInit )
            {
                // initialize the tree if not initialized
                if ( !ResearchTree.Initialized )
                    ResearchTree.Initialize();

                // initialize the queue
                foreach ( ResearchProjectDef research in _saveableQueue )
                {
                    // find a node that matches the research - or null if none found
                    Node node = ResearchTree.Forest.FirstOrDefault( n => n.Research == research );

                    // enqueue the node
                    if ( node != null )
                    {
                        Enqueue( node, true );
                    }
                }
            }
        }

        /// <summary>
        /// Override for Verse.ResearchMananager.MakeProgress
        ///
        /// Changes default pop-up when research is complete to an inbox message, and starts the next research in the queue - if available.
        /// </summary>
        /// <param name="amount"></param>
        public void MakeProgress( float amount )
        {
            // get research manager instance
            ResearchManager researchManager = Find.ResearchManager;

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
                    Pop();

                    // if the completed research locks anything, notify it.
                    researchManager.currentProj.Node().Locks.ForEach( node => { node.Notify_LockedOut( true ); node.Notify_WillBeLockedOut( false ); } );

                    // if there's something on the queue start it, and push an appropriate message
                    if ( _queue.Count > 0 )
                    {
                        researchManager.currentProj = _queue.First().Research;
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

        #endregion Methods
    }
}