// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tono.Logic
{
    /// <summary>
    /// make integer distribution
    /// </summary>
    /// <example>
    /// USAGE-1================================================
    ///	foreach (int n in new uIntegerDistributer(27, 8))  // 27 ÷ 8 = 3.375
    ///	{
    ///		Console.Write(n);
    ///		Console.Write(" : ");
    ///	}
    /// Result   4 : 3 : 4 : 3 : 3 : 4 : 3 : 3 :  (8 numbers,  sum of them = 27)
    /// 
    /// USAGE-2================================================
    ///	var id = new IntegerDistributer();
    ///	id.Add(1.6);
    ///	id.Add(1.6);
    ///	id.Add(2.6);
    ///	id.Add(1.8);
    ///	id.Add(1.4);  // 5 items.  Total=9
    ///	foreach (int n in id)
    ///	{
    ///		Console.Write(n);
    ///		Console.Write(" : ");
    ///	}
    /// Result  2 : 2 : 2 : 2 : 1 :  (5 numbers,  sum of them = 9)
    /// 
    /// This class have distribution mode
    ///  - Normal : head weight
    ///		var id = new IntegerDistributer(2, 12);
    ///		foreach (int n in id)
    ///		{
    ///			Console.Write(n);	Console.Write(" : ");
    ///		}
    /// Result   1 : 0 : 0 : 0 : 0 : 0 : 1 : 0 : 0 : 0 : 0 : 0 : 
    /// 
    ///  - Option : middle weight
    ///		var id = new IntegerDistributer(2, 12);
    ///		id.SetMiddlePriorityMode();  // to set middle weight
    ///		foreach (int n in id)
    ///		{
    ///			Console.Write(n);	Console.Write(" : ");
    ///		}
    /// Result  0 : 0 : 0 : 1 : 0 : 0 : 0 : 0 : 1 : 0 : 0 : 0 : 
    /// </example>
    public class IntegerDistributer : IEnumerable, IEnumerator
    {
        private bool _isFirstPriority = true;   // true=10001000  false=00100010
        private double _value = 0;
        private double _div = 1;
        private int _pos = -1;
        private List<int> _rets = null;
        private double _preValue = 0;

        /// <summary>
        /// added item count
        /// </summary>
        public int Count => _rets.Count;

        /// <summary>
        /// set distribution mode as head weight.
        /// NOTE: execute this method BEFORE Add values
        /// </summary>
        /// <remarks>
        /// results as 10001000 instead of 00100010
        /// </remarks>
        public void SetFirstPriorityMode()
        {
            _isFirstPriority = true;
        }

        /// <summary>
        /// set distribution mode as middle weight.
        /// NOTE: execute this method BEFORE Add values
        /// </summary>
        /// <remarks>
        /// results as 00100010 instead of 10001000
        /// </remarks>
        public void SetMiddlePriorityMode()
        {
            _isFirstPriority = false;
        }

        /// <summary>
        /// default constructor
        /// </summary>
        public IntegerDistributer()
        {
            InitAsValueAddMode();
        }

        /// <summary>
        /// set constructor
        /// </summary>
        /// <param name="vals"></param>
        public IntegerDistributer(IList<double> vals)
        {
            InitAsValueAddMode();
            Add(vals);
        }

        /// <summary>
        /// initialize instance as value Add mode ("USAGE-2" need add method) 
        /// </summary>
        private void InitAsValueAddMode()
        {
            _rets = new List<int>();
            _value = 0;
            _preValue = 0;
            _div = 0;
            _pos = -1;
        }

        /// <summary>
        /// add value
        /// </summary>
        /// <param name="val"></param>
        public void Add(double val)
        {
            Debug.Assert(_rets != null, "コンストラクタによって機能が違います。今はAddできるモードで構築されていません");

            _preValue = _value;
            _value += val;
            _div++;

            int ret;
            if (_isFirstPriority)
            {
                ret = (int)(Math.Ceiling(_value) - Math.Ceiling(_preValue));
            }
            else
            {
                ret = (int)(Math.Round(_value) - Math.Round(_preValue));
            }
            _rets.Add(ret);
        }

        /// <summary>
        /// add values
        /// </summary>
        /// <param name="vals"></param>
        public void Add(IList<double> vals)
        {
            if (vals != null)
            {
                for (int i = 0; i < vals.Count; i++)
                {
                    Add(vals[i]);
                }
            }
        }

        /// <summary>
        /// construct instance as value division mode "USAGE-1"
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="div">division number</param>
        public IntegerDistributer(double value, int div)
        {
            _value = value;
            _div = div;
        }

        /// <summary>
        /// construct instance as value division mode "USAGE-1"
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="div">division number</param>
        public IntegerDistributer(int value, int div)
        {
            _value = value;
            _div = div;
        }

        /// <summary>
        /// get cached value
        /// </summary>
        /// <param name="index">index (0 is first result)</param>
        /// <returns></returns>
        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _div)
                {
                    return 0;
                }
                else
                {
                    if (_rets == null)
                    {
                        double pre = _value / _div * index;
                        double now = _value / _div * (index + 1);
                        if (_isFirstPriority)
                        {
                            return (int)(Math.Ceiling(now) - Math.Ceiling(pre));
                        }
                        else
                        {
                            return (int)(Math.Round(now) - Math.Round(pre));
                        }
                    }
                    else
                    {
                        return _rets[index];
                    }
                }
            }
        }

        #region IEnumerable member

        /// <summary>
        /// IEnumeratorを返す
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator member

        /// <summary>
        /// 現在の値
        /// </summary>
        public object Current => this[_pos];

        /// <summary>
        /// 次の値取得
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _pos++;
            return _pos >= 0 && _pos < _div;
        }

        /// <summary>
        /// リセット
        /// </summary>
        public void Reset()
        {
            _pos = -1;
        }

        #endregion
    }
}
