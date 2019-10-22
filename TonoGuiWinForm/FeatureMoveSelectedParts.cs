// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �I������Ă���p�[�c���ړ�������
    /// </summary>
    public class FeatureMoveSelectedParts : FeatureBase, IMouseListener, IMultiTokenListener
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�p�[�c�ړ��̃g���K�i�}�E�X���{�^���j</summary>
        protected MouseState.Buttons _trigger;
        /// <summary>�p�[�c�̘A�g�ړ��̃g���K�i�}�E�X���{�^��+Shift�L�[�j</summary>
        protected MouseState.Buttons _FollowTrigger;

        /// <summary>�I�𒆂̃p�[�c�i���L�ϐ��j</summary>
        protected PartsCollectionBase _selectedParts;

        /// <summary>�I�𒆂̃p�[�c�Ƃ����Ӗ��ŃV���A���C�Y����ID</summary>
        protected NamedId _meansSelectedParts = NamedId.FromName("FeatureDataSerializeID");

        /// <summary>�p�[�c�ʒu�Ǘ��I�u�W�F�N�g</summary>
        protected PartsPositionManager _pos;

        /// <summary>�}�E�X�_�E�����̈ʒu���L������Bnull = �܂��h���b�O�J�n���Ă��Ȃ���������</summary>
        protected ScreenPos _mouseDownOriginal = null;

        /// <summary>�p�[�c�ɑ΂��鑀��̃��[�h</summary>
        protected PartsPositionManager.DevelopType _developmentMode = PartsPositionManager.DevelopType.Move;

        /// <summary>
        /// �N���b�N�����ꏊ�̋L��
        /// </summary>
        protected MouseState _clickPos;

        /// <summary>�}�E�X�{�^�����������ۂɕK�v�ȏ������J�n����g�[�N��</summary>
        protected NamedId[] _tokens = new NamedId[] { NamedId.FromName("MouseDownJob"), NamedId.FromName("MouseMoveJob") };

        /// <summary>�}�E�X�A�b�v���������ۂɕK�v�ȏ������J�n����g�[�N��</summary>
        protected NamedId _tokenMouseUpJob = NamedId.FromName("MoveMouseUpJob");

        #endregion

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public FeatureMoveSelectedParts()
        {
            // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
            _trigger = new MouseState.Buttons(true, false, false, false, false);
            _FollowTrigger = new MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// �������i���L�ϐ��̊����Ȃǁj
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // �X�e�[�^�X����
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));   // �I���ς݂̃p�[�c�ꗗ
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // �ړ����̃p�[�c�ꗗ
            _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));       // �ړ����̃p�[�c�ꗗ
        }

        /// <summary>
        /// �p�[�c�ړ�
        /// </summary>
        private void onFinalizeMoveParts()
        {
            _pos.SetNowPositionsToParts(Parts);
        }

        /// <summary>
        /// �}�E�X�_�E�������̍ŏI����
        /// </summary>
        protected virtual void _finalMouseDownPart()
        {
            var e = _clickPos;

            if (_selectedParts.Count > 0)
            {
                var tarPane = ClickPane;
                var parts = ClickParts; // Parts.GetPartsAt(e.Pos, true, out tarPane);
                if (parts != null)
                {
                    if (parts.IsOn(e.Pos, tarPane) == PartsBase.PointType.Inside)
                    {
                        _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.Move);
                        _mouseDownOriginal = e.Pos;
                        _pos.Initialize(_selectedParts);
                        Token.Add(TokenGeneral.TokenMouseDownNormalize, this);
                    }
                }
            }
        }

        /// <summary>
        /// �}�E�X�_�E�����̏���������
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            // MouseDown�g�[�N��
            if (who.Equals(_tokens[0]))
            {
                Finalizers.Add(new FinalizeManager.Finalize(_finalMouseDownPart));  // SezeSelected�E�E�E���p�����Ă���̂ŁAID�s�w��
                                                                                    //			Finalizers.Add(_tokenListenID, new ffFinalizer.Finalize(_finalMouseDownPart)); by Tono ������ 2006.2.2
            }
            // MouseMove�g�[�N��
            if (who.Equals(_tokens[1]))
            {
                OnMouseMove(null);
            }
        }


        #region IMouseListener �����o
        /// <summary>
        /// �}�E�X�_�E���C�x���g���h���b�O�J�n�g���K�Ƃ��Ď�������
        /// </summary>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                Start(_tokens[0]);
            }
        }

        /// <summary>
        /// �}�E�X�ړ����h���b�O���Ƃ��Ď�������
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_mouseDownOriginal != null)
            {
                // UNDO�p�̃f�[�^���L�^����
                if (Persister[UNDO].IsStartedChunk == false)
                {
                    Persister[REDO].StartChunk(GetType().Name + ".OnMouseMove");
                    Persister[UNDO].StartChunk(GetType().Name + ".OnMouseMove");

                    Persister[UNDO].Save(_selectedParts, _meansSelectedParts);
                    foreach (DictionaryEntry de in _pos)
                    {
                        var parts = (PartsBase)de.Key;

                        // �p�[�c��UNDO�ۑ�
                        Persister[UNDO].Save(parts, _meansSelectedParts);   // parts�̃t�����h�ۑ��́AFeatureDposeCheckOverlapped�ōs��

                        // ���R�[�h��UNDO�ۑ�
                        foreach (RecordBase rec in Link.GetRecordset(parts))
                        {
                            Persister[UNDO].Save(rec, _meansSelectedParts);
                        }
                    }
                }

                // ���ׂĂ̑I���p�[�c�̍��W�ύX�\�������
                if (e != null)
                {
                    _pos.Develop(_mouseDownOriginal, e.Pos, _developmentMode);
                }

                // ���ׂẴt�B�[�`���[���s��Ɉړ�������
                Finalizers.Add(new FinalizeManager.Finalize(onFinalizeMoveParts));
            }
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g���h���b�O�I���g���K�Ƃ��Ď�������
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
            Finalizers.Add(new FinalizeManager.Finalize(OnFinalizeMouseUpJob));
        }

        /// <summary>
        /// �}�E�X�A�b�v�̍ŏI����
        /// </summary>
        protected virtual void OnFinalizeMouseUpJob()
        {
            if (_mouseDownOriginal != null)
            {
                var movedCount = 0;

                // �ړ��p�[�c���f�[�^�ɔ��f������
                foreach (DictionaryEntry de in _pos)
                {
                    var parts = (PartsBase)de.Key;
                    var pos = (PartsPositionManager.Pos3)de.Value;
                    if (pos.Now.LT.Equals(pos.Org.LT) == false || pos.Now.RB.X != pos.Org.RB.X)
                    {
                        if (movedCount == 0)
                        {
                            Persister[REDO].Save(_selectedParts, _meansSelectedParts);
                        }
                        // �f�[�^�X�V
                        Link.Equalization(parts, false);    // Data�`Parts�̃f�[�^�A��

                        // REDO�i�����i�p�[�c�j
                        //savePartsWithFriend(Persister[REDO], parts, new Hashtable());

                        // REDO�i�����i���R�[�h�j
                        foreach (RecordBase rb in Link.GetRecordset(parts))
                        {
                            Persister[REDO].Save(rb, _meansSelectedParts);
                        }
                        movedCount++;
                    }
                }

                // �ړ��I��
                _mouseDownOriginal = null;
                _pos.Clear();

                Persister[REDO].EndChunk(); // REDO���ɍs�����Ƃ͏d�v
                Persister[UNDO].EndChunk();
                Pane.Invalidate(Pane.GetPaneRect());
            }
        }

        public void OnMouseWheel(MouseState e)
        {
        }
        #endregion
        #region IMultiTokenListener �����o

        public NamedId[] MultiTokenTriggerID => _tokens;

        #endregion
    }
}
