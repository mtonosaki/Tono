// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeaturePartsSelect �̊T�v�̐����ł��B
    /// �I���E�����̏����͎��̂Ƃ���
    /// �P�D���߂đI���������I��
    /// �Q�D�I�𒆂�I�������̂܂�
    /// �R�D�ʂ̂�I���������������Ă����I��
    /// �S�DShift�{�I�����ǉ��I���E����
    /// �T�D�������������N���b�N�����ׂĉ���
    /// </summary>
    public class FeaturePartsSelect : FeatureBase, IMouseListener
    {
        #region �����i�V���A���C�Y���Ȃ��j
        /// <summary>�I�𒆂̃p�[�c�i���L�ϐ��j</summary>
        private PartsCollectionBase _selectedParts;

        /// <summary>�P�ƃN���b�N�̃{�^���\��</summary>
        private readonly MouseState.Buttons _triggerSingle;
        /// <summary>�ǉ��{�^���\��</summary>
        private readonly MouseState.Buttons _triggerPlus;
        #endregion

        /// <summary>
        /// �B��̃R���X�g���N�^
        /// </summary>
        public FeaturePartsSelect()
        {
            _triggerSingle = new MouseState.Buttons(true, false, false, false, false);
            _triggerPlus = new MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// ����������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // �X�e�[�^�X����
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));
            _selectedParts.SetTemporaryMode();
        }

        #region IMouseListener �����o
        /// <summary>
        /// �p�[�c�I���������}�E�X�_�E���C�x���g�ōs��
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            var parts = Parts.GetPartsAt(e.Pos, true, out var tarPane);

            if (parts != null)
            {
                if (e.Attr.Equals(_triggerPlus))
                {
                    ((IPartsSelectable)parts).IsSelected = !((IPartsSelectable)parts).IsSelected;
                    Parts.Invalidate(parts, tarPane);
                    if (((IPartsSelectable)parts).IsSelected)
                    {
                        _selectedParts.Add(tarPane, parts);
                    }
                    else
                    {
                        _selectedParts.Remove(parts);
                    }
                }
                else if (e.Attr.Equals(_triggerSingle))
                {
                    if (((IPartsSelectable)parts).IsSelected == false)
                    {
                        // ���ׂĂ̑I���������i�p�[�c�O���N���b�N�j
                        foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                        {
                            ((IPartsSelectable)pe.Parts).IsSelected = false;
                            Parts.Invalidate(pe.Parts, pe.Pane);
                        }
                        _selectedParts.Clear();

                        // �w��p�[�c�̂ݑI�����
                        ((IPartsSelectable)parts).IsSelected = true;
                        _selectedParts.Add(tarPane, parts);
                        Parts.Invalidate(parts, tarPane);
                    }
                }
            }
            else
            {
                if (e.Attr.Equals(_triggerSingle) || e.Attr.Equals(_triggerPlus))
                {
                    // ���ׂĂ̑I���������i�p�[�c�O���N���b�N�j
                    foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                    {
                        ((IPartsSelectable)pe.Parts).IsSelected = false;
                        Parts.Invalidate(pe.Parts, pe.Pane);
                    }
                    _selectedParts.Clear();
                }
            }
        }

        /// <summary>
        /// �}�E�X�ړ��C�x���g
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            // �������Ȃ�
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
            // �������Ȃ�
        }

        /// <summary>
        /// �}�E�X�z�C�[���C�x���g
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
            // �������Ȃ�
        }
        #endregion
    }
}
