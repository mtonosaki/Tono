using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FreeDrawLayer �̊T�v�̐����ł��B
    /// </summary>
    public class FreeDrawLayer
    {
        private readonly int _layerLevel;
        private readonly IRichPane _parent;
        private Bitmap _bmp = null;
        private Graphics _gr = null;

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="layerLevel">���C���[���x��</param>
        public FreeDrawLayer(IRichPane parent, int layerLevel)
        {
            _layerLevel = layerLevel;
            _parent = parent;
        }

        /// <summary>
        /// ��ʏ������ł������ǂ��������ʂ���
        /// </summary>
        public bool IsReady => _parent.Graphics != null;

        /// <summary>
        /// �`��p�̃n���h��
        /// </summary>
        public Graphics Graphics
        {
            get
            {
                if (_gr == null)
                {
                    System.Diagnostics.Debug.Assert(_parent.Graphics != null, "Graphics�I�u�W�F�N�g���Q�Ƃ���^�C�~���O�́AcFeatureRich���`�悳�ꂽ��ł�");
                    var ts = new ThreadUtil();
                    var pH = ts.GetHandleControl(_parent.Control);
                    var g = Graphics.FromHwnd(pH);

                    _bmp = new Bitmap(_parent.GetPaneRect().Width, _parent.GetPaneRect().Height, g);
                    _gr = Graphics.FromImage(_bmp);
                }
                return _gr;
            }
        }

        /// <summary>
        /// ���̃t���[���C���[�Ɉ�x�ł��`�悵�����ǂ������ׂ�
        /// </summary>
        public bool IsUsing => _bmp != null;

        /// <summary>
        /// �i���������p���s���Ȃ��ł��������j�`��C���[�W�C���X�^���X
        /// </summary>
        public Image Image => _bmp;

        /// <summary>
        /// ���x��
        /// </summary>
        public int Level => _layerLevel;

        /// <summary>
        /// �n�b�V���R�[�h�����C���[���x��
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _layerLevel;
        }

    }
}
