namespace Tono.GuiWinForm
{
    /// <summary>
    /// IScrollListener �̊T�v�̐����ł��B
    /// </summary>
    public interface IScrollListener
    {
        /// <summary>
        /// �Y�[���̑ΏۂƂȂ�y�[��
        /// </summary>
        IRichPane[] ScrollEventTargets
        {
            get;
        }
        /// <summary>
        /// �Y�[�������������Ƃ������C�x���g
        /// </summary>
        /// <param name="rp">�C�x���g����M����y�[��</param>
        void ScrollChanged(IRichPane rp);
    }
}
