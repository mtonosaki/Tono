// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProcessKey = System.String;

namespace Tono.Jit
{
    public partial class JitProcess
    {
        /// <summary>
        /// work destination collection class 
        /// </summary>
        public class DestProcessKeys : IEnumerable<ProcessKey>
        {
            private readonly List<ProcessKey> _dstProcKeys = new List<ProcessKey>();

            /// <summary>
            /// destination count
            /// </summary>
            public int Count => _dstProcKeys.Count;

            /// <summary>
            /// add process (NOTE: add sequence is important because of for being sequence of CiSwitchNext)
            /// </summary>
            /// <param name="processKey">next process ID/Name</param>
            public void Add(ProcessKey processKey)
            {
                _dstProcKeys.Add(processKey);
            }

            public void Add(JitProcess process)
            {
                Add(process.ID);
            }

            /// <summary>
            /// add processes(see also comment of Add method)
            /// </summary>
            /// <param name="dst"></param>
            public void AddRange(IEnumerable<ProcessKey> dstProcKeys)
            {
                foreach (var dstProcKey in dstProcKeys)
                {
                    Add(dstProcKey);
                }
            }

            /// <summary>
            /// next processes key(Name/ID) (0=first process)
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public string this[int index]
            {
                get
                {
                    if (index < 0)
                    {
                        return null;
                    }
                    if (index < _dstProcKeys.Count)
                    {
                        return _dstProcKeys[MathUtil.Max(0, index)];
                    }
                    else
                    {
                        return _dstProcKeys[_dstProcKeys.Count - 1];
                    }
                }
            }

            /// <summary>
            /// Get first process ID/Name or null
            /// </summary>
            /// <returns></returns>
            public string FirstOrNull()
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

            public IEnumerator<string> GetEnumerator()
            {
                return _dstProcKeys.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dstProcKeys.GetEnumerator();
            }
        }
    }
}
