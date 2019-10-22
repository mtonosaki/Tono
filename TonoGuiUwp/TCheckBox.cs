// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Windows.UI.Xaml.Controls;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// UWP user control : CheckBox event token bridge version
    /// </summary>
    public class TCheckBox : CheckBox
    {
        public string TokenID { get; set; }

        public TCheckBox()
        {
            Checked += onChecked;
            Unchecked += onChecked;
        }

        private void onChecked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var view = ControlUtil.FindView(this);
            view?.AddToken(new ControlEventTokenTrigger
            {
                Sender = this,
                Name = Name,
                IsChecked = IsChecked,
                TokenID = TokenID,
                Remarks = "TCheckBox checked changed",
            });
        }
    }
}
