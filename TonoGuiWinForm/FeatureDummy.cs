// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 何もしないフィーチャー
    /// </summary>
    public class FeatureDummy : FeatureBase
    {
        public override void Start(NamedId who)
        {
            base.Start(who);
            System.Diagnostics.Debug.WriteLine("Dummy feature is launched.");
        }
    }
}
