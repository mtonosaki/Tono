// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �t�B�[�`���[�̑��x��]������\���쐬
    /// </summary>
    public class FeatureFeatureTimeKeeper : FeatureControlBridgeBase
    {
        private FormFeatureTimeKeeper _fo = null;

        /// <summary>
        /// ���j���[�N���ł��邩�`�F�b�N
        /// </summary>
        public override bool CanStart => _fo == null;

        /// <summary>
        /// �X�^�[�g
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
        /// �t�H�[�����N���[�Y������A���ɊJ����悤�ɏ�������
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
