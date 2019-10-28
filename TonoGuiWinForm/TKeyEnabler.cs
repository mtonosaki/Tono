// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573, 1587

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fiKeyEnabler �̊T�v�̐����ł��B
    /// </summary>
    public class TKeyEnabler : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Timer _timerAutoFocus;
        private System.ComponentModel.IContainer components;
        private static bool _enable = true;
        private Control _parentForm = null;

        /// <summary>
        /// �ꎞ�I�ɋ@�\�̗L���E�����ύX
        /// </summary>
        public static bool Enable
        {
            set => _enable = value;
        }

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public TKeyEnabler()
        {
            // ���̌Ăяo���́AWindows.Forms �t�H�[�� �f�U�C�i�ŕK�v�ł��B
            InitializeComponent();

            if (DesignMode == false)
            {
                SetStyle(ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                BackColor = Color.Transparent;
                Invalidate();
            }
        }

        /// <summary>
        /// �N�����̏�����
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Width > 1)
            {
                Location = new Point(0, 0);
                Size = new Size(1, 1);
            }

            // �S������Form���擾����
            var isTabControlFocusCatched = false;

            for (_parentForm = Parent; _parentForm != null && _parentForm is Form == false; _parentForm = _parentForm.Parent)
            {
                if (isTabControlFocusCatched == false)
                {
                    if (_parentForm is TabControl)  // Tab�R���g���[���́A�����KeyDown��D���ď�ʂ�UserControl�ɓn���Ȃ��̂ŁA�����Ŏ擾���Ă���
                    {
                        _parentForm.KeyDown += new KeyEventHandler(onKeyDown);
                        _parentForm.KeyUp += new KeyEventHandler(onKeyUp);
                        isTabControlFocusCatched = true;
                    }
                }
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
        /// <summary>
        /// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
        /// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            _timerAutoFocus = new System.Windows.Forms.Timer(components)
            {
                // 
                // timerAutoFocus
                // 
                Enabled = true
            };
            _timerAutoFocus.Tick += new System.EventHandler(timerAutoFocus_Tick);
            // 
            // fiKeyEnabler
            // 
            Name = "fiKeyEnabler";
            Size = new System.Drawing.Size(120, 80);

        }
        #endregion

        private KeyState _prevKeyState = new KeyState();

        /// <summary>
        /// �L�[�C�x���g��cFeatureRich�ɓ]������
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_enable == false)
            {
                return;
            }
            // TAB�L�[�̃t�B���^�����O
            if (e.KeyCode == Keys.Tab && e.Control == false && e.Shift == false)
            {
                return;
            }

            // ���̑��̃L�[�̓]��
            if (Parent is TGuiView)
            {
                var ke = KeyState.FromKeyEventArgs(e);
                if (ke.Equals(_prevKeyState) == false)
                {
                    ((TGuiView)Parent).OnKeyDown(ke);
                    _prevKeyState = ke;
                    System.Diagnostics.Debug.WriteLine(ke);
                }
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// �w�肳�ꂽ�L�[���ʏ�L�[���ǂ����𒲍�����
        /// </summary>
        /// <param name="keyData">��������L�[�R�[�h</param>
        /// <returns>�������� TRUE:�ʏ�L�[ / FALSE:����L�[</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:           // ���L�[
                case Keys.Down:         // ���L�[
                case Keys.Left:         // ���L�[
                case Keys.Right:        // ���L�[
                    break;
                default:
                    return base.IsInputKey(keyData);    // ���̑��̃L�[
            }
            return true;
        }


        /// <summary>
        /// �L�[�C�x���g��cFeatureRich�ɓ]������
        /// </summary>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_enable == false)
            {
                return;
            }
            // ���̑��̃L�[�̓]��
            if (Parent is TGuiView)
            {
                _prevKeyState = null;
                var ke = KeyState.FromKeyEventArgs(e);
                ((TGuiView)Parent).OnKeyUp(ke);
            }
            base.OnKeyUp(e);
        }

        /// <summary>
        /// �l�דI�ɃL�[�A�b�v�C�x���g�𔭍s����
        /// </summary>
        /// <param name="e"></param>
        public void KickKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        /// <summary>
        /// �y�C���g�����̃t�B���^�����O
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                RectangleF r = (ScreenRect.FromControl(this) - XyBase.FromInt(Left, Top));
                using (var fo = new Font("Arial", 12, FontStyle.Bold))
                {
                    e.Graphics.DrawString("K", fo, Brushes.WhiteSmoke, r, sf);
                }
            }
        }

        /// <summary>
        /// �w�i�y�C���g�����̃t�B���^�����O
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (DesignMode)
            {
                base.OnPaintBackground(pevent);
            }
        }
        #region ���g�p�i�l�q���� by Tono 2009.6.22�`)

        /// <summary>
        /// �}�E�X�ړ����̃t�H�[�J�X�R���g���[��
        /// </summary>
        //private void mouseMoved()
        //{
        //    if( _enable == false )
        //    {
        //        return;
        //    }

        //    Point mp = Parent.PointToClient(uMouseState.NowPosition);
        //    if( mp.X < 0 || mp.Y < 0 || mp.X > Parent.Width || mp.Y > Parent.Height )
        //    {
        //        return;
        //    }

        //    if (_parentForm != null)
        //    {
        //        if (_parentForm.ContainsFocus == false)
        //        {
        //            return;
        //        }
        //    }

        //    Control child = Parent.GetChildAtPoint(mp);
        //    if( child == null )
        //    {
        //        if (this.ContainsFocus == false)
        //        {
        //            focus(this);
        //        }
        //    } 
        //    else 
        //    {
        //        if (child.ContainsFocus == false)
        //        {
        //            focus(child);
        //        }
        //    }
        //}

        //private void focus(Control c)
        //{
        //    c.Select();
        //    c.Focus();
        //}

        /// <summary>
        /// �����I�Ƀt�H�[�J�X��ON�ɂ��鏈��
        /// </summary>
        private void timerAutoFocus_Tick(object sender, System.EventArgs e)
        {
            #region �l�q��
            //if( _enable == false )
            //{
            //    return;
            //}
            //if( DesignMode == false )
            //{
            //    // �x������������
            //    if( _isFirstInvalidate  == false )
            //    {
            //        _isFirstInvalidate = true;
            //        Parent.Invalidate();
            //    }

            //    // �}�E�X�ړ����o
            //    if( Math.Abs(uMouseState.NowPosition.X - _prevMousePos.X) > 4 || Math.Abs(uMouseState.NowPosition.Y - _prevMousePos.Y) > 4 )
            //    {
            //        mouseMoved();
            //        _prevMousePos = uMouseState.NowPosition;
            //    }
            //}
            #endregion
        }
        #endregion

        /// <summary>
        /// �T�C�Y�ύX���̏���
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (DesignMode)
            {
                Invalidate();
            }
            base.OnSizeChanged(e);
        }
    }
}
