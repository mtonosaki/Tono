// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    public interface IJitStageModel
    {
        /// <summary>
        /// having processes
        /// </summary>
        ProcessSet Procs { get; }

        void ProcsAdd(object obj);

        void ProcsRemove(object obj);

        JitProcess FindProcess(string processKey, bool isReturnNull = false);

        void AddProcLinks(object description);

        /// <summary>
        /// Save Process link
        /// </summary>
        /// <param name="procKey1"></param>
        /// <param name="procKey2"></param>
        void AddProcessLink(string procKeyFrom, string procKeyTo);

        void AddProcessLink(JitProcess from, JitProcess to);

        /// <summary>
        /// Get Process Key(ID/Name) Destinations
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <returns></returns>
        IReadOnlyList<string> GetProcessLinks(string procKeyFrom);

        IReadOnlyList<string> GetProcessLinks(JitProcess proc);
    }
}
