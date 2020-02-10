// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Tono.Jit
{
    /// <summary>
    /// JIT-Model Work object
    /// </summary>
    /// <remarks>
    /// Work is general item to make flow in just-in-time model that is not only physical item.
    /// </remarks>
    [JacTarget(Name = "Work")]
    public class JitWork : JitVariable, IJitObjectID
    {
        public string ID { get; set; } = JacInterpreter.MakeID("Work");

        /// <summary>
        /// Target Subset for this Work
        /// </summary>
        public JitSubset Subset { get; set; }

        /// <summary>
        /// Previous process (null = no previous)
        /// </summary>
        public (JitSubset Subset, JitProcess Process) PrevProcess { get; set; }

        /// <summary>
        /// Current process
        /// </summary>
        /// <remarks>
        /// If this work join to a parent work, CurrentProcess is set to null
        /// </remarks>
        public (JitSubset Subset, JitProcess Process) CurrentProcess { get; set; }

        /// <summary>
        /// Next process (null = no next)
        /// </summary>
        public (JitSubset Subset, JitProcess Process) NextProcess { get; set; }

        /// <summary>
        /// work status
        /// </summary>
        public JitWorkStatus Status { get; set; } = JitWorkStatus.None;

        /// <summary>
        /// time when this work enter to CurrentProcess of the Stage
        /// </summary>
        public DateTime EnterTime { get; set; }

        /// <summary>
        /// scheduled exit time of the Stage
        /// </summary>
        public DateTime ExitTime { get; set; }

        /// <summary>
        /// Kanban objects that move with this work. (NOTE: kanban is only one in this work normally. Do not expect to make multi kanban model)
        /// ワークと共に移動させるかんばん。注：通常は１枚だけ入るようにモデリングする必要がある。ワークに２枚以上かんばんが付くことを期待したモデルは作らない
        /// </summary>
        public List<JitKanban> Kanbans { get; private set; } = new List<JitKanban>();

        /// <summary>
        /// Child work
        /// </summary>
        public Dictionary<string/*ChildWorkName*/, JitWork> ChildWorks { get; private set; } = new Dictionary<string, JitWork>();

        /// <summary>
        /// the constructor of this class
        /// </summary>
        public JitWork()
        {
            Classes.Set(":Work");
            ChildVriables["Cost"][JitVariable.From("Count")].Value = JitVariable.From(1);  // カウントコスト
            ChildVriables["Cost"][JitVariable.From("Random")].Value = JitVariable.From(MathUtil.Rand0());  // ランダムコスト 0～0.99999999
        }

        /// <summary>
        /// Get process or null from tuple
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static JitProcess GetProcess((JitSubset Subset, JitProcess Process) tar)
        {
            if (tar == default)
            {
                return null;
            }
            else
            {
                return tar.Process;
            }
        }

        public static bool Equals((JitSubset Subset, JitProcess Process) left, (JitSubset Subset, JitProcess Process) right)
        {
            if (left == default || right == default) return false;
            return left == right;
        }

        public override bool Equals(object obj)
        {
            if (obj is JitWork w)
            {
                return w.ID == ID;
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
            return $"{GetType().Name} {Name}@{GetProcess(CurrentProcess)?.Name ?? "n/a"} → {GetProcess(NextProcess)?.Name ?? "n/a"} ID={ID}";
        }
    }

    /// <summary>
    /// Work status
    /// </summary>
    public enum JitWorkStatus
    {
        None,
        Stopping,
        Moving,
    }
}
