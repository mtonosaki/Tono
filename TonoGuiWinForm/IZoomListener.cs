namespace Tono.GuiWinForm
{
    /// <summary>
    /// �Y�[�������������������C�x���g�������ł���
    /// </summary>
    public interface IZoomListener
    {
        /// <summary>
        /// �Y�[���̑ΏۂƂȂ�y�[��
        /// </summary>
        IRichPane[] ZoomEventTargets
        {
            get;
        }
        /// <summary>
        /// �Y�[�������������Ƃ������C�x���g
        /// </summary>
        /// <param name="rp">�C�x���g����M����y�[��</param>
        void ZoomChanged(IRichPane rp);
    }
}
