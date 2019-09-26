using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    public partial class JitProcess
    {
        /// <summary>
        /// work destination collection class 
        /// </summary>
        public class Destinations : IEnumerable<JitProcess>
        {
            private readonly List<Func<JitProcess>> _dstFuncs = new List<Func<JitProcess>>();

            /// <summary>
            /// destination count
            /// </summary>
            public int Count => _dstFuncs.Count;

            /// <summary>
            /// add process (NOTE: add sequence is important because of for being sequence of CiSwitchNext)
            /// </summary>
            /// <param name="dstFunc">next process</param>
            public void Add(Func<JitProcess> dstFunc)
            {
                _dstFuncs.Add(dstFunc);
            }

            /// <summary>
            /// add processes(see also comment of Add method)
            /// </summary>
            /// <param name="dst"></param>
            public void AddRange(IEnumerable<Func<JitProcess>> dstFuncs)
            {
                foreach (Func<JitProcess> dstFunc in dstFuncs)
                {
                    Add(dstFunc);
                }
            }

            /// <summary>
            /// next processes (0=first process)
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public JitProcess this[int index]
            {
                get
                {
                    if (index < 0)
                    {
                        return null;
                    }
                    if (index < _dstFuncs.Count)
                    {
                        return _dstFuncs[MathUtil.Max(0, index)]();
                    }
                    else
                    {
                        return _dstFuncs[_dstFuncs.Count - 1]();
                    }
                }
            }

            /// <summary>
            /// Get first process or null
            /// </summary>
            /// <returns></returns>
            public JitProcess FirstOrNull()
            {
                if (Count > 0)
                {
                    return this[0];
                }
                else
                {
                    return null;
                }
            }

            public IEnumerator<JitProcess> GetEnumerator()
            {
                IEnumerable<JitProcess> ret =
                    from func in _dstFuncs
                    let proc = func()
                    select proc;
                return ret.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                IEnumerable<JitProcess> ret =
                    from func in _dstFuncs
                    let proc = func()
                    select proc;
                return ret.GetEnumerator();
            }
        }
    }
}
