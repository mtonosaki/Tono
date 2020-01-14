// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �I�������p�[�c��\�������A�Ő擪�ɂ���
    /// </summary>
    public class FeatureMoveSelectedPartsTop : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�P�ƃN���b�N�̃{�^���\��</summary>
        private readonly MouseState.Buttons _triggerSingle;
        /// <summary>�ǉ��{�^���\��</summary>
        private readonly MouseState.Buttons _triggerPlus;

        #endregion

        public FeatureMoveSelectedPartsTop()
        {
            _triggerSingle = new Tono.GuiWinForm.MouseState.Buttons(true, false, false, false, false);
            _triggerPlus = new Tono.GuiWinForm.MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// ����������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
        }

        /// <summary>
        /// �p�[�c�I���������}�E�X�_�E���C�x���g�ōs��
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_triggerPlus) || e.Attr.Equals(_triggerSingle))
            {

                var parts = ClickParts; // Parts.GetPartsAt(e.Pos, true, out tarPane);
                if (parts != null)
                {
                    ((PartsCollection)Parts).MovePartsZOrderToTop(parts);
                }
            }
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g�͕s�v
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g�͕s�v
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }
    }
}
