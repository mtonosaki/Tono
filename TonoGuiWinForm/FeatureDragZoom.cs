// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �h���b�O�ł̃Y�[�����T�|�[�g
    /// </summary>
    /// <remarks>
    /// �������J�ԍ��F���J2007-264807
    /// </remarks>
    public class FeatureDragZoom : Tono.GuiWinForm.FeatureBase, IMouseListener, IKeyListener
    {
        #region		����(�V���A���C�Y����)
        /** <summary>�Y�[�����J�n����g���K</summary> */
        protected MouseState.Buttons _trigger;
        protected bool _isSameXY;
        protected bool _isCenterLock;
        #endregion
        #region		����(�V���A���C�Y���Ȃ�)
        /// <summary>�}�E�X���N���b�N�������_�ł̃}�E�X���W</summary>
        protected ScreenPos _posDown = null;
        /// <summary>�}�E�X���N���b�N�������_�ł̃X�N���[����</summary>
        protected ScreenPos _scrollDown;
        /// <summary>�}�E�X���N���b�N�������̃Y�[���l</summary>
        protected XyBase _zoomDown;
        /// <summary>�}�E�X���N���b�N�����Ƃ��̃y�[��</summary>
        protected IRichPane _paneDown;
        /// <summary>�C�x���g�ɂ���ĕύX����J�[�\���̃��X�g</summary>
        protected Hashtable _CursorList = new Hashtable();
        /// <summary>���O�̃}�E�X�J�[�\���̏�</summary>
        protected MouseState.Buttons _prev = new MouseState.Buttons();
        /// <summary>�J�[�\����</summary>
        protected NamedId _tokenListenID = NamedId.FromName("CursorSetJob");
        /// <summary>
        /// �Y�[�����E�Ȃ̂Ŏ������_�ړ����Ȃ�
        /// </summary>
        protected DataSharingManager.Boolean _noscrollmove = null;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureDragZoom()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons
            {
                IsButton = true,
                IsButtonMiddle = false,
                IsCtrl = true,
                IsShift = false
            };
            _isSameXY = false;
            _isCenterLock = false;
        }

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            _noscrollmove = (DataSharingManager.Boolean)Share.Get("NoScrollMoveFlag", typeof(DataSharingManager.Boolean));
        }

        /// <summary>
        /// �p�����[�^�[�̏�����
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            string[] coms = param.Split(new char[] { ';' });
            foreach (string com in coms)
            {
                string[] od = com.Split(new char[] { '=' });
                if (od.Length < 2)
                {
                    continue;
                }

                if (od[0].ToLower() == "centerlock")
                {
                    _isCenterLock = Const.IsTrue(od[1]);
                }
                if (od[0].ToLower() == "samexy")
                {
                    _isSameXY = Const.IsTrue(od[1]);
                }
                if (od[0].ToLower() == "trigger")
                {
                    _trigger = new MouseState.Buttons();

                    string[] ts = od[1].Split(new char[] { '+' });
                    foreach (string t in ts)
                    {
                        if (t.ToLower() == "middle")
                        {
                            _trigger.IsButtonMiddle = true;
                        }
                        if (t.ToLower() == "button" || t.ToLower() == "left")
                        {
                            _trigger.IsButton = true;
                        }
                        if (t.ToLower() == "ctrl")
                        {
                            _trigger.IsCtrl = true;
                        }
                        if (t.ToLower() == "shift")
                        {
                            _trigger.IsShift = true;
                        }
                    }
                }
                else if (od[0].ToLower() == "cursor")
                {
                    if (od.Length == 3)
                    {
                        string[] ts = od[2].Split(new char[] { '.' });
                        string[] trs = od[1].Split(new char[] { '+' });
                        MouseState.Buttons trg = new Tono.GuiWinForm.MouseState.Buttons();
                        foreach (string t in trs)
                        {
                            if (t.ToLower() == "middle")
                            {
                                trg.IsButtonMiddle = true;
                            }

                            if (t.ToLower() == "button" || t.ToLower() == "left")
                            {
                                trg.IsButton = true;
                            }

                            if (t.ToLower() == "ctrl")
                            {
                                trg.IsCtrl = true;
                            }

                            if (t.ToLower() == "shift")
                            {
                                trg.IsShift = true;
                            }
                        }
                        if (od[2].ToLower().IndexOf("cursors") != -1)
                        {
                            Type t = typeof(System.Windows.Forms.Cursors);
                            System.Reflection.PropertyInfo pi = t.GetProperty(ts[ts.Length - 1].ToString());
                            if (pi != null)
                            {
                                _EventCursor = (Cursor)pi.GetValue(null, Array.Empty<object>());
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �g���K�i���s���ʃL�[�j��ύX����
        /// </summary>
        /// <param name="value">�V�����g���K�[</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        #region IMouseListener �����o
        /// <summary>
        /// �{�^��Down�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                _posDown = (ScreenPos)e.Pos.Clone();
                _paneDown = Pane;
                _zoomDown = (XyBase)Pane.Zoom.Clone();
                _scrollDown = (ScreenPos)Pane.Scroll.Clone();
            }
        }

        /// <summary>
        /// �}�E�XMove�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_posDown == null || _zoomDown == null || _scrollDown == null || e.Pane == null)
            {
                return;
            }

            if (e.Attr.Equals(_trigger))
            {
                // ��ʂ̊g��/�k��
                ScreenPos movePos = e.Pos - _posDown;          // �J�[�\���̈ړ��ʂ̌v�Z
                ScreenPos pdBak = (ScreenPos)_posDown.Clone();
                if (_isCenterLock)
                {
                    _posDown.X = e.Pane.GetPaneRect().LT.X + (e.Pane.GetPaneRect().RB.X - e.Pane.GetPaneRect().LT.X) / 2;
                    _posDown.Y = e.Pane.GetPaneRect().LT.Y + (e.Pane.GetPaneRect().RB.Y - e.Pane.GetPaneRect().LT.Y) / 2;
                }

                XyBase zoomNow = _zoomDown + movePos * 3;      // �Y�[���l�̎Z�o  ���x�ύX(���𑜓x�ɔ���) 2016.11.15 Tono 2��3

                // �Y�[���l���K��͈͓��Ɏ��߂�
                if (zoomNow.X > 4000)
                {
                    zoomNow.X = 4000;
                }

                if (zoomNow.Y > 4000)
                {
                    zoomNow.Y = 4000;
                }

                if (zoomNow.X < 5)
                {
                    zoomNow.X = 5;
                }

                if (zoomNow.Y < 5)
                {
                    zoomNow.Y = 5;
                }

                if (_isSameXY)
                {
                    zoomNow.Y = zoomNow.X;
                }

                Pane.Zoom = (XyBase)zoomNow.Clone();           // �Y�[���l�̔��f

                // �N���b�N�����ʒu����ɂ��ăY�[������悤�ɉ�ʂ��X�N���[������B
                double ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X�����̃Y�[�����̎Z�o
                double ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y�����̃Y�[�����̎Z�o

                ScreenPos beforeDownPos = _posDown - _scrollDown - e.Pane.GetPaneRect().LT;    // 
                ScreenPos afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

                if (_noscrollmove.value == false)
                {
                    Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
                }
                else
                {
                    _noscrollmove.value = false;
                }
                Pane.Invalidate(null);
                _posDown = pdBak;
            }
            else
            {
                OnMouseUp(e);
            }
        }

        /// <summary>
        /// �{�^��Up�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseUp(MouseState e)
        {
            _posDown = null;
            _zoomDown = null;
            _scrollDown = null;
        }

        /// <summary>
        /// �}�E�X�z�C�[���C�x���g
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseWheel(MouseState e)
        {
            // ���g�p
        }

        #endregion
        #region IKeyListener �����o

        public virtual void OnKeyDown(KeyState e)
        {
            if (MouseState.NowButtons.IsButton == false && e.IsControl == true)
            {
                FeatureBase.Cursor = _EventCursor;
                Finalizers.Add(_tokenListenID, new FinalizeManager.Finalize(onCursorSeFinalizert));
            }
        }

        public virtual void OnKeyUp(KeyState e)
        {
            if (e.IsControl == true)
            {
                if (FeatureBase.Cursor == _EventCursor)
                {
                    FeatureBase.Cursor = Cursors.Default;
                    Finalizers.Add(_tokenListenID, new FinalizeManager.Finalize(onCursorSeFinalizert));
                }
            }
        }

        #endregion
    }
}
