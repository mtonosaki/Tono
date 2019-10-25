// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tono.Logic
{
    /// <summary>
    /// polynomial of degree n least square method
    /// 最小二乗法の座標を作成(n次多項式で近似値を算出)する
    /// </summary>
    /// <example>
    /// var sai = new LeastSquaresNPolynominal(4)				// multinomial＝4
    /// {
    /// 	{1, 7.987},											// input sample 5 items {x, y}
    /// 	{2, 2.986},											// (you can use Add method too like  sai.Add(2, 2.986);)
    /// 	{3, 1.998},
    /// 	{4, 2.224},
    /// 	{5, 5.678},
    /// };
    /// sai.Calc(0.2);											// Start caluclation. 0.2 is gain size(step) to make linear x value between minimim and maximum Added
    /// foreach(var xy in sai)
    /// {
    /// 	Debug.WriteLine($"{xy.X}, {xy.Y}");
    /// }
    /// </example>
    public class LeastSquaresNPolynominal : IEnumerable<(double X, double Y)>
    {
        private readonly List<(double X, double Y)> _in = new List<(double X, double Y)>();
        private readonly List<(double X, double Y)> _out = new List<(double X, double Y)>();

        /// <summary>
        /// initial constructor
        /// </summary>
        /// <param name="multinomial"></param>
        public LeastSquaresNPolynominal(int multinomial = 3)
        {
            Multinomial = multinomial;
        }

        private int _multinomial = 3;

        /// <summary>
        /// multinomial value
        /// n-1次の多項式で近似（１以上)
        /// </summary>
        private int Multinomial
        {
            get => _multinomial;
            set
            {
                _multinomial = value;
                buf = null;
            }
        }

        /// <summary>
        /// get enumerator to query x-y tuple
        /// </summary>
        /// <returns></returns>
        public IEnumerator<(double X, double Y)> GetEnumerator()
        {
            return _out.GetEnumerator();
        }

        /// <summary>
        /// query x-y tuple
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(double X, double Y)> GetOutputs()
        {
            return _out;
        }

        /// <summary>
        /// add actual point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Add(double x, double y)
        {
            _in.Add((x, y));
            buf = null;
        }

        /// <summary>
        /// input data count
        /// </summary>
        private int Count => _in.Count;

        private double[] buf = null;

        private void MakeCache()
        {
            if (buf != null)
            {
                return;
            }
            var cache = new double[Multinomial, Multinomial + 1];

            for (var i = 0; i < Multinomial; i++)
            {
                for (var j = 0; j < Multinomial; j++)
                {
                    for (var k = 0; k < Count; k++)
                    {
                        cache[i, j] += Math.Pow(_in[k].X, i + j);
                    }
                }
            }
            for (var i = 0; i < Multinomial; i++)
            {
                for (var k = 0; k < Count; k++)
                {
                    cache[i, Multinomial] += Math.Pow(_in[k].X, i) * _in[k].Y;
                }
            }
            buf = MakeGauss(cache);
        }

        /// <summary>
        /// Start caluclation
        /// </summary>
        /// <param name="stepx">gain size of X value</param>
        /// <returns></returns>
        public List<(double X, double Y)> Calculation(double stepx = 0.01)
        {
            _out.Clear();
            for (var x = _in[0].X; x <= _in[Count - 1].X + stepx / 2; x += stepx)
            {
                _out.Add((x, GetY(x)));
            }
            return _out;
        }

        /// <summary>
        /// get Y value of specified X
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double GetY(double x)
        {
            MakeCache();
            var y = 0.0;
            for (var i = 0; i < Multinomial; i++)
            {
                y += buf[i] * Math.Pow(x, i);
            }
            return y;
        }

        private double[] MakeGauss(double[,] rc)
        {
            for (var i = 0; i < Multinomial; i++)
            {
                var m = 0.0;
                int pivot = i;

                for (var ii = i; ii < Multinomial; ii++)
                {
                    if (Math.Abs(rc[ii, i]) > m)
                    {
                        m = Math.Abs(rc[ii, i]);
                        pivot = ii;
                    }
                }
                if (pivot != i)
                {
                    for (var ii = 0; ii < Multinomial + 1; ii++)
                    {
                        var tmp = rc[i, ii];
                        rc[i, ii] = rc[pivot, ii];
                        rc[pivot, ii] = tmp;
                    }
                }
            }
            for (int i = 0; i < Multinomial; i++)
            {
                var p = rc[i, i];
                rc[i, i] = 1;

                for (var ii = i + 1; ii < Multinomial + 1; ii++)
                {
                    rc[i, ii] /= p;
                }

                for (var ii = i + 1; ii < Multinomial; ii++)
                {
                    var q = rc[ii, i];
                    for (var j = i + 1; j < Multinomial + 1; j++)
                    {
                        rc[ii, j] -= q * rc[i, j];
                    }
                    rc[ii, i] = 0;
                }
            }
            var tmpx = new double[Multinomial];
            for (var i = Multinomial - 1; i >= 0; i--)
            {
                tmpx[i] = rc[i, Multinomial];
                for (var j = Multinomial - 1; j > i; j--)
                {
                    tmpx[i] -= rc[i, j] * tmpx[j];
                }
            }
            return tmpx;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _out.GetEnumerator();
        }
    }
}
