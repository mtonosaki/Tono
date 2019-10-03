namespace Tono.GuiWinForm
{
    /// <summary>
    /// IScrollListener の概要の説明です。
    /// </summary>
    public interface IScrollListener
    {
        /// <summary>
        /// ズームの対象となるペーン
        /// </summary>
        IRichPane[] ScrollEventTargets
        {
            get;
        }
        /// <summary>
        /// ズームがあったことを示すイベント
        /// </summary>
        /// <param name="rp">イベントを受信するペーン</param>
        void ScrollChanged(IRichPane rp);
    }
}
