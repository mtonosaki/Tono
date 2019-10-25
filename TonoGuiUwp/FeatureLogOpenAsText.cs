// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// open log text file with URL scheme of ".txt"
    /// </summary>
    /// <example>
    /// XAML
    /// ＜tw:FeatureLogOpenAsText ListeningButtonNames="OPENLOGBUTTON" /＞　　this feature
    /// ＜tw:TMenuFlyoutItem Name="OPENLOGBUTTON" Text="Open Log" /＞　　　    feature fire UWP control
    /// </example>
    public class FeatureLogOpenAsText : FeatureBase
    {
        [EventCatch]
        public async Task OpenAsync(EventTokenButton token)
        {
            var text = LOG.GetAllLogText();
            var folder = ApplicationData.Current.TemporaryFolder;
            var file = await folder.CreateFileAsync("fcLogOpen_temp.txt", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteTextAsync(file, text);  // テンポラリファイルを消す
            await Launcher.LaunchFileAsync(file);     // URLスキームで起動
        }
    }
}
