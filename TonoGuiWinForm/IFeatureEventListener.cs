namespace Tono.GuiWinForm
{
    /// <summary>
    /// IFeatureEventListener の概要の説明です。
    /// フィーチャークラスが持つイベントインターフェース
    /// </summary>
    public interface IFeatureEventListener
    {
        /// <summary>
        /// イベントの転送先となるフィーチャークラスのインスタンスを指定する
        /// </summary>
        /// <param name="target">フィーチャークラスのインスタンス</param>
        void LinkFeature(FeatureBase target);
    }
}
