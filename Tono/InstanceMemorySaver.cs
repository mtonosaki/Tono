// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Helps to keep only one matching instance in Equals, not multiple
    /// </summary>
    public class InstanceMemorySaver<T> : IDisposable
    {
        private Dictionary<T, T> _data = new Dictionary<T, T>();

        /// <summary>
        /// Get shared instance
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T this[T key]
        {
            get
            {
                if (_data.TryGetValue(key, out T ret))
                {
                    return ret;
                }
                _data[key] = key;
                return key;
            }
        }

        #region IDisposable member

        public void Dispose()
        {
            if (_data != null)
            {
                _data.Clear();
                _data = null;
            }
        }

        #endregion
    }
}
