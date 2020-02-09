using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.Jit
{
    [JacTarget(Name = "StageSubset")]
    public class JitStageSubset : JitVariable, IJitObjectID
    {
        public string ID { get; set; } = JacInterpreter.MakeID("StageSubset");

        /// <summary>
        /// Stage runtime data
        /// </summary>
        public Func<IJitStageEngine> Engine { get; set; }

        /// <summary>
        /// Jit Sub Model
        /// </summary>
        public IJitStageModel Model { get; set; }

        /// <summary>
        /// Jit Child Models
        /// </summary>
        private List<JitStageSubset> ChildModels { get; set; }

        /// <summary>
        /// The Constructor
        /// </summary>
        public JitStageSubset()
        {
            Classes.Add(":StageSubset");
            Model = new JitStageModel();
            ChildModels = new List<JitStageSubset>();
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

        [JacListAdd(PropertyName = "Models")]
        public void ModelsAdd(object obj)
        {
            if (obj is JitStage stage)
            {
                throw new JitException(JitException.DoubleStage, stage.ID, this.ID);
            }
            if (obj is JitStageSubset subset)
            {
                ChildModels.Add(subset);
                subset.Engine = this.Engine;    // TODO: Local Find Engine
            }
        }

        [JacListAdd(PropertyName = "Procs")]
        public void ProcsAdd(object obj)
        {
            if (obj is JitProcess proc)
            {
                Model.Procs.Add(proc);
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
                Model.Procs.Remove(proc);
            }
            else
            {
                throw new JacException(JacException.Codes.TypeMismatch, $"Procs.Add type mismatch arg type={(obj?.GetType().Name ?? "null")}");
            }
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
                Model.AddProcessLink(key1, key2);
            }
            else
            {
                throw new JitException(JitException.TypeMissmatch, $"Expecting type JacPushLinkDescription but {description.GetType().Name}");
            }
        }

        [JacListRemove(PropertyName = "ProcLinks")]
        public void RemoveProcLinks(object description)
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
