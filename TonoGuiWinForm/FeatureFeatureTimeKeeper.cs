// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// フィーチャーの速度を評価する表示作成
    /// </summary>
    public class FeatureFeatureTimeKeeper : FeatureControlBridgeBase
    {
        private FormFeatureTimeKeeper _fo = null;

        /// <summary>
        /// メニュー起動できるかチェック
        /// </summary>
        public override bool CanStart => _fo == null;

        /// <summary>
        /// スタート
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            base.Start(who);
            _fo = new FormFeatureTimeKeeper((FeatureGroupRoot)GetRoot());
            Mes.Current.ResetText(_fo);

            _fo.FormClosed += new System.Windows.Forms.FormClosedEventHandler(_fo_FormClosed);
            _fo.Show(Pane.Control);
        }

        /// <summary>
        /// フォームがクローズしたら、次に開けるように準備する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _fo_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            _fo.Dispose();
            _fo = null;
        }
    }
}
