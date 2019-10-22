// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using Windows.UI.Xaml;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// interval timer utility
    /// </summary>
    public static class IntervalUtil
    {
        public static void Start(TimeSpan span, Action func)
        {
            var timer = new DispatcherTimer
            {
                Interval = span,
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                func();
                timer.Interval = span;
                timer.Start();
            };
            timer.Start();
        }
    }
}
