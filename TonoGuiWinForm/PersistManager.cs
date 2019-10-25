// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ffPersister �̊T�v�̐����ł��B
    /// �I�u�W�F�N�g�i����������S��
    /// </summary>
    public class PersistManager
    {
        public interface IGroupID
        {
            Id PersisterGroupID
            {
                get;
            }
        }
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>���R�[�_�Ǘ�</summary>
        private readonly IDictionary/*<string name,IRecorder>*/ _recorders = new HybridDictionary();

        #endregion

        /// <summary>
        /// ���R�[�_���w�肷��
        /// </summary>
        /// <param name="value">���R�[�_</param>
        public void AddRecorder(IRecorder value, Id id)
        {
            _recorders.Add(id.Value, value);
        }

        /// <summary>
        /// ���R�[�_�[����������
        /// </summary>
        /// <param name="value">���R�[�_�[</param>
        public void RemoveRecorder(Id value)
        {
            var rec = (IRecorder)_recorders[value.Value];
            _recorders.Remove(value.Value);
        }

        /// <summary>
        /// ���R�[�_�[����������
        /// </summary>
        /// <param name="value">���R�[�_�[</param>
        public void RemoveRecorder(IRecorder value)
        {
            foreach (DictionaryEntry de in _recorders)
            {
                if (de.Value == value)
                {
                    _recorders.Remove(de.Key);
                    return;
                }
            }
        }

        /// <summary>
        /// ���R�[�_�[�ւ̃u���b�W
        /// </summary>
        public class RecorderBridge
        {
            /// <summary>�ΏۂƂ��郌�R�[�_�[�I�u�W�F�N�g</summary>
            private readonly IRecorder _recorder;

            /// <summary>
            /// �B��̃R���X�g���N�^
            /// </summary>
            /// <param name="rec"></param>
            internal RecorderBridge(IRecorder rec)
            {
                _recorder = rec;
            }

            /// <summary>
            /// �ۑ�
            /// </summary>
            /// <param name="value"></param>
            public void Save(object value, Id savingObjectID)
            {
                if (_recorder != null)
                {
                    _recorder.RecorderSave(value, savingObjectID);
                }
            }

            /// <summary>
            /// ���ׂĉi�����̏�������
            /// </summary>
            public void Reset()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderReset();
                }
            }

            /// <summary>
            /// �`�����N�J�n
            /// </summary>
            public void StartChunk(string debugName)
            {
                if (_recorder != null)
                {
                    _recorder.RecorderStartChunk(debugName);
                }
            }

            /// <summary>
            /// �`�����Nk�����ǂ�����������
            /// </summary>
            /// <returns>true = �`�����N��</returns>
            public bool IsStartedChunk
            {
                get
                {
                    if (_recorder == null)
                    {
                        return false;
                    }
                    else
                    {
                        return _recorder.RecorderIsChunkStarted;
                    }
                }
            }

            /// <summary>
            /// �`�����N�I��
            /// </summary>
            public void EndChunk()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderEndChunk();
                }
            }

            /// <summary>
            /// �`�����N�L�����Z��
            /// </summary>
            public void CancelChunk()
            {
                if (_recorder != null)
                {
                    _recorder.RecorderCancelChunk();
                }
            }

        }

        /// <summary>
        /// �w��O���[�v�ɑ�����Persister��Chunk�K�w���w�肷��i�w��K�w�ȍ~��؂�̂Ă�j
        /// </summary>
        /// <param name="groupID">�O���[�v��ID</param>
        /// <param name="length">�K�w</param>
        public void SetChunkLength(NamedId groupID, int length)
        {
            foreach (IRecorder rec in _recorders.Values)
            {
                if (rec is PersistManager.IGroupID)
                {
                    if (((PersistManager.IGroupID)rec).PersisterGroupID == groupID)
                    {
                        rec.RecorderSetChunkLength(length);
                    }
                }
            }
        }

        /// <summary>
        /// ���R�[�_�[���擾����
        /// </summary>
        public RecorderBridge this[Id id] => new RecorderBridge((IRecorder)_recorders[id.Value]);
    }
}
