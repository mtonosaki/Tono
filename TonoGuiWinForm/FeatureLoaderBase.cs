#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public abstract class FeatureLoaderBase
    {
        /// <summary>
        /// �Ǎ��J�n
        /// </summary>
        public abstract void Load(FeatureGroupRoot root, string fname);
    }
}