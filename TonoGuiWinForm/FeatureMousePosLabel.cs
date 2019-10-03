using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureMousePosLabel �̊T�v�̐����ł��B
    /// �w�肵���R���g���[���ɁA�}�E�X�̍��W��\������t�B�[�`���[�N���X
    /// ������g�p���邱�ƂŁA���A���^�C���Ƀ}�E�X���W��m�邱�Ƃ��ł���
    /// </summary>
    public class FeatureMousePosLabel : FeatureBase, IMouseListener
    {
        private Control _mousePosLabel;

        /// <summary>
        /// �R���g���[�����擾�A�ݒ肷��
        /// </summary>
        public Control TargetControl
        {
            get => _mousePosLabel;
            set => _mousePosLabel = value;
        }

        /// <summary>
        /// �}�E�X�ړ����̃C�x���g
        /// </summary>
        public void OnMouseMove(MouseState e)
        {

            Control c;
            if (Pane is TPane)
            {
                c = ((TPane)Pane).Parent;
            }
            else
            {
                c = (Control)Pane;
            }


            // �}�E�X���W�ɉ����āA�ꏊ�������A�C�e�����ړ�����
            // �ڂ������ɍ��

            //e.Pos;���ꂪ�}�E�X���W

            _mousePosLabel.Text = e.Pos.Y + "," + e.Pos.X;

        }

        #region IMouseListener �����o
        public void OnMouseDown(MouseState e)
        {
            // ���g�p
        }

        public void OnMouseUp(MouseState e)
        {
            // ���g�p
        }

        public void OnMouseWheel(MouseState e)
        {
            // ���g�p
        }


        #endregion
    }
}
