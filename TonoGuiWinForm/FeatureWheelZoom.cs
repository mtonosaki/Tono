namespace Tono.GuiWinForm
{
    /// <summary>
    /// Ctrl+�}�E�X�z�C�[���ŃY�[�����T�|�[�g����@�\
    /// </summary>
    public class FeatureWheelZoom : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region		����(�V���A���C�Y����)
        /** <summary>�Y�[�����J�n����g���K</summary> */
        private MouseState.Buttons _trigger;
        #endregion
        #region		����(�V���A���C�Y���Ȃ�)
        /// <summary>
        /// �Y�[�����E�Ȃ̂Ŏ������_�ړ����Ȃ�
        /// </summary>
        private DataSharingManager.Boolean _noscrollmove = null;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureWheelZoom()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = false,
                IsShift = false
            };
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
        /// �g���K�i���s���ʃL�[�j��ύX����
        /// </summary>
        /// <param name="value">�V�����g���K�[</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        /// <summary>
        /// �������
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (param != "")
            {
                var s = param.ToUpper();
                if (s.IndexOf('X') < 0)
                {
                    _isX = false;
                }
                if (s.IndexOf('Y') < 0)
                {
                    _isY = false;
                }
            }
        }

        private bool _isX = true;
        private bool _isY = true;


        private void log(string name, object o)
        {
            System.Diagnostics.Debug.WriteLine(name + " : " + o.ToString());
        }

        #region IMouseListener �����o

        /// <summary>
        /// �}�E�XMove�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseMove(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �{�^��Down�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseDown(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �{�^��Up�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseUp(MouseState e)
        {
            // ���g�p
        }

        /// <summary>
        /// �}�E�XWheel�C�x���g
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                // �I���y�[���̕`��̈�̒��S���Y�[���̃Z���^�[�̐ݒ�
                var _posDown = new ScreenPos
                {
                    X = e.Pane.GetPaneRect().LT.X + e.Pos.X - e.Pane.GetPaneRect().LT.X,
                    Y = e.Pane.GetPaneRect().LT.Y + e.Pos.Y - e.Pane.GetPaneRect().LT.Y
                };
                var _scrollDown = (ScreenPos)Pane.Scroll.Clone();
                var _zoomDown = (XyBase)Pane.Zoom.Clone();
                var vol = (int)(GeoEu.Length(Pane.Zoom.X, Pane.Zoom.Y) / 1000 * e.Delta.Y * 0.1);

                // ��ʂ̊g��/�k��

                XyBase zoomNow;
                if (_isX && !_isY)
                {
                    zoomNow = Pane.Zoom + XyBase.FromInt(vol, 0);          // �Y�[���l�̎Z�o
                }
                else if (!_isX && _isY)
                {
                    zoomNow = Pane.Zoom + XyBase.FromInt(0, vol);          // �Y�[���l�̎Z�o
                }
                else
                {
                    zoomNow = Pane.Zoom + vol;                          // �Y�[���l�̎Z�o
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

                var beforeDownPos = _posDown - _scrollDown - e.Pane.GetPaneRect().LT;    // 
                var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

                if (_noscrollmove.value == false)
                {
                    Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
                }
                else
                {
                    _noscrollmove.value = false;
                }
                Pane.Invalidate(null);
            }
            else
            {
                OnMouseUp(e);
            }
        }

        #endregion
    }
}
