// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using ProcessKey = System.String;

namespace Tono.Jit
{
    public abstract class JitProcessGroup : JitProcess
    {
        private readonly LinkedList<ProcessKey> procKeySeq = new LinkedList<ProcessKey>();

        /// <summary>
        /// add child process as top priority
        /// </summary>
        /// <param name="procKey">Process Key(ID/Name)</param>
        public virtual void Add(JitStage stage, ProcessKey procKey, bool isCheckNoInstanceError = true)
        {
            procKeySeq.AddFirst(procKey);
            if (isCheckNoInstanceError)
            {
                if( stage.FindProcess(procKey) == null)
                {
                    throw new JitException(JitException.FormatNoProcKey, procKey);
                }
            }
        }

        /// <summary>
        /// query child processes order by priority
        /// </summary>
        public IEnumerable<ProcessKey> ChildProcessKeys
        {
            get
            {
                for (var node = procKeySeq.First; node != null; node = node.Next)
                {
                    yield return node.Value;
                }
            }
        }

        /// <summary>
        /// process count in this group
        /// </summary>
        public int Count => procKeySeq.Count;
    }
}
