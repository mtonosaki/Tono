namespace Tono.GuiWinForm
{
    /// <summary>
    /// IFeatureEventListener �̊T�v�̐����ł��B
    /// �t�B�[�`���[�N���X�����C�x���g�C���^�[�t�F�[�X
    /// </summary>
    public interface IFeatureEventListener
    {
        /// <summary>
        /// �C�x���g�̓]����ƂȂ�t�B�[�`���[�N���X�̃C���X�^���X���w�肷��
        /// </summary>
        /// <param name="target">�t�B�[�`���[�N���X�̃C���X�^���X</param>
        void LinkFeature(FeatureBase target);
    }
}
