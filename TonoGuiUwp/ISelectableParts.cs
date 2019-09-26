namespace Tono.Gui.Uwp
{
    /// <summary>
    /// parts interface of selectable support
    /// </summary>
    public interface ISelectableParts
    {
        /// <summary>
        /// 指定座標にパーツがあるかどうかを調べる
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="pos"></param>
        /// <returns>0=指定座標ピッタリ、0.99999=指定座標でギリギリ選択できる,  1～指定座標では対象外</returns>
        float SelectingScore(IDrawArea pane, ScreenPos pos);

        /// <summary>
        /// true=選択状態
        /// </summary>
        bool IsSelected { get; set; }
    }

    public interface IMovableParts
    {
        /// <summary>
        /// 移動前の座標を覚えておくメソッド
        /// </summary>
        void SaveLocationAsOrigin();

        /// <summary>
        /// 指定スクリーン座標の量だけ移動させる
        /// </summary>
        /// <param name="pane">対象のペーン</param>
        /// <param name="offset">SaveLocationAsOriginで記憶した位置からの移動量（スクリーン座標）</param>
        void Move(IDrawArea pane, ScreenSize offset);

        /// <summary>
        /// パーツが移動していたかどうかを調べる（SaveLocationAsOrigin時点の座標と異なるかどうか）
        /// </summary>
        /// <returns></returns>
        bool IsMoved();
    }
}
