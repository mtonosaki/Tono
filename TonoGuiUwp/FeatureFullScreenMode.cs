using System.Collections.Generic;
using Windows.System;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// toggle UWP full screen mode by F11 key or UWP control click event setting at ListeningButtonNames property
    /// </summary>
    /// <example>
    /// XAML
    /// ＜tw:FeatureFullScreenMode ListeningButtonNames="FULLMODEBUTTON,STARTBUTTON" /＞
    /// </example>
    [FeatureDescription(En = "F11 key to full screen mode", Jp = "フルスクリーン[F11]")]
    public class FeatureFullScreenMode : FeatureBase, IKeyListener
    {
        private static readonly KeyListenSetting[] _keys = new KeyListenSetting[]
        {
            new KeyListenSetting
            {
                Name = "FullScreenByKey",
                KeyStates = new[]
                {
                    (VirtualKey.F11, KeyListenSetting.States.Down),
                },
            },
        };
        public IEnumerable<KeyListenSetting> KeyListenSettings => _keys;

        /// <summary>
        /// Start from EventTokenButton(UWP control event) set at ListeningButtonNames
        /// </summary>
        [EventCatch]
        public void Start(EventTokenButton _)
        {
            OnKey(null);
        }

        /// <summary>a
        /// key event to toggle full screen mode
        /// </summary>
        /// <param name="ks"></param>
        public void OnKey(KeyEventToken kt)
        {
            if (ControlUtil.IsFullScreenMode())
            {
                ControlUtil.ExitFullScreenMode();
            }
            else
            {
                ControlUtil.SetFullScreenMode();
            }
            View.UpdateLayout();
        }
    }
}
