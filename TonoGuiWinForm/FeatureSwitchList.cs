// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    public class FeatureSwitchList : FeatureBase
    {
        public override void Start(NamedId who)
        {
            var fo = new FormFeatureSwitchList();
            fo.SetFeatureRootGroup(GetRoot());
            if (fo.ShowDialog(Pane.Control) == DialogResult.OK)
            {
            }
        }
    }
}
