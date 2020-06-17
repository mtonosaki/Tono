// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeaturePartsSelect の概要の説明です。
    /// 選択・解除の条件は次のとおり
    /// １．初めて選択→それを選択
    /// ２．選択中を選択→そのまま
    /// ３．別のを選択→他を解除してそれを選択
    /// ４．Shift＋選択→追加選択・解除
    /// ５．何も無い所をクリック→すべて解除
    /// </summary>
    public class FeaturePartsSelect : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズしない）
        /// <summary>選択中のパーツ（共有変数）</summary>
        private PartsCollectionBase _selectedParts;

        /// <summary>単独クリックのボタン構成</summary>
        private readonly MouseState.Buttons _triggerSingle;
        /// <summary>追加ボタン構成</summary>
        private readonly MouseState.Buttons _triggerPlus;
        #endregion

        /// <summary>
        /// 唯一のコンストラクタ
        /// </summary>
        public FeaturePartsSelect()
        {
            _triggerSingle = new MouseState.Buttons(true, false, false, false, false);
            _triggerPlus = new MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));
            _selectedParts.SetTemporaryMode();
        }

        #region IMouseListener メンバ
        /// <summary>
        /// パーツ選択処理をマウスダウンイベントで行う
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            var parts = Parts.GetPartsAt(e.Pos, true, out var tarPane);

            if (parts != null)
            {
                if (e.Attr.Equals(_triggerPlus))
                {
                    ((IPartsSelectable)parts).IsSelected = !((IPartsSelectable)parts).IsSelected;
                    Parts.Invalidate(parts, tarPane);
                    if (((IPartsSelectable)parts).IsSelected)
                    {
                        _selectedParts.Add(tarPane, parts);
                    }
                    else
                    {
                        _selectedParts.Remove(parts);
                    }
                }
                else if (e.Attr.Equals(_triggerSingle))
                {
                    if (((IPartsSelectable)parts).IsSelected == false)
                    {
                        // すべての選択を解除（パーツ外をクリック）
                        foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                        {
                            ((IPartsSelectable)pe.Parts).IsSelected = false;
                            Parts.Invalidate(pe.Parts, pe.Pane);
                        }
                        _selectedParts.Clear();

                        // 指定パーツのみ選択状態
                        ((IPartsSelectable)parts).IsSelected = true;
                        _selectedParts.Add(tarPane, parts);
                        Parts.Invalidate(parts, tarPane);
                    }
                }
            }
            else
            {
                if (e.Attr.Equals(_triggerSingle) || e.Attr.Equals(_triggerPlus))
                {
                    // すべての選択を解除（パーツ外をクリック）
                    foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                    {
                        ((IPartsSelectable)pe.Parts).IsSelected = false;
                        Parts.Invalidate(pe.Parts, pe.Pane);
                    }
                    _selectedParts.Clear();
                }
            }
        }

        /// <summary>
        /// マウス移動イベント
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            // 何もしない
        }

        /// <summary>
        /// マウスアップイベント
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
            // 何もしない
        }

        /// <summary>
        /// マウスホイールイベント
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
            // 何もしない
        }
        #endregion
    }
}
