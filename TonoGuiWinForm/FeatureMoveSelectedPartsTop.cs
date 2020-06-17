// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 選択したパーツを表示順序、最先頭にする
    /// </summary>
    public class FeatureMoveSelectedPartsTop : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズしない）

        /// <summary>単独クリックのボタン構成</summary>
        private readonly MouseState.Buttons _triggerSingle;
        /// <summary>追加ボタン構成</summary>
        private readonly MouseState.Buttons _triggerPlus;

        #endregion

        public FeatureMoveSelectedPartsTop()
        {
            _triggerSingle = new Tono.GuiWinForm.MouseState.Buttons(true, false, false, false, false);
            _triggerPlus = new Tono.GuiWinForm.MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
        }

        /// <summary>
        /// パーツ選択処理をマウスダウンイベントで行う
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_triggerPlus) || e.Attr.Equals(_triggerSingle))
            {

                var parts = ClickParts; // Parts.GetPartsAt(e.Pos, true, out tarPane);
                if (parts != null)
                {
                    ((PartsCollection)Parts).MovePartsZOrderToTop(parts);
                }
            }
        }

        /// <summary>
        /// マウス移動イベントは不要
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
        }

        /// <summary>
        /// マウスアップイベントは不要
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }
    }
}
