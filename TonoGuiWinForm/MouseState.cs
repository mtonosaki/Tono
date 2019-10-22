// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �}�E�X�C�x���g�̑�����\������N���X
    /// </summary>
    public class MouseState
    {
        private static MouseState _contextSave = new MouseState();
        internal static void saveContext(MouseEventArgs e, IRichPane pane)
        {
            _contextSave = FromMouseEventArgs(e, pane);
            _contextSave.Attr.IsShift = (Form.ModifierKeys & Keys.Shift) != 0;
            _contextSave.Attr.IsCtrl = (Form.ModifierKeys & Keys.Control) != 0;
        }
        /// <summary>
        /// �R���e�L�X�g���j���[��\�����钼�O�̏��
        /// </summary>
        public static MouseState StateAtContext => _contextSave;
        #region Buttons �N���X
        public class Buttons : ICloneable
        {
            /// <summary>�}�E�X�{�^���̉������</summary>
            public bool IsButton;

            /// <summary>�}�E�X���S�{�^���̉������</summary>
            public bool IsButtonMiddle;

            /// <summary>�_�u���N���b�N���</summary>
            public bool IsDoubleClick;

            /// <summary>CTRL�L�[�̉������</summary>
            public bool IsCtrl;

            /// <summary>SHIFT�L�[�̉������</summary>
            public bool IsShift;

            /// <summary>
            /// �f�t�H���g�R���X�g���N�^
            /// </summary>
            public Buttons()
            {
                IsButton = false;
                IsButtonMiddle = false;
                IsDoubleClick = false;
                IsCtrl = false;
                IsShift = false;
            }

            /// <summary>
            /// �������R���X�g���N�^
            /// </summary>
            /// <param name="button">�}�E�X�{�^���̏�ԁu</param>
            /// <param name="middle">�}�E�X�����{�^���̏��</param>
            /// <param name="ctrl">CTRL�L�[�̏��</param>
            /// <param name="shift">SHIFT�L�[�̏��</param>
            public Buttons(bool button, bool middle, bool ctrl, bool shift, bool doubleclick)
            {
                IsShift = shift;
                IsButton = button;
                IsButtonMiddle = middle;
                IsCtrl = ctrl;
                IsDoubleClick = doubleclick;
            }

            public override string ToString()
            {
                var s = "";
                if (IsButton)
                {
                    s += "<L>";
                }

                if (IsButtonMiddle)
                {
                    s += "<M>";
                }

                if (IsDoubleClick)
                {
                    s += "<D>";
                }

                if (IsCtrl)
                {
                    s += "[CTRL]";
                }

                if (IsShift)
                {
                    s += "[SHIFT]";
                }

                if (string.IsNullOrEmpty(s))
                {
                    s = "(no buttons)";
                }
                return s;
            }


            /// <summary>
            /// MouseEventArgs�ł͍\�z���؂�Ȃ������t���O�𖄂ߍ��킹��
            /// </summary>
            /// <param name="value">���ߍ��킹�Ɏg�����i��ɃL�[�̏�ԁj</param>
            public void SetKeyFrags(Buttons value)
            {
                IsCtrl = value.IsCtrl;
                IsShift = value.IsShift;
            }

            /// <summary>
            /// MouseEventArgs�ł͍\�z���؂�Ȃ������t���O�𖄂ߍ��킹��
            /// </summary>
            /// <param name="value">���ߍ��킹�Ɏg�����i��ɃL�[�̏�ԁj</param>
            public void SetKeyFrags(KeyState value)
            {
                IsCtrl = value.IsControl;
                IsShift = value.IsShift;
            }

            /// <summary>
            /// �w�肵���r�b�g��true�̍��ڂ� false�ɂ���iUp�C�x���g�őΏۂ�false�ɂ��鎞�Ɏg�p����j
            /// </summary>
            /// <param name="value">false�ɂ������r�b�g���Z�b�g���Ă�����Buttons</param>
            public void ResetKeyFlags(Buttons value)
            {
                if (value.IsButton)
                {
                    IsButton = false;
                }

                if (value.IsButtonMiddle)
                {
                    IsButtonMiddle = false;
                }

                if (value.IsDoubleClick)
                {
                    IsDoubleClick = false;
                }

                if (value.IsCtrl)
                {
                    IsCtrl = false;
                }

                if (value.IsShift)
                {
                    IsShift = false;
                }
            }

            /// <summary>
            /// �w�肵���{�^�����ׂĂ���v���Ă��邩�ǂ����𒲂ׂ�
            /// </summary>
            /// <param name="value">���ׂ�{�^��</param>
            /// <returns>true = ��v / false = �s��v</returns>
            public override bool Equals(object value)
            {
                if (value is MouseState.Buttons)
                {
                    return GetHashCode() == value.GetHashCode();
                }
                return false;
            }

            /// <summary>
            /// �C���X�^���X�̏�Ԃ�\���n�b�V���R�[�h
            /// </summary>
            /// <returns>�n�b�V���R�[�h</returns>
            public override int GetHashCode()
            {
                var ret = 0;
                if (IsButton)
                {
                    ret |= 0x01;
                }

                if (IsButtonMiddle)
                {
                    ret |= 0x02;
                }

                if (IsCtrl)
                {
                    ret |= 0x04;
                }

                if (IsShift)
                {
                    ret |= 0x08;
                }

                if (IsDoubleClick)
                {
                    ret |= 0x10;
                }

                return ret;
            }
            #region ICloneable �����o

            public object Clone()
            {
                return new Buttons(IsButton, IsButtonMiddle, IsCtrl, IsShift, IsDoubleClick);
            }

            #endregion
        }
        #endregion

        #region �����i�V���A���C�Y���K�v�j

        /// <summary>�}�E�X�X�N���[�����W</summary>
        public ScreenPos Pos = new ScreenPos();
        /// <summary>�z�C�[���̈ړ���</summary>
        public XyBase Delta = new XyBase();
        /// <summary>�{�^����</summary>
        public Buttons Attr = new Buttons();
        /// <summary>�N���b�N���ꂽ�y�[��</summary>
        protected IRichPane _paneAtPos = null;

        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        private static ScreenPos _nowPos = null;
        private static Buttons _nowButtons = null;

        /// <summary>
        /// �C���X�^���X�̕���
        /// </summary>
        /// <returns></returns>
        public MouseState Clone()
        {
            var ret = new MouseState
            {
                Pos = (ScreenPos)Pos.Clone(),
                Delta = Delta,
                Attr = (Buttons)Attr.Clone(),
                _paneAtPos = _paneAtPos
            };
            return ret;
        }

        /// <summary>
        /// ���݂̈ʒu�ƃ{�^���𔽉f����C���X�^���X�i
        /// </summary>
        public static MouseState Now
        {
            get
            {
                var ret = new MouseState
                {
                    Attr = NowButtons,
                    Pos = NowPosition,
                    Pane = null
                };
                return ret;
            }
        }

        /// <summary>
        /// ���݂̏�ԁi�{�^���j
        /// </summary>
        public static Buttons NowButtons
        {
            get
            {
                if (_nowButtons == null)
                {
                    for (; ; Thread.Sleep(0))   // .NET���X���b�h�Z�[�t�ŗ�O���o�����A�����I�ɍĎ��s����
                    {
                        try
                        {
                            _nowButtons = new Buttons
                            {
                                IsButton = (Control.MouseButtons & MouseButtons.Left) != 0 ? true : false,
                                IsButtonMiddle = (Control.MouseButtons & MouseButtons.Middle) != 0 ? true : false,
                                IsCtrl = (Form.ModifierKeys & Keys.Control) != 0 ? true : false,
                                IsShift = (Form.ModifierKeys & Keys.Shift) != 0 ? true : false,
                                IsDoubleClick = false
                            };
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                var ret = _nowButtons;  // Tono:NowButtons���X�V����悤�ɏC���i���R�[�_�n�Ő��������삷�邩�͖����؁j
                _nowButtons = null;
                return ret;
            }
        }

        /// <summary>���݂̏��</summary>
        public static ScreenPos NowPosition
        {
            get
            {
                for (; ; Thread.Sleep(0))   // .NET���X���b�h�Z�[�t�ŗ�O���o�����A�����I�ɍĎ��s����
                {
                    try
                    {
                        if (_nowPos == null)
                        {
                            return ScreenPos.FromInt(Control.MousePosition.X, Control.MousePosition.Y);
                        }
                        else
                        {
                            return (ScreenPos)_nowPos.Clone();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            set => _nowPos = value;
        }

        /// <summary>
        /// �C���X�^���X�̕�����
        /// </summary>
        /// <returns>������</returns>
        public override string ToString()
        {
            return Pos.ToString() + " " + "D" + Delta.ToString() + " " + Attr.ToString() + (_paneAtPos != null ? " at " + _paneAtPos.IdText : "");
        }


        /// <summary>
        /// �N���b�N�����Ƃ���̃y�[��
        /// </summary>
        public IRichPane Pane
        {
            get => _paneAtPos;
            set => _paneAtPos = value;
        }

        /// <summary>
        /// �}�E�X���W 0 / �{�^���t���[��Ԃ̃I�u�W�F�N�g / Pane = null
        /// </summary>
        public static MouseState FreeZero
        {
            get
            {
                var ret = new MouseState();
                ret.Attr.IsButton = false;
                ret.Attr.IsButtonMiddle = false;
                ret.Attr.IsCtrl = false;
                ret.Attr.IsDoubleClick = false;
                ret.Attr.IsShift = false;
                ret.Delta = XyBase.FromInt(0, 0);
                ret.Pos = ScreenPos.FromInt(0, 0);
                ret._paneAtPos = null;
                return ret;
            }
        }

        /// <summary>
        /// �}�E�X�C�x���g��������C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="e">�}�E�X�C�x���g����</param>
        /// <returns>�V�����C���X�^���X</returns>
        public static MouseState FromMouseEventArgs(System.Windows.Forms.MouseEventArgs e, IRichPane posPane)
        {
            var ret = new MouseState();
            ret.Pos.X = e.X;
            ret.Pos.Y = e.Y;
            ret.Delta.X = 0;
            ret.Delta.Y = e.Delta;
            ret._paneAtPos = posPane;
            ret.Attr.IsButton = (e.Button == System.Windows.Forms.MouseButtons.Left);
            ret.Attr.IsButtonMiddle = (e.Button == System.Windows.Forms.MouseButtons.Middle);
            if (e.Clicks > 1)
            {
                ret.Attr.IsDoubleClick = true;
            }

            ret.Attr.IsCtrl = false;
            ret.Attr.IsShift = false;
            return ret;
        }
    }
}
