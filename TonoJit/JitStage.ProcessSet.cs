// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;

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

            /// <summary>
            /// process count
            /// </summary>
            public int Count => _procs.Count;

            public void Add(JitProcess proc)
            {
                _procs.Add(proc);
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
            }

            public JitProcess Find(string procKey)
            {
                // TODO: Speed up
                var ret = _procs.Where(a => a.ID.Equals(procKey)).FirstOrDefault();
                if (ret == null)
                {
                    ret = _procs.Where(a => a.Name?.Equals(procKey) ?? false).FirstOrDefault();
                }
                return ret;
            }

            /// <summary>
            /// get process by Name/ID
            /// 名前で子プロセスを検索。遅延評価はこのタイミングで行ったものを覚えておく
            /// </summary>
            /// <param name="procKey"></param>
            /// <returns></returns>
            public JitProcess this[string procKey]
            {
                get => Find(procKey);
            }

            public JitProcess this[int index] => _procs[index];
        }
    }
}
