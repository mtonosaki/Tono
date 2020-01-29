// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary>
    /// Signals of JIT-model named Kanban
    /// </summary>
    /// <remarks>
    /// 1-Kanban can call 1-Work
    /// </remarks>
    [JacTarget(Name = "Kanban")]
    public class JitKanban : JitVariable, IJitObjectID
    {
        /// <summary>
        /// Kanvan instance ID (auto numbering)
        /// </summary>
        public string ID { get; set; } = JacInterpreter.MakeID("Kanban");

        public int TestID { get => (int)(ChildVriables["TestID"].Value ?? int.MinValue); set => ChildVriables["TestID"] = JitVariable.From(value); }

        public JitStage Stage { get; set; }

        /// <summary>
        /// The constuctor of this class
        /// </summary>
        public JitKanban()
        {
            Classes.Set(":Kanban");
        }


        /// <summary>
        /// Previous process (Work origin, Kanban destination)
        /// かんばんを投入する（ワークがある）工程 = FROM
        /// </summary>
        public string PullFromProcessKey { get; set; }

        /// <summary>
        /// This process (Work destination, Kanban origin)
        /// ワークの目的地 = TO
        /// </summary>
        public string PullToProcessKey { get; set; }

        /// <summary>
        /// kanban owner work (if no work, null)
        /// かんばんが付いているワーク（無ければ null)
        /// </summary>
        public JitWork Work { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is JitKanban ka)
            {
                return ka.ID == ID;
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

        public override string ToString()
        {
            return $"{GetType().Name} ID={ID} From={(PullFromProcessKey ?? "?")} To={(PullToProcessKey ?? "?")}";
        }
    }
}
