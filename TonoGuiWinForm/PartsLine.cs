using System.Drawing;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �����̕`��I�u�W�F�N�g
    /// </summary>
    public class PartsLine : PartsBase
    {
        #region		����(�V���A���C�Y����)
        // �y���I�u�W�F�N�g
        private Pen _pen = new Pen(Color.FromArgb(255, 96, 128));
        #endregion
        #region		����(�V���A���C�Y���Ȃ�)
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public PartsLine()
        {
        }

        /// <summary>
        /// �`��
        /// </summary>
        /// <param name="rp"></param>
        public override bool Draw(IRichPane rp)
        {
            var pos = GetScRect(rp);        // �ʒu���擾����
                                            // InClip������ƉE������̐������`��ł��Ȃ��ׁAInClip�͂��Ȃ�
                                            //if( isInClip(rp, pos) == false )	// �`��s�v�ł���΁A�Ȃɂ����Ȃ�
                                            //{
                                            //	return false;
                                            //}
                                            //Mask(rp, eMask.Specification);	// ����}�X�N�݂̂ŕ\��
            rp.Graphics.DrawLine(_pen, pos.LT, pos.RB);
            return true;
        }

        /// <summary>
        /// �y���I�u�W�F�N�g�iSET����ꍇ�A����Pen��Dispose���邱�Ɓj
        /// </summary>
        public Pen Style
        {
            get => _pen;
            set => _pen = value;
        }

        /// <summary>
        /// ���̑����̎擾/�ݒ�
        /// </summary>
        public float LineWidth
        {
            get => _pen.Width;
            set => _pen.Width = value;
        }

        /// <summary>
        /// ���̐F�̎擾/�ݒ�
        /// </summary>
        public virtual Color LineColor
        {
            get => _pen.Color;
            set => _pen.Color = value;
        }

        /// <summary>
        /// �y���̐��̃X�^�C�����擾/�ݒ�
        /// </summary>
        public System.Drawing.Drawing2D.DashStyle DashStyle
        {
            get => _pen.DashStyle;
            set => _pen.DashStyle = value;
        }

        /// <summary>
        /// �y���ɏI�[�̃X�^�C���̎擾/�ݒ�
        /// </summary>
        public System.Drawing.Drawing2D.LineCap EndCap
        {
            get => _pen.EndCap;
            set => _pen.EndCap = value;
        }

        /// <summary>
        /// �y���̎n�[�̃X�^�C���̎擾/�ݒ�
        /// </summary>
        public System.Drawing.Drawing2D.LineCap StartCap
        {
            get => _pen.StartCap;
            set => _pen.StartCap = value;
        }
    }
}
