using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 文字を数字として取り扱う
    /// </summary>
    public class StringNumber
    {
        #region よく使うインスタンス
        /// <summary>
        /// 0-1で表現する2進数
        /// </summary>
        public static readonly StringNumber D2 = new StringNumber('0', '1');
        /// <summary>
        /// 0-7で表現する8進数
        /// </summary>
        public static readonly StringNumber D8 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7');
        /// <summary>
        /// 0-9で表現する10進数
        /// </summary>
        public static readonly StringNumber D10 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        /// <summary>
        /// 0-9, A-Zで表現する36進数
        /// </summary>
        public static readonly StringNumber A36 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z');
        #endregion

        /// <summary>
        /// 例外
        /// </summary>
        public class StrNumberBadCharacterExceltion : Exception
        {
            public StrNumberBadCharacterExceltion(char bad)
                : base(string.Format("Bad character error '{0}'", bad))
            {
            }
        }

        /// <summary>
        /// 各桁の値
        /// </summary>
        private readonly Dictionary<char, int> _cv = new Dictionary<char, int>();
        /// <summary>
        /// 各桁の値
        /// </summary>
        private readonly Dictionary<int, char> _vc = new Dictionary<int, char>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="digits">最初がゼロに相当するキャラクタ</param>
        public StringNumber(params char[] digits)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                _cv[digits[i]] = i;
                _vc[i] = digits[i];
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="digits">最初がゼロに相当するキャラクタ</param>
        /// <param name="isSeq">true=序数モード / false=量数モード</param>
        public StringNumber(bool isSeq, params char[] digits)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                _cv[digits[i]] = i;
                _vc[i] = digits[i];
            }
        }

        /// <summary>
        /// 桁数
        /// </summary>
        public int Figures => _cv.Count;

        /// <summary>
        /// 値取得
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string this[int val]
        {
            get
            {
                // 符号
                var sb = new StringBuilder();
                if (val < 0)
                {
                    sb.Append('-');
                    val = -val;
                }
                if (_vc.ContainsKey(val))
                {
                    sb.Append(_vc[val]);
                    return sb.ToString();
                }
                var digi = (int)Math.Ceiling(Math.Log(val + 1, Figures));
                for (var i = digi; i > 0; i--)
                {
                    var db = Math.Pow(Figures, i);
                    var ds = Math.Pow(Figures, i - 1);
                    var v1 = (val % db) / ds;
                    sb.Append(_vc[(int)v1]);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 値文字列作成
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public int this[string val]
        {
            get
            {
                double sig = 1;
                double ret = 0;
                for (var i = 0; i < val.Length; i++)
                {
                    if (i == 0)
                    {
                        if (val[i] == '-')
                        {
                            sig = -1;
                            continue;
                        }
                    }
                    var k = Math.Pow(Figures, val.Length - i - 1);
                    if (_cv.TryGetValue(val[i], out var v1))
                    {
                        ret = ret + k * v1;
                    }
                    else
                    {
                        throw new StrNumberBadCharacterExceltion(val[i]);
                    }
                }
                return (int)(sig * ret);
            }
        }
    }
}
