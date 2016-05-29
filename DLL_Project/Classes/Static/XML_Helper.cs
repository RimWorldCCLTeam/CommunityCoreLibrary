using System;
using System.Xml;

namespace CommunityCoreLibrary
{
    
    public static class XML_Helper
    {
        
        // TODO:  Move this method to an extension class (question, under what though?)
        public static bool                  HasChildNode( this XmlNode xmlNode, string childNode )
        {
            var xmlEnumerator = xmlNode.ChildNodes.GetEnumerator();
            while( xmlEnumerator.MoveNext() )
            {
                var node = (XmlNode) xmlEnumerator.Current;
                if( node.Name == childNode )
                {   // Node exists
                    return true;
                }
            }
            // Node doesn't exist
            return false;
        }

    }

}
