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

            public JitProcess FindById(string processid)
            {
                return _procs.Where(a => a.ID.Equals(processid)).FirstOrDefault();
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
                    return _procs.Where(a => a.Name?.Equals(procname) ?? false).FirstOrDefault();
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
