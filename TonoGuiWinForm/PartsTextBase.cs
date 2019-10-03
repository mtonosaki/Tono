using System;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 表示・編集のための、テキスト基本クラス
    /// 背景塗りつぶしは、派生クラスで行うこと（デザイン柔軟性のため）
    /// このクラスは、テキスト表示しかしない。
    /// </summary>
    public abstract class PartsTextBase : PartsBase, IPartsSelectable
    {
        #region 属性（シリアライズする）
        private string _fontName;
        private float _fontSize;
        private Color _fontColor;
        private bool _isVertText;
        protected Color _bgColor;
        protected Color _lineColor;
        private bool _isSelected;
        private bool _isItalic;
        private bool _isBold;
        private ImeMode _imeMode;
        private bool _isMultiLine;
        #endregion
        #region 属性（シリアライズしない）
        [NonSerialized]
        protected StringFormat _sf = new StringFormat(); // テキストのフォーマットを作成
        [NonSerialized]
        private Font _lastFont = null;

        #endregion

        #region プロパティ

        /// <summary>
        /// テキストマージン（Top）
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// テキストマージン（Left）
        /// </summary>
        public int MarginLeft { get; set; }

        /// <summary>
        /// フォントサイズ
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public bool IsItalic
        {
            get => _isItalic;
            set => _isItalic = value;
        }

        public bool IsBold
        {
            get => _isBold;
            set => _isBold = value;
        }

        public ImeMode ImeMode
        {
            get => _imeMode;
            set => _imeMode = value;
        }

        /// <summary>
        /// フォント名
        /// </summary>
        public string FontName
        {
            get => _fontName;
            set => _fontName = value;
        }

        /// <summary>
        /// 最後に表示したフォントを返す
        /// </summary>
        public Font LastFont
        {
            get => _lastFont;
            set => _lastFont = value;
        }

        /// <summary>
        /// 背景色
        /// </summary>
        public Color BackColor
        {
            get => _bgColor;
            set => _bgColor = value;
        }

        /// <summary>
        /// 線の色
        /// </summary>
        public Color LineColor
        {
            get => _lineColor;
            set => _lineColor = value;
        }

        /// <summary>
        /// フォントの色
        /// </summary>
        public Color FontColor
        {
            get => _fontColor;
            set => _fontColor = value;
        }

        /// <summary>
        /// 改行を許可するか？
        /// </summary>
        public bool IsMultiLine
        {
            get => _isMultiLine;
            set => _isMultiLine = value;
        }
        #endregion

        /// <summary>
        /// 基本クラスのコンストラクタ
        /// </summary>
        protected PartsTextBase()
        {
            _fontName = "MS UI Gothic";
            _fontSize = 9.0f;
            _fontColor = Color.Black;
            _bgColor = Color.FromArgb(192, 224, 192);
            _lineColor = Color.FromArgb(96, 192, 96);
            _isVertText = false;
            _isSelected = false;
            _imeMode = ImeMode.NoControl;
            _isMultiLine = false;
            TextAlignHorz = StringAlignment.Near;
            TextAlignVert = StringAlignment.Near;
        }

        /// <summary>
        /// GDI開放
        /// </summary>
        public override void Dispose()
        {
            if (_lastFont != null)
            {
                _lastFont.Dispose(); _lastFont = null;
            }
            base.Dispose();
        }

        /// <summary>
        /// テキストの表示位置（水平）
        /// </summary>
        public StringAlignment TextAlignHorz { get; set; }

        /// <summary>
        /// テキストの表示位置（垂直）
        /// </summary>
        public StringAlignment TextAlignVert { get; set; }

        /// <summary>
        /// 縦向きに回転？
        /// </summary>
        public bool IsDirectionVertical { get; set; }

        /// <summary>
        /// 描画オーバーライド
        /// </summary>
        /// <param name="rp"></param>
        /// <returns></returns>
        public override bool Draw(IRichPane rp)
        {
            var spos = GetScRect(rp);
            if (isInClip(rp, spos) == false)    // 描画不要であれば、なにもしない
            {
                return false;
            }


            // テキスト描画
            var fsize = GetPointFromPoint(_fontSize, rp);
            if (fsize > 3)
            {
                _sf.Alignment = TextAlignHorz;
                _sf.LineAlignment = TextAlignVert;
                _sf.Trimming = StringTrimming.EllipsisPath;
                if (IsDirectionVertical)
                {
                    _sf.FormatFlags |= StringFormatFlags.DirectionVertical;
                }
                if (IsVerticalText)
                {
                    _sf.FormatFlags = StringFormatFlags.DirectionVertical;
                }
                rp.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                var style = (_isItalic ? FontStyle.Italic : 0) | (_isBold ? FontStyle.Bold : 0);
                if (_lastFont != null)
                {
                    _lastFont.Dispose();
                }
                if (_lastFont != null)
                {
                    _lastFont.Dispose();
                }
                _lastFont = new Font(_fontName, fsize, style);  // フォントを作成
                var sr = (ScreenRect)spos.Clone();
                sr.LT.X += MarginLeft;
                sr.LT.Y += MarginTop;
                using (Brush brush = new SolidBrush(_fontColor))
                {
                    rp.Graphics.DrawString(Text, _lastFont, brush, sr, _sf);   // テキストを描画
                }
            }
            return true;
        }

        /// <summary>
        /// ズームを考慮したポイント数を計算
        /// </summary>
        /// <param name="point">期待値</param>
        /// <param name="rp">リッチペーン</param>
        /// <returns>ズームを考慮したポイント数</returns>
        public float GetPointFromPoint(float point, IRichPane rp)
        {
            return (float)(point * GeoEu.Length(rp.Zoom.X, rp.Zoom.Y) / 1000 / Math.Sqrt(2));
        }

        /// <summary>
        /// 単位ポイントから、スクリーン座標系の値を取得する
        /// </summary>
        /// <param name="point">ポイント数</param>
        /// <returns>スクリーン座標系に換算した値</returns>
        public float GetScFromPoint(float point, IRichPane rp)
        {
            var p = CodeRect.FromLTRB((int)(GetMillimeterFromPoint(point) * 1000), 0, 0, 0);
            var lr = GetPtRect(p);
            return (float)(lr.LT.X * GeoEu.Length(rp.Zoom.X, rp.Zoom.Y) / 1000);
        }

        /// <summary>
        /// ポイントをミリメートルに変換する
        /// </summary>
        /// <param name="point">ポイント</param>
        /// <returns>ミリメートル</returns>
        public float GetMillimeterFromPoint(float point)
        {
            return 100f / 284.53f * point;
        }

        /// <summary>
        /// 縦書きテキストかどうかのフラグ
        /// </summary>
        public bool IsVerticalText
        {
            get => _isVertText;
            set => _isVertText = value;
        }

        #region IPartsSelectable メンバ

        public bool IsSelected
        {
            get => _isSelected;
            set => _isSelected = value;
        }

        #endregion

    }
}
