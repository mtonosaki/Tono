using System.Collections.Generic;
using System.Linq;

namespace Tono.Logic
{
    /// <summary>
    /// solver for Traveling salesman problem
    /// optimize middle nodes only (not change start and end node)
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
    public class TspResolverStartEndFix<TUnit> : TspResolverBase<TUnit>
    {
        protected override void Start()
        {
            var indexes = Collection.Seq(List.Count).ToArray();
            var buf = new int[indexes.Length - 2];
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
            ret += CostCaluclator(List[p[p.Length - 1]], List[List.Count - 1], CaluclationStage.FinalCostFix);
            return ret;
        }
    }
}
