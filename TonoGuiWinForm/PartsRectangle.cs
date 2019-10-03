using System;
using System.Drawing;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 四角形の描画オブジェクト
    /// </summary>
    [Serializable]
    public class PartsRectangle : PartsBase, ICloneable, ISerializable
    {
        #region		属性(シリアライズする)
        /** <summary>線の色</summary> */
        protected Color _penColor = Color.Black;
        protected float _lineWidth = 1;
        protected int _minFontVisibleZoom = 250;

        /** <summary>テキストの色</summary> */
        protected Color _textColor = Color.Black;

        /** <summary>縦書き</summary> */
        private bool _isVertText;
        /** <summary>フォント</summary> */
        protected string _fontFace = "Arial";
        /** <summary>フォントサイズ</summary> */
        protected float _fontSize = 9.0f;
        /// <summary>自動縦横設定機能</summary>
        private bool _isAutoVertical = false;
        /// <summary>
        /// マージン（全方向）
        /// </summary>
        public int _margin = 0;

        /// <summary>
        /// クリップ処理を無視する
        /// </summary>
        public bool IsClipDraw { get; set; }

        #endregion

        #region ISerializable メンバ
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            SerializerEx.GetObjectData(typeof(PartsRectangle), this, info, context, false);
        }
        protected PartsRectangle(SerializationInfo info, StreamingContext context) : base(info, context)

        {
            SerializerEx.Instanciate(typeof(PartsRectangle), this, info, context, false);
        }

        #endregion
        #region		属性(シリアライズしない)

        [NonSerialized] protected ScreenRect spos = null;
        [NonSerialized] protected StringFormat _sf = new StringFormat(); // テキストのフォーマットを作成

        #endregion

        #region ICloneable メンバ

        public override object Clone()
        {
            var ret = (PartsRectangle)base.Clone();
            ret._textColor = _textColor;
            ret._penColor = _penColor;
            ret._lineWidth = _lineWidth;
            ret._isVertText = _isVertText;
            ret._fontFace = _fontFace;
            ret._fontSize = _fontSize;
            ret._isAutoVertical = _isAutoVertical;
            ret.IsClipDraw = IsClipDraw;
            return ret;
        }

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PartsRectangle()
        {
            FontFace = "MS UI Gothic";
            StringAlignment = StringAlignment.Center;
            LineAlignment = StringAlignment.Center;
            StringTrimming = StringTrimming.EllipsisPath;
            IsClipDraw = true;
        }

        /// <summary>
        /// 最小フォントサイズ
        /// </summary>
        public float MinimunFontSize { get; set; }

        /// <summary>
        /// 最大フォントサイズ
        /// </summary>
        public float MaximumFontSize { get; set; }

        /// <summary>
        /// 自動縦書き設定
        /// </summary>
        public bool IsAutoVerticalText
        {
            get => _isAutoVertical;
            set => _isAutoVertical = value;
        }

        /// <summary>
        /// テキストマージン（全方向）
        /// </summary>
        public int TextMargin
        {
            get => _margin;
            set => _margin = value;
        }

        /// <summary>
        /// 自動縦書きプロパティ変更
        /// </summary>
        protected void checkAutoVert(IRichPane rp)
        {
            if (IsAutoVerticalText)
            {
                if (spos == null)
                {
                    spos = GetScRect(rp);
                }

                if (spos.Height > spos.Width)
                {
                    IsVerticalText = true;
                }
                else
                {
                    IsVerticalText = false;
                }
            }
        }

        /// <summary>
        /// 文字列の横配置
        /// </summary>
        protected StringAlignment StringAlignment { get; set; }

        /// <summary>
        /// 文字列の縦配置
        /// </summary>
        protected StringAlignment LineAlignment { get; set; }

        /// <summary>
        /// 文字列トリミング設定
        /// </summary>
        protected StringTrimming StringTrimming { get; set; }

        /// <summary>
        /// 描画
        /// </summary>
        /// <param name="rp">描画制御ハンドル</param>
        public override bool Draw(IRichPane rp)
        {
            spos = GetScRect(rp);

            if (isInClip(rp, spos) == false && IsClipDraw)  // 描画不要であれば、なにもしない
            {
                return false;
            }

            using (var pen = new Pen(_penColor, _lineWidth))
            {
                rp.Graphics.DrawRectangle(pen, spos.LT.X, spos.LT.Y, spos.Width, spos.Height); // 矩形を描画
            }
            if (rp.Zoom > XyBase.FromInt(_minFontVisibleZoom, _minFontVisibleZoom))    // 縮小率が２５％をきったらテキストの表示はしない
            {
                _sf.Alignment = StringAlignment;
                _sf.LineAlignment = LineAlignment;
                _sf.Trimming = StringTrimming;
                if (IsVerticalText)
                {
                    _sf.FormatFlags = StringFormatFlags.DirectionVertical;
                }
                using (var font = new Font(_fontFace, Math.Min(MaximumFontSize, Math.Max(MinimunFontSize, _fontSize * rp.Zoom.Y / 1000))))
                { // フォントを作成
                    var sposm = (ScreenRect)spos.Clone();
                    sposm.Deflate(_margin);
                    rp.Graphics.DrawString(Text, font, new SolidBrush(_textColor), sposm, _sf); // テキストを描画
                }
            }

            drawSelected(rp);   // 選択状態を描画
            return true;
        }


        private static readonly Pen _highlightpen2 = new Pen(Color.FromArgb(128, 255, 255, 224));
        private static readonly Pen _highlightpen3 = new Pen(Color.FromArgb(64, 255, 255, 192));

        /// <summary>
        /// 選択状態の標準実装（各Drawでコールするか、独自に選択状態を実装すること）
        /// </summary>
        /// <param name="rp">リッチペーン</param>
        protected override void drawSelected(IRichPane rp)
        {
            if (this is IPartsSelectable)
            {
                if (((IPartsSelectable)this).IsSelected)
                {
                    rp.Graphics.DrawRectangle(Pens.White, spos);
                    var rr = (ScreenRect)spos.Clone();
                    rr.Deflate(XyBase.FromInt(1, 1));
                    rp.Graphics.DrawRectangle(_highlightpen2, rr);
                    rr.Deflate(XyBase.FromInt(1, 1));
                    rp.Graphics.DrawRectangle(_highlightpen3, rr);
                }
            }
        }

        /// <summary>
        /// 縦書きの取得/設定
        /// </summary>
        public bool IsVerticalText
        {
            get => _isVertText;
            set => _isVertText = value;
        }

        /// <summary>
        /// 線の色の取得/設定
        /// </summary>
        public Color LineColor
        {
            get => _penColor;
            set => _penColor = value;
        }

        /// <summary>
        /// 線の太さの取得/設定
        /// </summary>
        public float LineWidth
        {
            get => _lineWidth;
            set => _lineWidth = value;
        }

        /// <summary>
        /// テキストの色の取得/設定
        /// </summary>
        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        /// <summary>
        /// フォントの取得/設定
        /// </summary>
        public string FontFace
        {
            get => _fontFace;
            set => _fontFace = value;
        }

        /// <summary>
        /// フォントサイズの取得/設定
        /// </summary>
        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }
    }
}
