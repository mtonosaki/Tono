using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �����𐔎��Ƃ��Ď�舵��
    /// </summary>
    public class StringNumber
    {
        #region �悭�g���C���X�^���X
        /// <summary>
        /// 0-1�ŕ\������2�i��
        /// </summary>
        public static readonly StringNumber D2 = new StringNumber('0', '1');
        /// <summary>
        /// 0-7�ŕ\������8�i��
        /// </summary>
        public static readonly StringNumber D8 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7');
        /// <summary>
        /// 0-9�ŕ\������10�i��
        /// </summary>
        public static readonly StringNumber D10 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        /// <summary>
        /// 0-9, A-Z�ŕ\������36�i��
        /// </summary>
        public static readonly StringNumber A36 = new StringNumber('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z');
        #endregion

        /// <summary>
        /// ��O
        /// </summary>
        public class StrNumberBadCharacterExceltion : Exception
        {
            public StrNumberBadCharacterExceltion(char bad)
                : base(string.Format("Bad character error '{0}'", bad))
            {
            }
        }

        /// <summary>
        /// �e���̒l
        /// </summary>
        private readonly Dictionary<char, int> _cv = new Dictionary<char, int>();
        /// <summary>
        /// �e���̒l
        /// </summary>
        private readonly Dictionary<int, char> _vc = new Dictionary<int, char>();

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="digits">�ŏ����[���ɑ�������L�����N�^</param>
        public StringNumber(params char[] digits)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                _cv[digits[i]] = i;
                _vc[i] = digits[i];
            }
        }

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="digits">�ŏ����[���ɑ�������L�����N�^</param>
        /// <param name="isSeq">true=�������[�h / false=�ʐ����[�h</param>
        public StringNumber(bool isSeq, params char[] digits)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                _cv[digits[i]] = i;
                _vc[i] = digits[i];
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        public int Figures => _cv.Count;

        /// <summary>
        /// �l�擾
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string this[int val]
        {
            get
            {
                // ����
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
        /// �l������쐬
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
