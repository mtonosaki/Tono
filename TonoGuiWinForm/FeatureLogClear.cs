// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ���O���N���A����
    /// </summary>
    public class FeatureLogClear : FeatureBase
    {
        /// <summary>
        /// ���O�N���A
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            LOG.Clear();
        }
    }
}
