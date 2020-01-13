// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// エクセルの列名形式で文字と数字の変換をサポートする
    /// </summary>
    public class StringNumberExcel
    {
        /// <summary>
        /// Excelカラム名
        /// </summary>
        public static readonly StringNumberExcel Col = new StringNumberExcel();

        /// <summary>
        /// 例外
        /// </summary>
        public class StrUtilNumberBadCharacterException : Exception
        {
            /// <summary>
            /// 文字列エラー
            /// </summary>
            /// <param name="bad"></param>
            public StrUtilNumberBadCharacterException(char bad) : base(string.Format("Bad character error '{0}'", bad))
            {
            }
        }


        /// <summary>
        /// 閾値保存（高速化）
        /// </summary>
        private static readonly List<int> _threshold = new List<int>();

        /// <summary>
        /// 閾値生成処理
        /// </summary>
        static StringNumberExcel()
        {
            var th = 0;
            _threshold.Add(th);
            for (var i = 0; i < 7; i++)
            {
                th += (int)Math.Pow(26, i);
                _threshold.Add(th);
            }
        }

        /// <summary>
        /// 文字列からエクセルヘッダ形式の数字を返す
        /// A=1, Z=26, AA=27
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int this[string val]
        {
            get
            {
                double ret = 0;
                for (var i = 0; i < val.Length; i++)
                {
                    var c = val[val.Length - i - 1];
                    c = char.ToUpper(c);
                    double d1 = (c - 'A');
                    if (d1 < 0 || d1 > 26)
                    {
                        throw new StrUtilNumberBadCharacterException(c);
                    }
                    var k = Math.Pow(26, i);
                    var v = (d1 + 1) * k;

                    ret += v;
                }
                return (int)ret;
            }
        }

        /// <summary>
        /// 数字から、エクセルヘッダ形式の文字列を作成する
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string this[int val]
        {
            get
            {
                var sb = new StringBuilder();
                int digit;
                for (digit = 0; digit < _threshold.Count; digit++)
                {
                    if (val < _threshold[digit + 1])
                    {
                        break;
                    }
                }
                if (digit < 1)
                {
                    return "";
                }

                double v0 = val;
                for (var i = 0; i < digit; i++)
                {
                    var k1 = Math.Pow(26, i);
                    var v1 = Math.Floor((v0 - _threshold[i]) / k1);
                    var v2 = (v1 - 1) % 26 + 1;
                    var c = (char)('@' + v2);
                    sb.Insert(0, c);
                }

                return sb.ToString();
            }
        }
    }
}
