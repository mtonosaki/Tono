// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureCursorProviderOnParts �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureCursorProviderOnParts : FeatureBase, IKeyListener, IMouseListener
    {
        private bool _isTop = true;
        private bool _isRight = true;
        private bool _isLeft = true;
        private bool _isBottom = true;
        private bool _isInside = true;
        private bool _isMove = true;
        private IRichPane _filterPane = null;
        private int _filterLayer = int.MinValue;

        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�ʏ�J�[�\�����L������</summary>
        private Cursor _normalCursor;

        /// <summary>���݂̃L�[�̏�Ԃ��L��</summary>
        private MouseState _ms = new MouseState();

        /// <summary>�v������J�[�\���i��Őݒ肷����́j</summary>
        private Cursor _requestedCursor = null;

        /// <summary>�J�[�\���\����ԁi���L�ϐ��j</summary>
        private DataSharingManager.Int _state;

        /// <summary>�p�[�c�ʒu�Ǘ��I�u�W�F�N�g</summary>
        protected PartsPositionManager _pos;

        /// <summary>�������i�x�������̃^�C�}�[�n���h���j</summary>
        private GuiTimer.Handle _th = null;

        #endregion

        #region ���
        /// <summary>
        /// �p�����[�^���
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);

            var coms = param.Split(new char[] { ';' });
            foreach (var com in coms)
            {
                var od = com.Split(new char[] { '=' });
                if (od.Length < 2)
                {
                    continue;
                }

                if (od[0].ToLower() == "pane")
                {
                    _filterPane = Pane.GetPane(od[1]);
                }
                if (od[0].ToLower() == "layer")
                {
                    _filterLayer = int.Parse(od[1]);
                }
                if (od[0].ToLower() == "avoid")
                {
                    var ts = od[1].Split(new char[] { ',' });
                    foreach (var tt in ts)
                    {
                        var t = tt.Trim().ToLower();
                        switch (t)
                        {
                            case "top":
                                _isTop = false;
                                break;
                            case "left":
                                _isLeft = false;
                                break;
                            case "right":
                                _isRight = false;
                                break;
                            case "bottom":
                                _isBottom = false;
                                break;
                            case "inside":
                                _isInside = false;
                                break;
                            case "move":
                                _isMove = false;
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// ����������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            if (Pane is IControlUI)
            {
                _normalCursor = ((IControlUI)Pane).Cursor;
            }
            else
            {
                throw new NotSupportedException("FeatureCursorProvider�́AIControlUI���������Ă���Pane�ɂ̂ݎg�p�ł��܂�");
            }

            // �X�e�[�^�X����
            _state = (DataSharingManager.Int)Share.Get("CursorProviderStatus", typeof(DataSharingManager.Int));
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // �ړ����̃p�[�c�ꗗ
        }

        /// <summary>
        /// �J�[�\�����Z�b�g����i�t�@�C�i���C�Y�j
        /// </summary>
        private delegate void SetCursorCallback();

        private void onCursorSet()
        {
            if (_requestedCursor != null)
            {
                if (Pane.Control.InvokeRequired)
                {
                    var d = new SetCursorCallback(onCursorSet);
                    Pane.Control.Invoke(d);
                }
                else
                {
                    Pane.Control.Cursor = _requestedCursor;
                }
            }
        }

        /// <summary>�}�E�X�J�[�\���ύX�x������(������)</summary>
        /// <param name="param">�}�E�X���W</param>
        private void proc(object param)
        {
            var e = (MouseState)param;
            _ms = e;
            IRichPane tarPane;
            PartsBase parts;
            if (_filterPane == null)
            {
                parts = Parts.GetPartsAt(_ms.Pos, true, out tarPane);
            }
            else
            {
                parts = Parts.GetPartsAt(_ms.Pos, _filterPane, _filterLayer, true);
                tarPane = _filterPane;
            }
            _requestedCursor = _isMove ? _normalCursor : null;
            if (parts != null)
            {
                // ���E���̃`�F�b�N�Ώۂ��i��
                var check = PartsBase.PointType.Inside | PartsBase.PointType.Outside;
                if (_isLeft)
                {
                    check |= PartsBase.PointType.OnLeft;
                }

                if (_isRight)
                {
                    check |= PartsBase.PointType.OnRight;
                }

                if (_isTop)
                {
                    check |= PartsBase.PointType.OnTop;
                }

                if (_isBottom)
                {
                    check |= PartsBase.PointType.OnBottom;
                }

                // ���E����Ȃǂ��`�F�b�N
                switch (parts.IsOn(_ms.Pos, tarPane, check))
                {
                    case PartsBase.PointType.Inside:
                        _requestedCursor = _isInside ? Cursors.Hand : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnLeft:
                        _requestedCursor = _isLeft ? Cursors.VSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnRight:
                        _requestedCursor = _isRight ? Cursors.VSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnTop:
                        _requestedCursor = _isTop ? Cursors.HSplit : _requestedCursor;
                        break;
                    case PartsBase.PointType.OnBottom:
                        _requestedCursor = _isBottom ? Cursors.HSplit : _requestedCursor;
                        break;
                }
            }
            onCursorSet();
        }

        #region IKeyListener �����o

        public void OnKeyDown(KeyState e)
        {
            _ms.Attr.SetKeyFrags(e);
        }

        public void OnKeyUp(KeyState e)
        {
            _ms.Attr.SetKeyFrags(e);
        }

        #endregion

        #region IMouseListener �����o

        public void OnMouseMove(MouseState e)
        {
            Timer.Stop(_th);
            if (_state.value == 0)  // FeatureCursorProviderKey �ŃJ�[�\���ύX���Ă��Ȃ���Ԃ̎��̂݁A�������s������
            {
                if (_pos.Count == 0)
                {
                    _th = Timer.AddTrigger(e, 50, new GuiTimer.Proc1(proc));
                }
            }
        }

        public void OnMouseDown(MouseState e)
        {
            _ms = e;
        }

        public void OnMouseUp(MouseState e)
        {
            if (e != null)
            {
                _ms.Attr.ResetKeyFlags(e.Attr);
                _ms.Attr.SetKeyFrags(e.Attr);
            }
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
    }
}
