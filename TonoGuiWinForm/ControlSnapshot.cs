// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �R���g���[���̉摜���r�b�g�}�b�v�ɕۑ�����
    /// </summary>
    public class ControlSnapshot
    {
        #region GDI32�̎g�p
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        #endregion

        //�t�H�[���̃C���[�W��ۑ�����ϐ�
        private Bitmap _imageBuffer;

        /// <summary>�^�[�Q�b�g�ƂȂ�R���g���[��</summary>
        private readonly Control _target;

        /// <summary>
        /// �\�z�q
        /// </summary>
        /// <param name="target">�ΏۃR���g���[��</param>
        public ControlSnapshot(Control target)
        {
            _target = target;
        }

        /// <summary>
        /// ���݂̏�Ԃ̃X�i�b�v�V���b�g���L�^����
        /// </summary>
        public void Snapshot()
        {
            var b = _target.Visible;
            if (b == false)
            {
                _target.Visible = true;
                Application.DoEvents();
            }
            captureScreen(_target);
            _target.Visible = b;
        }

        /// <summary>
        /// ���݂̏�Ԃ̃r�b�g�}�b�v
        /// </summary>
        public Image Image => _imageBuffer;

        /// <summary>
        /// �Ώۂ̃R���g���[��
        /// </summary>
        public Control Control => _target;


        //�t�H�[���̃C���[�W���擾����
        private void captureScreen(Control c)
        {
            var ctrlG = c.CreateGraphics();
            var s = c.Size;
            _imageBuffer = new Bitmap(s.Width, s.Height, ctrlG);
            var tarG = Graphics.FromImage(_imageBuffer);
            var dc1 = ctrlG.GetHdc();
            var dc2 = tarG.GetHdc();
            BitBlt(dc2, 0, 0, c.ClientRectangle.Width, c.ClientRectangle.Height, dc1, 0, 0, 0xcc0020);
            ctrlG.ReleaseHdc(dc1);
            tarG.ReleaseHdc(dc2);
        }
    }
}
