// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

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

        /// <summary>
        /// Previous process (Work origin, Kanban destination)
        /// かんばんを投入する（ワークがある）工程 = FROM
        /// </summary>
        public JitLocation PullFrom { get; set; }

        /// <summary>
        /// This process (Work destination, Kanban origin)
        /// ワークの目的地 = TO
        /// </summary>
        public JitLocation PullTo { get; set; }

        /// <summary>
        /// kanban owner work (if no work, null)
        /// かんばんが付いているワーク（無ければ null)
        /// </summary>
        public JitWork Work { get; set; }

        /// <summary>
        /// The constuctor of this class
        /// </summary>
        public JitKanban()
        {
            Classes.Set(":Kanban");
        }

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
            return $"{GetType().Name} ID={ID} From=\"{PullFrom?.FullPath ?? "null"}\" To=\"{PullTo?.FullPath ?? "null"}\"";
        }

        /// <summary>
        /// Find JitStage instance
        /// </summary>
        /// <returns></returns>
        public JitStage FindStage()
        {
            if (PullFrom.Stage != null)
            {
                return PullFrom.Stage;
            }

            if (PullTo.Stage != null)
            {
                return PullTo.Stage;
            }

            return null;
        }

    }
}
