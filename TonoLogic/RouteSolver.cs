using System.Collections.Generic;

namespace Tono.Logic
{
    /// <summary>
    /// Route solver base abstract class
    /// </summary>
    public abstract class RouteSolver
    {
        /// <summary>
        /// link interface
        /// </summary>
        public interface ILink
        {
            /// <summary>
            /// folow links
            /// </summary>
            /// <returns></returns>
            IEnumerable<ILink> NextLinks { get; }

            /// <summary>
            /// cost caluclation method
            /// </summary>
            /// <returns></returns>
            double GetCost(ILink previous);
        }

        /// <summary>
        /// find links from "start" link to "end" link
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="end">end position</param>
        /// <returns></returns>
        public abstract IList<ILink> Solve(ILink start, ILink end);
    }
}
