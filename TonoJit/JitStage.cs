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
        public string ID { get; set; } = JacInterpreter.MakeID("StageSubset");

        /// <summary>
        /// Stage runtime data
        /// </summary>
        public Func<IJitStageEngine> Engine { get; set; }

        /// <summary>
        /// Jit Sub Model
        /// </summary>
        public IJitSubset Model { get; set; }

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitStage() : base()
        {
            Classes.Add(":Stage");
            Model = new JitSubset();

            // Prepare Master Engine
            var engine = new JitStageEngine();
            Engine = (() => engine);
        }

        /// <summary>
        /// Hashcode by ID
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Comparison with ID
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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
        public void AddProcess(object obj)
        {
            if (obj is JitProcess proc)
            {
                Model.ChildProcesses.Add(proc);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListRemove(PropertyName = "Procs")]
        public void RemoveProcess(object obj)
        {
            if (obj is JitProcess proc)
            {
                Model.ChildProcesses.Remove(proc);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
        }

        [JacListAdd(PropertyName = "ProcLinks")]
        public void AddProcessLink(object description)
        {
            if (description == null)
            {
                throw new JitException(JitException.NullValue, "ProcLinks");
            }
            if (description is JacPushLinkDescription push)
            {
                var key1 = push.From is JitProcess p1 ? p1.ID : push.From?.ToString();
                var key2 = push.To is JitProcess p2 ? p2.ID : push.To?.ToString();
                Model.AddProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
        }

        [JacListRemove(PropertyName = "ProcLinks")]
        public void RemoveProcessLink(object description)
        {
            if (description == null)
            {
                throw new JitException(JitException.NullValue, "ProcLinks");
            }
            if (description is JacPushLinkDescription push)
            {
                var key1 = push.From is JitProcess p1 ? p1.ID : push.From?.ToString();
                var key2 = push.To is JitProcess p2 ? p2.ID : push.To?.ToString();
                Model.RemoveProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
        }
    }
}
