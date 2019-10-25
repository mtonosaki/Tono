// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Drawing;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ツールチップの管理
    /// </summary>
    public class FeatureToolTip : FeatureBase
    {
        #region	属性(シリアライズする)
        /// <summary>ツールチップを表示するトリガ</summary>
        protected MouseState.Buttons _trigger;

        #endregion

        #region	属性(シリアライズしない)

        /// <summary>ツールチップに表示する文字列</summary>
        private readonly PartsTooltip _tt = new PartsTooltip();

        private IRichPane _rp;

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureToolTip()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = false,
                IsCtrl = false,
                IsShift = false
            };
        }

        /// <summary>
        /// 最後の位置
        /// </summary>
        protected CodeRect LastRect
        {
            get
            {
                if (_tt == null)
                {
                    return null;
                }
                return _tt.Rect;
            }
        }

        /// <summary>
        /// 背景色が指定できる
        /// </summary>
        protected Color BackColor
        {
            get => _tt.BackColor;
            set => _tt.BackColor = value;
        }

        /// <summary>
        /// テキスト色が指定できる
        /// </summary>
        protected Color TextColor
        {
            get => _tt.TextColor;
            set => _tt.TextColor = value;
        }

        /// <summary>
        /// 小アイコンの色を指定する
        /// </summary>
        public Color SquareColor
        {
            set => _tt.SquareColor = value;
        }

        /// <summary>
        /// 左上のアイコン
        /// </summary>
        public Image LtImage
        {
            set => _tt.LtImage = value;
        }

        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _rp = Pane.GetPane("Resource");
        }

        /// <summary>
        /// 表示位置が明示できる / null = マウスのカーソル付近
        /// </summary>
        public ScreenPos Position
        {
            set => _tt.Position = value;
        }

        /// <summary>
        /// レイヤーIDのオーバーロード
        /// </summary>
        protected virtual int LayerID => Const.Layer.Tooltip;

        /// <summary>
        /// 表示文字列の取得/設定
        /// </summary>
        public string Text
        {
            get => _tt.Text;
            set
            {
                if (_tt.Text != value)
                {
                    _tt.Text = value;
                    Parts.Clear(_rp, LayerID);
                    if (string.IsNullOrEmpty(_tt.Text) == false)
                    {
                        _tt.RequestPosition();
                        Parts.Add(_rp, _tt, LayerID);
                        //Parts.Invalidate(_tt, _rp);
                        Pane.Invalidate(null);
                    }
                    else
                    {
                        Pane.Invalidate(null);
                    }
                }
                else
                {
                    Pane.Invalidate(null);
                }
            }
        }
    }
}
