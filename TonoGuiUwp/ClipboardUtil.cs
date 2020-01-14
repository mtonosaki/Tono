// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Windows.ApplicationModel.DataTransfer;

namespace Tono.Gui.Uwp
{

    /// <summary>
    /// Clipboard utility (Windows10)
    /// </summary>
    public class ClipboardUtil
    {
        public static readonly ClipboardUtil Current = new ClipboardUtil();

        private readonly DataPackage _dp = new DataPackage();
        public void Set(string text)
        {
            _dp.SetText(text);
            Clipboard.SetContent(_dp);
        }
    }
}
