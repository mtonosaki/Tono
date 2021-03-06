﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections.Generic;

namespace Tono
{
    /// <summary>
    /// ID type that have name
    /// </summary>
    public class NamedId
    {
        /// <summary>
        /// nothing id
        /// </summary>
        public static readonly NamedId Nothing = new NamedId { Name = "Nothing", Id = new Id { Value = int.MinValue } };

        /// <summary>
        /// ID integer value
        /// </summary>
        public Id Id { get; private set; }

        /// <summary>
        /// the ID's name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// implicit cast operator from NamedId to Id
        /// </summary>
        /// <param name="nid"></param>
        public static implicit operator Id(NamedId nid)
        {
            return nid?.Id ?? Nothing;
        }

        private static Dictionary<string, NamedId> buf = null;
        private static Dictionary<int, NamedId> idbuf = null;

        /// <summary>
        /// Make instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NamedId From(string name, int value)
        {
            return From(name, new Id { Value = value });
        }

        /// <summary>
        /// Make instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static NamedId From(string name, Id value)
        {
            if (buf == null)
            {
                buf = new Dictionary<string, NamedId>();
                idbuf = new Dictionary<int, NamedId>();
            }
            lock (buf)
            {
                NamedId ret = new NamedId
                {
                    Id = value,
                    Name = name,
                };
                buf[name] = ret;
                idbuf[value.Value] = ret;
                return ret;
            }
        }

        public static NamedId FromIDNoName(int id)
        {
            return FromIDNoName(new Id { Value = id });
        }

        public static NamedId FromIDNoName(Id id)
        {
            if (buf == null)
            {
                buf = new Dictionary<string, NamedId>();
                idbuf = new Dictionary<int, NamedId>();
            }
            lock (buf)
            {
                if (idbuf.TryGetValue(id.Value, out var ret))
                {
                    return ret;
                }
                else
                {
                    for (; ; )
                    {
                        var num = MathUtil.Rand(int.MinValue, int.MaxValue);
                        if (idbuf.ContainsKey(num) == false)
                        {
                            ret = new NamedId
                            {
                                Id = new Id { Value = num },
                                Name = $"__NamedId_NoName_{id}__",
                            };
                            idbuf[num] = ret;
                            buf[ret.Name] = ret;
                            return ret;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get Instance by name if you have already created. If new one, create as new.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static NamedId FromName(string name)
        {
            if (buf == null)
            {
                buf = new Dictionary<string, NamedId>();
                idbuf = new Dictionary<int, NamedId>();
            }
            lock (buf)
            {
                if (buf.TryGetValue(name, out var ret))
                {
                    return ret;
                }
                else
                {
                    for (; ; )
                    {
                        var num = MathUtil.Rand(int.MinValue, int.MaxValue);
                        if (idbuf.ContainsKey(num) == false)
                        {
                            ret = new NamedId
                            {
                                Id = new Id { Value = num },
                                Name = name,
                            };
                            idbuf[num] = ret;
                            buf[name] = ret;
                            return ret;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{Name}(id={Id})";
        }
        public override bool Equals(object obj)
        {
            if (obj is NamedId ni)
            {
                return ni.Id == Id;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Id.Value;
        }
    }
}
