// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono.Jit
{
    public partial class JitStage
    {
        /// <summary>
        /// process collection
        /// </summary>
        public class ProcessSet
        {
            private readonly List<JitProcess> _procs = new List<JitProcess>();
            private readonly Dictionary<string, JitProcess> cache = new Dictionary<string, JitProcess>();

            /// <summary>
            /// process count
            /// </summary>
            public int Count => _procs.Count;

            public void Add(JitProcess proc)
            {
                _procs.Add(proc);
                cache[proc.Name] = proc;
            }

            public void Add(IEnumerable<JitProcess> procs)
            {
                foreach (var proc in procs)
                {
                    Add(proc);
                }
            }

            public void Remove(JitProcess proc)
            {
                _procs.Remove(proc);
                cache.Remove(proc.Name);
            }

            /// <summary>
            /// get process by name
            /// 名前で子プロセスを検索。遅延評価はこのタイミングで行ったものを覚えておく
            /// </summary>
            /// <param name="procname"></param>
            /// <returns></returns>
            public JitProcess this[string procname]
            {
                get
                {
                    if (cache.TryGetValue(procname, out JitProcess proc))
                    {
                        return proc;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public JitProcess this[int index]
            {
                get
                {
                    return _procs[index];
                }
            }
        }
    }
}
