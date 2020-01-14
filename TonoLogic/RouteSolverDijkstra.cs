// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tono.Logic
{

    /// <summary>
    /// Route solver implement with Dijekstra method
    /// </summary>
    public class RouteSolverDijkstra : RouteSolver
    {
        public override IList<ILink> Solve(ILink start, ILink end)
        {
            var links = new Dictionary<ILink, double/*score*/>();
            var prev = new Dictionary<ILink, ILink>(); // 最短リンク元の記憶
            var endlink = new Dictionary<ILink, ILink>();

            links[start] = 0;
            var now = start;

            for (; ; )
            {
                foreach (var next in now.NextLinks)
                {
                    if (endlink.ContainsKey(next))
                    {
                        continue;
                    }
                    var nsc = CalcScore(links, now) + next.GetCost(now);
                    if (nsc < CalcScore(links, next))
                    {
                        links[next] = nsc;
                        prev[next] = now;
                    }
                }
                links.Remove(now);
                endlink[now] = now;

                // find minimum link
                var min = double.PositiveInfinity;
                ILink minL = null;
                foreach (var kv in links)
                {
                    if (kv.Value < min)
                    {
                        minL = kv.Key;
                        min = kv.Value;
                    }
                }
                if (minL == null)
                {
                    return new List<ILink>();
                }
                Debug.Assert(minL != null);
                if (minL.Equals(end))
                {
                    break;
                }
                now = minL;
            }
            var ret = new LinkedList<ILink>();
            for (var tar = end; tar.Equals(start) == false; tar = prev[tar])
            {
                ret.AddFirst(tar);
            }
            ret.AddFirst(start);
            return ret.ToList();
        }

        /// <summary>
        /// score caluclator
        /// </summary>
        /// <param name="links"></param>
        /// <param name="link"></param>
        /// <returns></returns>
        private double CalcScore(IDictionary<ILink, double/*score*/> links, ILink link)
        {
            if (links.TryGetValue(link, out double ret))
            {
                return ret;
            }
            else
            {
                return double.PositiveInfinity;
            }
        }

        /// <summary>
        /// caluclate total cost
        /// </summary>
        /// <param name="links"></param>
        /// <returns></returns>
        private static double CalcTotalCost(IList<ILink> links)
        {
            var ret = 0.0;
            for (var i = 0; i < links.Count; i++)
            {
                ret += links[i].GetCost(i == 0 ? null : links[i - 1]);
            }
            return ret;
        }
    }
}
