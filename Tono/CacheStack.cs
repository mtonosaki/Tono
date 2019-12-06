// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Cache stack utility
    /// </summary>
    /// <typeparam name="TKEY">key type</typeparam>
    /// <typeparam name="TVAL"></typeparam>
    /// <example>
    /// var stack = new uCacheStack＜IpNetwork, GeoIP2＞();
    /// stack.AddLast(new MemoryCache()); // Fast cache layer
    /// stack.AddLast(new DiskCache());   // Persist cache of local storage (middle speed)
    /// stack.AddLast(new CloudCache());  // Sharing cache of cloud storage (low speed)
    /// stack.FeedbackLastDefault = true; // true : feed back value to fast cache from slow cache.
    /// </example>
    public class CacheStack<TKEY, TVAL>
    {
        /// <summary>
        /// Cache handler. This is the base class of your cache accessor
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<OK>")]
        public abstract class Handler
        {
            /// <summary>
            /// Set value to cache
            /// </summary>
            /// <param name="key"></param>
            /// <param name="val"></param>
            /// <param name="param">optional object for cache control. NULL is acceptable</param>
            public abstract void Set(TKEY key, TVAL val, CacheStack.IParams param);

            /// <summary>
            /// Get value from cache
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public abstract TVAL Get(TKEY key, CacheStack.IParams param);

            /// <summary>
            /// Clear values
            /// </summary>
            public virtual void ClearVolatileCache()
            {
            }
        }

        public LinkedList<Handler> Stack { get; } = new LinkedList<Handler>();

        /// <summary>
        /// true : feed back value to fast cache from slow cache.
        /// </summary>
        public bool FeedbackLastDefault { get; set; } = false;

        /// <summary>
        /// Get value and set it to faster caches
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param">Customize option parameter : null is acceptable</param>
        /// <returns></returns>
        public TVAL Getset(TKEY key, CacheStack.IParams param = null)
        {
            var ret = GetsetNoFeedback(key, param);
            if (FeedbackLastDefault && (ret?.Equals(default) ?? false))
            {
                Stack.First?.Value.Set(key, default, param);
            }
            return ret;
        }

        /// <summary>
        /// Get value but NOT feedback to faster caches.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param">Customize option parameter : null is acceptable</param>
        /// <returns></returns>
        public TVAL GetsetNoFeedback(TKEY key, CacheStack.IParams param = null)
        {
            for (LinkedListNode<Handler> node = Stack.First; node != null; node = node.Next)
            {
                var val = node.Value.Get(key, param);
                if (val?.Equals(default) == false)
                {
                    for (node = node.Previous; node != null; node = node.Previous)
                    {
                        node.Value.Set(key, val, param);    // Save to faster cache.
                    }
                    return val;
                }
            }
            return default;
        }

        /// <summary>
        /// Set value to the all cache stack forcely.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="param">Customize option parameter : null is acceptable</param>
        public void Set(TKEY key, TVAL value, CacheStack.IParams param = null)
        {
            for (var node = Stack.First; node != null; node = node.Next)
            {
                node.Value.Set(key, value, param);
            }
        }

        /// <summary>
        /// Clear cache value
        /// </summary>
        public void ClearVolatileCache()
        {
            for (var node = Stack.First; node != null; node = node.Next)
            {
                node.Value.ClearVolatileCache();
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1052:Static holder types should be Static or NotInheritable", Justification = "<OK>")]
    public class CacheStack
    {
        /// <summary>
        /// Cache optional parameter
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "<OK>")]
        public interface IParams
        {
        }
    }
}
