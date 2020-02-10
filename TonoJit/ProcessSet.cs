// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProcessKey = System.String;

namespace Tono.Jit
{
    #region Dummy Process Object
    public class JitProcessDummy : JitProcess
    {
        public override string ID { get; set; } = JacInterpreter.MakeID("ProcessDummy");
        public override void AddAndAdjustExitTiming(JitStage.WorkEventQueue events, JitWork work) => throw new NotAllowErrorException();
        public override JitKanban AddKanban(IJitStageEngine engine, JitKanban kanban, DateTime now) => throw new NotAllowErrorException();
        public override void Enter(JitWork work, DateTime now) => throw new NotAllowErrorException();
        public override void Exit(JitWork work) => throw new NotAllowErrorException();
        public override JitWork ExitCollectedWork(JitSubset subset, DateTime now) => throw new NotAllowErrorException();
    }
    #endregion

    /// <summary>
    /// Process INSTANCE Collection
    /// </summary>
    public class ProcessSet : IEnumerable<JitProcess>
    {
        public class AddedEventArgs : EventArgs
        {
            public JitProcess Process { get; set; }
        }

        public event EventHandler<AddedEventArgs> Added;
        private readonly List<JitProcess> _procData = new List<JitProcess>();

        /// <summary>
        /// process count
        /// </summary>
        public int Count => _procData.Count;

        /// <summary>
        /// Add Process
        /// </summary>
        /// <param name="proc"></param>
        public void Add(JitProcess proc)
        {
            Remove(proc);
            _procData.Add(proc);

            Added?.Invoke(this, new AddedEventArgs
            {
                Process = proc,
            });
        }

        /// <summary>
        /// Reserve Process {Name or ID} as procKey
        /// </summary>
        /// <param name="procKey"></param>
        public void Add(ProcessKey procKey)
        {
            Remove(procKey);

            JitProcess proc;
            _procData.Add(proc = new JitProcessDummy
            {
                Name = procKey,
            });
            Added?.Invoke(this, new AddedEventArgs
            {
                Process = proc,
            });
        }

        /// <summary>
        /// Add Range
        /// </summary>
        /// <param name="procs"></param>
        public void Add(IEnumerable<JitProcess> procs)
        {
            foreach (var proc in procs)
            {
                Add(proc);
            }
        }

        /// <summary>
        /// Remove a process
        /// </summary>
        /// <param name="proc"></param>
        public void Remove(JitProcess proc)
        {
            Remove(proc.ID);
            Remove(proc.Name);
        }

        /// <summary>
        /// Remove the all process that have procKey in Name and ID
        /// </summary>
        /// <param name="procKey"></param>
        public void Remove(ProcessKey procKey)
        {
            var dels = _procData.Where(a => a.ID == procKey || a.Name == procKey).ToArray();
            foreach (var proc in dels)
            {
                _procData.Remove(proc);
            }
        }

        /// <summary>
        /// Find Process Key(ID or Name of lazy connection)
        /// </summary>
        /// <param name="procKey"></param>
        /// <returns></returns>
        public JitProcess FindProcess(ProcessKey procKey, bool isReturnNull = false)
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

        /// <summary>
        /// IEnumerable＜JitProcess＞
        /// </summary>
        /// <returns></returns>
        public IEnumerator<JitProcess> GetEnumerator()
        {
            return _procData.GetEnumerator();
        }

        /// <summary>
        /// IEnumerable＜JitProcess＞
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _procData.GetEnumerator();
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
        /// Get Process Key
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        public static ProcessKey GetProcessKey(JitProcess proc)
        {
            if (proc is JitProcessDummy)
            {
                return proc.Name;
            }
            else
            {
                if (proc.Name == null)
                {
                    return proc.ID;
                }
                else
                {
                    return proc.Name;
                }
            }
        }

        public JitProcess this[int index] => _procData[index];

        public JitProcess this[ProcessKey procKey] => FindProcess(procKey);
    }
}
