// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Tono
{
    /// <summary>
    /// Wild card string match utility
    /// </summary>
    /// <remarks>
    /// aaa					contains aaa
    /// -"aaa"				not contains aaa
    /// aaa bbb				contain both aaa and bbb
    /// aaa -"bbb"			contain aaa then remove record contains bbb
    /// aaa {^ABC} ccc		contain aaa and ccc, and match regular expression ^ABC  ---   {} means regular expression
    /// a???b				match string that length is 5 that start with a and end with b. --- ? means a character(not null)
    /// a*b					match string start with a and end with b or just "ab"
    /// ^ABC				match string starts with ABC
    /// ABC$				match string ends with ABC
    /// 
    /// the meta charactors of regular expression
    /// . ^ $ [ ] * + ? | ( )
    /// </remarks>
    public class StrMatch
    {
        private static readonly string _metastr = "\".^$[]*+?|()\\";
        private readonly List<List<Regex>> _resCollects = new List<List<Regex>>();
        private readonly List<List<Regex>> _resRemoves = new List<List<Regex>>();
        private readonly bool _isAllMatch = false;
        private readonly int NRemove = 0;

        /// <summary>
        /// make instance with specific match pattern
        /// </summary>
        /// <param name="pattern"></param>
        public StrMatch(string pattern)
        {
            var reopt = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            var isRegularExpressioning = false;
            var isDoubleQuatationing = false;
            var isEscaping = false;

            var expss = new List<List<string>>();
            var str = new StringBuilder();

            var exps = new List<string>();
            expss.Add(exps);

            pattern = pattern.Trim();
            for (var ci = 0; ci < pattern.Length; ci++)
            {
                var c = pattern[ci];
                if (isRegularExpressioning == false)
                {
                    if (c == '\\' || isEscaping)
                    {
                        isEscaping = !isEscaping;
                        str.Append(c);
                        continue;
                    }
                    if (c == '{')
                    {
                        isRegularExpressioning = true;
                    }
                    if (c == '\"')
                    {
                        isDoubleQuatationing = !isDoubleQuatationing;
                        str.Append(c);
                        continue;
                    }
                    if (c == '|' && !isDoubleQuatationing)
                    {
                        var cc = str.ToString().Trim();
                        if (cc != null)
                        {
                            exps.Add(cc);
                            expss.Add(exps = new List<string>());   // prepare next OR operator
                        }
                        str = new StringBuilder();
                        continue;
                    }
                    if (char.IsWhiteSpace(c) && !isDoubleQuatationing)
                    {
                        var cc = str.ToString().Trim();
                        if (cc != "")
                        {
                            exps.Add(cc);
                        }
                        str = new StringBuilder();
                    }
                    else
                    {
                        str.Append(c);
                    }
                }
                else
                {
                    str.Append(c);
                    if (c == '}')
                    {
                        isRegularExpressioning = false;
                        exps.Add(str.ToString());
                        str = new StringBuilder();
                    }
                }
            }
            if (str.ToString().Trim() != "")
            {
                exps.Add(str.ToString());
            }

            foreach (var strs in expss)
            {
                var resRemove = new List<Regex>();
                _resRemoves.Add(resRemove);
                var resCollect = new List<Regex>();
                _resCollects.Add(resCollect);

                var nStartMark = 0; // number of ＾ character
                var nEndMark = 0;   // number of ＄ character
                foreach (var com in strs)
                {
                    var com2 = StrMatch.ConvertSimpleAsterisk(com);
                    if (com2.StartsWith("^"))
                    {
                        nStartMark++;
                    }

                    if (com2.EndsWith("$") && com2.StartsWith("-") == false)
                    {
                        nEndMark++;
                    }
                }
                if (nStartMark > 1)
                {
                    Debug.WriteLine("Warning : Found ^ mark twice or more");
                }
                if (nEndMark > 1)
                {
                    Debug.WriteLine("Warning : Found $ mark twice of more");
                }

                foreach (var com in strs)
                {
                    if (com.Length < 1)
                    {
                        continue;
                    }
                    if ((com.StartsWith("{") || com.StartsWith("-{")) && com.EndsWith("}"))
                    {
                        var re = new Regex(com.Substring(com.IndexOf('{') + 1, com.Length - com.IndexOf('{') - 2), reopt);
                        if (com[0] == '-')
                        {
                            resRemove.Add(re);
                            NRemove++;
                        }
                        else
                        {
                            resCollect.Add(re);
                        }
                    }
                    else
                    {
                        var isRemove = false;
                        string restr;
                        if (com.StartsWith("-"))
                        {
                            restr = com.Substring(1, com.Length - 1);
                            if (restr == "")
                            {
                                restr = " ";    // Restore space character that removed with Trim function
                            }
                            isRemove = true;
                        }
                        else
                        {
                            restr = com;
                        }

                        // Remove double quotations
                        var preC = '\0';
                        var restr2 = new StringBuilder();
                        for (var i = 0; i < restr.Length; i++)
                        {
                            var c = restr[i];
                            if (c == '\"' && preC != '\\')
                            {
                                preC = c;
                                continue;
                            }
                            restr2.Append(c);
                            preC = c;
                        }
                        restr = restr2.ToString();

                        // Support simple asterisk expression
                        restr = StrMatch.ConvertSimpleAsterisk(restr);

                        // Change * or ? character to regular expression
                        restr = restr.Replace(".", "\\.");
                        restr = restr.Replace("*", ".*");
                        restr = restr.Replace("?", ".?");

                        // Kaizen
                        restr = Regex.Replace(restr, "&", "\\s*&\\s*"); // Not care space character around & mark

                        // remove consecutive space
                        int ll;
                        do
                        {
                            ll = restr.Length;
                            restr = restr.Replace("  ", " ");
                        } while (ll != restr.Length);
                        restr = restr.Replace(" ", " +");

                        if (restr.Length > 0)
                        {
                            try
                            {
                                var re = new Regex(restr, reopt);
                                if (isRemove)
                                {
                                    resRemove.Add(re);
                                    NRemove++;
                                }
                                else
                                {
                                    resCollect.Add(re);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
            }

            if (NRemove == 0)
            {
                _isAllMatch = IsMatch("龗鱻麤");
            }
            else
            {
                _isAllMatch = false;
            }
        }

        /// <summary>
        /// Test with OR operator
        /// </summary>
        /// <returns></returns>
        public bool IsMatch(string str)
        {
            if (IsAllMatch)
            {
                return true;
            }
            for (var i = 0; i < _resRemoves.Count; i++)
            {
                if (evaluateWithResex(str, _resCollects[i], _resRemoves[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Test list with regular expression as AND operator
        /// </summary>
        /// <param name="str">strings</param>
        /// <param name="collects">match</param>
        /// <param name="removes">remove</param>
        /// <returns></returns>
        private bool evaluateWithResex(string str, List<Regex> collects, List<Regex> removes)
        {
            foreach (var re in removes)
            {
                if (re.IsMatch(str))
                {
                    return false;
                }
            }
            foreach (var re in collects)
            {
                if (re.IsMatch(str) == false)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check the all match string
        /// </summary>
        public bool IsAllMatch => _isAllMatch;

        /// <summary>
        /// Support * character as start/end with function
        /// AAA*  ： Starts with AAA
        /// *BBB　： Ends with BBB
        /// *CC*　： Contains CC
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertSimpleAsterisk(string str)
        {
            var nAst = 0;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '*')
                {
                    nAst++;
                }
            }
            if (str.StartsWith("*") && str.EndsWith("*") && str.Length > 2 && nAst == 2)
            {
                return str.Substring(1, str.Length - 2);
            }
            if (str.StartsWith("*") && str.Length > 1 && nAst == 1)
            {
                return str.Substring(1) + "$";
            }
            if (str.EndsWith("*") && str.Length > 1 && nAst == 1)
            {
                return "^" + str.Substring(0, str.Length - 1);
            }
            return str;
        }

        /// <summary>
        /// Change pattern to fazzy one for example removes ^ mark
        /// </summary>
        /// <param name="fs">original match string</param>
        /// <returns>fazzy match string</returns>
        public static string MakeFilterFazzy(string fs)
        {
            var isRegexBracket = false;
            fs = fs.Trim();
            if (fs.StartsWith("{") && fs.EndsWith("}"))
            {
                fs = fs.Substring(1, fs.Length - 2);
                isRegexBracket = true;
            }

            // Remove escape sequence
            var isEscaping = false;
            var ret = new StringBuilder();
            for (var i = 0; i < fs.Length; i++)
            {
                var c = fs[i];
                if (c == '\\' || isEscaping)
                {
                    isEscaping = !isEscaping;
                    if (isEscaping == false)
                    {
                        ret.Append(' ');
                    }
                    continue;
                }
                ret.Append(c);
            }
            fs = ret.ToString();

            fs = ConvertSimpleAsterisk(fs);
            fs = fs.Replace("^", "");
            fs = fs.Replace("$", "");
            fs = fs.Replace(",", " ");

            // consider string in double quotation.
            var isDoubleQuatationing = false;
            var isMinus = false;
            ret = new StringBuilder();
            for (var i = 0; i < fs.Length; i++)
            {
                var c = fs[i];
                if (c == '\"')
                {
                    isDoubleQuatationing = !isDoubleQuatationing;
                    if (isMinus)
                    {
                        ret.Append('\a');
                        isMinus = false;
                    }
                }
                if (c == '-')
                {
                    if (isDoubleQuatationing || isRegexBracket)
                    {
                        if (isMinus)
                        {
                            ret.Append('\a');
                            ret.Append(' ');
                        }
                        ret.Append('\a');
                        isMinus = true;
                    }
                }
                if (char.IsWhiteSpace(c))
                {
                    if (isMinus)
                    {
                        ret.Append('\a');
                        isMinus = false;
                    }
                }
                ret.Append(c);
            }
            if (isMinus)
            {
                ret.Append('\a');
            }
            fs = ret.ToString();

            // remove double quotations
            fs = fs.Replace("\"", "");
            fs = fs.Replace("\a", "\"");    // restore saved double quotation

            int n;
            do
            {
                n = fs.Length;
                fs = fs.Replace("  ", " ");
            } while (n != fs.Length);
            fs = fs.Trim();

            return fs;
        }

        /// <summary>
        /// find a pattern matched string
        /// </summary>
        /// <param name="sin">input</param>
        /// <param name="regstr">regular expression</param>
        /// <returns></returns>
        public static string GetRegexResult(string sin, string regstr)
        {
            var reg = new Regex(regstr, RegexOptions.IgnoreCase);
            var match = reg.Match(sin);
            var ret = "";
            while (match.Success)
            {
                if (match.Value.Length > ret.Length)
                {
                    ret = match.Value;
                }
                match = match.NextMatch();
            }
            return ret;
        }

        /// <summary>
        /// Convert meta string to non-meta string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceMetaStr(string str)
        {
            var ret = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (_metastr.IndexOf(c) >= 0)
                {
                    ret.Append('\\');
                }
                ret.Append(c);
            }
            return ret.ToString();
        }
    }
}