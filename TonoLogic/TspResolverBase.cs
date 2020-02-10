// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tono.Logic
{
    public abstract class TspResolverBase<TUnit>
    {
        /// <summary>
        /// Caluclation stage
        /// </summary>
        public enum CaluclationStage
        {
            InitCost,
            Normal,
            FinalCostFix,
            FinalCostLoop,
        }

        /// <summary>
        /// cost caluclator delegate method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public delegate double GetCostMethod(TUnit x, TUnit y, CaluclationStage stage);

        public IList<TUnit> List = null;
        public GetCostMethod CostCaluclator = null;

        public abstract void Start();

        /// <summary>
        /// calculation cost
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        protected abstract double CalcCost(int[] p);

        /// <summary>
        /// Optimize node list
        /// </summary>
        /// <param name="buf">nodes except start and end node</param>
        /// <returns></returns>
        protected int[] Optimize(int[] buf)
        {
            var sw = Stopwatch.StartNew();

            var n = buf.Length;
            var cs = new int[n + 1];
            for (var k = 0; k <= n; k++)
            {
                cs[k] = k;
            }

            var res = (int[])buf.Clone();
            if (buf.Length <= 0)
            {
                return res;
            }
            var rescost = CalcCost(buf);
            var i = 0;
            do
            {
                var t = buf[i];
                var q = (i & 1) == 0 ? 0 : cs[i];
                buf[i] = buf[q];
                buf[q] = t;

                var cost = CalcCost(buf);
                if (rescost > cost)
                {
                    res = (int[])buf.Clone();
                    rescost = cost;
                }

                for (i = 0; cs[i] == 0; ++i)
                {
                    cs[i] = i;
                }
                cs[i]--;

            } while (i < n);

            if (sw.ElapsedMilliseconds >= 5000)
            {
                Debug.WriteLine($"TspResolver Elapsed = {sw.Elapsed.TotalMilliseconds:0}[ms]");
            }
            return res;
        }
    }
}
