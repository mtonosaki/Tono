// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tono
{
    /// <summary>
    /// String utility
    /// </summary>
    public static class StrUtil
    {
        private static readonly char[] nums = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Make repeat characters
        /// </summary>
        /// <param name="str"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Repeat(string str, int count)
        {
            var ret = new StringBuilder();
            for (var i = count; i > 0; i--)
            {
                ret.Append(str);
            }
            return ret.ToString();
        }

        /// <summary>
        /// Make repeat characters
        /// </summary>
        /// <param name="str"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static string StrDup(string str, int count)
        {
            return Repeat(str, count);
        }

        /// <summary>
        /// Parse short (space, empty = zero)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static short ParseShort(string str)
        {
            if (str == null)
            {
                return 0;
            }
            var s1 = str.Trim();
            if (s1 == "")
            {
                return 0;
            }
            return short.Parse(s1);
        }

        /// <summary>
        /// convert number to h:m:s (60 decimal system)
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        public static string ToHms(double hour)
        {
            var h = Math.Floor(hour);
            var m = Math.Floor((hour - h) * 60);
            var s = Math.Floor(hour * 3600) % 60;
            return string.Format("{0}:{1:D2}:{2:D2}", (int)h, (int)m, (int)s);
        }
        /// <summary>
        /// Parse int (space, empty = zero)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int ParseInt(string str)
        {
            if (str == null)
            {
                return 0;
            }
            var s1 = str.Trim();
            if (s1 == "")
            {
                return 0;
            }
            return int.Parse(s1);
        }

        /// <summary>
        /// parse double concidering % as 0.01 times
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static double ParseDouble(string number)
        {
            if (number == null)
            {
                return 0;
            }
            var s1 = number.Trim();
            if (s1 == "")
            {
                return 0;
            }
            if (s1.EndsWith("%"))
            {
                return double.Parse(s1.Substring(0, s1.Length - 1)) / 100;
            }
            else
            {
                return double.Parse(s1);
            }
        }

        /// <summary>
        /// process escape string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string EnEscape(string str)
        {
            var ret = str;
            ret = ret.Replace("\\a", "\a");
            ret = ret.Replace("\\b", "\b");
            ret = ret.Replace("\\f", "\f");
            ret = ret.Replace("\\n", "\n");
            ret = ret.Replace("\\r", "\r");
            ret = ret.Replace("\\t", "\t");
            ret = ret.Replace("\\v", "\v");
            ret = ret.Replace("\\\"", "\"");
            ret = ret.Replace("\\'", "'");
            ret = ret.Replace("\\\\", "\\");
            return ret;
        }

        /// <summary>
        /// object.ToString() but returns default if null or empty
        /// </summary>
        /// <param name="value"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static string S(object value, string def)
        {
            if (value == null)
            {
                return def;
            }
            var str = value.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return def;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// object.ToString() but returns "" if null
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string S(object str)
        {
            return S(str, "");
        }

        /// <summary>
        /// make formatted string from double value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public static string ToDoubleStrig(double value, int decimals)
        {
            if (Math.Abs(value) >= Math.Pow(10, decimals))
            {
                return Math.Round(value, decimals).ToString();
            }

            if (value == 0)
            {
                return string.Format("{0:0." + StrUtil.StrDup("0", decimals - 1) + "}", value);
            }
            if (Math.Abs(value) < 0.00000000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 11) + "}", Math.Round(value, decimals + 11)));
            }

            if (Math.Abs(value) < 0.0000000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 10) + "}", Math.Round(value, decimals + 10)));
            }

            if (Math.Abs(value) < 0.000000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 9) + "}", Math.Round(value, decimals + 9)));
            }

            if (Math.Abs(value) < 0.00000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 8) + "}", Math.Round(value, decimals + 8)));
            }

            if (Math.Abs(value) < 0.0000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 7) + "}", Math.Round(value, decimals + 7)));
            }

            if (Math.Abs(value) < 0.000001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 6) + "}", Math.Round(value, decimals + 6)));
            }

            if (Math.Abs(value) < 0.00001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 5) + "}", Math.Round(value, decimals + 5)));
            }

            if (Math.Abs(value) < 0.0001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 4) + "}", Math.Round(value, decimals + 4)));
            }

            if (Math.Abs(value) < 0.001)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 3) + "}", Math.Round(value, decimals + 3)));
            }

            if (Math.Abs(value) < 0.01)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 2) + "}", Math.Round(value, decimals + 2)));
            }

            if (Math.Abs(value) < 0.1)
            {
                return removeLastZero(string.Format("{0:0." + StrDup("0", decimals + 1) + "}", Math.Round(value, decimals + 1)));
            }

            return removeLastZero(Math.Round(value, decimals).ToString());
        }

        /// <summary>
        /// remove small zero as 0.0003000 → 0.0003
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string removeLastZero(string number)
        {
            if (number.IndexOf('.') >= 0)
            {
                for (var i = number.Length - 1; number[i] != '.'; i--)
                {
                    if (number[i] != '0')
                    {
                        number = number.Substring(0, i + 1);
                        break;
                    }
                }
            }
            return number;
        }

        /// <summary>
        /// find max length of line
        /// </summary>
        /// <param name="str"></param>
        /// <returns>string length(trimed one's)</returns>
        public static int GetMaxLineLength(string str)
        {
            var strs = str.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var max = 0;
            foreach (var s in strs)
            {
                var l = s.Trim().Length;
                if (l > max)
                {
                    max = l;
                }
            }
            return max;
        }

        /// <summary>
        /// get the string next to the search key in line
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="findKey">key string</param>
        /// <param name="def">return string if not found in the str</param>
        /// <returns>trimmed line string(next to the findKey)</returns>
        public static string GetLineValue(string str, string findKey, string def)
        {
            var ids = str.IndexOf(findKey);
            if (ids < 0)
            {
                return def;
            }
            var st = ids + findKey.Length;
            var ide = str.IndexOf('\r', st);
            var ret = str.Substring(st, ide - st);
            return ret.Trim();
        }

        /// <summary>
        /// 文字列内にある最初に見つかる数字の部分を返す（負記号、小数点、カンマは数字として扱わない）
        /// find index of number string [0-9]
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="start">start position (0=first character)</param>
        /// <param name="position">return number first position</param>
        /// <param name="length">return number string length</param>
        /// <returns>true=success / false=not found number</returns>
        public static bool IndexOfNumber(string str, int start, out int position, out int length)
        {
            position = int.MaxValue;
            int i;
            for (i = 0; i < str.Length; i++)
            {
                if (position == int.MaxValue)
                {
                    if (str[i] >= '0' && str[i] <= '9')
                    {
                        position = i;
                    }
                }
                else
                {
                    if (str[i] < '0' || str[i] > '9')
                    {
                        break;
                    }
                }
            }
            if (position == int.MaxValue)
            {
                length = 0;
                return false;
            }
            else
            {
                length = i - position;
                return true;
            }
        }

        /// <summary>
        /// regular expression to find number
        /// </summary>
        private static readonly Regex _regToGetNumber = new Regex("(^\\-?[0-9]+\\.?[0-9]*)|([0-9]+\\.?[0-9]*)", RegexOptions.Compiled);

        /// <summary>
        /// find first character that is not number
        /// </summary>
        /// <param name="str">target string</param>
        /// <param name="start">start finding position(0=first character)</param>
        /// <returns>-1=the all characters are as number</returns>
        public static int IndexOfNotNumber(string str, int start)
        {
            var match = _regToGetNumber.Match(str, start);
            if (match.Success)
            {
                var id = str.IndexOf(match.Value, start);
                if (id > start)
                {
                    return 0;
                }
                else
                {
                    var pos = id + match.Value.Length;
                    if (pos >= str.Length)
                    {
                        return -1;
                    }
                    else
                    {
                        return pos;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// find position of number or character that related to number in input string
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="start">start finding position (0=first character)</param>
        /// <returns></returns>
        public static int IndexOfNumber(string str, int start)
        {
            for (var i = start; i < str.Length; i++)
            {
                if (str[i] == '-' || str[i] == '+' || str[i] == '.')
                {
                    return i;
                }
                if (str[i] >= '0' && str[i] <= '9')
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// compare string considering number like pattern (1,11,2 → 1,2,11）
        /// </summary>
        /// <param name="x">string1</param>
        /// <param name="y">string2</param>
        /// <returns>0=equal / x ＜ y ? -1,  x ＞ y ? 1</returns>
        public static int ComparerThinkNumber(string x, string y)
        {
            var ptx = 0;
            var pty = 0;
            for (; ; )
            {
                var xi = x.IndexOfAny(nums, ptx);
                var yi = y.IndexOfAny(nums, pty);
                if (xi < 0 || yi < 0)
                {
                    return x.Substring(ptx).CompareTo(y.Substring(pty));
                }
                var c1 = x.Substring(ptx, xi - ptx).CompareTo(y.Substring(pty, yi - pty));
                if (c1 == 0)
                {
                    var xiL = StrUtil.IndexOfNotNumber(x, xi);
                    if (xiL < 0)
                    {
                        xiL = x.Length;
                    }
                    var yiL = StrUtil.IndexOfNotNumber(y, yi);
                    if (yiL < 0)
                    {
                        yiL = y.Length;
                    }
                    try
                    {
                        var vx = double.Parse(x.Substring(xi, xiL - xi));
                        var vy = double.Parse(y.Substring(yi, yiL - yi));
                        if (Math.Abs(vx - vy) < double.Epsilon * 10)
                        {
                            ptx = xiL;
                            pty = yiL;
                            continue;
                        }
                        return vx < vy ? -1 : 1;
                    }
                    catch (Exception)
                    {
                        return x.Substring(ptx).CompareTo(y.Substring(pty));
                    }
                }
                else
                {
                    return c1;
                }
            }
        }


        /// <summary>
        /// check string configured number only or not
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true=number only(Not check Int64/Int32/Int16 range)</returns>
        public static bool IsIntegerString(string str)
        {
            for (var i = 0; i < str.Length; i++)
            {
                if (i == 0)
                {
                    if (str[i] == '-')
                    {
                        continue;
                    }
                }
                if (str[i] < '0' || str[i] > '9')
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Split string with separation character. each string will be trimmed. Remve empty string from result
        /// </summary>
        /// <param name="str">input</param>
        /// <returns>lines</returns>
        public static string[] SplitTrim(string str, string separation)
        {
            var ss = str.Split(new string[] { separation }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < ss.Length; i++)
            {
                ss[i] = ss[i].Trim();
            }
            return ss;
        }

        /// <summary>
        /// split string with space character considering to keep space characters between double quotations.
        /// </summary>
        /// <param name="str">input</param>
        /// <returns>lines except empty one</returns>
        public static string[] SplitSpaceConsideringDoubleQuatation(string str)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            var isDQ = false;   // DQ=Double Quotation
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == '\"')
                {
                    isDQ = !isDQ;
                    continue;
                }
                if (isDQ == false)
                {
                    if (char.IsWhiteSpace(c))
                    {
                        var ss = sb.ToString().Trim();
                        if (ss.Length > 0)
                        {
                            ret.Add(ss);
                            sb = new StringBuilder();
                        }
                    }
                }
                sb.Append(c);
            }
            var sss = sb.ToString().Trim();
            if (sss.Length > 0)
            {
                ret.Add(sss);
            }

            var rets = new string[ret.Count];
            for (var i = 0; i < ret.Count; i++)
            {
                rets[i] = ret[i];
            }
            return rets;
        }

        /// <summary>
        /// split string with separation character considering to keep them between double quotations.
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="separation">区切り文字</param>
        /// <param name="isTrim"></param>
        /// <returns></returns>
        public static string[] SplitConsideringDoubleQuatation(string str, char separation, bool isTrim)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            var isDQ = false;
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == '\"')
                {
                    isDQ = !isDQ;
                    continue;
                }
                if (isDQ == false)
                {
                    if (c == separation)
                    {
                        string ss = sb.ToString();
                        if (isTrim)
                        {
                            ss = ss.Trim();
                        }
                        ret.Add(ss);
                        sb = new StringBuilder();
                        continue;
                    }
                }
                sb.Append(c);
            }
            var sss = sb.ToString().Trim();
            if (sss.Length > 0)
            {
                ret.Add(sss);
            }

            var rets = new string[ret.Count];
            for (var i = 0; i < ret.Count; i++)
            {
                rets[i] = ret[i];
            }
            return rets;
        }

        /// <summary>
        /// split string with separation character considering to keep them between double quotations.
        /// </summary>
        /// <param name="str">文字列</param>
        /// <param name="separation">区切り文字</param>
        /// <param name="isTrim"></param>
        /// <returns></returns>
        /// <example>
        /// t1 = StrUtil.SplitConsideringDoubleQuatation2("aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, true, true);
        /// separations '='TOP PRIORITY ----- ':'HIGH ----- ' 'LOW  (left is high priority)
        /// ret
        ///     "aaa", ""       // no separator yet
        ///     "bbb", "="      // "=bbb"
        ///     "ccc", ":"      // " : ccc"  ':' takes precedence over ' '
        ///     "ddd", "="      // " = ddd"  '=' takes precedence over ' '
        ///     "eee", "="      // " =: eee" '=' takes precedence over both ':' and ' '
        /// </example>
        public static (string Block, string Separator)[] SplitConsideringDoubleQuatation(string str, char[] separations, bool isTrim, bool isIgnoreEmpty)
        {
            var ret = new List<(string Block, string Separator)>();
            var sb = new StringBuilder();
            var isDQ = false;
            var lastSeparator = "";
            var lastSepId = int.MaxValue;
            var sepstr = string.Join("", separations);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == '\"')
                {
                    isDQ = !isDQ;
                    continue;
                }
                if (isDQ == false)
                {
                    var sepid = sepstr.IndexOf(c);
                    if (sepid >= 0)
                    {
                        var ss = sb.ToString();
                        if (isTrim)
                        {
                            ss = ss.Trim();
                        }
                        if (isIgnoreEmpty == false || ss.Length > 0)
                        {
                            ret.Add((ss, lastSeparator));
                            lastSepId = int.MaxValue;
                        }
                        sb.Clear();
                        if (sepid < lastSepId)
                        {
                            lastSeparator = c.ToString();
                            lastSepId = sepid;
                        }
                        continue;
                    }
                }
                sb.Append(c);
            }
            var sss = sb.ToString().Trim();
            if (sss.Length > 0)
            {
                ret.Add((sss, lastSeparator));
            }
            return ret.ToArray();
        }

        /// <summary>
        /// split string with separation character considering to keep them between double quotations.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="separations"></param>
        /// <param name="isTrim"></param>
        /// <param name="isIgnoreEmpty"></param>
        /// <returns></returns>
        /// <example>
        /// t1 = StrUtil.SplitConsideringDoubleQuatation(" aaa=bbb : ccc = ddd =: eee  ", new[] { '=', ':', ' ' }, true, true);
        /// separations '='TOP PRIORITY ----- ':'HIGH ----- ' 'LOW  (left is high priority)
        /// ret
        ///     "aaa"
        ///     "="
        ///     "bbb"
        ///     ":"
        ///     "ccc"
        ///     "="
        ///     "ddd"
        ///     "="
        ///     ":"
        ///     "eee"
        /// </example>
        public static string[] SplitConsideringQuatationContainsSeparator(string str, char[] separations, bool isTrim, bool isIgnoreEmpty, bool isRemoveQuotation = true)
        {
            var ret = new List<string>();
            var sb = new StringBuilder();
            var isDQ = false;
            char preQ = char.MinValue;
            var sepstr = string.Join("", separations);
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (isDQ == false && (c == '\"' || c == '\''))
                {
                    preQ = c;
                    isDQ = true;
                    if (isRemoveQuotation) continue;
                }
                else
                if (isDQ && c == preQ)
                {
                    isDQ = false;
                    if (isRemoveQuotation) continue;
                }
                else
                if (isDQ == false)
                {
                    var sepid = sepstr.IndexOf(c);
                    if (sepid >= 0)
                    {
                        var ss = sb.ToString();
                        if (isTrim)
                        {
                            ss = ss.Trim();
                        }
                        if (sb.Length > 0 && (isIgnoreEmpty == false || ss.Length > 0))
                        {
                            ret.Add(ss);
                            sb.Clear();
                        }
                        var sep = sepstr[sepid].ToString();
                        if (isTrim)
                        {
                            sep = sep.Trim();
                        }
                        if (isIgnoreEmpty == false || sep.Length > 0)
                        {
                            ret.Add(sep);
                        }
                        continue;
                    }
                }
                sb.Append(c);
            }
            var sss = sb.ToString();
            if (isTrim)
            {
                sss = sss.Trim();
            }
            if (sb.Length > 0 && (isIgnoreEmpty == false || sss.Length > 0))
            {
                ret.Add(sss);
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Split string with separate character then sort the result collection
        /// </summary>
        /// <param name="str">input</param>
        /// <returns>lines</returns>
        public static IList<string> SplitTrimSort(string str, string separation)
        {
            var ss = str.Split(new string[] { separation }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < ss.Length; i++)
            {
                ss[i] = ss[i].Trim();
            }
            var ret = new List<string>(ss);
            ret.Sort(ComparerThinkNumber);
            return ret;
        }

        /// <summary>
        /// get left side string
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="count"># of charactors</param>
        /// <returns></returns>
        /// <remarks>
        /// Substring(0, count)
        /// </remarks>
        public static string Left(string str, int count)
        {
            if (str.Length <= count)
            {
                return str;
            }
            else
            {
                return str.Substring(0, count);
            }
        }

        /// <summary>
        /// get right side string
        /// </summary>
        /// <param name="str">input</param>
        /// <param name="count"># of characters</param>
        /// <returns></returns>
        public static string Right(string str, int count)
        {
            if (str.Length <= count)
            {
                return str;
            }
            else
            {
                return str.Substring(str.Length - count, count);
            }
        }

        /// <summary>
        /// get middle string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="start">start position(0=first character)</param>
        /// <param name="count"># of character</param>
        /// <returns></returns>
        public static string Mid(string str, int start, int count = int.MaxValue)
        {
            return str.Substring(start, Math.Min(count, str.Length - start));
        }

        /// <summary>
        /// get middle string of regular expression
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexPattern"></param>
        /// <param name="defaultStr"></param>
        /// <returns></returns>
        /// <example>
        /// MidOn("ABC123DE456FG", "[0-9]+") = "123"
        /// </example>
        public static string MidOn(string str, string regexPattern, string defaultStr)
        {
            var regex = new Regex(regexPattern);
            var res = regex.Match(str)?.Value ?? null;
            if (res == null)
            {
                return defaultStr;
            }
            else
            {
                return res;
            }
        }

        /// <summary>
        /// get the middle string next to the regexSkip
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexSkip"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <example>
        /// MidSkip("ABCDEFG", "CD") = "EFG"
        /// </example>
        public static string MidSkip(string str, string regexSkip, int count = int.MaxValue)
        {
            var regex = new Regex(regexSkip);
            var skip = regex.Match(str)?.Value ?? null;
            if (skip == null)
            {
                return str;
            }
            else
            {
                var st = str.IndexOf(skip) + skip.Length;
                return str.Substring(st, Math.Min(count, str.Length - st));
            }
        }

        /// <summary>
        /// get the middle string next to the regexSkip
        /// </summary>
        /// <param name="s"></param>
        /// <param name="regexSkip"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <example>
        /// MidSkip("ABCDEFG", new Regex("CD")) = "EFG"
        /// </example>
        public static string MidSkip(string s, Regex regexSkip, int count = int.MaxValue)
        {
            var skip = regexSkip.Match(s)?.Value ?? null;
            if (skip == null)
            {
                return s;
            }
            else
            {
                var st = s.IndexOf(skip) + skip.Length;
                return s.Substring(st, Math.Min(count, s.Length - st));
            }
        }

        /// <summary>
        /// get the middle string from the regexSkip
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexFind"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <example>
        /// MidFind("ABCDEFG", "CD") = "CDEFG"
        /// </example>
        public static string MidFind(string str, string regexFind, int count = int.MaxValue)
        {
            var regex = new Regex(regexFind);
            var find = regex.Match(str)?.Value ?? str;
            var st = str.IndexOf(find);
            return str.Substring(st, Math.Min(count, str.Length - st));
        }

        /// <summary>
        /// get the middle string from the regexSkip
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexFind"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <example>
        /// MidFind("ABCDEFG", new Regex("CD")) = "CDEFG"
        /// </example>
        public static string MidFind(string str, Regex regexFind, int count = int.MaxValue)
        {
            var find = regexFind.Match(str)?.Value ?? str;
            var st = str.IndexOf(find);
            return str.Substring(st, Math.Min(count, str.Length - st));
        }

        /// <summary>
        /// get left side string before regexFind
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexFind"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /// <example>
        /// LeftBefore("ABCDEFG", "CD") = "AB"
        /// LeftBefore("Tokyo123World", @"\d+") = "Tokyo"
        /// </example>
        public static string LeftBefore(string str, string regexFind, int count = int.MaxValue)
        {
            var regex = new Regex(regexFind);
            var find = regex.Match(str)?.Value;
            if (find == null || find == "")
            {
                return str;
            }
            var st = str.IndexOf(find);
            return str.Substring(0, st);
        }

        /// <summary>
        /// get left side string from regexFind(including the key string)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regexFind"></param>
        /// <returns></returns>
        /// <example>
        /// LeftOn("ABCDEFG", "CD") = ABCD
        /// </example>
        public static string LeftOn(string str, string regexFind)
        {
            var regex = new Regex(regexFind);
            var find = regex.Match(str)?.Value;
            if (find == null)
            {
                return str;
            }
            var st = str.IndexOf(find);
            return str.Substring(0, st + find.Length);
        }


        /// <summary>
        /// remove double quotations from string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveQuatation(string str)
        {
            if (str != null)
            {
                if (str.StartsWith("\"") && str.EndsWith("\""))
                {
                    str = str.Substring(1, str.Length - 2);
                }
            }
            return str;
        }
    }
}
