// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// Group by utility.
    /// </summary>
    /// <remarks>
    /// List＜uPart＞ data = ... // pepare a list object
    /// using (uClassifier＜string, uPart＞ pc = new uClassifier＜string, uPart＞())
    /// {
    ///    pc.Classify(data, a => a.PartNo.Pn12);
    ///    textBoxNPn12.Text = string.Format("Pn12 = {0} objects", pc.CountKey);
    /// }
    /// </remarks>
    public class Classifier<TKey, TValue> : IDisposable
    {
        private readonly Dictionary<TKey, List<TValue>> _data = new Dictionary<TKey, List<TValue>>();

        private int _nData = 0;

        /// <summary>
        /// 構築子
        /// </summary>
        public Classifier()
        {
        }

        #region IDisposable member

        public void Dispose()
        {
            Clear();
        }

        #endregion

        private void Clear()
        {
            _data.Clear();
            _nData = 0;
        }

        /// <summary>
        /// Count the kind of keys.
        /// </summary>
        public int CountKey => _data.Count;

        /// <summary>
        /// Count the all data
        /// </summary>
        public int CountData => _nData;

        /// <summary>
        /// Do group by
        /// </summary>
        /// <param name="list">Data</param>
        /// <param name="keygen">to choose key</param>
        public void Classify(IEnumerable<TValue> list, Func<TValue, TKey> keygen)
        {
            Clear();

            foreach (var val in list)
            {
                var key = keygen(val);
                if (_data.TryGetValue(key, out List<TValue> vals) == false)
                {
                    _data[key] = vals = new List<TValue>();
                }
                vals.Add(val);
                _nData++;
            }
        }
    }
}
