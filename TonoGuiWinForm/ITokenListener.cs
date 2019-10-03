#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ITokenListener の概要の説明です。
    /// </summary>
    public interface ITokenListener
    {
        /// <summary>
        /// トリガーとなるトークンのID
        /// </summary>
        NamedId TokenTriggerID
        {
            get;
        }
    }

    /// <summary>
    /// 複数トークンをサポートする
    /// </summary>
    public interface IMultiTokenListener
    {
        NamedId[] MultiTokenTriggerID
        {
            get;
        }
    }
}
