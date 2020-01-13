// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    public class PartsTooltip : PartsBase
    {
        private Brush _bg = new SolidBrush(Color.FromArgb(172, 0, 0, 0));
        private readonly Brush _bgs = new SolidBrush(Color.FromArgb(64, 0, 0, 0));
        private Brush _tc = new SolidBrush(Color.FromArgb(255, 255, 255, 255));
        private Brush _square = new SolidBrush(Color.FromArgb(0, 0, 0, 0));
        private Image _ltImage = null;

        private Control _pane = null;
        private ScreenPos _specPos = null;

        public override void Dispose()
        {
            // dpTooltip�́A�����o�ϐ��Ƃ��Ē����ԕێ�����̂ŁAGDI�J�������Ă͂����Ȃ�
            //if (_bg != null){	_bg.Dispose(); _bg = null;	}
            //if (_bgs != null){	_bgs.Dispose(); _bgs = null; }
            //if ( _tc != null ){	_tc.Dispose(); _tc = null; }
            //if ( _square != null ){ _square.Dispose(); _square = null; }
            //if ( _ltImage != null){ _ltImage.Dispose(); _ltImage = null; }
            base.Dispose();
        }

        /// <summary>
        /// ����ɕ\������A�C�R��
        /// </summary>
        public Image LtImage
        {
            set
            {
                if (value == null)
                {
                    if (_ltImage != null)
                    {
                        //_ltImage.Dispose();
                        _ltImage = null;
                    }
                }
                else
                {
                    _ltImage = value;
                }
            }
        }

        /// <summary>
        /// �l�p���A�C�R���̐F
        /// </summary>
        public Color SquareColor
        {
            set
            {
                if (value == null || value == Color.Transparent)
                {
                    if (_square != null)
                    {
                        _square.Dispose();
                        _square = null;
                    }
                }
                else
                {
                    _square = new SolidBrush(value);
                }
            }
        }

        /// <summary>
        /// �����\���ʒu
        /// </summary>
        public ScreenPos Position
        {
            set => _specPos = value;
        }

        /// <summary>
        /// �c�[���`�b�v�̐F
        /// </summary>
        public Color BackColor
        {
            get => ((SolidBrush)_bg).Color;
            set
            {
                if (_bg != null)
                {
                    _bg.Dispose();
                }
                _bg = new SolidBrush(value);
            }
        }

        /// <summary>
        /// �c�[���`�b�v���̕����̐F
        /// </summary>
        public Color TextColor
        {
            get => ((SolidBrush)_tc).Color;
            set
            {
                if (_tc != null)
                {
                    _tc.Dispose();
                }
                _tc = new SolidBrush(value);
            }
        }

        /// <summary>
        /// �`��
        /// </summary>
        public override bool Draw(IRichPane rp)
        {
            var renderHint = rp.Graphics.TextRenderingHint;
            try
            {
                const float LP = 1.2f;  // �s��

                rp.Graphics.TextRenderingHint = TextRenderingHint.SystemDefault;

                // �����̍s����T�C�Y�Ȃǂ����߂�
                Font font, fontTitle;
                var isNeedFontDispose = false;
                if (Mes.Current != null)
                {
                    font = Mes.Current.GetFont("tooltip.Body");
                    fontTitle = Mes.Current.GetFont("tooltip.Title");
                }
                else
                {
                    fontTitle = font = new Font("Arial Black", 10.0f, FontStyle.Bold);
                    font = new Font("Courier New", 9.0f, FontStyle.Regular);
                    isNeedFontDispose = true;
                }
                var text = Text.Replace("\r\n", "\r");
                text = text.Replace("\n", "");
                var ls = text.Split(new char[] { '\r' });
                if (ls.Length < 1)
                {
                    return true;
                }
                var fh = font.GetHeight(rp.Graphics) * LP;
                fh = (float)Math.Floor(fh + 0.9);
                var fh0 = fontTitle.GetHeight(rp.Graphics) * LP + 2;

                // ���P�[�V��������
                if (Rect.LT.X == int.MaxValue)
                {
                    if (_pane == null)
                    {
                        // PointToClient�֐����g�p���������AcFeatureRich�łȂ��Ƌ@�\���Ȃ����߁A��x�����擾���Ă���
                        for (_pane = rp.Control; _pane is TGuiView == false; _pane = _pane.Parent)
                        {
                            ;
                        }
                    }
                    ScreenPos mp;
                    if (_specPos == null)
                    {
                        mp = _pane.PointToClient(MouseState.NowPosition);
                    }
                    else
                    {
                        mp = (ScreenPos)_specPos.Clone();
                    }
                    var sss = Text.Replace(ls[0], "").Trim();
                    var szB = rp.Graphics.MeasureString(sss, font);
                    var szT = rp.Graphics.MeasureString(ls[0], fontTitle);
                    var sz = new SizeF(szT.Width > szB.Width ? szT.Width : szB.Width, fh0 + fh * (ls.Length - 1));
                    Rect = CodeRect.FromLTWH(mp.X, mp.Y + 24, (int)(sz.Width + 2f), (int)(sz.Height + 2f));
                    Rect.Inflate(XyBase.FromInt(3, 3));

                    XyBase of = (Rect.RB + XyBase.FromInt(3, 3)) - rp.GetPaneRect().RB;
                    if (of.Y > 0)
                    {
                        Rect -= XyBase.FromInt(0, Rect.Height + 26);
                    }
                    if (of.X > 0)
                    {
                        Rect -= XyBase.FromInt(of.X, 0);
                    }
                    if (Rect.LT.Y < rp.GetPaneRect().LT.Y)
                    {
                        Rect.Transfer(XyBase.FromInt(0, rp.GetPaneRect().LT.Y - Rect.LT.Y));
                    }
                    if (Rect.LT.X < 0)
                    {
                        Rect.Transfer(XyBase.FromInt(rp.GetPaneRect().LT.X - Rect.LT.X, 0));
                    }

                }
                if (string.IsNullOrEmpty(Text))
                {
                    return true;
                }

                // �o�b�N
                rp.Graphics.FillRectangle(_bg, Rect.LT.X, Rect.LT.Y, Rect.Width, Rect.Height);

                // �V���h�E
                rp.Graphics.FillRectangle(_bgs, Rect.LT.X + Rect.Width, Rect.LT.Y + 2, 2, Rect.Height - 2);
                rp.Graphics.FillRectangle(_bgs, Rect.LT.X + 2, Rect.LT.Y + Rect.Height, Rect.Width, 2);

                // �n�C���C�g
                //			Point[] pts = new Point[]{new Point(Rect.LT.X, Rect.LT.Y + Rect.Height), new Point(Rect.LT.X, Rect.LT.Y), new Point(Rect.LT.X + Rect.Width, Rect.LT.Y)};
                //			rp.Graphics.DrawLines(Pens.White, pts);

                // �l�p���A�C�R��
                if (_square != null)
                {
                    rp.Graphics.FillRectangle(_square, Rect.LT.X + 3, Rect.LT.Y + 4, 7, 7);
                    rp.Graphics.DrawRectangle(Pens.Black, Rect.LT.X + 3, Rect.LT.Y + 4, 7, 7);
                }
                // �e�L�X�g
                var p = new PointF(Rect.LT.X + 3, Rect.LT.Y + 3);
                for (var i = 0; i < ls.Length; i++)
                {
                    rp.Graphics.DrawString(ls[i], (i == 0 ? fontTitle : font), _tc, p);
                    p.Y += (i == 0 ? fh0 : fh);
                }
                // �A�C�R��
                if (_ltImage != null)
                {
                    rp.Graphics.DrawImage(_ltImage, Rect.LT.X - 4, Rect.LT.Y - 4, _ltImage.Width, _ltImage.Height);
                }

                // GDI�J��
                if (isNeedFontDispose)
                {
                    font.Dispose();
                    fontTitle.Dispose();
                }
            }
            finally
            {
                rp.Graphics.TextRenderingHint = renderHint;
            }
            return true;
        }

        /// <summary>
        /// �c�[���`�b�v�̏ꏊ���Čv�Z����悤�ɗv������
        /// </summary>
        public void RequestPosition()
        {
            Rect.LT.X = int.MaxValue;
        }

    }
}
