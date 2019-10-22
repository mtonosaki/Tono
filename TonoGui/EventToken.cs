// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Threading;

namespace Tono.Gui
{
    /// <summary>
    /// Event message object between features
    /// </summary>
    /// <remarks>
    /// This is for a good way of loose coupling design
    /// features cannot call the other feature directly so use this messaging system.
    /// </remarks>
    public class EventToken
    {
        /// <summary>
        /// previous event token reference to know event link
        /// </summary>
        public EventToken Previous { get; set; }

        /// <summary>
        /// event sender object (feature)
        /// </summary>
        public object Sender { get; set; }

        /// <summary>
        /// thread managed ID of sender(feature) when throwing event
        /// </summary>
        public int ThreadManagedID { get; protected set; }

        /// <summary>
        /// The constructor
        /// </summary>
        public EventToken()
        {
            try
            {
                ThreadManagedID = Thread.CurrentThread.ManagedThreadId;
            }
            catch
            {
                ThreadManagedID = -1;
            }
        }

        /// <summary>
        /// Remarks for debug, logging
        /// </summary>
        public string Remarks { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name} TID={ThreadManagedID} {Remarks}";
        }
    }


}
