// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ITimeSpan ‚ÌŠT—v‚Ìà–¾‚Å‚·B
    /// </summary>
    public interface ITimeSpan
    {
        DateTimeEx Start { get; set; }
        DateTimeEx End { get; set; }
        DateTimeEx Span { get; }
    }
}
