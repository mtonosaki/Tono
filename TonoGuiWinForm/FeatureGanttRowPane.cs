// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureGanttRowPane �̊T�v�̐����ł��B
    /// �K���g�`���[�g�̍s�w�b�_�̕\���E�Ǘ�
    /// </summary>
    public class FeatureGanttRowPane : FeatureBase
    {
        #region �ꏊ���������邽�߂̃N���X

        /// <summary>
        /// �s����肷�邽�߂̌������u
        /// </summary>
        public class RowManager
        {
            private readonly IDictionary _idpos = new HybridDictionary();
            private readonly IDictionary _posid = new HybridDictionary();

            /// <summary>
            /// ���C����
            /// </summary>
            /// <returns></returns>
            public int GetLineN()
            {
                return _posid.Count + 1;
            }

            /// <summary>
            /// �f�[�^����������Ȃ��ꍇ�̒l
            /// NA
            /// </summary>
            public static int NA => int.MinValue + 100;

            /// <summary>
            /// �f�[�^��o�^����
            /// </summary>
            /// <param name="id">�����L�[</param>
            /// <param name="ptpos">�p�[�c���W</param>
            public void Add(Id rowid, int ptpos)
            {
                _idpos[rowid.Value] = ptpos;
                _posid[ptpos] = rowid.Value;
            }

            /// <summary>
            /// �S�ēo�^�ς݂�IDPOS�֘A���폜����
            /// </summary>
            public void Clear()
            {
                _idpos.Clear();
                _posid.Clear();
            }

            /// <summary>
            /// �sID���w�肵�ăp�[�c���W��Ԃ�
            /// �o�^����Ă��Ȃ��ꍇ��NA��Ԃ�
            /// </summary>
            public int this[Id rowid]
            {
                get
                {
                    var ret = _idpos[rowid.Value];
                    if (ret == null)
                    {
                        return NA;
                    }
                    return (int)ret;
                }
            }

            /// <summary>
            /// �p�[�c���W���w�肵�čsID��Ԃ�
            /// �o�^����Ă��Ȃ��ꍇ��NA��Ԃ�
            /// </summary>
            public Id this[int ptpos]
            {
                get
                {
                    var ret = _posid[ptpos];
                    if (ret == null)
                    {
                        return new Id { Value = NA };
                    }
                    return new Id { Value = (int)ret };
                }
            }

            /// <summary>
            /// ID�̃R���N�V������Ԃ�
            /// </summary>
            /// <returns>ID�ꗗ</returns>
            public ICollection GetIDs()
            {
                return _idpos.Keys;
            }

            /// <summary>
            /// �p�[�c���W�̃R���N�V������Ԃ�
            /// </summary>
            /// <returns>�p�[�c���W�ꗗ</returns>
            public ICollection GetPartsPositions()
            {
                return _posid.Keys;
            }

            /// <summary>
            /// Key=ID / Value=Pos��DictionaryEnumerator���擾����
            /// </summary>
            /// <returns>IDictionaryEnumerator</returns>
            public IDictionaryEnumerator GetIDPosEnumerator()
            {
                return _idpos.GetEnumerator();
            }
        }
        #endregion
    }
}
