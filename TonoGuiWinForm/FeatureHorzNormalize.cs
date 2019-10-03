using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �I�u�W�F�N�g�̉��ړ��̐������T�|�[�g���܂��B
    /// </summary>
    public class FeatureHorzNormalize : Tono.GuiWinForm.FeatureBase, IMouseListener, ITokenListener
    {
        #region	����(�V���A���C�Y����)
        /// <summary>�ړ��������Ď�����g���K</summary>
        protected MouseState _trigger = new MouseState();
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�I�𒆂̃p�[�c</summary>
        protected PartsBase _SelectedParts = null;
        /// <summary>P3�I�u�W�F�N�g�˃p�[�c���X�g�̃����N</summary>
        protected Hashtable _P3toPartsList = new Hashtable();
        /// <summary>�p�[�c��P3�I�u�W�F�N�g�̃����N</summary>
        protected IDictionary<PartsBase, PartsPositionManager.Pos3> _PartsToP3 = new Dictionary<PartsBase, PartsPositionManager.Pos3>();
        /// <summary>�p�[�c�ʒu�Ǘ��I�u�W�F�N�g</summary>
        protected PartsPositionManager _pos;
        /// <summary>�A�g�ړ����[�h��ON(1)/OFF(0)</summary>
        protected DataSharingManager.Int _FollowMoveMode = null;
        /// <summary>�}�E�X�̏�Ԃ����L�ϐ��Ɠ���</summary>
        protected MouseState _clickPos;
        /// <summary>�}�E�X�{�^�����������ۂɕK�v�ȏ������J�n����g�[�N��</summary>
        protected NamedId _tokenMouseDownJob = NamedId.FromName("MouseDownJob");
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public FeatureHorzNormalize()
        {
        }

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            // �X�e�[�^�X����
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // �ړ����̃p�[�c�ꗗ
            _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));  // �ړ����̃p�[�c�ꗗ
        }

        /// <summary>
        ///  �g�[�N���ɂ��N���C�x���g
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            Finalizers.Add(new FinalizeManager.Finalize(FinalMouseDownJob));
        }

        /// <summary>
        /// �}�E�X�_�E�������̍ŏI����
        /// </summary>
        protected virtual void FinalMouseDownJob()
        {
        }

        /// <summary>
        /// �}�E�X�_�E�������̍ŏI����
        /// </summary>
        protected virtual void FinalMouseMoveJob()
        {
        }

        #region IMouseListener �����o
        /// <summary>
        /// �}�E�X���[�u
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
        }
        /// <summary>
        /// �}�E�X�_�E��
        /// </summary>
        public virtual void OnMouseDown(MouseState e)
        {
        }
        /// <summary>
        /// �}�E�X�A�b�v
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
        }
        /// <summary>
        /// �}�E�X�z�C�[��
        /// </summary>
        public virtual void OnMouseWheel(MouseState e)
        {
        }
        #endregion
        #region ITokenListener �����o
        public virtual NamedId TokenTriggerID => TokenGeneral.TokenMouseDownNormalize;
        #endregion
    }
}
