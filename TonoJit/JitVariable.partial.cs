// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tono.Jit
{
    public partial class JitVariable
    {
        /// <summary>
        /// General Class
        /// </summary>
        public static class Class
        {
            public const string Object = ":Object";
            public const string String = ":String";
            public const string Double = ":Double";
            public const string Int = ":Int";
        }

        /// <summary>
        /// Variable exception
        /// </summary>
        public abstract class VtpmlException : Exception
        {
            public VtpmlException()
            {
            }
            public VtpmlException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// syntax error exception
        /// </summary>
        public class SyntaxErrorException : VtpmlException
        {
            public SyntaxErrorException()
            {
            }
            public SyntaxErrorException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// not allow operation error exception
        /// </summary>
        public class NotAllowErrorException : VtpmlException
        {
            public NotAllowErrorException()
            {
            }
            public NotAllowErrorException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// child value accessor THIS[child]
        /// </summary>
        public class ChildValueDic
        {
            public JitVariable Parent { get; set; }

            private Dictionary<string, JitVariable> _dat = new Dictionary<string, JitVariable>();

            public ChildValueDic Clone()
            {
                ChildValueDic ret = new ChildValueDic
                {
                    Parent = Parent,
                    _dat = new Dictionary<string, JitVariable>(_dat),
                };
                return ret;
            }

            /// <summary>
            /// check child variable by name
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public bool Contains(string name)
            {
                return _dat.ContainsKey(name);
            }

            /// <summary>
            /// check specified instance in children 指定変数が含まれるかどうかを調べる
            /// </summary>
            /// <param name="instanceName"></param>
            /// <returns></returns>
            public JitVariable GetValueOrNull(string instanceName)
            {
                if (instanceName == "Global")
                {
                    return JitVariable.Global;
                }

                if (_dat.TryGetValue(instanceName, out JitVariable tm))
                {
                    return tm;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// get child value by jfVariable key
            /// </summary>
            /// <param name="instanceName"></param>
            /// <returns></returns>
            public JitVariable this[JitVariable instanceName]
            {
                get => this[instanceName.Value.ToString()];
                set => this[instanceName.Value.ToString()] = value;
            }

            /// <summary>
            /// access to a child value by instance name
            /// </summary>
            /// <param name="instanceName"></param>
            /// <returns></returns>
            public JitVariable this[string instanceName]
            {
                get
                {
                    if (instanceName == "Global")
                    {
                        return JitVariable.Global;
                    }

                    if (_dat.TryGetValue(instanceName, out JitVariable tm) == false)
                    {
                        tm = JitVariable.Null(Class.Object);
                        tm.Name = instanceName;
                        tm.Parent = Parent;
                        _dat[instanceName] = tm;
                    }
                    return tm;
                }
                set
                {
                    if (instanceName == "Global")
                    {
                        throw new JitVariable.NotAllowErrorException("To change Global object is NOT allowed.");
                    }

                    JitVariable tm = value.Clone();
                    tm.Parent = Parent;
                    _dat[instanceName] = tm;
                }
            }

            public IEnumerable<string> Names => _dat.Keys;
        }

        /// <summary>
        /// Class list
        /// </summary>
        public class ClassList : IEnumerable<string>
        {
            private List<string> _dat = new List<string>();
            private bool isHashed = false;
            private int hashcode = 0;

            public ClassList()
            {
            }
            public ClassList(ClassList src)
            {
                _dat.AddRange(src._dat);
            }

            /// <summary>
            /// make a new class list instance from string
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public static ClassList From(string value)
            {
                ClassList ret = new ClassList
                {
                    value
                };
                return ret;
            }

            /// <summary>
            /// Add a class name
            /// </summary>
            /// <param name="classNames"></param>
            public void Add(string classNames)
            {
                string cn = classNames.Trim();
                if (cn.StartsWith(":") == false)
                {
                    throw new SyntaxErrorException("Class name should by started with : character");
                }
                List<string> adds = JitVariable.GetClassNames(classNames).ToList();
                if (adds.Count < 3)
                {
                    foreach (string c in adds)
                    {
                        if (_dat.Contains(c) == false)
                        {
                            _dat.Add(c);
                        }
                    }
                }
                else
                {
                    _dat.AddRange(adds);
                    _dat = _dat.Distinct().ToList();
                }
                isHashed = false;
            }

            public void Add(ClassList from)
            {
                List<string> adds = from.ToList();
                if (adds.Count < 3)
                {
                    foreach (string c in adds)
                    {
                        if (_dat.Contains(c) == false)
                        {
                            _dat.Add(c);
                        }
                    }
                }
                else
                {
                    _dat.AddRange(adds);
                    _dat = _dat.Distinct().ToList();
                }
                isHashed = false;
            }

            public void Add(JitVariable from)
            {
                Add(from.Classes);
            }

            public void Set(string classNames)
            {
                _dat = new List<string>(JitVariable.GetClassNames(classNames));
                if (_dat.Contains(Class.Object) == false)
                {
                    _dat.Insert(0, Class.Object);
                }
                isHashed = false;
            }

            public void Set(string[] classNames)
            {
                _dat = new List<string>(classNames);
                if (_dat.Contains(Class.Object) == false)
                {
                    _dat.Insert(0, Class.Object);
                }
                isHashed = false;
            }

            public void Remove(string className)
            {
                _dat.Remove(className);
                isHashed = false;
            }

            public IEnumerator<string> GetEnumerator()
            {
                return _dat.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _dat.GetEnumerator();
            }

            public string this[int index] => _dat[index];

            public override string ToString()
            {
                return string.Join("", _dat.OrderBy(ks => ks));
            }

            public override bool Equals(object obj)
            {
                if (obj is ClassList cn)
                {
                    int n = 0;
                    foreach (string c in cn)
                    {
                        if (_dat.Contains(c) == false)
                        {
                            return false;
                        }
                        else
                        {
                            n++;
                        }
                    }
                    return n == _dat.Count && n == cn._dat.Count;
                }
                return false;
            }
            public override int GetHashCode()
            {
                if (isHashed == false)
                {
                    hashcode = ToString().GetHashCode();
                }
                return hashcode;
            }
        }
    }
}
