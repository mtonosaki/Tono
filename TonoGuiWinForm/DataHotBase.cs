// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �f�[�^�f�t�H���g
    /// �f�[�^�I�u�W�F�N�g���s�v�ȏꍇ�Ɏg�p���Ă�������
    /// </summary>
    public sealed class DataHotDefault : DataHotBase
    {
    }


    /// <summary>
    /// �A�v���P�[�V�����ŗL�̃f�[�^���Ǘ������{�N���X
    /// </summary>
    [Serializable]
    public abstract class DataHotBase
    {
        #region �V���A���C�Y���Ȃ���ԕϐ�
        [NonSerialized]
        private bool _isModified = false;
        /// <summary>
        /// �_�[�e�B�[�t���O������
        /// </summary>
        public virtual void SetModified()
        {
            _isModified = true;
        }
        /// <summary>
        /// �_�[�e�B�[�t���O��ύX����
        /// </summary>
        /// <param name="sw"></param>
        public virtual void SetModified(bool sw)
        {
            _isModified = sw;
        }
        /// <summary>
        /// �_�[�e�B�[�t���O
        /// </summary>
        public virtual bool IsModified => _isModified;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        [NonSerialized]
        private readonly object _mySingleton = new object();
        #endregion

        /// <summary>
        /// �������[�g�i�f�[�^�S�́j
        /// </summary>
        public virtual object SyncRoot => _mySingleton;

        /// <summary>
        /// �S�A�v���P�[�V�����f�[�^����������
        /// </summary>
        public virtual void Clear()
        {
        }

        /// <summary>
        /// ���R�[�h���폜����
        /// </summary>
        /// <param name="table">�e�[�u��</param>
        /// <param name="rec">���R�[�h</param>
        public virtual void Remove(TableCollection table, object rec)
        {
            table.DirectRemove(rec);
        }

        /// <summary>
        /// ���R�[�h��ǉ�����
        /// </summary>
        /// <param name="table">�e�[�u��</param>
        /// <param name="rec">���R�[�h</param>
        public virtual void Add(TableCollection table, object rec)
        {
            table.DirectAdd(rec);
        }
    }
}
