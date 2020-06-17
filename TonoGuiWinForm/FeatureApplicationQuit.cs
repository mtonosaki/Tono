// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureApplicationQuit の概要の説明です。
    /// </summary>
    public class FeatureApplicationQuit : FeatureControlBridgeBase
    {
        /// <summary>
        /// 初期化
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
        /// 終了をキャンセル
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parent_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)   // シャットダウンを妨げない
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
        /// アプリ終了
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            Pane.Control.FindForm().Visible = false;
            ((DataSharingManager.Int)Share.Get("ApplicationQuitFlag", typeof(DataSharingManager.Int))).value = 1;
        }
    }
}
