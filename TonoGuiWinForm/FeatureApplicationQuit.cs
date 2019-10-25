// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureApplicationQuit �̊T�v�̐����ł��B
    /// </summary>
    public class FeatureApplicationQuit : FeatureControlBridgeBase
    {
        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();
            var parent = GetParentForm();
            if (parent != null)
            {
                parent.FormClosing += parent_FormClosing;
            }
        }

        /// <summary>
        /// �I�����L�����Z��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parent_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)   // �V���b�g�_�E����W���Ȃ�
            {
                if (Data.IsModified)
                {
                    var mes = Mes.Current["FeatureApplicationQuit", "ConfirmQuit"];
                    var cap = Mes.Current["FeatureApplicationQuit", "ConfirmQuitCaption"];
                    if (MessageBox.Show(mes, cap, MessageBoxButtons.OKCancel) != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        /// <summary>
        /// �A�v���I��
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            Pane.Control.FindForm().Visible = false;
            ((DataSharingManager.Int)Share.Get("ApplicationQuitFlag", typeof(DataSharingManager.Int))).value = 1;
        }
    }
}
