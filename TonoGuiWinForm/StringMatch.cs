﻿// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 文字列をワイルドカード付で評価
    /// </summary>
    /// <remarks>
    ///	構文
    /// aaa					aaaが含まれる場合にマッチ
    /// -"aaa"				aaaが含まれない場合にマッチ
    /// aaa bbb				aaaとbbb両方含む場合にマッチ
    /// aaa -"bbb"			aaaが含まれるものの内、bbbが含まれない場合にマッチ
    /// aaa {^ABC} ccc		aaaとcccが含まれ、さらに正規表現で ^ABCでマッチするものをマッチさせる {}は正規表現という意味
    /// a???b				５文字で、aで始まりbで終わるもの。? は、任意の一文字を意味する（文字無しは含まない）
    /// a*b					aで始まりbで終わる文字列。abも含む
    /// ^ABC				ABCで始まるもの。
    /// ABC$				ABCで終わるもの。
    /// 
    /// 正規表現のメタ文字
    /// . ^ $ [ ] * + ? | ( )
    /// </remarks>
    public class StringMatch
    {
        private const string _metastr = "\".^$[]*+?|()\\";
        private readonly List<List<Regex>> _resCollects = new List<List<Regex>>();
        private readonly List<List<Regex>> _resRemoves = new List<List<Regex>>();
        private readonly bool _isAllMatch = false;
        private readonly int NRemove = 0;

        /// <summary>
        /// 比較文字を指定して構築
        /// </summary>
        /// <param name="pattern"></param>
        public StringMatch(string pattern)
        {
            var reopt = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            var isRegularExpressioning = false;    // 正規表現構文中
            var isDoubleQuatationing = false;      // ダブルクォーテーション中
            var isEscaping = false;    // エスケープシーケンス中

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
                            expss.Add(exps = new List<string>());   // 次のOR条件を準備
                        }
                        str = new StringBuilder();
                        continue;
                    }
                    if (char.IsWhiteSpace(c) && !isDoubleQuatationing)
                    {
                        var cc = str.ToString().Trim();
                        if (string.IsNullOrEmpty(cc) == false)
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
            if (string.IsNullOrEmpty(str.ToString().Trim()) == false)
            {
                exps.Add(str.ToString());
            }

            // comsで処理
            foreach (var strs in expss)
            {
                var resRemove = new List<Regex>();
                _resRemoves.Add(resRemove); // OR条件で複数登録できる
                var resCollect = new List<Regex>();
                _resCollects.Add(resCollect);   // OR条件用に複数登録できる

                var nStartMark = 0; // ＾記号の個数
                var nEndMark = 0;   // ＄記号の個数
                foreach (var com in strs)
                {
                    var com2 = StringMatch.ConvertSimpleAsterisk(com);
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
                    LOG.WriteLine(LLV.WAR, "～で始まるのフィルタ指定が、２回以上重なりましたので、うまく検索できないと思います");
                }
                if (nEndMark > 1)
                {
                    LOG.WriteLine(LLV.WAR, "～で終わるのフィルタ指定が、２回以上重なりましたので、うまく検索できないと思います");
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
                            if (string.IsNullOrEmpty(restr))
                            {
                                restr = " ";    // Trimで消えたスペースを復元
                            }
                            isRemove = true;
                        }
                        else
                        {
                            restr = com;
                        }

                        // ダブルクォーテーションを取る
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

                        // シンプルアスタリスク表記をサポート
                        restr = StringMatch.ConvertSimpleAsterisk(restr);

                        // アスタリスクやクエスッションを正規表現のものに変換する
                        restr = restr.Replace(".", "\\.");
                        restr = restr.Replace("*", ".*");
                        restr = restr.Replace("?", ".?");

                        // 入力のクセから開放される
                        restr = Regex.Replace(restr, "&", "\\s*&\\s*"); // & 記号の前後に、スペース有ってもなくても良くする

                        // 連続したスペースを消す
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
        /// OR条件で評価する
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
        /// リストに入っている正規表現を AND条件で評価する
        /// </summary>
        /// <param name="str">評価する文字列</param>
        /// <param name="collects">収集用</param>
        /// <param name="removes">除外用</param>
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
        /// 全マッチする文字列かどうかを調べる
        /// </summary>
        public bool IsAllMatch => _isAllMatch;

        /// <summary>
        /// アスタリスクを使った「始まる」「終わる」のサポート
        /// AAA*  ： AAAから始まる
        /// *BBB　： BBBで終わる
        /// *CC*　： CCを含む
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
        /// 行頭指定など、制限をはずしてゆるいフィルタ文字列を作成する
        /// </summary>
        /// <param name="fs">ファジー前の文字列</param>
        /// <returns>ファジー後の文字列</returns>
        public static string MakeFilterFazzy(string fs)
        {
            // 正規表現指定ブラケットは不要
            var isRegexBracket = false;
            fs = fs.Trim();
            if (fs.StartsWith("{") && fs.EndsWith("}"))
            {
                fs = fs.Substring(1, fs.Length - 2);
                isRegexBracket = true;
            }

            // エスケープシーケンスを取る
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

            // ダブルクォーテーションと正規文字ブラケット｛｝の内側にマイナス記号があれば、マイナスを含む文字列はダブルクォーテーションで括る処理の準備
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

            // ダブルクォーテーションを取る
            fs = fs.Replace("\"", "");
            fs = fs.Replace("\a", "\"");    // 保護したダブルクォーテーションを復帰

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
        /// 指定文字列の正規表現で検索されて文字列を１つ返す
        /// </summary>
        /// <param name="sin">文字列</param>
        /// <param name="regstr">正規表現</param>
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
        /// メタ文字が含まれる場合、メタではない形式に変換する
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
