// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573, 1587

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fiKeyEnabler の概要の説明です。
    /// </summary>
    public class TKeyEnabler : System.Windows.Forms.UserControl
    {
        private System.ComponentModel.IContainer components;
        private static bool _enable = true;
        private Control _parentForm = null;

        /// <summary>
        /// 一時的に機能の有効・無効変更
        /// </summary>
        public static bool Enable
        {
            set => _enable = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TKeyEnabler()
        {
            // この呼び出しは、Windows.Forms フォーム デザイナで必要です。
            InitializeComponent();

            if (DesignMode == false)
            {
                SetStyle(ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                BackColor = Color.Transparent;
                Invalidate();
            }
        }

        /// <summary>
        /// 起動時の初期化
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Width > 1)
            {
                Location = new Point(0, 0);
                Size = new Size(1, 1);
            }

            // 担当するFormを取得する
            var isTabControlFocusCatched = false;

            for (_parentForm = Parent; _parentForm != null && _parentForm is Form == false; _parentForm = _parentForm.Parent)
            {
                if (isTabControlFocusCatched == false)
                {
                    if (_parentForm is TabControl)  // Tabコントロールは、勝手にKeyDownを奪って上位のUserControlに渡さないので、ここで取得しておく
                    {
                        _parentForm.KeyDown += new KeyEventHandler(onKeyDown);
                        _parentForm.KeyUp += new KeyEventHandler(onKeyUp);
                        isTabControlFocusCatched = true;
                    }
                }
            }
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        /// <summary>
        /// 使用されているリソースに後処理を実行します。
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナで生成されたコード 
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            // 
            // fiKeyEnabler
            // 
            Name = "fiKeyEnabler";
            Size = new System.Drawing.Size(120, 80);

        }
        #endregion

        private KeyState _prevKeyState = new KeyState();

        /// <summary>
        /// キーイベントをcFeatureRichに転送する
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_enable == false)
            {
                return;
            }
            // TABキーのフィルタリング
            if (e.KeyCode == Keys.Tab && e.Control == false && e.Shift == false)
            {
                return;
            }

            // その他のキーの転送
            if (Parent is TGuiView)
            {
                var ke = KeyState.FromKeyEventArgs(e);
                if (ke.Equals(_prevKeyState) == false)
                {
                    ((TGuiView)Parent).OnKeyDown(ke);
                    _prevKeyState = ke;
                    System.Diagnostics.Debug.WriteLine(ke);
                }
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// 指定されたキーが通常キーかどうかを調査する
        /// </summary>
        /// <param name="keyData">調査するキーコード</param>
        /// <returns>調査結果 TRUE:通常キー / FALSE:特殊キー</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:           // ↑キー
                case Keys.Down:         // ↓キー
                case Keys.Left:         // ←キー
                case Keys.Right:        // →キー
                    break;
                default:
                    return base.IsInputKey(keyData);    // その他のキー
            }
            return true;
        }


        /// <summary>
        /// キーイベントをcFeatureRichに転送する
        /// </summary>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (_enable == false)
            {
                return;
            }
            // その他のキーの転送
            if (Parent is TGuiView)
            {
                _prevKeyState = null;
                var ke = KeyState.FromKeyEventArgs(e);
                ((TGuiView)Parent).OnKeyUp(ke);
            }
            base.OnKeyUp(e);
        }

        /// <summary>
        /// 人為的にキーアップイベントを発行する
        /// </summary>
        /// <param name="e"></param>
        public void KickKeyUp(KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        /// <summary>
        /// ペイント処理のフィルタリング
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                RectangleF r = (ScreenRect.FromControl(this) - XyBase.FromInt(Left, Top));
                using (var fo = new Font("Arial", 12, FontStyle.Bold))
                {
                    e.Graphics.DrawString("K", fo, Brushes.WhiteSmoke, r, sf);
                }
            }
        }

        /// <summary>
        /// 背景ペイント処理のフィルタリング
        /// </summary>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (DesignMode)
            {
                base.OnPaintBackground(pevent);
            }
        }

        /// <summary>
        /// 自動的にフォーカスをONにする処理
        /// </summary>
        private void timerAutoFocus_Tick(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// サイズ変更時の処理
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (DesignMode)
            {
                Invalidate();
            }
            base.OnSizeChanged(e);
        }
    }
}
