#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IReadonlyable �̊T�v�̐����ł��B
    /// </summary>
    public interface IReadonlyable
    {
        bool IsReadonly { get; }
        void SetReadonly();
    }
}
