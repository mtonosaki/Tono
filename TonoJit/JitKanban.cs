// Copyright (c) Manabu Tonosaki All rights reserved.
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
    public class JitKanban : JitVariable
    {
        private static int _counter = 0;

        /// <summary>
        /// Kanvan instance ID (auto numbering)
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The constuctor of this class
        /// </summary>
        public JitKanban()
        {
            ID = ++_counter;
            Classes.Set(":Kanban");
        }

        /// <summary>
        /// NOTE: This is for testing. reset kanban ID counter
        /// </summary>
        public static void ResetIDCounter()
        {
            _counter = 0;
        }

        /// <summary>
        /// Previous process (Work origin, Kanban destination)
        /// かんばんを投入する（ワークがある）工程 = FROM
        /// </summary>
        public Func<JitProcess> PullFrom { get; set; }

        /// <summary>
        /// This process (Work destination, Kanban origin)
        /// ワークの目的地 = TO
        /// </summary>
        public Func<JitProcess> PullTo { get; set; }

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
            return ID;
        }

        public override string ToString()
        {
            return $"{GetType().Name} ID={ID} From={(PullFrom?.Invoke()?.Name ?? "?")} To={(PullTo?.Invoke()?.Name ?? "?")}";
        }
    }
}
