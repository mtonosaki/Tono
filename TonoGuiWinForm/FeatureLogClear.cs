// Copyright (c) Manabu Tonosaki All rights reserved.
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
