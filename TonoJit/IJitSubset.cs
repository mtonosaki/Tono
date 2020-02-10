// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    public interface IJitSubset
    {
        /// <summary>
        /// having processes
        /// </summary>
        ProcessSet ChildProcesses { get; }

        JitProcess FindProcess(string processKey, bool isReturnNull = false);

        /// <summary>
        /// Save Process link
        /// </summary>
        /// <param name="procKey1"></param>
        /// <param name="procKey2"></param>
        void AddProcessLink(string procKeyFrom, string procKeyTo);

        void AddProcessLink(JitProcess from, JitProcess to);

        /// <summary>
        /// Remove Process Link
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <param name="procKeyTo"></param>
        void RemoveProcessLink(string procKeyFrom, string procKeyTo);

        void RemoveProcessLink(JitProcess from, JitProcess to);

        /// <summary>
        /// Get Process Key(ID/Name) Destinations
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <returns></returns>
        IReadOnlyList<string> GetProcessLinks(string procKeyFrom);

        IReadOnlyList<string> GetProcessLinks(JitProcess proc);
    }
}
