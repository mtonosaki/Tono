#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// IAllTokenCompleted �̊T�v�̐����ł��B
    /// ���ׂẴg�[�N���������I������Ƃ��Ɏ󂯂�C�x���g�B
    /// OnAllTokenCompleted�Ńt�@�C�i���C�U�A�g�[�N���𓊂��Ă��A��������Ȃ��B
    /// </summary>
    public interface IAllTokenCompletedListener
    {
        void OnAllTokenCompleted();
    }
}
