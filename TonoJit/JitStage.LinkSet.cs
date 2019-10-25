// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono.Jit
{
    public partial class JitStage
    {
        /// <summary>
        /// process link
        /// </summary>
        public class LinkSet
        {
            /// <summary>
            /// destination processes 目的地セット
            /// </summary>
            public class Destinations
            {
                private readonly List<JitProcess> _dsts = new List<JitProcess>();

                public void SetDestination(JitProcess dst)
                {
                    if (_dsts.Contains(dst) == false)
                    {
                        _dsts.Add(dst);
                    }
                }

                public void SetDestination(IEnumerable<JitProcess> dsts)
                {
                    foreach (JitProcess dst in dsts)
                    {
                        SetDestination(dst);
                    }
                }

                public JitProcess this[int index]
                {
                    get
                    {
                        if (index < _dsts.Count)
                        {
                            return _dsts[index];
                        }
                        else
                        {
                            return null;
                        }
                    }
                }

                /// <summary>
                /// get first process or null  最初の行先を取得
                /// </summary>
                /// <returns></returns>
                public JitProcess FirstOrNull()
                {
                    return this[0];
                }
            }

            private readonly Dictionary<JitProcess/*from*/, Destinations> _links = new Dictionary<JitProcess, Destinations>();

            /// <summary>
            /// make new link between "from" to "to" processes
            /// </summary>
            /// <param name="from"></param>
            /// <param name="to"></param>
            public void SetPushLink(JitProcess from, JitProcess to)
            {
                this[from].SetDestination(to);
            }

            /// <summary>
            /// get destination proccesses
            /// </summary>
            /// <param name="from"></param>
            /// <returns></returns>
            public Destinations this[JitProcess from]
            {
                get
                {
                    if (_links.TryGetValue(from, out Destinations ret) == false)
                    {
                        _links[from] = ret = new Destinations();
                    }
                    return ret;
                }
            }
        }
    }
}
