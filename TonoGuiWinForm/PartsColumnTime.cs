// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// dpColumnTime �̊T�v�̐����ł��B
    /// </summary>
    public class PartsColumnTime : PartsBase
    {
        /** <summary>�ڐ���\���̉𑜓x 1.0=�ꎞ�Ԉ�{�\�� / 0.5=��{�\��	</summary> */
        private readonly double _reshval = 0.5;
        /// <summary>�ꎞ�ԕ���\������̂ɕK�v�ȃh�b�g���i�p�[�c���W�j</summary>
        private readonly LayoutPos _dph = LayoutPos.FromInt(200, 0);
        /** <summary>���̐F												</summary> */
        private Pen _lc1 = new Pen(Color.LightGray);
        /** <summary>���̐F												</summary> */
        private SolidBrush _bc = new SolidBrush(Color.DimGray);
        /** <summary>�����̐F											</summary> */
        protected SolidBrush _tc = new SolidBrush(Color.White);
        /// <summary>�t�H���g</summary>
        protected Font _font = new Font("Arial", 8);

        /** <summary>[�Y�[��]�{���ƕ\�����鎞�Ԃ̗��x�̑Ή��\				</summary> */
        private readonly double[] _zmRes = { 0.3, 0.15, 0.07, 0.04, 0.02 };
        /** <summary>�Y�[���{����[�\�����鎞�Ԃ̗��x]�̑Ή��\				</summary> */
        private readonly double[] _tmRes = { 1, 2, 4, 8, 24 };
        /// <summary>
        /// �T�\���̌Œ�i�J�X�^�}�C�Y�j
        /// </summary>
        protected string[] _strDays = null;


        /// <summary>
        /// GDI�J��
        /// </summary>
        public override void Dispose()
        {
            if (_font != null)
            {
                _lc1.Dispose();
                _bc.Dispose();
                _tc.Dispose();
                _font.Dispose(); _font = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// ���̕�������J�X�^�}�C�Y����
        /// </summary>
        public ICollection StringDays
        {
            set
            {
                if (value == null)
                {
                    _strDays = null;
                }
                else
                {
                    if (value.Count == 0)
                    {
                        _strDays = new string[] { "" };
                    }
                    else
                    {
                        _strDays = new string[value.Count];
                        var i = 0;
                        for (var e = value.GetEnumerator(); e.MoveNext();)
                        {
                            _strDays[i++] = e.Current.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ꎞ�ԕ���\������̂ɕK�v�ȃh�b�g���i�p�[�c���W�jDots Per Hour
        /// </summary>
        public LayoutPos DPH => _dph;

        /// <summary>
        /// �w�i�F
        /// </summary>
        public Color BackColor
        {
            get => _bc.Color;
            set => _bc = new SolidBrush(value);
        }

        /// <summary>
        /// ���̐F
        /// </summary>
        public Color LineColor
        {
            get => _lc1.Color;
            set => _lc1 = new Pen(value);
        }

        /// <summary>
        /// �w�i�F
        /// </summary>
        public Color TextColor
        {
            get => _tc.Color;
            set => _tc = new SolidBrush(value);
        }


        /// <summary>
        /// �`��
        /// </summary>
        /// <param name="sp">�`�搧��n���h��</param>
        public override bool Draw(IRichPane rp)
        {
            if (Rect.Width <= 1)
            {
                var pr = rp.Convert(rp.GetPaneRect());
                Rect = CodeRect.FromLTRB(pr.LT.X, pr.LT.Y, pr.RB.X, pr.RB.Y);
            }

            var zx = (double)rp.Zoom.X / 1000;

            var org = rp.Scroll;

            RectangleF b = rp.GetPaneRect();
            rp.Graphics.FillRectangle(_bc, b);

            var mv0 = -org.X / (zx * _dph.X);    // �\���J�n�ʒu�̍œK�� mv[��]
            var mv = mv0;
            mv += (_reshval - mv % _reshval) - _reshval;    // 0.5���Ԗ߂����ʒu����`��͂��߂�
            var mvx = (b.Right - org.X) / (zx * _dph.X); // �\���I���ʒu�̍œK�� mvx[��]

            var preDs = "";
            var isFirst = true;
            var isLeftWeek = false;

            // �\��
            for (var t2 = mv/*[��]*/; t2 <= mvx; t2 += _reshval)
            {
                var t = t2 + (1.0 / 7200);

                // �����̕\��
                string s;
                var x = org.X + t * _dph.X * zx;
                if (t >= 0)
                {
                    s = (Math.Floor(t) % 24.0).ToString("0") + ":" + (Math.Floor(Math.Abs(t) * 60) % 60).ToString("00");
                }
                else
                {
                    if (Math.Floor(t) % 24.0 == 0)
                    {
                        s = (Math.Floor(t) % 24.0).ToString("0");
                    }
                    else
                    {
                        s = (Math.Floor(t) % 24.0 + 24.0).ToString("0");
                    }
                    s = s + ":" + (Math.Floor(Math.Abs(t) * 60 + 1) % 60).ToString("00");
                }

                var isDrawTime = true;
                for (var i = 0; i < _zmRes.Length; i++)
                {
                    if (zx < _zmRes[i] && t / _tmRes[i] - Math.Floor(t / _tmRes[i]) > 0.1)
                    {
                        isDrawTime = false;
                        break;
                    }
                }
                var pen = _lc1;

                if (isDrawTime)
                {
                    rp.Graphics.DrawString(s, _font, _tc, (float)x - 14 + b.Left, b.Bottom - 22);   //�����̕`��
                    rp.Graphics.DrawLine(pen, (float)x + b.Left, b.Bottom - 9, (float)x + b.Left, b.Bottom - 1);    //���̕`��i�e�L�X�g������ꏊ�j
                }
                else
                {
                    if (zx > 0.02)
                    {
                        if (t - Math.Floor(t) > 0.4)
                        {
                            rp.Graphics.DrawLine(pen, (float)(x + b.Left), b.Bottom - 5, (float)(x + b.Left), b.Bottom - 1);    //���̕`��i30���P�ʁj�e�L�X�g���Ȃ��ĎO�\���P��
                        }
                        else
                        {
                            rp.Graphics.DrawLine(pen, (float)(x + b.Left), b.Bottom - 5, (float)(x + b.Left), b.Bottom - 1);    //���̕`��i1���Ԉȏ�P�ʁj�e�L�X�g���Ȃ��ꏊ
                        }
                    }
                }
                // �j���̕\��
                if (((int)Math.Floor(t * 60) % 1440) == 0)
                {
                    var day = (((int)t2) / 24 + 7000);
                    var wx = x < 0 ? b.Left : x + b.Left;
                    var ds = drawDayString(rp, wx, b.Bottom - 36, day);
                    preDs = ds;
                    if (isFirst)
                    {
                        isFirst = false;
                        var dss = rp.Graphics.MeasureString(ds, _font);
                        if (wx > b.Left + dss.Width + 12)
                        {
                            isLeftWeek = true;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(preDs) || isLeftWeek)
            {
                var day = (int)((mv0 + 24 * 7000) / 24);
                //string ds = _strDays != null ? _strDays[day % _strDays.Length] : uTime.GetDayString(day % 7);
                //rp.Graphics.DrawString(ds, _font, _tc, b.Left, b.Bottom - 36);
                var ds = drawDayString(rp, b.Left, b.Bottom - 36, day);
                preDs = ds;
            }
            return true;
        }

        /// <summary>
        /// �j���̕������`�悷��
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="ds"></param>
        protected virtual string drawDayString(IRichPane rp, double x, double y, int day)
        {
            var ds = _strDays != null ? _strDays[day % _strDays.Length] : DateTimeEx.GetDayString(day % 7);
            rp.Graphics.DrawString(ds, _font, _tc, (float)x, (float)y); //�����̕`��
            return ds;
        }
    }
}
