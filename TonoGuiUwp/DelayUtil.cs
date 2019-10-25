// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using Windows.UI.Xaml;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// one time time with DispacherTimer
    /// </summary>
    public static class DelayUtil
    {
        public static void Start(TimeSpan ts, Action act)
        {
            var timer = new DispatcherTimer
            {
                Interval = ts,
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                act();
            };
            timer.Start();
        }
    }
}
