using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureRowColIndicator �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureRowColIndicator : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y����j
        private string _tarPaneIdText;
        private Control _vl;
        private Control _hl;
        #endregion

        #region �����i�V���A���C�Y���Ȃ��j
        private IRichPane _tarPane = null;
        #endregion

        /// <summary>
        /// �ΏۂƂȂ�y�[���̖��̂��Z�b�g����
        /// </summary>
        public string TargetPaneIdText
        {
            get => _tarPaneIdText;
            set => _tarPaneIdText = value;
        }

        /// <summary>
        /// �}�E�X�ړ����̃C�x���g
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            // �ΏۂƂȂ�y�[�����擾����
            if (_tarPane == null)
            {
                _tarPane = Pane.GetPane(_tarPaneIdText);

                Control c;
                if (_tarPane is TPane)
                {
                    c = ((TPane)_tarPane).Parent;
                }
                else
                {
                    c = (Control)_tarPane;
                }
                var pr = _tarPane.GetPaneRect();
                c.Controls.Add(_vl = new Control
                {
                    ForeColor = Color.Green,
                    BackColor = System.Drawing.Color.Black,
                    Name = "GIdeVertLabel",
                    Size = new Size(8, 1),
                    //�����̍��W�ݒ�
                    Location = new Point(pr.LT.X, pr.LT.Y)
                });
                c.Controls.Add(_hl = new Control
                {
                    ForeColor = Color.Green,
                    BackColor = System.Drawing.Color.Black,
                    Name = "GIdeHorzLabel",
                    //�����̍��W�ݒ�
                    Location = new Point(pr.LT.X, pr.LT.Y),
                    Size = new Size(1, 8)
                });

            }
            // �}�E�X���W�ɉ����āA�ꏊ�������A�C�e�����ړ�����
            //e.Pos�F�}�E�X���W
            //_vl.Top�FY���W(�c�̓���)�@_hl�FX���W(���̓���)
            _vl.Top = e.Pos.Y;
            _hl.Left = e.Pos.X;
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
