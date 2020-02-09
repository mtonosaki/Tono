// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

namespace Tono.Logic
{
    /// <summary>
    /// solver for Traveling salesman problem
    /// optimize as Loop nodes
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
    public class TspResolverLoop<TUnit> : TspResolverBase<TUnit>
    {
        public override void Start()
        {
            var indexes = Collection.Seq(List.Count).ToArray();
            var buf = new int[indexes.Length - 1];
            for (var i = 0; i < buf.Length; i++)
            {
                buf[i] = indexes[i + 1];
            }
            var res = Optimize(buf);
            for (var i = 0; i < res.Length; i++)
            {
                indexes[i + 1] = res[i];
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
            var ret = CostCaluclator(List[0], List[p[0]], CaluclationStage.InitCost);
            for (var i = 1; i < p.Length; ++i)
            {
                ret += CostCaluclator(List[p[i - 1]], List[p[i]], CaluclationStage.Normal);
            }
            ret += CostCaluclator(List[p[p.Length - 1]], List[0], CaluclationStage.FinalCostLoop);
            return ret;
        }
    }
}
