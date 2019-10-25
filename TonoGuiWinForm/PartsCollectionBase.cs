// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{

    /// <summary>
    /// daPartsBase �̊T�v�̐����ł��B
    /// �f�[�^�Ǘ��̊�{�N���X
    /// </summary>
    public abstract class PartsCollectionBase : ICloneable
    {
        #region �񋓗p�N���X
        /// <summary>
        /// �p�[�c��񋓂���ۂ�Current�f�[�^�ɂȂ�^
        /// </summary>
        public struct PartsEntry
        {
            /// <summary>�p�[�c</summary>
            public PartsBase Parts;

            /// <summary>���̃p�[�c���o�^����Ă���y�[��</summary>
            public IRichPane Pane;

            /// <summary>���̃p�[�c�������郌�C���[���x��</summary>
            public int LayerLevel;

            /// <summary>
            /// �������R���X�g���N�^
            /// </summary>
            /// <param name="parts">�p�[�c</param>
            /// <param name="pane">�y�[��</param>
            /// <param name="layerLevel">���x���l</param>
            public PartsEntry(PartsBase parts, IRichPane pane, int layerLevel)
            {
                Parts = parts;
                Pane = pane;
                LayerLevel = layerLevel;
            }

            public override bool Equals(object obj)
            {
                if (obj is PartsEntry pe)
                {
                    return Parts.Equals(pe.Parts) && Pane == pe.Pane && LayerLevel == pe.LayerLevel;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator ==(PartsEntry a, PartsEntry b) => a.Equals(b);
            public static bool operator !=(PartsEntry a, PartsEntry b) => !a.Equals(b);
            public override int GetHashCode()
            {
                return Parts.GetHashCode();
            }

            #region Data tips for debugging
#if DEBUG
            /// <summary>
            /// 
            /// </summary>
            public string _
            {
                get
                {
                    var s = "";
                    if (Parts != null)
                    {
                        s = "Parts=" + s.ToString() + "  ";
                    }
                    if (Pane != null)
                    {
                        s += "Pane = " + Pane.IdText;
                    }
                    return s;
                }
            }
#endif
            #endregion
        }

        /// <summary>
        /// �p�[�c�񋓗pIEnumerator
        /// </summary>
        public interface IPartsEnumerator : IEnumerator
        {
            /// <summary>�p�[�c</summary>
            PartsBase Parts { get; }

            /// <summary>���̃p�[�c���o�^����Ă���y�[��</summary>
            IRichPane Pane { get; }
        }
        #endregion

        #region	����(�V���A���C�Y����)
        /// <summary>
        /// �N���b�N�s���n��
        /// </summary>
        private List<ScreenRect> _skipzones = new List<ScreenRect>();
        #endregion

        #region �����i�V���A���C�Y���Ȃ��j
        /// <summary>�t�B�[�`���[�T�C�N�����ō폜���ꂽ�p�[�c�̈ꗗ</summary>
        [NonSerialized]
        private IList _removedParts;
        /// <summary>�t�B�[�`���[�T�C�N�����Œǉ����ꂽ�p�[�c�̈ꗗ</summary>
        [NonSerialized]
        private IList _addedParts;
        #endregion

        /// <summary>
        /// Clone�Ȃǂŗp����
        /// </summary>
        /// <param name="dst"></param>
        protected void copyBasePropertyTo(PartsCollectionBase dst)
        {
            if (_removedParts is ListDummy)
            {
                dst._removedParts = new ListDummy();
                dst._addedParts = new ListDummy();
            }
            else
            {
                dst._removedParts = new ArrayList(_removedParts);
                dst._addedParts = new ArrayList(_addedParts);
            }
            dst._skipzones = new List<ScreenRect>(_skipzones);
        }

        protected PartsCollectionBase()
        {
            _removedParts = new ArrayList();
            _addedParts = new ArrayList();
        }

        /// <summary>
        /// �o�^�E�폜�̃C�x���g�����p�̑�������Ȃ��y�ʐݒ�
        /// </summary>
        public void SetTemporaryMode()
        {
            _removedParts = new ListDummy();
            _addedParts = new ListDummy();
        }

        /// <summary>
        /// �`�揈�����s�킹��iPaint�C�x���g����R�[�������̂ŁA���[�U�[�͎��s�֎~
        /// </summary>
        public virtual void ProvideDrawFunction()
        {
        }

        /// <summary>
        /// �s���n��
        /// </summary>
        /// <param name="zone"></param>
        /// <returns></returns>
        public void AddSkipZone(ScreenRect zone)
        {
            _skipzones.Add(zone);
        }

        /// <summary>
        /// �w��ʒu���X�L�b�v�]�[�����ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected bool isInSkipzone(ScreenPos pos)
        {
            foreach (var r in _skipzones)
            {
                if (r.IsIn(pos))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// �w�肵����̃p�[�c�̏d�Ȃ蔻�������
        /// </summary>
        /// <param name="pane1">�p�[�c1�ʒu���v�Z����y�[��</param>
        /// <param name="parts1">�p�[�c1</param>
        /// <param name="pane2">�p�[�c2�ʒu���v�Z����y�[��</param>
        /// <param name="parts2">�p�[�c2</param>
        /// <param name="isIllusionCheck">true = �C�����[�W�������l������</param>
        /// <returns>true = �d�Ȃ��Ă��� / false = �d�Ȃ��Ă��Ȃ�</returns>
        public virtual bool IsOverlapped(IRichPane pane1, PartsBase parts1, IRichPane pane2, PartsBase parts2, bool isIllusionCheck)
        {
            return false;
        }

        private static Mes _prevMessage = null;

        /// <summary>
        /// uMes�̏�Ԃ̕ω������o������A�S�p�[�c��TextFormat�ɂ��Text���X�V����
        /// </summary>
        public virtual void CheckAndResetLocalized()
        {
            if (object.ReferenceEquals(_prevMessage, Mes.Current) == false)
            {
                _prevMessage = Mes.Current;
                foreach (PartsEntry pe in this)
                {
                    pe.Parts.ResetTextByFormat();
                }
            }
        }

        /// <summary>
        /// �w��ID�i�ʏ��uRowKey�j���w�肵�āAdpBase.LT.Y == pos.ID �̍s�����ׂĂ�߂�
        /// </summary>
        /// <param name="pos">�����L�[</param>
        /// <returns>�w��L�[�ɍ��v����p�[�c�Q</returns>
        public virtual IList<PartsBase> GetPartsByLocationID(Id pos)
        {
            return null;
        }

        /// <summary>
        /// �y�[�����w�肵�āA�w��ID�i�ʏ��uRowKey�j���w�肵�āA
        /// dpBase.LT.Y == pos.ID �̍s�̎w��y�[���̕`��̈���ɂ���p�[�c��߂�
        /// </summary>
        /// <param name="rp">�`�悷��y�[��</param>
        /// <param name="pos">�����L�[</param>
        /// <returns>�w��L�[�ɍ��v����p�[�c�Q</returns>
        public virtual IList<PartsBase> GetPartsByLocationID(IRichPane rp, Id pos)
        {
            return null;
        }


        /// <summary>
        /// �w��y�[���ɂ���w��^�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="dpType">�^</param>
        /// <returns>�p�[�c�̃C���X�^���X�̎Q�� / null = ������Ȃ�����</returns>
        public abstract PartsBase GetSample(IRichPane rp, Type dpType);

        /// <summary>
        /// �w��y�[���ɂ���w��^�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <returns>�p�[�c�̃C���X�^���X�̎Q�� / null = ������Ȃ�����</returns>
        public abstract PartsBase GetSample();

        /// <summary>
        /// �p�[�c��ǉ�����i�ŉ����C���[�ɒǉ������j
        /// </summary>
        /// <param name="target">��y�[��</param>
        /// <param name="value">�ǉ�����p�[�c</param>
        public void Add(IRichPane target, PartsBase value)
        {
            Add(target, value, 0);
        }
        /// <summary>
        /// �p�[�c��ǉ�����
        /// </summary>
        /// <param name="target">��y�[��</param>
        /// <param name="value">�ǉ�����p�[�c</param>
        /// <param name="layerLevel">�p�[�c�̃��C���[�i�O���ŉ��j</param>
        public virtual void Add(IRichPane target, PartsBase value, int layerLevel)
        {
            _addedParts.Add(new PartsCollectionBase.PartsEntry(value, target, layerLevel));
        }

        /// <summary>
        /// �p�[�c��ǉ�����
        /// </summary>
        /// <param name="value">�ǉ�����p�[�c</param>
        public void Add(PartsEntry value)
        {
            Add(value.Pane, value.Parts, value.LayerLevel);
        }

        /// <summary>
        /// �w��ID�̃p�[�c���擾����
        /// </summary>
        /// <param name="partsID"></param>
        /// <returns>null = �p�[�c��������Ȃ�����</returns>
        public virtual PartsBase GetParts(Id partsID)
        {
            foreach (PartsEntry pe in this)
            {
                if (pe.Parts.ID == partsID)
                {
                    return pe.Parts;
                }
            }
            return null;
        }

        /// <summary>
        /// �w��ID�̃p�[�c�����݂��邩���ׂ�
        /// �i�S�p�[�c�X�L�����j
        /// </summary>
        /// <param name="partsID"></param>
        /// <returns></returns>
        public bool Contains(Id partsID)
        {
            return GetParts(partsID) == null ? false : true;
        }

        /// <summary>
        /// �w��p�[�c���ĕ`��v������
        /// </summary>
        /// <param name="parts">�ĕ`�悷��p�[�c�̗̈�</param>
        /// <param name="rp">�w��p�[�c�ɑ������b�`�y�[��</param>
        public virtual void Invalidate(PartsBase parts, IRichPane rp)
        {
            var r = ((ScreenRect)parts.GetScRect(rp, parts.Rect).GetPpSize()) & rp.GetPaneRect();
            rp.Invalidate(r);
        }

        /// <summary>
        /// ���ׂĂ̓o�^���폜����
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// ���ׂĂ̓o�^���폜����
        /// </summary>
        public abstract void Clear(IRichPane targetPane);

        /// <summary>
        /// ���ׂĂ̓o�^���폜����
        /// </summary>
        public abstract void Clear(IRichPane targetPane, int layerLevel);

        /// <summary>
        /// �w�肵���^�̃p�[�c�����ׂč폜����i�������Ȃ���NotSupportException�j
        /// </summary>
        /// <param name="type"></param>
        /// <returns>��������</returns>
        public virtual int Clear(Type type)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// �w��}�E�X���W�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="pos">���W</param>
        /// <param name="isSelectableOnly"></param>
        /// <returns>�擾�ł����p�[�c / null=�Ȃ�</returns>
        public PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly)
        {
            return GetPartsAt(pos, isSelectableOnly, out var rp);
        }

        /// <summary>
        /// �w��}�E�X���W�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="pos">���W</param>
        /// <param name="rp">�p�[�c��������y�[��</param>
        /// <param name="isSelectableOnly"></param>
        /// <returns>�擾�ł����p�[�c / null=�Ȃ�</returns>
        public virtual PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly, out IRichPane rp)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// �w��̈���̃p�[�c����������
        /// </summary>
        /// <param name="pos">���W</param>
        /// <param name="rp">�����y�[��</param>
        /// <param name="layer">�������C���[</param>
        /// <param name="isSelectableOnly">�I���\�ȃp�[�c�̂݌���</param>
        /// <returns>�擾�ł����p�[�c / null=�Ȃ�</returns>
        public virtual PartsBase GetPartsAt(ScreenPos pos, IRichPane rp, int layer, bool isSelectableOnly)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// �w��p�[�c�Əd�Ȃ��Ă���p�[�c�����ׂĎ擾����
        /// </summary>
        /// <param name="partsClass">�p�[�c�̃N���X�^�C�v typeof(object)�őS��</param>
        /// <param name="tar">�擾�Ώ�</param>
        /// <param name="rp">�y�[��</param>
        /// <param name="checkIllustion"></param>
        /// <returns>�p�[�c�̃R���N�V����</returns>
        public virtual ICollection<PartsBase> GetOverlappedParts(Type partsClass, PartsBase tar, IRichPane rp, bool checkIllustion)
        {
            return new List<PartsBase>();
        }

        /// <summary>
        /// �w��p�[�c���폜����
        /// </summary>
        /// <param name="value"></param>
        public virtual void Remove(PartsBase value)
        {
            _removedParts.Add(value);
        }

        #region IEnumerable �����o

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual IPartsEnumerator GetEnumerator()
        {
            return null;
        }

        #endregion

        /// <summary>
        /// �o�^����Ă���p�[�c������Ԃ�
        /// </summary>
        public abstract int Count
        {
            get;
        }
        #region ICloneable �����o

        /// <summary>
        /// �p�[�c�Z�b�g�̃N���[���i�e�v�f�̒��g�̓N���[������Ȃ��j
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        #endregion
    }
}
