// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �������Ȃ��t�B�[�`���[
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
