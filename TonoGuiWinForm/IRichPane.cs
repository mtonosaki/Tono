// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �y�[���ւ̃A�N�Z�X��񋟂���C���^�[�t�F�[�X
    /// </summary>
    public interface IRichPane
    {
        /// <summary>
        /// ID�e�L�X�g
        /// </summary>
        string IdText { get; set; }

        /// <summary>
        /// IRichPane�̎��̂̃R���g���[���^
        /// </summary>
        System.Windows.Forms.Control Control
        {
            get;
        }

        /// <summary>
        /// �e�y�[����Ԃ�
        /// </summary>
        /// <returns>null = �e�͂��Ȃ�</returns>
        IRichPane GetParent();

        /// <summary>
        /// ���O�Ńy�[������������i�x���̂Œ��Ӂj
        /// </summary>
        /// <param name="name">��������y�[����Name�v���p�e�B</param>
        /// <returns>���������y�[�� / null = ������Ȃ�����</returns>
        IRichPane GetPane(string name);

        /// <summary>
        /// �y�[���̗̈��Ԃ��C���^�[�t�F�[�X
        /// </summary>
        /// <returns>�̈�</returns>
        ScreenRect GetPaneRect();

        /// <summary>
        /// �`�悪�K�v�ȗ̈��Ԃ��C���^�[�t�F�[�X
        /// </summary>
        /// <returns>�̈�</returns>
        ScreenRect GetPaintClipRect();

        /// <summary>
        /// ��ʂ��ĕ`�悷��
        /// </summary>
        /// <param name="rect">�ĕ`��X�N���[����Έʒu�i�y�[�����΍��W�łȂ��j / null=�S�̈�</param>
        void Invalidate(ScreenRect rect);

        /// <summary>
        /// �Y�[���{����Ԃ��C���^�[�t�F�[�X
        ///�@�~10[%]�̒l���i�[����Ă���
        /// </summary>
        XyBase Zoom
        {
            get;
            set;
        }

        /// <summary>
        /// �X�N���[���ʂ�Ԃ��C���^�[�t�F�[�X
        /// �v���X�����́A��ʂ̉E��
        /// </summary>
        ScreenPos Scroll
        {
            get;
            set;
        }

        /// <summary>
        /// �O���t�B�b�N�I�u�W�F�N�g
        /// </summary>
        System.Drawing.Graphics Graphics
        {
            get;
        }

        /// <summary>
        /// �p�[�c���W����X�N���[���i�}�E�X�j���W�ɕϊ�����
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenPos Convert(LayoutPos value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenRect Convert(LayoutRect value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        ScreenPos GetZoomed(LayoutPos value);

        /// <summary>
        /// �X�N���[���i�}�E�X�j���W����p�[�c���W�ɕϊ�����
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutPos Convert(ScreenPos value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutRect Convert(ScreenRect value);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        LayoutPos GetZoomed(ScreenPos value);
    }
}
