// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tono
{
    /// <summary>
    /// Synonym management utility
    /// </summary>
    public class Synonym
    {
        /// <summary>
        /// Version string
        /// </summary>
        public string Version = "NO_VERSION_INFO";

        private readonly List<Regex> _regs = new List<Regex>();

        /// <summary>
        /// original strings
        /// </summary>
        private readonly List<string> _froms = new List<string>();

        /// <summary>
        /// converted strings
        /// </summary>
        private readonly List<string> _tos = new List<string>();

        private string _fromstr = null;
        private string _tostr = null;

        /// <summary>
        /// register a synonym
        /// </summary>
        /// <param name="from">original word</param>
        /// <param name="to">converted word</param>
        public void Add(string from, string to)
        {
            var from2 = StrMatch.ReplaceMetaStr(from);
            Regex reg;
            if (from.EndsWith("#"))
            {
                from = from.Substring(0, from.Length - 1);
                from2 = from2.Substring(0, from2.Length - 1);
                reg = new Regex(string.Format("(^|[^A-Z0-9]){0}[0-9]+", from2), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            else
            {
                reg = new Regex(string.Format("(^|[^A-Z0-9]){0}($|[^A-Z0-9])", from2), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            _regs.Add(reg);
            _froms.Add(from);
            _tos.Add(to);
        }

        private string replaceEvaluator(Match m)
        {
            var ret = m.ToString();
            var id = ret.IndexOf(_fromstr, StringComparison.CurrentCultureIgnoreCase);
            if (id >= 0)
            {
                ret = ret.Substring(0, id) + _tostr + ret.Substring(id + _fromstr.Length);
            }
            return ret;
        }

        /// <summary>
        /// get synonym word
        /// </summary>
        /// <param name="wordstr"></param>
        /// <returns></returns>
        public string Get1WordOf(string wordstr)
        {
            for (var i = 0; i < _froms.Count; i++)
            {
                _fromstr = _froms[i];
                _tostr = _tos[i];
                wordstr = _regs[i].Replace(wordstr, replaceEvaluator);
            }
            return wordstr;
        }

        /// <summary>
        /// replace the all words to them synonym
        /// </summary>
        /// <param name="linestring"></param>
        /// <returns>replaced string</returns>
        public string GetMultiWordsOf(string linestring)
        {
            var regex = new Regex("[0-9a-zA-Z/_'#%$&-]+");
            var matches = regex.Matches(linestring);
            var dic = new Dictionary<string, string>();
            for (var i = 0; i < matches.Count; i++)
            {
                dic[matches[i].Value] = Get1WordOf(matches[i].Value);
            }
            int id;
            var startid = 0;
            var ret = linestring;
            foreach (var kv in dic)
            {
                if (kv.Key != kv.Value)
                {
                    for (; ; )
                    {
                        id = ret.IndexOf(kv.Key, startid, StringComparison.CurrentCultureIgnoreCase);
                        if (id < 0)
                        {
                            break;
                        }
                        else
                        {
                            ret = ret.Substring(0, id) + kv.Value + ret.Substring(id + kv.Key.Length);
                            startid = id + kv.Value.Length;
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// clear synonym database
        /// </summary>
        public void Clear()
        {
            Version = "NO_VERSION_INFO";
            _regs.Clear();
            _froms.Clear();
            _tos.Clear();
        }
    }
}
