using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using RimWorld;
using Verse;

namespace CommunityCoreLibrary
{

    public static class InterModCommunication
    {

        #region Fields

        private static Dictionary<string,Action>    handlers;

        private static Dictionary<string,Action<object>>    packetHandlers;

        #endregion

        #region Messages with no Packets

        /// <summary>
        /// Registers a message handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="callback">Action callback when the message is broadcast.</param>
        public static void           RegisterForMessage( string message, Action callback )
        {
            if( handlers == null )
            {
                handlers = new Dictionary<string, Action>();
            }
            if( handlers.ContainsKey( message ) )
            {
                handlers[ message ] += callback;
            }
            else
            {
                handlers.Add( message, callback );
            }
        }

        /// <summary>
        /// Broadcasts a message to a handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        public static void          BroadcastMessage( string message )
        {
            if( handlers == null )
            {
                handlers = new Dictionary<string, Action>();
            }
            if( !handlers.ContainsKey( message ) )
            {
                return;
            }
            handlers[ message ].Invoke();
        }

        #endregion

        #region Messages with Packets

        /// <summary>
        /// Registers a message handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="callback">Action callback when the message is broadcast with a packet.</param>
        public static void           RegisterForMessagePacket( string message, Action<object> callback )
        {
            if( packetHandlers == null )
            {
                packetHandlers = new Dictionary<string, Action<object>>();
            }
            if( packetHandlers.ContainsKey( message ) )
            {
                packetHandlers[ message ] += callback;
            }
            else
            {
                packetHandlers.Add( message, callback );
            }
        }

        /// <summary>
        /// Broadcasts a message to a handler with an attached packet.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="packet">Packet to send.</param>
        public static void       BroadcastMessagePacket( string message, object packet )
        {
            if( packetHandlers == null )
            {
                packetHandlers = new Dictionary<string, Action<object>>();
            }
            if( !packetHandlers.ContainsKey( message ) )
            {
                return;
            }
            packetHandlers[ message ].Invoke( packet );
        }

        #endregion

    }

}
