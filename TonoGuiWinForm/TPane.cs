using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// cRichPane の概要の説明です。
    /// </summary>
    public class TPane : Control, IRichPane, IControlUI
    {
        /// <summary>デザインで識別しやすくする名称</summary>
        private string _name = "";
        /// <summary>必要なデザイナ変数です。</summary>
        private readonly System.ComponentModel.Container components = null;
        /// <summary>ズーム値</summary>
        private XyBase _zoom = XyBase.FromInt(1000, 1000);
        /// <summary>スクロール量。＋が右下方向</summary>
        private ScreenPos _scroll = ScreenPos.FromInt(0, 0);
        /// <summary>X方向ズームしない = true</summary>
        private bool _isZoomLockX = false;
        /// <summary>Y方向ズームしない = true</summary>
        private bool _isZoomLockY = false;
        /// <summary>X方向スクロールしない = true</summary>
        private bool _isScrollLockX = false;
        /// <summary>Y方向スクロールしない = true</summary>
        private bool _isScrollLockY = false;

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TPane()
        {
            InitializeComponent();

            base.Visible = DesignMode;
        }

        /// <summary>
        /// X方向のスクロール・ズームしない
        /// </summary>
        [Description("X方向のズームしない = true")]
        [Category("Tono.GuiWinForm")]
        public bool IsZoomLockX
        {
            
            get => _isZoomLockX;
            
            set => _isZoomLockX = value;
        }

        /// <summary>
        /// Y方向のスクロール・ズームしない
        /// </summary>
        [Description("Y方向のズームしない = true")]
        [Category("Tono.GuiWinForm")]
        public bool IsZoomLockY
        {
            
            get => _isZoomLockY;
            
            set => _isZoomLockY = value;
        }

        /// <summary>
        /// X方向のスクロールしない
        /// </summary>
        [Description("X方向のスクロールしない = true")]
        [Category("Tono.GuiWinForm")]
        public bool IsScrollLockX
        {
            
            get => _isScrollLockX;
            
            set => _isScrollLockX = value;
        }

        /// <summary>
        /// Y方向のスクロールしない
        /// </summary>
        [Description("Y方向のスクロールしない = true")]
        [Category("Tono.GuiWinForm")]
        public bool IsScrollLockY
        {
            
            get => _isScrollLockY;
            
            set => _isScrollLockY = value;
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

        #region Component Designer generated code
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // cRichPane
            // 
            BackColor = System.Drawing.Color.Blue;
            ImeMode = System.Windows.Forms.ImeMode.Disable;

        }
        #endregion


        /// <summary>
        /// 領域を返す
        /// </summary>
        /// <returns>領域を表す矩形</returns>
        
        public ScreenRect GetPaneRect()
        {
            return ScreenRect.FromLTWH(Left, Top, Width, Height);
        }

        /// <summary>
        /// 文字列生成
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{RichPane={0}}}", IdText);
        }

        /// <summary>
        /// 背景描画イベント処理
        /// </summary>
        protected override void OnPaintBackground(System.Windows.Forms.PaintEventArgs pevent)
        {
            if (DesignMode)
            {
                base.OnPaintBackground(pevent);
            }
        }

        /// <summary>
        /// 描画イベント処理
        /// </summary>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
                using (var drawFont = new Font("MS UI Gothic", 9))
                {
                    string s;
                    if (_name == "")
                    {
                        s = "(名称未設定)";
                    }
                    else
                    {
                        s = "(" + _name + ")";
                    }
                    var col = Brushes.White;
                    if (BackColor.GetBrightness() > 0.7)
                    {
                        col = Brushes.Black;
                    }
                    e.Graphics.DrawString(s, drawFont, col, 4, 4);
                }


                // アンカーを表示
                Brush brush = new SolidBrush(Color.FromArgb(80, 0, 0, 0));
                var ap = new Pen(brush, 1)
                {
                    EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor
                };
                if ((Anchor & AnchorStyles.Top) != 0)
                {
                    e.Graphics.DrawLine(Pens.Black, Width / 2 - 4, 3, Width / 2 + 4, 3);
                    e.Graphics.DrawLine(ap, Width / 2, 11, Width / 2, 3);
                }
                if ((Anchor & AnchorStyles.Bottom) != 0)
                {
                    e.Graphics.DrawLine(Pens.Black, Width / 2 - 4, Height - 4, Width / 2 + 4, Height - 4);
                    e.Graphics.DrawLine(ap, Width / 2, Height - 12, Width / 2, Height - 4);
                }
                if ((Anchor & AnchorStyles.Left) != 0)
                {
                    e.Graphics.DrawLine(Pens.Black, 3, Height / 2 - 4, 3, Height / 2 + 4);
                    e.Graphics.DrawLine(ap, 11, Height / 2, 3, Height / 2);
                }
                if ((Anchor & AnchorStyles.Right) != 0)
                {
                    e.Graphics.DrawLine(Pens.Black, Width - 4, Height / 2 - 4, Width - 4, Height / 2 + 4);
                    e.Graphics.DrawLine(ap, Width - 12, Height / 2, Width - 4, Height / 2);
                }
                ap.Dispose();
                brush.Dispose();
            }
        }

        /// <summary>
        /// デザイン時に識別しやすくする色を指定する（アプリの動作には影響しない）
        /// </summary>
        [Description("デザイン時に識別しやすくする色（アプリの動作には影響しない）")]
        [ParenthesizePropertyName(true)]
        [Category("Design")]
        public Color IdColor
        {
            
            get => base.BackColor;
            
            set => base.BackColor = value;
        }

        /// <summary>
        /// ペーンを特定するユニークなテキスト
        /// </summary>
        [Description("ペーンを特定するユニークなテキスト")]
        [ParenthesizePropertyName(true)]
        [Category("Design")]
        public string IdText
        {
            
            get => _name;
            
            set
            {
                var safe = "0123456789.abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                _name = "";
                for (var i = 0; i < value.Length; i++)
                {
                    if (safe.IndexOf(value[i]) >= 0)
                    {
                        _name += value[i];
                    }
                }
                Invalidate();
            }
        }

        #region プロパティの表示を隠す
        [Browsable(false)]
        public new string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        [Browsable(false)]
        public new bool AllowDrop
        {
            get => base.AllowDrop;
            set => base.AllowDrop = value;
        }

        [Browsable(false)]
        public new System.Windows.Forms.ContextMenu ContextMenu
        {
            get => base.ContextMenu;
            set => base.ContextMenu = value;
        }

        [Browsable(false)]
        public new System.Windows.Forms.ImeMode ImeMode
        {
            get => base.ImeMode;
            set => base.ImeMode = value;
        }

        [Browsable(false)]
        public new int TabIndex
        {
            get => base.TabIndex;
            set => base.TabIndex = value;
        }

        [Browsable(false)]
        public new bool TabStop
        {
            get => base.TabStop;
            set => base.TabStop = value;
        }

        [Browsable(false)]
        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (DesignMode)
                {
                    base.Visible = value;
                }
                else
                {
                    base.Visible = false;
                }
            }
        }

        [Browsable(false)]
        public new System.Drawing.Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value;
        }

        [Browsable(false)]
        public new System.Drawing.Image BackgroundImage
        {
            get => base.BackgroundImage;
            set => base.BackgroundImage = value;
        }

        [Browsable(false)]
        public new System.Windows.Forms.Cursor Cursor
        {
            get => base.Cursor;
            set => base.Cursor = value;
        }

        [Browsable(false)]
        public new System.Drawing.Font Font
        {
            get => base.Font;
            set => base.Font = value;
        }

        [Browsable(false)]
        public new System.Drawing.Color ForeColor
        {
            get => base.ForeColor;
            set => base.ForeColor = value;
        }

        [Browsable(false)]
        public new System.Windows.Forms.RightToLeft RightToLeft
        {
            get => base.RightToLeft;
            set => base.RightToLeft = value;
        }

        [Browsable(false)]
        public new bool CausesValidation
        {
            get => base.CausesValidation;
            set => base.CausesValidation = value;
        }

        [Browsable(false)]
        public new bool Enabled
        {
            get => base.Enabled;
            set => base.Enabled = value;
        }

        [Browsable(false)]
        public new string AccessibleDescription
        {
            get => base.AccessibleDescription;
            set => base.AccessibleDescription = value;
        }

        [Browsable(false)]
        public new string AccessibleName
        {
            get => base.AccessibleName;
            set => base.AccessibleName = value;
        }

        [Browsable(false)]
        public new System.Windows.Forms.AccessibleRole AccessibleRole
        {
            get => base.AccessibleRole;
            set => base.AccessibleRole = value;
        }
        #endregion

        /// <summary>
        /// アンカー変更処理をオーバーライドする
        /// </summary>
        public override AnchorStyles Anchor
        {
            
            get => base.Anchor;
            
            set
            {
                base.Anchor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// サイズ変更イベントを処理する
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            Invalidate();
            base.OnSizeChanged(e);
        }
        #region IRichPane メンバ

        /// <summary>
        /// 自分のインスタンスをControl型で返す
        /// </summary>
        public Control Control
        {
            
            get => this;
        }

        /// <summary>
        /// 親ペーンを取得する
        /// </summary>
        /// <returns></returns>
        
        public IRichPane GetParent()
        {
            if (Parent is IRichPane)
            {
                return (IRichPane)Parent;
            }
            return null;
        }

        /// <summary>
        /// 直近のcFeatureRichを返す
        /// </summary>
        /// <returns>cFeatureRichの参照 / null = 無い</returns>
        
        public TGuiView GetFeatureRich()
        {
            for (IRichPane rp = this; rp != null; rp = rp.GetParent())
            {
                if (rp is TGuiView)
                {
                    return (TGuiView)rp;
                }
            }
            return null;
        }

        /// <summary>
        /// 名前でペーンを検索する
        /// </summary>
        /// <param name="tar">検索ペーンの階層現在位置</param>
        /// <param name="name">検索Name</param>
        /// <returns>見つかったペーン / null = その階層には見つからなかった</returns>
        private IRichPane _findPaneByIdText(Control tar, string IdText)
        {
            foreach (Control c in tar.Controls)
            {
                if (c is IRichPane)
                {
                    if (((IRichPane)c).IdText == IdText)
                    {
                        return (IRichPane)c;
                    }
                }
                var ret = _findPaneByIdText(c, IdText);
                if (ret != null)
                {
                    return ret;
                }
            }
            return null;
        }

        /// <summary>
        /// ペーンをIdTextで検索する
        /// </summary>
        /// <param name="IdText">見つけたいペーンのIdText</param>
        /// <returns>見つかったペーン / null = そのIdTextのペーンは無い</returns>
        public IRichPane GetPane(string IdText)
        {
            IRichPane root;
            for (root = this; root.GetParent() != null; root = root.GetParent())
            {
                ;
            }

            return _findPaneByIdText((Control)root, IdText);
        }

        public ScreenPos GetZoomed(LayoutPos value)
        {
            XyBase ret = value * _zoom / 1000;
            return ScreenPos.FromInt(ret.X, ret.Y);
        }
        public LayoutPos GetZoomed(ScreenPos value)
        {
            XyBase ret = value * 1000 / _zoom;
            return LayoutPos.FromInt(ret.X, ret.Y);
        }
        public ScreenPos Convert(LayoutPos value)
        {
            XyBase ret = (value * _zoom / 1000) + _scroll + GetPaneRect().LT;
            return ScreenPos.FromInt(ret.X, ret.Y);
        }
        public ScreenRect Convert(LayoutRect value)
        {
            var ret = (value * _zoom / 1000) + _scroll + GetPaneRect().LT;
            return ScreenRect.FromLTWH(ret.LT.X, ret.LT.Y, ret.Width, ret.Height);
        }

        public LayoutPos Convert(ScreenPos value)
        {
            XyBase ret = (value - GetPaneRect().LT - _scroll) * 1000 / _zoom;
            return LayoutPos.FromInt(ret.X, ret.Y);
        }
        public LayoutRect Convert(ScreenRect value)
        {
            var ret = (value - GetPaneRect().LT - _scroll) * 1000 / _zoom;
            return LayoutRect.FromLTRB(ret.LT.X, ret.LT.Y, ret.RB.X, ret.RB.Y);
        }

        public XyBase Zoom
        {
            
            get => _zoom;
            set
            {
                if (value != null)
                {
                    if (_isZoomLockX)
                    {
                        value.X = 1000;
                    }

                    if (_isZoomLockY)
                    {
                        value.Y = 1000;
                    }

                    if (Parent is TGuiView)
                    {
                        _zoom = ((TGuiView)Parent).ZoomCheck(value);
                    }
                    else
                    {
                        _zoom = value;
                    }
                    // ズームイベントを送信する
                    var rp = GetFeatureRich();
                    if (rp != null)
                    {
                        rp.SendZoomChangedEvent(this);
                    }
                }
                else
                {
                    _zoom = XyBase.FromInt(1000, 1000);
                }
            }
        }

        public ScreenPos Scroll
        {
            
            get => _scroll;
            set
            {
                if (value != null)
                {
                    if (_isScrollLockX)
                    {
                        value.X = 0;
                    }

                    if (_isScrollLockY)
                    {
                        value.Y = 0;
                    }

                    _scroll = value;
                    // スクロールイベントを送信する
                    var rp = GetFeatureRich();
                    if (rp != null)
                    {
                        rp.SendScrollChangedEvent(this);
                    }
                }
                else
                {
                    value = ScreenPos.FromInt(0, 0);
                }
            }
        }

        /// <summary>
        /// グラフィックオブジェクト
        /// </summary>
        public System.Drawing.Graphics Graphics
        {
            
            get => ((TGuiView)Parent).GetCurrentGraphics();
        }

        /// <summary>
        /// 描画が必要な領域を返すインターフェース
        /// </summary>
        /// <returns>領域</returns>
        
        public ScreenRect GetPaintClipRect()
        {
            return ((TGuiView)Parent).GetPaintClipRect();
        }

        /// <summary>
        /// 画面を再描画する
        /// </summary>
        /// <param name="rect">再描画する範囲</param>
        public void Invalidate(ScreenRect rect)
        {
            ((TGuiView)Parent).Invalidate(rect); // cFeatureRichのInvalidateを実行する(直接ControlのInvalidateを実行しない）
        }


        #endregion

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (DesignMode)
            {
                return;
            }

            base.OnKeyDown(e);
        }
        #region IControlUI メンバ

        Cursor Tono.GuiWinForm.IControlUI.Cursor
        {
            get => Parent.Cursor;
            set => Parent.Cursor = value;
        }

        #endregion
    }
}
