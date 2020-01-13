// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Tono.Logic
{
    /// <summary>
    /// solver for Traveling salesman problem
    /// all nodes are shuffled to try minimim cost
    /// </summary>
    /// <typeparam name="TUnit">node type</typeparam>
    /// <example>
    /// USAGE ==================================
    /// var tsp = new TspResolver
    /// {
    ///    List = NODELIST,
    ///    CostCaluclator = ***,
    /// };
    /// tsp.Start();
    /// Result : update NODELIST
    /// </example>
    public class TspResolverShuffle<TUnit> : TspResolverBase<TUnit>
    {
        protected override void Start()
        {
            var indexes = Collection.Seq(List.Count).ToArray();
            var buf = indexes.ToArray();
            var res = Optimize(buf);
            for (var i = 0; i < res.Length; i++)
            {
                indexes[i] = res[i];
            }
            var tmp = new List<TUnit>(List);
            List.Clear();
            for (var i = 0; i < indexes.Length; i++)
            {
                List.Add(tmp[indexes[i]]);
            }
        }
        protected override double CalcCost(int[] p)
        {
            var ret = 0.0;
            for (var i = 1; i < p.Length; ++i)
            {
                ret += CostCaluclator(List[p[i - 1]], List[p[i]], CaluclationStage.Normal);
            }
            return ret;
        }
    }
}
