// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Diagnostics;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tono.Gui.Uwp
{
    public class TTextValidate : TextBox
    {
        /// <summary>
        /// Regex pattern for validation
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Error message text block object
        /// </summary>
        public TextBlock ErrorMessageText { get; set; }

        public TTextValidate()
        {
            TextChanged += OnTextChanged;
        }

        private Regex pattern = null;

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorMessageText.Visibility = IsValid ? Visibility.Collapsed : Visibility.Visible;
        }

        public bool IsValid
        {
            get
            {
                if (ErrorMessageText == null || string.IsNullOrEmpty(Pattern))
                {
                    return true;
                }
                if (pattern == null)
                {
                    pattern = new Regex(Pattern);
                }
                return pattern.IsMatch(this.Text);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name} Name={Name}";
        }
    }
}
