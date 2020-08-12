// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using static Tono.Jit.Utils;
using ProcessKey = System.String;
using ProcessKeyPath = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// Jit Stage Model
    /// </summary>
    [JacTarget(Name = "Subset")]
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
        private readonly Dictionary<ProcessKey, List<ProcessKeyPath>> _processKeyLinks = new Dictionary<ProcessKey, List<ProcessKeyPath>>();

        /// <summary>
        /// Child Processes
        /// </summary>
        private readonly List<(string InstanceName, JitProcess ClassObject)> _childProcess = new List<(ProcessKey InstanceName, JitProcess ClassObject)>();

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
        public void AddChildProcess(JitProcess proc, string instanceName = null)
        {
            RemoveChildProcess(proc, instanceName);
            _childProcess.Add((instanceName, proc));

            if (proc is JitSubset subset)
            {
                subset.ProcessAdded += Subset_ProcessAdded;
            }
            Subset_ProcessAdded(this, new ProcessAddedEventArgs
            {
                Target = this,
                Process = proc,
            });
        }

        private void Subset_ProcessAdded(object sender, ProcessAddedEventArgs e)
        {
            ProcessAdded?.Invoke(this, e);
        }

        /// <summary>
        /// Add dummy process
        /// </summary>
        /// <param name="procKey"></param>
        public void AddChildProcess(ProcessKey procKey)
        {
            RemoveChildProcess(procKey);
            var ip = procKey.Split('@');

            var proc = new JitProcessDummy
            {
                ProcessKey = ip[ip.Length - 1], // note : JitProcessDummy.ProcessKey should not have Instance Name
            };
            _childProcess.Add((ip.Length == 1 ? null : ip[0], proc));

            ProcessAdded?.Invoke(this, new ProcessAddedEventArgs
            {
                Target = this,
                Process = proc,
            });
        }

        /// <summary>
        /// Remove Process class from this subset
        /// </summary>
        /// <param name="proc"></param>
        public void RemoveChildProcess(JitProcess proc, string instanceName = null)
        {
            if (instanceName != null)
            {
                instanceName = instanceName + "@";
            }
            else
            {
                instanceName = "";
            }
            if (proc is JitProcessDummy dummy)
            {
                RemoveChildProcess(instanceName + dummy.ProcessKey);
            }
            else
            {
                RemoveChildProcess(instanceName + proc.Name);
            }
            RemoveChildProcess(instanceName + proc.ID);
        }

        /// <summary>
        /// Remove the all process that have procKey in Name and ID
        /// </summary>
        /// <param name="procKey"></param>
        public void RemoveChildProcess(ProcessKey procKey)
        {
            string instanceName = null;
            var ipkey = procKey.Split('@');
            if (ipkey.Length == 2)
            {
                instanceName = ipkey[0];
                procKey = ipkey[1];
            }
            var dels = new List<(string InstanceName, JitProcess ClassObject)>();
            foreach (var ip in _childProcess)
            {
                if (ip.InstanceName == null || ip.InstanceName == instanceName)
                {
                    if (ip.ClassObject is JitProcessDummy dmy)
                    {
                        if (dmy.ProcessKey == procKey)
                        {
                            dels.Add(ip);
                        }
                    }
                    else
                    {
                        if (ip.ClassObject.ID == procKey || ip.ClassObject.Name == procKey)
                        {
                            dels.Add(ip);
                        }
                    }
                }
            }
            foreach (var proc in dels)
            {
                _childProcess.Remove(proc);
            }
        }

        /// <summary>
        /// Get child process enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerable<JitProcess> GetChildProcesses()
        {
            return _childProcess.Select(ip => ip.ClassObject);
        }

        /// <summary>
        /// Get child process of the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public JitProcess GetChildProcess(int index)
        {
            return _childProcess[index].ClassObject;
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
                if (isReturnNull)
                {
                    return null;
                }
                else
                {
                    throw new JitException(JitException.NullProcKey);
                }
            }
            var ipkey = procKey.Split('@');
            if (ipkey.Length == 2)
            {
                procKey = ipkey[1];
            }
            var rets1 = _childProcess
                .Where(a => a.ClassObject.ID == procKey);
            var rets2 = _childProcess
                .Where(a => a.ClassObject.Name == procKey);
            var ret = rets1.Concat(rets2)
                .Where(a => a.InstanceName == null || a.InstanceName == ipkey[0])
                .FirstOrDefault();

            if (ret != default)
            {
                return ret.ClassObject;
            }
            if (isReturnNull)
            {
                return null;
            }
            else
            {
                throw new JitException(JitException.NoProcKey, $"{procKey} in {this}");
            }
        }

        /// <summary>
        /// Find next process of "fromProc"
        /// </summary>
        /// <param name="fromProc"></param>
        /// <returns></returns>
        private JitProcess FindNextProcess(JitProcess fromProc)
        {
            var links = GetProcessLinkPathes(fromProc);
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
            foreach (var ip in _childProcess)
            {
                var pkey = GetProcessKey(ip.ClassObject);
                if (ip.InstanceName == null)
                {
                    yield return pkey;
                }
                else
                {
                    yield return $"{ip.InstanceName}@{pkey}";
                }
            }
        }

        /// <summary>
        /// Save Process link
        /// </summary>
        /// <param name="procKey1">Process key of this subset</param>
        /// <param name="procKey2">Process Key that have subset path</param>
        public void AddProcessLink(ProcessKey procKeyFrom, ProcessKeyPath procKeyPathTo)
        {
            var links = _processKeyLinks.GetValueOrDefault(procKeyFrom, true, a => new List<string>());
            if (links.Contains(procKeyPathTo) == false)
            {
                links.Add(procKeyPathTo);
            }
        }

        /// <summary>
        /// Add Process Link in this subset
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void AddProcessLink(JitProcess from, JitProcess to)
        {
            AddProcessLink(GetProcessKey(from), GetProcessKey(to));
        }

        /// <summary>
        /// Remove process link by Process Key(ID/Name)
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <param name="procKeyPathTo"></param>
        public void RemoveProcessLink(ProcessKey procKeyFrom, ProcessKeyPath procKeyPathTo)
        {
            var li = GetProcessLinkPathes(procKeyFrom);
            var links = _processKeyLinks.Values.Where(a => ReferenceEquals(a, li)).FirstOrDefault();
            if (links != null)
            {
                links.Remove(procKeyPathTo);
                var pt = FindChildProcess(procKeyPathTo);
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

        /// <summary>
        /// Remove process link
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void RemoveProcessLink(JitProcess from, JitProcess to)
        {
            RemoveProcessLink(from.ID, to.ID);
        }

        /// <summary>
        /// Get Process Key(ID/Name) Destinations
        /// </summary>
        /// <param name="procKeyFrom"></param>
        /// <returns></returns>
        public IReadOnlyList<ProcessKeyPath> GetProcessLinkPathes(ProcessKey procKeyFrom)
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
            var list4 = new List<ProcessKeyPath>();
            _processKeyLinks[procKeyFrom] = list4;

            return list4;
        }

        /// <summary>
        /// Get Process Link list
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        public IReadOnlyList<ProcessKey> GetProcessLinkPathes(JitProcess proc)
        {
            return GetProcessLinkPathes(GetProcessKey(proc));
        }
    }
}
