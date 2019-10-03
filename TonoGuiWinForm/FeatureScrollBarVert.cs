using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �c�X�N���[���o�[
    /// Parameter�̗�
    /// Pane=Resource;Speed=0.8
    /// </summary>
    public class FeatureScrollBarVert : FeatureBase, IMouseListener, IPartsRemoveListener
#if DEBUG == false
, IAutoRemovable
#endif
    {
        /// <summary>
        /// �X�N���[���o�[���z�u�����y�[��
        /// </summary>
        private IRichPane _tarPane = null;

        /// <summary>
        /// �X�s�[�h
        /// </summary>
        private double _speed = 1.0;

        /// <summary>
        /// �o�[�p�[�c
        /// </summary>
        private PartsScrollbarV _bar = null;

        /// <summary>
        /// �g�p����y�[�����w�肷��
        /// �L�q��:  Pane=Resource
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            var coms = new List<string>(StrUtil.SplitTrim(param, ";"));
            foreach (var com in coms)
            {
                var ops = new List<string>(StrUtil.SplitTrim(com, "="));
                if (ops.Count == 2)
                {
                    if (ops[0].ToUpper() == "PANE")
                    {
                        _tarPane = Pane.GetPane(ops[1]);
                    }
                    if (ops[0].ToUpper() == "SPEED")
                    {
                        try
                        {
                            _speed = double.Parse(ops[1]);
                            if (_speed == 0.0 || Math.Abs(_speed) > 1000)
                            {
                                throw new Exception();
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Assert(false, GetType().Name + " : 'Speed' Parameter syntax error\r\nusage : Speed=<value>\r\nvalue> 1.0=Normal speed / 0.5=Half.(max 1000)");
                            throw e.InnerException;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �X�N���[���o�[�̃p�[�c�C���X�^���X�𐶐�����
        /// </summary>
        /// <returns></returns>
        protected virtual PartsScrollbarV createScrollBarPart()
        {
            return new PartsScrollbarV();
        }

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _tarPane = Pane.GetPane("Resource");	//�f�t�H���g�BParseParameter�Ŏw�肵�Ă��������B
            _bar = createScrollBarPart();
            Parts.Add(_tarPane, _bar, Const.Layer.StaticLayers.ScrollBarV);
        }

        #region IMouseListener �����o

        public void OnMouseMove(MouseState e)
        {
            _bar.MouseNow = e;

            // �o�[�̈ʒu�����ĕ`��
            var br = _tarPane.GetPaneRect();
            br.LT.X = br.RB.X - PartsScrollbarV.Width;
            Pane.Invalidate(br);

            if (_bar.IsOn)
            {
                var k = _speed * _bar.Acc;

                var ly = (_downPos.Y - e.Pos.Y) * k;
                ly = Math.Pow(Math.Abs(ly), 1.25) * (ly > 0 ? 1 : -1);	// ��������ړ�������A�����x�I�Ɉړ�����
                Pane.Scroll = ScreenPos.FromInt(Pane.Scroll.X, _downScroll.Y + (int)ly);
                _bar.SetHighlight(false);
                Pane.Invalidate(null);
            }
            else
            {
                _bar.SetHighlight(true);
            }
        }

        private ScreenPos _downPos = ScreenPos.FromInt(0, 0);
        private ScreenPos _downScroll = ScreenPos.FromInt(0, 0);
        private ScreenRect _skipzone = null;

        public void OnMouseDown(MouseState e)
        {
            if (_bar.MouseNow == null)
            {
                return;
            }

            // ��x�����A�N���b�N�s���т�o�^
            if (_skipzone == null)
            {
                var pr = _tarPane.GetPaneRect();
                _skipzone = ScreenRect.FromLTWH(pr.RB.X - PartsScrollbarV.Width, pr.LT.Y, PartsScrollbarV.Width, pr.Height);
                Parts.AddSkipZone(_skipzone);
            }

            // �N���b�N����
            _bar.SetOn(true, true);
            _bar.SetHighlight(false);
            _downPos = (ScreenPos)e.Pos.Clone();
            _downScroll = (ScreenPos)Pane.Scroll.Clone();

            _bar.Acc = 1;
            if (e.Attr.IsShift && !e.Attr.IsCtrl)
            {
                _bar.Acc = 4;
            }

            if (!e.Attr.IsShift && e.Attr.IsCtrl)
            {
                _bar.Acc = 0.5f;
            }

            if (e.Attr.IsShift && e.Attr.IsCtrl)
            {
                _bar.Acc = 16;
            }

            Pane.Invalidate(null);
        }

        public void OnMouseUp(MouseState e)
        {
            if (_bar.MouseNow == null)
            {
                return;
            }

            _bar.SetOn(false, false);
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion

        #region IPartsRemoveListener �����o

        public void OnPartsRemoved(System.Collections.ICollection removedPartsSet)
        {
            if (_bar != null)
            {
                if (Parts.Contains(_bar.ID) == false)
                {
                    Parts.Add(_tarPane, _bar);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// �X�N���[���o�[�̃p�[�c
    /// </summary>
    public class PartsScrollbarV : PartsBase, IPartsVisible
    {
        /// <summary>
        /// �p�[�c�̍���
        /// </summary>
        public static readonly int Width = 16;
        protected static readonly Brush _bgOn = new SolidBrush(Color.FromArgb(192, 128, 128, 128));
        protected static readonly Brush _bgHi = new SolidBrush(Color.FromArgb(96, 128, 128, 128));
        protected static readonly Brush _bgOff = new SolidBrush(Color.FromArgb(64, 255, 255, 255));
        protected static readonly Brush _bgCurOn = new SolidBrush(Color.FromArgb(192, 192, 192, 255));
        protected static readonly Brush _bgCurOff = new SolidBrush(Color.FromArgb(64, 255, 255, 255));
        protected static readonly Pen _offL = new Pen(Color.FromArgb(192, 255, 255, 255));
        protected static readonly Pen _offD = new Pen(Color.FromArgb(32, 0, 0, 0));

        /// <summary>
        /// �����̕\���p 1=�ʏ푬�x
        /// </summary>
        private float _acc = 1;

        /// <summary>
        /// �����x�\���̒l
        /// </summary>
        public float Acc
        {
            get => _acc;
            set => _acc = value;
        }

        private bool _isOn = false;
        /// <summary>
        /// ON/OFF�\���̐ݒ�
        /// </summary>
        public new bool IsOn => _isOn;

        private bool _isHighlight = false;
        public bool IsHighlight => _isHighlight;
        public void SetHighlight(bool sw)
        {
            if (sw)
            {
                _isHighlight = _scm.IsIn(_now.Pos);
            }
            else
            {
                _isHighlight = sw;
            }
        }

        private MouseState _now = null;
        public MouseState MouseNow
        {
            get => _now;
            set => _now = value;
        }

        public void SetOn(bool sw, bool evaluatePos)
        {
            if (sw)
            {
                if (evaluatePos)
                {
                    _isOn = _scm.IsIn(_now.Pos);
                    return;
                }
            }
            _isOn = sw;
        }

        /// <summary>
        /// �J�[�\���̈ʒu
        /// </summary>
        private ScreenRect _scm = ScreenRect.FromLTWH(0, 0, 0, 0);

        /// <summary>
        /// �`��
        /// </summary>
        /// <param name="rp"></param>
        /// <returns></returns>
        public override bool Draw(IRichPane rp)
        {
            if (_isVisible == false || _now == null)
            {
                return true;
            }
            var sr = rp.GetPaneRect();
            sr.LT.X = sr.RB.X - Width;

            _scm = (ScreenRect)sr.Clone();
            _scm.LT.Y = _now.Pos.Y - 24;
            _scm.RB.Y = _scm.LT.Y + 48;
            sr.LT.X += 1;

            Brush bgcur;
            if (_isOn)
            {
                rp.Graphics.FillRectangle(_bgOn, sr);
                bgcur = _bgCurOn;
            }
            else if (_isHighlight)
            {
                rp.Graphics.FillRectangle(_bgHi, sr);
                bgcur = _bgHi;
            }
            else
            {
                rp.Graphics.FillRectangle(_bgOff, sr);
                bgcur = _bgCurOff;
            }

            // �J�[�\��
            rp.Graphics.FillRectangle(bgcur, _scm);
            rp.Graphics.DrawLine(_offL, _scm.LT.X, _scm.LT.Y, _scm.RB.X, _scm.LT.Y);
            rp.Graphics.DrawLine(_offD, _scm.RB.X, _scm.LT.Y, _scm.RB.X, _scm.RB.Y);
            rp.Graphics.DrawLine(_offD, _scm.RB.X, _scm.RB.Y, _scm.LT.X, _scm.RB.Y);
            rp.Graphics.DrawLine(_offL, _scm.LT.X, _scm.RB.Y, _scm.LT.X, _scm.LT.Y);
            // ���
            var cx = (_scm.LT.X + _scm.RB.X) / 2;
            rp.Graphics.DrawLine(_offL, cx, _scm.LT.Y, cx, _scm.LT.Y - 24 * (float)Math.Sqrt(_acc));
            rp.Graphics.DrawLine(_offL, cx, _scm.LT.Y - 24 * (float)Math.Sqrt(_acc), cx - 4, _scm.LT.Y - (24 * (float)Math.Sqrt(_acc) - 6));
            rp.Graphics.DrawLine(_offL, cx, _scm.LT.Y - 24 * (float)Math.Sqrt(_acc), cx + 4, _scm.LT.Y - (24 * (float)Math.Sqrt(_acc) - 6));
            // ���
            rp.Graphics.DrawLine(_offL, cx, _scm.RB.Y, cx, _scm.RB.Y + 24 * (float)Math.Sqrt(_acc));
            rp.Graphics.DrawLine(_offL, cx, _scm.RB.Y + 24 * (float)Math.Sqrt(_acc), cx - 4, _scm.RB.Y + (24 * (float)Math.Sqrt(_acc) - 6));
            rp.Graphics.DrawLine(_offL, cx, _scm.RB.Y + 24 * (float)Math.Sqrt(_acc), cx + 4, _scm.RB.Y + (24 * (float)Math.Sqrt(_acc) - 6));
            return true;
        }

        #region IPartsVisible �����o

        private bool _isVisible = true;
        public bool Visible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        #endregion
    }
}