using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace CommunityCoreLibrary
{
    
    public class ThinkNode_PrioritySorter_SubtreesByTag : ThinkNode_PrioritySorter
    {
        
        public string                       insertTag;

        private List<ThinkTreeDef>          _matchedTrees;

        private List<ThinkTreeDef>          matchedTrees
        {
            get
            {
                if( _matchedTrees == null )
                {
                    _matchedTrees = DefDatabase<ThinkTreeDef>
                        .AllDefs
                        .Where( def => def.insertTag == this.insertTag )
                        .OrderByDescending( def => def.insertPriority )
                        .ToList();
                }
                return _matchedTrees;
            }
        }

        public override ThinkNode           DeepCopy()
        {
            var nodes = (ThinkNode_PrioritySorter_SubtreesByTag) base.DeepCopy();
            nodes.insertTag = this.insertTag;
            nodes.minPriority = this.minPriority;
            return (ThinkNode) nodes;
        }

        protected override void             ResolveSubnodes()
        {
            if( _matchedTrees.NullOrEmpty() )
            {
                return;
            }
            using( List<ThinkTreeDef>.Enumerator enumerator = matchedTrees.GetEnumerator() )
            {
                while( enumerator.MoveNext() )
                {
                    this.subNodes.Add( enumerator.Current.thinkRoot );
                }
            }
        }

        private ThinkNode                   HighestPriorityNode( Pawn pawn )
        {
            if( matchedTrees.NullOrEmpty() )
            {
                return null;
            }
            var priority = 0.0f;
            ThinkNode node = null;
            for( var index = 0; index < matchedTrees.Count; ++index )
            {
                var nodePriority = matchedTrees[ index ].thinkRoot.GetPriority( pawn );
                if( nodePriority > priority )
                {
                    priority = nodePriority;
                    node = matchedTrees[ index ].thinkRoot;
                }
            }
            return node;
        }

        public override float               GetPriority( Pawn pawn )
        {
            if( matchedTrees.NullOrEmpty() )
            {
                return 0.0f;
            }
            var priority = 0.0f;
            for( var index = 0; index < matchedTrees.Count; ++index )
            {
                var nodePriority = matchedTrees[ index ].thinkRoot.GetPriority( pawn );
                if( nodePriority > priority )
                {
                    priority = nodePriority;
                }
            }
            return priority;
        }
        
        public override ThinkResult         TryIssueJobPackage( Pawn pawn )
        {
            if( matchedTrees.NullOrEmpty() )
            {
                return ThinkResult.NoJob;
            }
            var thinkNode = HighestPriorityNode( pawn );
            if( thinkNode == null )
            {
                return ThinkResult.NoJob;
            }
            return thinkNode.TryIssueJobPackage( pawn );
        }

    }

}
