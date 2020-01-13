// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �G�N�Z���̗񖼌`���ŕ����Ɛ����̕ϊ����T�|�[�g����
    /// </summary>
    public class StringNumberExcel
    {
        /// <summary>
        /// Excel�J������
        /// </summary>
        public static readonly StringNumberExcel Col = new StringNumberExcel();

        /// <summary>
        /// ��O
        /// </summary>
        public class StrUtilNumberBadCharacterException : Exception
        {
            /// <summary>
            /// ������G���[
            /// </summary>
            /// <param name="bad"></param>
            public StrUtilNumberBadCharacterException(char bad) : base(string.Format("Bad character error '{0}'", bad))
            {
            }
        }


        /// <summary>
        /// 臒l�ۑ��i�������j
        /// </summary>
        private static readonly List<int> _threshold = new List<int>();

        /// <summary>
        /// 臒l��������
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
        /// �����񂩂�G�N�Z���w�b�_�`���̐�����Ԃ�
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
        /// ��������A�G�N�Z���w�b�_�`���̕�������쐬����
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
