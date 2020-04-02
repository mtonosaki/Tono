// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ログをクリアする
    /// </summary>
    public class FeatureLogClear : FeatureBase
    {
        /// <summary>
        /// ログクリア
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            LOG.Clear();
        }
    }
}
