// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureMenuZoom �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureMenuZoom : FeatureBase
    {
        #region �����i�V���A���C�Y����j
        #endregion
        #region �����i�V���A���C�Y���Ȃ�

        /// <summary>�Y�[���l���i�[����</summary>
        private double zoom = 0.0;

        /// <summary>�X�N���[���E�Y�[���C�x���g����M����y�[��</summary>
        private IRichPane[] _tarRps = null;


        #endregion

        /// <summary>
        /// �p�����[�^���擾����
        /// </summary>
        public override void ParseParameter(string param)
        {
            zoom = double.Parse(param);
        }

        public override void OnInitInstance()
        {
            _tarRps = new IRichPane[] { Pane.GetPane("Resource") };
        }

        /// <summary>
        /// �w�肳�ꂽ�l�ɃY�[������
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            // �I���y�[���̕`��̈�̒��S���Y�[���̃Z���^�[�̐ݒ�
            var _posDown = new ScreenPos
            {
                X = _tarRps[0].GetPaneRect().LT.X + (_tarRps[0].GetPaneRect().RB.X - _tarRps[0].GetPaneRect().LT.X) / 2,       //�y�[����X���W�̒��S
                Y = _tarRps[0].GetPaneRect().LT.Y + (_tarRps[0].GetPaneRect().RB.Y - _tarRps[0].GetPaneRect().LT.Y) / 2       //�y�[����Y���W�̒��S
            };
            var _scrollDown = (ScreenPos)Pane.Scroll.Clone();       //�Y�[���O�̃X�N���[���l
            var _zoomDown = (XyBase)Pane.Zoom.Clone();                 //�Y�[���O�̃Y�[���l
                                                                       // ��ʂ̊g��/�k��
            var intZ = (int)(zoom * 100);                           //�Y�[���l�̎擾
            Pane.Zoom = XyBase.FromInt(1000, 1000);                    // �Y�[���l�̏�����
            var zoomX = Pane.Zoom.X * intZ / 100;
            var zoomY = Pane.Zoom.Y * intZ / 100;
            var zoomNow = XyBase.FromInt(zoomX, zoomY);                // �Y�[���l�̎Z�o
            Pane.Zoom = zoomNow;                                    // �Y�[���l�̔��f

            // �N���b�N�����ʒu����ɂ��ăY�[������悤�ɉ�ʂ��X�N���[������B
            var ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X�����̃Y�[�����̎Z�o
            var ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y�����̃Y�[�����̎Z�o

            var beforeDownPos = _posDown - _scrollDown - _tarRps[0].GetPaneRect().LT;    // 
            var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

            Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);     //�X�N���[���l�̔��f

            Pane.Invalidate(null);
        }



    }
}
