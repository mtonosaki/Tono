// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IReadonlyable ‚ÌŠT—v‚Ìà–¾‚Å‚·B
    /// </summary>
    public interface IReadonlyable
    {
        bool IsReadonly { get; }
        void SetReadonly();
    }
}
