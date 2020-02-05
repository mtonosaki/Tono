// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    /// <summary>
    /// JIT-Model Root Object : Stage
    /// 工程やワーク全体を動かす 根幹のオブジェクト
    /// </summary>
    [JacTarget(Name = "Stage")]
    public partial class JitStage : JitVariable, IJitObjectID
    {
        public string ID { get; set; } = JacInterpreter.MakeID("Stage");

        /// <summary>
        /// having processes
        /// </summary>
        public ProcessSet Procs { get; private set; }

        /// <summary>
        /// Stage runtime data
        /// </summary>
        public IJitStageEngine Engine { get; set; }

        /// <summary>
        /// Process Push Links
        /// </summary>
        private Dictionary<string, List<string>> _processKeyLinks = new Dictionary<string, List<string>>();


        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitStage()
        {
            Classes.Add(":Stage");
            Procs = new ProcessSet();
            Engine = new JitStageEngine();
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is JitStage st)
            {
                return st.ID == ID;
            }
            else
            {
                return false;
            }
        }
        public override string ToString()
        {
            return $"{GetType().Name} ID={ID}";
        }

        [JacListAdd(PropertyName = "Procs")]
        public void ProcsAdd(object obj)
        {
            if (obj is JitProcess proc)
            {
                Procs.Add(proc);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListRemove(PropertyName = "Procs")]
        public void ProcsRemove(object obj)
        {
            if (obj is JitProcess proc)
            {
                Procs.Remove(proc);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
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

        [JacListAdd(PropertyName = "ProcLinks")]
        public void AddProcLinks(object description)
        {
            if (description == null)
            {
                throw new JitException(JitException.NullValue, "ProcLinks");
            }
            if (description is JacPushLinkDescription push)
            {
                var key1 = push.From is JitProcess p1 ? p1.ID : push.From?.ToString();
                var key2 = push.To is JitProcess p2 ? p2.ID : push.To?.ToString();
                AddProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
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
