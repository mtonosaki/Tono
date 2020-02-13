// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.Utils;
using ProcessKey = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// Jit Stage Model
    /// </summary>
    public class JitSubset : JitProcess
    {
        public override string ID { get; set; } = JacInterpreter.MakeID("Subset");

        public class ProcessAddedEventArgs : EventArgs
        {
            public JitSubset Target { get; set; }
            public JitProcess Process { get; set; }
        }

        public event EventHandler<ProcessAddedEventArgs> ProcessAdded;


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
        /// Child Processes
        /// </summary>
        private readonly List<JitProcess> _procData = new List<JitProcess>();

        /// <summary>
        /// The constructor
        /// </summary>
        public JitSubset()
        {
            Classes.Add(":Subset");
        }

        public override string ToString()
        {
            return $"{GetType().Name} {(Name ?? "")} (ID={ID})";
        }

        public override bool Equals(object obj)
        {
            if (obj is JitSubset ss)
            {
                return ss.ID.Equals(ID);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Add process
        /// </summary>
        /// <param name="proc"></param>
        public void AddChildProcess(JitProcess proc)
        {
            RemoveChildProcess(proc);
            _procData.Add(proc);
            ProcessAdded?.Invoke(this, new ProcessAddedEventArgs
            {
                Target = this,
                Process = proc,
            });
        }

        /// <summary>
        /// Add dummy process
        /// </summary>
        /// <param name="procKey"></param>
        public void AddChildProcess(ProcessKey procKey)
        {
            RemoveChildProcess(procKey);

            JitProcess proc;
            _procData.Add(proc = new JitProcessDummy
            {
                ProcessKey = procKey,
            });
            ProcessAdded?.Invoke(this, new ProcessAddedEventArgs
            {
                Target = this,
                Process = proc,
            });
        }

        /// <summary>
        /// Add processes to this subset
        /// </summary>
        /// <param name="procs"></param>
        public void AddChildProcess(IEnumerable<JitProcess> procs)
        {
            foreach (var proc in procs)
            {
                AddChildProcess(proc);
            }
        }

        /// <summary>
        /// Remove Process class from this subset
        /// </summary>
        /// <param name="proc"></param>
        public void RemoveChildProcess(JitProcess proc)
        {
            if (proc is JitProcessDummy dummy)
            {
                RemoveChildProcess(dummy.ProcessKey);
            }
            else
            {
                RemoveChildProcess(proc.Name);
            }
            RemoveChildProcess(proc.ID);
        }

        /// <summary>
        /// Remove the all process that have procKey in Name and ID
        /// </summary>
        /// <param name="procKey"></param>
        public void RemoveChildProcess(ProcessKey procKey)
        {
            var col1 =
                from t in _procData
                let dmy = t as JitProcessDummy
                where dmy != null
                where dmy.ProcessKey == procKey
                select dmy;
            var col2 =
                from t in _procData
                where t is JitProcessDummy == false
                where t.ID == procKey || t.Name == procKey
                select t;
            var dels = col1.Concat(col2).ToArray();

            foreach (var proc in dels)
            {
                _procData.Remove(proc);
            }
        }

        public IEnumerable<JitProcess> GetChildProcesses()
        {
            return _procData;
        }

        public JitProcess GetChildProcess(int index)
        {
            return _procData[index];
        }

        /// <summary>
        /// Find Process Key(ID or Name of lazy connection)
        /// </summary>
        /// <param name="procKey"></param>
        /// <returns></returns>
        public JitProcess FindChildProcess(ProcessKey procKey, bool isReturnNull = false)
        {
            if (procKey == null)
            {
                if (isReturnNull) return null; else throw new JitException(JitException.NullProcKey);
            }
            var ret = _procData.Where(a => a.ID == procKey).FirstOrDefault();
            if (ret == null)
            {
                ret = _procData.Where(a => a.Name == procKey).FirstOrDefault();
            }
            if (ret != null || isReturnNull)
            {
                return ret;
            }
            throw new JitException(JitException.NoProcKey, $"{procKey} in {this}");
        }


        private JitProcess findNextProcess(JitProcess fromProc)
        {
            var links = GetProcessLinks(fromProc);
            var key = links.FirstOrDefault();
            var ret = FindChildProcess(key, true);
            return ret;
        }


        /// <summary>
        /// Get Process key list
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ProcessKey> GetProcessKeys()
        {
            foreach (var proc in _procData)
            {
                yield return GetProcessKey(proc);
            }
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
                var pt = FindChildProcess(procKeyTo);
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
            var proc = FindChildProcess(procKeyFrom, true);
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
            return GetProcessLinks(GetProcessKey(proc));
        }
    }
    #region Dummy Process Object
    public class JitProcessDummy : JitProcess
    {
        public override string ID { get; set; } = JacInterpreter.MakeID("ProcessDummy");

        public ProcessKey ProcessKey { get; set; } // Possible Name or ID

        public override string Name { get => throw new NotAllowErrorException(); set => throw new NotAllowErrorException(); }
        public override void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work) => throw new NotAllowErrorException();
        public override JitKanban AddKanban(IJitEngine engine, JitSubset subset, JitKanban kanban, DateTime now) => throw new NotAllowErrorException();
        public override void Enter(JitWork work, DateTime now) => throw new NotAllowErrorException();
        public override void Exit(JitWork work) => throw new NotAllowErrorException();
        public override JitWork ExitCollectedWork(IJitEngine engine, JitSubset subset, DateTime now) => throw new NotAllowErrorException();
    }
    #endregion
}
