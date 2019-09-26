using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// UWP user control : Button event token bridge version
    /// </summary>
    public class TButton : Button, IButtonListener
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
                Content = Content,
                TappedRoutedEventArgs = tap,
                KeyboardAcceleratorInvokedEventArgs = key,
                Sender = this,
                Remarks = $"TButton.Name = {Name} @ {(DateTime.Now.ToString("HH:mm:ss"))}",
            });
        }
    }
}
