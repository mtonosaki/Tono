// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.JitStage;
using ProcessKey = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// Jit Stage Model
    /// </summary>
    public class JitSubset : JitProcess
    {
        public override string ID { get; set; } = JacInterpreter.MakeID("Subset");

        /// <summary>
        /// Simulator Engine
        /// </summary>
        public Func<IJitStageEngine> Engine { get; set; }

        /// <summary>
        /// having processes
        /// </summary>
        public ProcessSet ChildProcesses { get; private set; }

        /// <summary>
        /// Process key(ID/Name) of Connector In
        /// </summary>
        public ProcessKey ConnectorIn { get; set; }

        /// <summary>
        /// Process key(ID/Name) of Connector Out
        /// </summary>
        public ProcessKey ConnectorOut { get; set; }

        /// <summary>
        /// Process Push Links
        /// </summary>
        private Dictionary<ProcessKey, List<string>> _processKeyLinks = new Dictionary<ProcessKey, List<string>>();

        /// <summary>
        /// The constructor
        /// </summary>
        public JitSubset()
        {
            Classes.Add(":Subset");
            ChildProcesses = new ProcessSet();
        }

        private JitProcess findNextProcess(JitProcess fromProc)
        {
            var links = GetProcessLinks(fromProc);
            var key = links.FirstOrDefault();
            var ret = ChildProcesses.FindProcess(key, true);
            return ret;
        }

        /// <summary>
        /// Save Process link
        /// </summary>
        /// <param name="procKey1"></param>
        /// <param name="procKey2"></param>
        public void AddProcessLink(ProcessKey procKeyFrom, ProcessKey procKeyTo)
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

        public void RemoveProcessLink(ProcessKey procKeyFrom, ProcessKey procKeyTo)
        {
            var li = GetProcessLinks(procKeyFrom);
            var links = _processKeyLinks.Values.Where(a => ReferenceEquals(a, li)).FirstOrDefault();
            if (links != null)
            {
                links.Remove(procKeyTo);
                var pt = ChildProcesses.FindProcess(procKeyTo);
                if (pt != null)
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
        public IReadOnlyList<ProcessKey> GetProcessLinks(ProcessKey procKeyFrom)
        {
            if (_processKeyLinks.TryGetValue(procKeyFrom, out var list))
            {
                return list;
            }
            var proc = ChildProcesses.FindProcess(procKeyFrom, true);
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
            var list4 = new List<ProcessKey>();
            _processKeyLinks[procKeyFrom] = list4;

            return list4;
        }
        public IReadOnlyList<ProcessKey> GetProcessLinks(JitProcess proc)
        {
            return GetProcessLinks(proc.ID);
        }
    }
}
