using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using Verse;
using Verse.AI;

namespace CommunityCoreLibrary
{
    
    public static class ThinkNode_Extensions
    {

        public static void                  ReplaceThinkNodeClass( this ThinkNode node, Type oldClass, Type newClass )
        {
            if( node.subNodes.NullOrEmpty() )
            {
                return;
            }
            // Do replacement in this node
            node.ReplaceSubNodeClass( oldClass, newClass );
            // Now check nodes subnodes
            foreach( var subNode in node.subNodes )
            {
                subNode.ReplaceThinkNodeClass( oldClass, newClass );
            }
        }

        public static void                  ReplaceSubNodeClass( this ThinkNode node, Type oldClass, Type newClass )
        {
            if( node.subNodes.NullOrEmpty() )
            {
                return;
            }
            for( int i = node.subNodes.Count - 1; i >= 0; --i )
            {
                var subNode = node.subNodes[ i ];
                if( subNode.GetType() == oldClass )
                {
                    // Remove old node
                    node.subNodes.Remove( subNode );
                    // Create new node
                    subNode = (ThinkNode) Activator.CreateInstance( newClass );
                    // Place new node in same place as old node
                    node.subNodes.Insert( i, subNode );
                }
            }
        }

    }

}
