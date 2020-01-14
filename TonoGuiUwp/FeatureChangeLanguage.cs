// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// change UWP control captions
    /// </summary>
    /// <example>
    /// XAML
    /// ＜tw:FeatureChangeLanguage LanguageCode="jp" LinsteningButtonNames="LANGBUTTON" IsMute="true" /＞
    /// </example>
    [FeatureDescription(En = "Change display language", Jp = "表示言語切替")]
    public class FeatureChangeLanguage : FeatureBase
    {
        /// <summary>
        /// true = no log mode
        /// </summary>
        public bool IsMute { get; set; } = false;

        /// <summary>
        /// current language code "en" / "jp"
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        /// change language event that will be called from UWP control click setting at ListeningButtonName property
        /// </summary>
        /// <param name="token"></param>
        [EventCatch]
        public void Start(EventTokenButton token)
        {
            Mes.ChangeLanguage(LanguageCode);
            if (IsMute == false)
            {
                LOG.AddMes(LLV.WAR, "RestartForLanguage").Solo();
            }
            Redraw();
        }
    }
}
