// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �f�t�H���g�����N
    /// �����N���s�v�ȏꍇ�Ɏg�p���Ă�������
    /// </summary>
    public sealed class DataLinkDefault : DataLinkBase
    {
        public override void Clear()
        {
        }

        public override void SetEquivalent(RecordBase record, PartsBase parts)
        {
        }

        public override void RemoveEquivalent(PartsBase parts)
        {
        }

        public override ICollection GetRecordset(PartsBase key)
        {
            return Const.ZeroCollection;
        }

        public override ICollection GetPartsset(RecordBase key)
        {
            return Const.ZeroCollection;
        }
    }

    /// <summary>
    /// daLinkBase �̊T�v�̐����ł��B
    /// Parts��AppData�����т�������Ǘ������{�N���X
    /// </summary>
    public abstract class DataLinkBase
    {
        /// <summary>���R�[�h����p�[�c�ɒl�ϊ�����f���Q�[�g</summary>
        public delegate void RPAdapter(RecordBase fromValue, PartsBase toValue);

        /// <summary>�p�[�c���烌�R�[�h�ɒl�ϊ�����f���Q�[�g</summary>
        public delegate void PRAdapter(PartsBase fromValue, RecordBase toValue);

        #region �����i�V���A���C�Y����j

        private readonly IList<RPAdapter> _adapterRtoPs = new List<RPAdapter>();
        private readonly IList<PRAdapter> _adapterPtoRs = new List<PRAdapter>();

        #endregion


        /// <summary>
        /// ���R�[�h���p�[�c �l�]���f���Q�[�g���w�肷��
        /// </summary>
        /// <param name="value">�f���Q�[�g</param>
        public void SetRPAdapter(RPAdapter value)
        {
            _adapterRtoPs.Add(value);
        }

        /// <summary>
        /// �p�[�c�����R�[�h�@�l�]���f���Q�[�g���w�肷��
        /// </summary>
        /// <param name="value"></param>
        public void SetPRAdapter(PRAdapter value)
        {
            _adapterPtoRs.Add(value);
        }

        /// <summary>
        /// �ύX���ꂽ���R�[�h�̒l���p�[�c�ɔ��f������
        /// �i���̃��\�b�h���s���x��UNDO�ΏۂƂ��邩�ǂ����́AIsAutoChunkState�Ŏw�肷��j�j
        /// </summary>
        /// <param name="record">�V�����Ȃ������R�[�h</param>
        /// <returns>���f��p�[�c�Q�Ɓi�Q�l�j</returns>
        public ICollection Equalization(RecordBase record)
        {
            System.Diagnostics.Debug.Assert(_adapterRtoPs.Count > 0, "Equalization����ɂ́A�܂�SetRPAdapter�ŃA�_�v�^�[���w�肷��K�v������܂�");
            var partsset = GetPartsset(record);
            if (partsset.Count > 0)
            {
                foreach (PartsBase parts in partsset)
                {
                    foreach (var rpa in _adapterRtoPs)
                    {
                        // ���R�[�h�̒l���p�[�c�ɔ��f
                        rpa(record, parts);
                    }
                }
            }
            return partsset;
        }

        /// <summary>
        /// �ύX���ꂽ�p�[�c�̒l�����R�[�h�ɔ��f������
        /// �i���̃��\�b�h���s���x��UNDO�ΏۂƂ��邩�ǂ����́AIsAutoChunkState�Ŏw�肷��j�j
        /// </summary>
        /// <param name="parts">�V�����Ȃ����p�[�c</param>
        /// <returns>���f�ヌ�R�[�h�Q�Ɓi�Q�l�j</returns>
        public ICollection Equalization(PartsBase parts)
        {
            System.Diagnostics.Debug.Assert(_adapterPtoRs.Count >= 0, "Equalization����ɂ́A�܂�SetPRAdapter�ŃA�_�v�^�[���w�肷��K�v������܂�");
            var records = GetRecordset(parts);
            if (records.Count > 0)
            {
                foreach (RecordBase record in records)
                {
                    foreach (var pra in _adapterPtoRs)
                    {
                        // �p�[�c�̒l�����R�[�h�ɔ��f
                        pra(parts, record);
                    }
                }
            }
            return records;
        }

        /// ���̃��\�b�h�͏����Ă������� by Tono �`�F�b�N�A�E�g�ł��Ȃ������̂ŁA�b��
        public ICollection Equalization(PartsBase parts, bool dummy)
        {
            return Equalization(parts);
        }

        /// <summary>
        /// �����N�����ׂăN���A����
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// �f�[�^�̃����N����ۑ�����
        /// </summary>
        /// <param name="record">�e�[�u���̃��R�[�h</param>
        /// <param name="parts">�p�[�c</param>
        public abstract void SetEquivalent(RecordBase record, PartsBase parts);

        /// <summary>
        /// �p�[�c���w�肵�ă����N�����폜����
        /// </summary>
        /// <param name="parts">�폜����p�[�c</param>
        public abstract void RemoveEquivalent(PartsBase parts);

        /// <summary>
        /// �p�[�c�w�肵�ă��R�[�h���擾����iNULL��������j
        /// </summary>
        /// <param name="key">�p�[�c</param>
        /// <returns>���R�[�h</returns>
        public RecordBase GetRecordOrNull(PartsBase key)
        {
            var rs = GetRecordset(key);
            if (rs.Count == 0)
            {
                return null;
            }
            var e = rs.GetEnumerator();
            var ret = e.MoveNext();
            return (RecordBase)e.Current;
        }

        /// <summary>
        /// �p�[�c�w�肵�ă��R�[�h���擾����
        /// </summary>
        /// <param name="key">�p�[�c</param>
        /// <returns>���R�[�h</returns>
        public RecordBase GetRecord(PartsBase key)
        {
            var ret = GetRecordOrNull(key);
            System.Diagnostics.Debug.Assert(ret != null, "�����N��񂪂���܂���");
            return ret;
        }

        /// <summary>
        /// �p�[�c�w�肵�ă��R�[�h���擾����
        /// </summary>
        /// <param name="key">�p�[�c</param>
        /// <returns>���R�[�h�̃R���N�V����</returns>
        public abstract ICollection GetRecordset(PartsBase key);

        /// <summary>
        /// ���R�[�h���w�肵�ăp�[�c���擾����
        /// </summary>
        /// <param name="key">���R�[�h</param>
        /// <returns>�p�[�c</returns>
        public PartsBase GetParts(RecordBase key)
        {
            var rs = GetPartsset(key);
            if (rs.Count > 0)
            {
                var e = rs.GetEnumerator();
                e.MoveNext();
                return (PartsBase)e.Current;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// ���R�[�h���w�肵�ăp�[�c���擾����
        /// </summary>
        /// <param name="key">���R�[�h</param>
        /// <returns>�p�[�c</returns>
        public abstract ICollection GetPartsset(RecordBase key);
    }
}
