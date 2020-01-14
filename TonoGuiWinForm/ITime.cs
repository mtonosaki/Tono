// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITime
    {
        int Hour { get; }
        int Minute { get; }
        int Second { get; }
        int Day { get; }
        int TotalSeconds { get; set; }
        int TotalMinutes { get; set; }
        double TotalDays { get; set; }
    }
}
