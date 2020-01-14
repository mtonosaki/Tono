// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// コントロールの画像をビットマップに保存する
    /// </summary>
    public class ControlSnapshot
    {
        #region GDI32の使用
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        #endregion

        //フォームのイメージを保存する変数
        private Bitmap _imageBuffer;

        /// <summary>ターゲットとなるコントロール</summary>
        private readonly Control _target;

        /// <summary>
        /// 構築子
        /// </summary>
        /// <param name="target">対象コントロール</param>
        public ControlSnapshot(Control target)
        {
            _target = target;
        }

        /// <summary>
        /// 現在の状態のスナップショットを記録する
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
        /// 現在の状態のビットマップ
        /// </summary>
        public Image Image => _imageBuffer;

        /// <summary>
        /// 対象のコントロール
        /// </summary>
        public Control Control => _target;


        //フォームのイメージを取得する
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
