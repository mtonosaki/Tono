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
        /// Stage runtime data
        /// </summary>
        public IJitStageEngine Engine { get; set; }

        /// <summary>
        /// Jit Sub Model
        /// </summary>
        public IJitStageModel Model { get; set; }

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitStage()
        {
            Classes.Add(":Stage");
            Engine = new JitStageEngine();
            Model = new JitStageModel();
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
            Model.ProcsAdd(obj);
        }

        [JacListRemove(PropertyName = "Procs")]
        public void ProcsRemove(object obj)
        {
            Model.ProcsRemove(obj);
        }
        [JacListAdd(PropertyName = "ProcLinks")]
        public void AddProcLinks(object description)
        {
            Model.AddProcLinks(description);
        }
    }
}
