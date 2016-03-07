using CommunityCoreLibrary;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

// ReSharper disable PossibleNullReferenceException
// reflection is dangerous - deal with it. Fluffy.

// Moved dangerous cross-instance class jump to it's own detour class.
// Created required static methods to perform the same access.  E.

namespace CommunityCoreLibrary.ResearchTree
{
    
    public static class Queue
    {
        #region Fields

        internal static readonly Texture2D      CircleFill = ContentFinder<Texture2D>.Get("UI/ResearchTree/circle-fill");
        private static readonly List<Node>      _queue     = new List<Node>();

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
            if(
                ( next != null )&&
                ( next.Research != null )
            )
            {
                Find.ResearchManager.currentProj = next.Research; // null if next is null.
            }
            else
            {
                Find.ResearchManager.currentProj = null;
            }
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
            if( !_queue.NullOrEmpty() )
            {
                Node node = _queue[0];
                _queue.RemoveAt( 0 );
                return node;
            }
            return null;
        }

        public static int Count
        {
            get
            {
                return _queue.Count;
            }
        }

        public static Node First()
        {
            return _queue.First();
        }

        public static List<ResearchProjectDef>  ToList()
        {
            return _queue.Select( node => node.Research ).ToList();
        }

        public static void                      FromList( List<ResearchProjectDef> loadQueue )
        {
            // initialize the queue
            foreach ( ResearchProjectDef research in loadQueue )
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

        #endregion Methods
    }

}
