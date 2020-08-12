// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace Tono.Logic
{
    /// <summary>
    /// Distribute items with GCM(Goal Chasing Method) algorithm
    /// 目標追跡法(Goal Chasing Method)で 指定配分の発生率でキーを分散させる
    /// </summary>
    /// <example>
    ///		var gcm = new GcmDistributer();
    ///		gcm.Add("AA", 6);  // AA  6 times
    ///		gcm.Add("BB", 3);  // BB  3 times
    ///		gcm.Add("CC", 1);  // CC  1 time
    ///		foreach (object key in gcm)
    ///		{
    ///			Debug.Write(key.ToString() + " : ");
    ///		}
    /// Result：AA : BB : AA : AA : BB : AA : CC : AA : BB : AA :  （as you see, distributes items AAx6 times, BBx3, CCx1）
    /// 
    /// ■You can make unlimited result like below
    ///		var gcm = new GcmDistributer();
    ///		gcm.Add("AA", 6);
    ///		gcm.Add("BB", 3);
    ///		gcm.Add("CC", 1);
    ///		for( ;; )
    ///		{
    ///			gcm.MoveNext();
    ///			Debug.Write(gcm.Current.ToString() + " : ");
    ///		}
    /// Result：AA : BB : AA : AA : BB : AA : CC : AA : BB : AA : AA : BB : AA : AA : BB : AA : CC : AA : BB : AA : ...... forever
    /// </example>
    public class GcmDistributer : IEnumerable, IEnumerator, IEnumerable<object>
    {
        private double _totalValue = 0;
        private readonly Dictionary<object, double> _kn = new Dictionary<object, double>();
        private Dictionary<object, double> _res = null;
        private object _current = null;
        private int _loop = 0;

        /// <summary>
        /// clear instnace
        /// </summary>
        public void Clear()
        {
            _totalValue = 0;
            _kn.Clear();
            _res.Clear();
        }

        /// <summary>
        /// total value of added frequency
        /// </summary>
        public double TotalValue => _totalValue;

        /// <summary>
        /// キーと頻度を登録する
        /// </summary>
        /// <param name="key">キー。GetHashCodeとEqualsが実装されているオブジェクト</param>
        /// <param name="frequency">頻度</param>
        public void Add(object key, double frequency)
        {
            if (_kn.TryGetValue(key, out double pn))
            {
                _kn[key] = pn + frequency;
            }
            else
            {
                _kn[key] = frequency;
            }
            _totalValue += frequency;
        }

        public double this[object key]
        {
            get => _kn[key];
            set
            {
                _totalValue = _totalValue - _kn.GetValueOrDefault(key, 0) + value;
                _kn[key] = value;
            }
        }

        #region IEnumerable member

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator member

        public object Current => _current;

        public bool MoveNext()
        {
            // prepare cache
            if (_res == null)
            {
                _res = new Dictionary<object, double>();
                foreach (var key in _kn.Keys)
                {
                    _res[key] = 0;
                }
                _loop = 0;
            }

            // calculate
            var maxval = -1.0;
            _current = null;
            foreach (var kv in _kn)
            {
                var val = _res[kv.Key];
                val += kv.Value;
                _res[kv.Key] = val;
                if (val > maxval)
                {
                    maxval = val;
                    _current = kv.Key;
                }
            }
            _res[_current] -= _totalValue;
            _loop++;

            return _loop <= _totalValue;
        }

        /// <summary>
        /// reset instance
        /// </summary>
        public void Reset()
        {
            _res.Clear();
            _res = null;
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            foreach (var ret in this)
            {
                yield return ret;
            }
        }

        #endregion
    }
}
