// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.JitStage;

namespace Tono.Jit
{
    /// <summary>
    /// Jit Stage Model
    /// </summary>
    public class JitStageModel : IJitStageModel
    {
        /// <summary>
        /// having processes
        /// </summary>
        public ProcessSet Procs { get; private set; }

        /// <summary>
        /// Process Push Links
        /// </summary>
        private Dictionary<string, List<string>> _processKeyLinks = new Dictionary<string, List<string>>();

        /// <summary>
        /// The constructor
        /// </summary>
        public JitStageModel()
        {
            Procs = new ProcessSet();
        }

        public JitProcess FindProcess(string processKey, bool isReturnNull = false)
        {
            if (string.IsNullOrEmpty(processKey))
            {
                if (isReturnNull)
                {
                    return null;
                }
                else
                {
                    throw new JitException(JitException.FormatNoProcKey);
                }
            }
            var ret = Procs[processKey];
            if (ret == null && isReturnNull == false)
            {
                throw new JitException(JitException.FormatNoProcKey, processKey);
            }
            return ret;
        }

        /// <summary>
        /// Save Process link
        /// </summary>
        /// <param name="procKey1"></param>
        /// <param name="procKey2"></param>
        public void AddProcessLink(string procKeyFrom, string procKeyTo)
        {
            var links = _processKeyLinks.GetValueOrDefault(procKeyFrom, true, a => new List<string>());
            if (links.Contains(procKeyTo) == false)
            {
                links.Add(procKeyTo);
            }
        }

        public void AddProcessLink(JitProcess from, JitProcess to)
        {
            AddProcessLink(from.ID, to.ID);
        }

        public void RemoveProcessLink(string procKeyFrom, string procKeyTo)
        {
            var li = GetProcessLinks(procKeyFrom);
            var links = _processKeyLinks.Values.Where(a => ReferenceEquals(a, li)).FirstOrDefault();
            if( links != null)
            {
                links.Remove(procKeyTo);
                var pt = FindProcess(procKeyTo);
                if( pt != null)
                {
                    links.Remove(pt.ID);
                    if (pt.Name != null)
                    {
                        links.Remove(pt.Name);
                    }
                }
            }
        }
        public void RemoveProcessLink(JitProcess from, JitProcess to)
        {
            RemoveProcessLink(from.ID, to.ID);
        }

        /// <summary>
        /// Get Process Key(ID/Name) Destinations
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <returns></returns>
        public IReadOnlyList<string> GetProcessLinks(string procKeyFrom)
        {
            if (_processKeyLinks.TryGetValue(procKeyFrom, out var list))
            {
                return list;
            }
            var proc = FindProcess(procKeyFrom);
            if (proc != null)
            {
                if (_processKeyLinks.TryGetValue(proc.ID, out var list2))
                {
                    return list2;
                }
                if (_processKeyLinks.TryGetValue(proc.Name, out var list3))
                {
                    return list3;
                }
                procKeyFrom = proc.ID;
            }
            var list4 = new List<string>();
            _processKeyLinks[procKeyFrom] = list4;

            return list4;
        }
        public IReadOnlyList<string> GetProcessLinks(JitProcess proc)
        {
            return GetProcessLinks(proc.ID);
        }

    }
}
