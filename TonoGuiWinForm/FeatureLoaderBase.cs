// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public abstract class FeatureLoaderBase
    {
        /// <summary>
        /// 読込開始
        /// </summary>
        public abstract void Load(FeatureGroupRoot root, string fname);
    }
}
