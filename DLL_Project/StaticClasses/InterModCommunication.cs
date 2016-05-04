using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace CommunityCoreLibrary
{
    public static class InterModCommunication
    {
        #region Fields

        private static Dictionary<string,Action>   handlers;

        #endregion Fields

        #region Methods

        /// <summary>
        /// Broadcasts a message to a handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        public static void BroadcastMessage( string message )
        {
            if ( handlers == null )
            {
                handlers = new Dictionary<string, Action>();
            }
            if ( !handlers.ContainsKey( message ) )
            {
                return;
            }
            handlers[message].Invoke();
        }

        /// <summary>
        /// Registers a message handler.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        /// <param name="callback">Action callback when the message is broadcast.</param>
        public static void RegisterForMessage( string message, Action callback )
        {
            if ( handlers == null )
            {
                handlers = new Dictionary<string, Action>();
            }
            if ( handlers.ContainsKey( message ) )
            {
                handlers[message] += callback;
            }
            handlers.Add( message, callback );
        }

        #endregion Methods
    }
}