// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// UWP user control : MenuFlyoutItem event token bridge version
    /// </summary>
    public class TMenuFlyoutItem : MenuFlyoutItem, IButtonListener
    {
        public override string ToString()
        {
            return $"{GetType().Name} Name={Name}";
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            invokeFeatures(tap: e);
            base.OnTapped(e);
        }

        protected override void OnKeyboardAcceleratorInvoked(KeyboardAcceleratorInvokedEventArgs args)
        {
            invokeFeatures(key: args);
            base.OnKeyboardAcceleratorInvoked(args);
        }

        private void invokeFeatures(TappedRoutedEventArgs tap = null, KeyboardAcceleratorInvokedEventArgs key = null)
        {
            var view = ControlUtil.FindView(this);
            view.AddToken(new EventTokenButton
            {
                Name = Name,
                Content = Text,
                TappedRoutedEventArgs = tap,
                KeyboardAcceleratorInvokedEventArgs = key,
                Sender = this,
                Remarks = $"twMenuFlyoutItem.Name = {Name} @ {(DateTime.Now.ToString("HH:mm:ss"))}",
            });
        }
    }
}
