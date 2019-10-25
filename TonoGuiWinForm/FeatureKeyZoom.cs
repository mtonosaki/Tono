// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureKeyZoom �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureKeyZoom : FeatureBase, IKeyListener, IMultiTokenListener
    {
        private IRichPane _tarPane = null;
        private bool _isTokenOnly = false;

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            _tarPane = Pane.GetPane("Resource");
        }

        /// <summary>
        /// �N���p�����[�^
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (string.IsNullOrEmpty(param) == false)
            {
                foreach (var com in param.Split(';'))
                {
                    if (com.Trim().ToUpper() == "TOKENSTARTONLY")
                    {
                        _isTokenOnly = true;
                        continue;
                    }
                    var ss = com.Split(new char[] { ',' });
                    System.Diagnostics.Debug.Assert(ss.Length == 2, "FeatureKeyZoom�̃p�����[�^�́A\"X�̒l,Y�̒l\"�Ə����Ă�������");
                    _x = int.Parse(ss[0]);
                    _y = int.Parse(ss[1]);
                }
            }
        }

        /// <summary>
        /// �Y�[����X
        /// </summary>
        private int _x = 100;

        /// <summary>
        /// �Y�[����Y
        /// </summary>
        private int _y = 100;

        /// <summary>
        /// X�Y�[�����邩�H
        /// </summary>
        private bool _isX => _x != 0;

        /// <summary>
        /// Y�Y�[�����邩�H
        /// </summary>
        private bool _isY => _y != 0;

        /// <summary>
        /// �w��ʂ̃Y�[�����s��
        /// </summary>
        /// <param name="value"></param>
        private void zoom(int x, int y)
        {
            // �I���y�[���̕`��̈�̒��S���Y�[���̃Z���^�[�̐ݒ�
            var _posDown = new ScreenPos
            {
                X = _tarPane.GetPaneRect().LT.X + _tarPane.GetPaneRect().Width / 2 - _tarPane.GetPaneRect().LT.X,
                Y = _tarPane.GetPaneRect().LT.Y + _tarPane.GetPaneRect().Height / 2 - _tarPane.GetPaneRect().LT.Y
            };
            var _scrollDown = (ScreenPos)Pane.Scroll.Clone();
            var _zoomDown = (XyBase)Pane.Zoom.Clone();

            // ��ʂ̊g��/�k��
            XyBase zoomNow;
            if (_isX && !_isY)
            {
                zoomNow = Pane.Zoom + XyBase.FromInt(x, 0);            // �Y�[���l�̎Z�o
            }
            else if (!_isX && _isY)
            {
                zoomNow = Pane.Zoom + XyBase.FromInt(0, y);            // �Y�[���l�̎Z�o
            }
            else
            {
                zoomNow = Pane.Zoom + x;                            // �Y�[���l�̎Z�o
            }
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

            Pane.Zoom = (XyBase)zoomNow.Clone();           // �Y�[���l�̔��f

            // �N���b�N�����ʒu����ɂ��ăY�[������悤�ɉ�ʂ��X�N���[������B
            var ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X�����̃Y�[�����̎Z�o
            var ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y�����̃Y�[�����̎Z�o

            var beforeDownPos = _posDown - _scrollDown - _tarPane.GetPaneRect().LT;  // 
            var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

            Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
            Pane.Invalidate(null);
        }

        /// <summary>
        /// �L�[�A�b�v�C�x���g���̏���
        /// </summary>
        public void OnKeyUp(KeyState e)
        {
            if (_isTokenOnly == false)
            {
                // �Y�[���A�b�v�L�[
                if (e.IsControl && e.Key == Keys.Add)
                {
                    zoom(_x, _y);
                }

                // �Y�[���_�E���L�[
                if (e.IsControl && e.Key == Keys.Subtract)
                {
                    zoom(-_x, -_y);
                }
            }
        }

        /// <summary>
        /// �L�[�_�E���C�x���g���̏���
        /// </summary>
        public void OnKeyDown(KeyState e)
        {
        }

        /// <summary>
        /// �g�[�N���N�����T�|�[�g
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            if (KeyZoomUp.Equals(who))
            {
                zoom(_x, _y);
            }
            if (KeyZoomDown.Equals(who))
            {
                zoom(-_x, -_y);
            }
        }

        #region IMultiTokenListener �����o

        public static readonly NamedId KeyZoomUp = NamedId.FromName("FeatureKeyZoom.Up");
        public static readonly NamedId KeyZoomDown = NamedId.FromName("FeatureKeyZoom.Down");
        private static readonly NamedId[] _tokenTrigger = new NamedId[] { KeyZoomUp, KeyZoomDown };

        public NamedId[] MultiTokenTriggerID => _tokenTrigger;

        #endregion
    }
}
