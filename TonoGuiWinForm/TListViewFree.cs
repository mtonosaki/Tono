// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Tono.GuiWinForm�� �����E�g��k�� ���X�g�r���[
    /// </summary>
    /// <example>
    /// �J�����P�߂́A�_�~�[��݂��邱��
    /// 
    ///		// �ŏ��̃J�����A�݌v��K�v
    /// ===== �\�z�q =====
    ///		ColumnHeader hd = new ColumnHeader();	// �_�~�[�p�̃J�������ЂƂǉ�����̂��d�v
    ///		hd.Text = "*";
    ///		hd.Width = 20;
    ///		_lv.Columns.Add(hd);
    /// 
    /// =====�w�b�_�쐬=====
    ///			ColumnHeader hd = new ColumnHeader(cs);
    ///			hd.Text = "�J�����P";
    ///			_lv.Columns.Add(hd);
    ///		�ȏ�J��Ԃ�
    /// =====�f�[�^�쐬=====
    ///			ListViewItem li = new ListViewItem(""); // �_�~�[�p�̃w�b�_
    ///			li.SubItems.Add("�_�~�[�̉��̃J����1");
    ///			li.SubItems.Add("�_�~�[�̉��̃J����2");
    ///			_lv.Items.Add(li);
    /// </example>
    public partial class TListViewFree : UserControl
    {
        #region �C�x���g����
        /// <summary>
        /// �������]������C�x���g����
        /// </summary>
        public class StringAugmentEventArgs : EventArgs
        {
            public string Value;
            public StringAugmentEventArgs()
            {
                Value = "";
            }
            public StringAugmentEventArgs(string str)
            {
                Value = str;
            }
        }

        /// <summary>
        /// �e�L�X�g�C�x���g
        /// </summary>
        public class TooltipTextRequestArgs : EventArgs
        {
            public int Column = -1;
            public int Row = -1;
            public string Text = string.Empty;
            public ListViewItem Item = null;

            public TooltipTextRequestArgs()
            {
                Text = string.Empty;
            }

            public TooltipTextRequestArgs(string value)
            {
                Text = value;
            }
        }
        #endregion
        #region Tono.GuiWinForm �f�[�^�\��

        /// <summary>
        /// Tono.GuiWinForm�f�[�^�i�i���f�[�^���Ǘ��j
        /// </summary>
        public class HotData : DataHotBase
        {
            /// <summary>
            /// ���X�g�A�C�e���A�ǉ����
            /// </summary>
            public class Status
            {
                /// <summary>
                /// �s�\���t���O
                /// </summary>
                public bool Visible = true;

                /// <summary>
                /// �L�[�ɂ���ăO���[�v������Ă����j�[�N�Łu�Ȃ��v���Ƃ̃t���O
                /// �i�o�^����Ă���񁔂́A���j�[�N�ł͂Ȃ��A�P�Ȃ��\�s�j
                /// �����ɓo�^����Ă��Ȃ��̂́A���j�[�N�ł���BValue��True�̂��͓̂o�^����Ȃ�
                /// </summary>
                private readonly Dictionary<int/*column*/, int/*��ސ�*/> _noOfDuplicatedInfo = new Dictionary<int, int>();
                private readonly Dictionary<int/*column*/, uCouple<TListViewFree.SummaryMode, object/*�T�}�����*/>> _summaryInfo = new Dictionary<int, uCouple<SummaryMode, object>>();

                /// <summary>
                /// �O���[�v����錳�̃��R�[�h��
                /// </summary>
                private int _nGroupedRecord = 1;

                /// <summary>
                /// �L�[�ɂ���ăO���[�v������Ă����j�[�N�ł��邩�ǂ��������Z�b�g����
                /// </summary>
                public void ResetNotUniqueInfo()
                {
                    _noOfDuplicatedInfo.Clear();
                    _summaryInfo.Clear();
                }

                /// <summary>
                /// �O���[�v����錳�̃��R�[�h��
                /// </summary>
                public int nGroupedRecord
                {
                    get => _nGroupedRecord;
                    set => _nGroupedRecord = value;
                }

                /// <summary>
                /// �L�[�ɂ���ăO���[�v������Ă����j�[�N�ł��邱�Ƃ��w�肷��
                /// </summary>
                /// <param name="column">��</param>
                /// <param name="nDuplicatedInfo">1=���j�[�N / 2�ȏ�=����ސ�</param>
                public void SetUnique(int column, int nDuplicatedInfo, SummaryMode mode, object summaryInfo)
                {
                    if (nDuplicatedInfo == 1)
                    {
                        if (_noOfDuplicatedInfo.ContainsKey(column))
                        {
                            _noOfDuplicatedInfo.Remove(column);
                            _summaryInfo.Remove(column);
                        }
                    }
                    else
                    {
                        _noOfDuplicatedInfo[column] = nDuplicatedInfo;
                        _summaryInfo[column] = new uCouple<SummaryMode, object>(mode, summaryInfo);
                    }
                }

                /// <summary>
                /// �w��񂪁A�O���[�v���ɂ���āA����ނ̃o���G�[�V�����ɂȂ������𒲂ׂ�
                /// </summary>
                /// <param name="column">��</param>
                /// <returns>1=�P��ށ����j�[�N�B2�ȏ�=��ސ��B�s���ł͂Ȃ�</returns>
                public int CheckNoOfKindOfTheInfo(int column)
                {
                    if (_noOfDuplicatedInfo.TryGetValue(column, out var nDuplicatedInfo))
                    {
                        return nDuplicatedInfo;
                    }
                    else
                    {
                        return 1;   // �o�^����Ă��Ȃ��Ȃ�A���j�[�N�ł���B
                    }
                }

                /// <summary>
                /// �T�}�������擾����
                /// </summary>
                /// <param name="column"></param>
                /// <returns></returns>
                public object GetSummaryValue(int column)
                {
                    if (_summaryInfo.ContainsKey(column) == false)
                    {
                        return null;
                    }
                    else
                    {
                        return _summaryInfo[column].V2;
                    }
                }

                /// <summary>
                /// �T�}�����[�h���擾����
                /// </summary>
                /// <param name="column"></param>
                /// <returns></returns>
                public SummaryMode GetSummaryMode(int column)
                {
                    if (_summaryInfo.ContainsKey(column) == false)
                    {
                        return SummaryMode.OneOfRecord;
                    }
                    return _summaryInfo[column].V1;
                }
            }

            /// <summary>
            /// �O���[�v�������ꂽ�i�������ꂽ�j
            /// </summary>
            public event ColumnClickEventHandler GroupKeyChanged;

            /// <summary>
            /// �A�C�e���R���N�V�����i�t�B���^�[��̃A�C�e���̂݁B�t�B���^���ꂽ�A�C�e����_status��Key�ɓ����Ă���j
            /// </summary>
            private readonly ListViewItemCollection _lvis = new ListViewItemCollection();

            /// <summary>
            /// �A�C�e���̃X�e�[�^�X
            /// </summary>
            private readonly Dictionary<ListViewItem, Status> _status = new Dictionary<ListViewItem, Status>();

            /// <summary>
            /// �w�b�_�R���N�V����
            /// </summary>
            private readonly ColumnHeaderCollection _lvhs = new ColumnHeaderCollection();

            /// <summary>
            /// �t�B���^�R���N�V����
            /// </summary>
            private readonly List<string> _filters = new List<string>();


            /// <summary>
            /// �t�B���^�[�E�O���[�v�������ꂽListView�Item
            /// </summary>
            private ListViewItemCollection _filtered_lvis = null;

            /// <summary>
            /// �O���[�v�����̃L�[�ƂȂ��ԍ�
            /// </summary>
            private int _column_no_of_distinctedRow = -1;


            /// <summary>
            /// �S�t�B���^�[��Ԃ��������āA���ׂĂ̍s��\������
            /// </summary>
            public void FilterOff()
            {
                var disped = new Dictionary<ListViewItem, bool>();
                foreach (ListViewItem lvi in _lvis)
                {
                    disped.Add(lvi, true);
                }
                foreach (var lvi in _status.Keys)
                {
                    if (_status.TryGetValue(lvi, out var status))
                    {
                        if (status.Visible == false)
                        {
                            _lvis.Add(lvi);
                            status.Visible = true;
                        }
                    }
                }
                SetDistinctRow(GetColumnNoOfDistinctedRow());
            }

            /// <summary>
            /// �܂Ƃ߂ĕ\����Ԃ�ύX
            /// </summary>
            /// <param name="items"></param>
            /// <param name="sw"></param>
            public void SetVisible(ICollection<ListViewItem> items, bool sw)
            {
                if (sw == false)
                {
                    var itemsSpeed = new Dictionary<ListViewItem, bool>();
                    foreach (var lvi in items)
                    {
                        itemsSpeed[lvi] = true;
                    }
                    var tmp = new List<ListViewItem>();
                    for (var i = 0; i < _lvis.Count; i++)
                    {
                        tmp.Add(_lvis[i]);
                    }
                    _lvis.Clear();
                    for (var i = 0; i < tmp.Count; i++)
                    {
                        var lvi = tmp[i];
                        if (itemsSpeed.ContainsKey(lvi) == false)
                        {
                            _lvis.Add(lvi);
                        }
                    }
                    foreach (var lvi in itemsSpeed.Keys)
                    {
                        if (_status.TryGetValue(lvi, out var status))
                        {
                            status.Visible = false;
                        }
                        else
                        {
                            status = new Status
                            {
                                Visible = false
                            };
                            _status[lvi] = status;
                        }
                    }
                }
                else
                {
                    foreach (var lvi in items)
                    {
                        if (_status.TryGetValue(lvi, out var status))
                        {
                            if (status.Visible == false)
                            {
                                status.Visible = true;
                                _lvis.Add(lvi);
                            }
                        }
                    }
                }
                SetDistinctRow(GetColumnNoOfDistinctedRow());
            }

            /// <summary>
            /// �A�C�e���̃X�e�[�^�X���擾����
            /// </summary>
            /// <param name="lvi"></param>
            /// <returns></returns>
            public Status GetStatus(ListViewItem lvi)
            {
                if (_status.TryGetValue(lvi, out var ret))
                {
                    return ret;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// �\����Ԃ�ύX
            /// �x���̂ŁASetVisible(ICollection...�����p���Ă�������
            /// </summary>
            /// <param name="item"></param>
            /// <param name="sw"></param>
            public void SetVisible(ListViewItem item, bool sw)
            {
                var isIn = false;
                foreach (ListViewItem lvi in _lvis)
                {
                    if (object.ReferenceEquals(lvi, item))
                    {
                        isIn = true;
                        break;
                    }
                }
                if (sw == true)
                {
                    if (isIn == false)
                    {
                        _lvis.Add(item);
                    }
                }
                else
                {
                    if (isIn)
                    {
                        var tmp = new List<ListViewItem>();
                        foreach (ListViewItem lvi in _lvis)
                        {
                            tmp.Add(lvi);
                        }
                        _lvis.Clear();
                        foreach (var lvi in tmp)
                        {
                            if (object.ReferenceEquals(lvi, item) == false)
                            {
                                _lvis.Add(lvi);
                            }
                        }
                    }
                }
                if (_status.TryGetValue(item, out var stat))
                {
                    stat.Visible = sw;
                }
                else
                {
                    stat = new Status
                    {
                        Visible = sw
                    };
                    _status[item] = stat;
                }
                SetDistinctRow(GetColumnNoOfDistinctedRow());
            }


            /// <summary>
            /// �O���[�v�������ꂽ�񂪎w�肳��Ă��邩�ǂ����𒲂ׂ�
            /// </summary>
            public bool IsDistinctedRow => _column_no_of_distinctedRow >= 0;

            /// <summary>
            /// �O���[�v�����̃L�[�ƂȂ��ԍ�
            /// </summary>
            /// <returns>��ԍ� / -1=�O���[�v�������Ă��Ȃ�</returns>
            public int GetColumnNoOfDistinctedRow()
            {
                return _column_no_of_distinctedRow;
            }

            private static readonly Dictionary<Type, MethodInfo> cast_to_double = new Dictionary<Type, MethodInfo>();

            /// <summary>
            /// �w��J�������P��ނP�s�ɂȂ�悤�ɁA�t�B���^�����O����Items���쐬����
            ///�\�[�g�����
            /// </summary>
            /// <param name="column">��ԍ��B-1=����</param>
            public void SetDistinctRow(int column)
            {
                // ���j�[�N�����N���A����
                foreach (var kv in _status)
                {
                    kv.Value.ResetNotUniqueInfo();
                }

                // �}�C�i�X�J�����́A�������Ӗ����� ������������������������������������������������
                bool isChanged; // �C�x���g���s�p �_�[�e�B�[�t���O
                if (column < 0)
                {
                    isChanged = _column_no_of_distinctedRow >= 0;
                    if (_filtered_lvis != null)
                    {
                        _filtered_lvis.Clear();
                    }
                    _filtered_lvis = null;
                    _column_no_of_distinctedRow = -1;
                    if (isChanged)
                    {
                        GroupKeyChanged?.Invoke(this, new ColumnClickEventArgs(-1));    // �C�x���g���s
                    }
                    return;
                }

                // �O���[�v��w��@������������������������������������������������������������������
                isChanged = _column_no_of_distinctedRow != column;  // �C�x���g���s�p�_�[�e�B�[�t���O
                _column_no_of_distinctedRow = column;
                var groupbykey = new Dictionary<string, List<ListViewItem>>();
                foreach (ListViewItem lvi in _lvis)
                {
                    if (groupbykey.TryGetValue(lvi.SubItems[column].Text, out var items) == false)
                    {
                        items = new List<ListViewItem>
                        {
                            lvi
                        };
                        groupbykey[lvi.SubItems[column].Text] = items;
                    }
                    else
                    {
                        items.Add(lvi);
                    }
                }

                _filtered_lvis = new ListViewItemCollection();
                foreach (var lvis in groupbykey.Values)
                {
                    _filtered_lvis.Add(lvis[0]);

                    // ���j�[�N����
                    for (var c = 0; c < _lvhs.Count; c++)   // �w�b�_�����[�v
                    {
                        if (_status.TryGetValue(lvis[0], out var status) == false)
                        {
                            status = new Status();
                            _status[lvis[0]] = status;
                        }

                        var summaryMode = _lvhs.SummaryMode[c];
                        var dupcheck = new Dictionary<string, object>();
                        if (summaryMode == SummaryMode.OneOfRecord)
                        {
                            for (var i = 0; i < lvis.Count; i++)
                            {
                                dupcheck[lvis[i].SubItems[c].Text] = this;
                            }
                            if (dupcheck.Count > 1)
                            {
                                status.SetUnique(c, dupcheck.Count, summaryMode, null);
                            }
                        }
                        else
                        {
                            double summary;
                            switch (summaryMode)
                            {
                                case SummaryMode.Minimum:
                                    summary = double.PositiveInfinity;
                                    break;
                                case SummaryMode.Maximum:
                                    summary = double.NegativeInfinity;
                                    break;
                                default:
                                    summary = 0;
                                    break;
                            }
                            //var summary = summaryMode switch
                            //{
                            //    SummaryMode.Minimum => double.PositiveInfinity,
                            //    SummaryMode.Maximum => double.NegativeInfinity,
                            //    _ => 0,
                            //};
                            var isAllSameValue = true;
                            double preval = 0;
                            for (var i = 0; i < lvis.Count; i++)
                            {
                                var si = lvis[i].SubItems[c];
                                double val = 0;
                                if (si is ListViewSubItemEx)
                                {
                                    var raw = ((ListViewSubItemEx)si).RawData;
                                    if (raw is double)
                                    {
                                        val = (double)raw;
                                    }
                                    else if (raw is int)
                                    {
                                        val = (int)raw;
                                    }
                                    else if (raw is float)
                                    {
                                        val = (float)raw;
                                    }
                                    else
                                    {
                                        var isNG = true;
                                        if (cast_to_double.TryGetValue(raw.GetType(), out var mi) == false)
                                        {
                                            mi = raw.GetType().GetMethod("op_Implicit");
                                            cast_to_double[raw.GetType()] = mi;
                                        }
                                        if (mi != null)
                                        {
                                            if (mi.ReturnType.Equals(typeof(double)))
                                            {
                                                val = (double)mi.Invoke(null, new object[] { raw });
                                                isNG = false;
                                            }
                                            else if (mi.ReturnType.Equals(typeof(float)))
                                            {
                                                val = (float)mi.Invoke(null, new object[] { raw });
                                                isNG = false;
                                            }
                                            else if (mi.ReturnType.Equals(typeof(int)))
                                            {
                                                val = (int)mi.Invoke(null, new object[] { raw });
                                                isNG = false;
                                            }
                                        }
                                        if (isNG)
                                        {
                                            val = double.Parse(lvis[i].SubItems[c].Text);
                                        }
                                    }
                                }
                                else
                                {
                                    val = double.Parse(lvis[i].SubItems[c].Text);
                                }
                                if (isAllSameValue)
                                {
                                    if (i > 0)
                                    {
                                        if (val != preval)
                                        {
                                            isAllSameValue = false;
                                        }
                                    }
                                    else
                                    {
                                        preval = val;
                                    }
                                }
                                switch (summaryMode)
                                {
                                    case SummaryMode.Average:
                                    case SummaryMode.Sum:
                                        summary += val;
                                        break;
                                    case SummaryMode.Count:
                                        summary++;
                                        break;
                                    case SummaryMode.Maximum:
                                        if (val > summary)
                                        {
                                            summary = val;
                                        }
                                        break;
                                    case SummaryMode.Minimum:
                                        if (val < summary)
                                        {
                                            summary = val;
                                        }
                                        break;
                                    default:
                                        Debug.Assert(false, "���ׂĂ�summaryMode�ɂ��ăv���O�������Ă�������");
                                        break;
                                }
                            }
                            if (summaryMode == SummaryMode.Average)
                            {
                                summary /= lvis.Count;
                            }
                            if (summaryMode == SummaryMode.Maximum || summaryMode == SummaryMode.Minimum)
                            {
                                if (!isAllSameValue)    // �S�������l�Ȃ�A�ő�E�ŏ��\���łȂ��Ă��悢�i�������̕����A�Z���ɍő�������A�C�R�����t���Ȃ��Č��₷���j
                                {
                                    status.SetUnique(c, lvis.Count, summaryMode, summary);
                                }
                            }
                            else
                            {
                                status.SetUnique(c, lvis.Count, summaryMode, summary);
                            }
                        }
                    }
                    // �O���[�v���ꂽ�s����ۑ�����
                    if (lvis.Count > 1)
                    {
                        if (_status.ContainsKey(lvis[0]) == false)  // �Q�s�ȏオ�܂�������v���Ă����ꍇ�AStatus���o�^����Ă��Ȃ��̂ŁA�����Ń��j�[�N�����œo�^����
                        {
                            _status[lvis[0]] = new Status();
                        }
                    }
                    if (_status.ContainsKey(lvis[0]))
                    {
                        _status[lvis[0]].nGroupedRecord = lvis.Count;
                    }
                }

                if (isChanged)
                {
                    GroupKeyChanged?.Invoke(this, new ColumnClickEventArgs(_column_no_of_distinctedRow));
                }
            }

            /// <summary>
            /// �A�C�e���̃R���N�V����
            /// �iSetDistinct���Ă���ꍇ�́A���ꂪ���f���ꂽ�R���N�V�����j
            /// </summary>
            public ListViewItemCollection Items
            {
                get
                {
                    if (_filtered_lvis != null)
                    {
                        return _filtered_lvis;
                    }
                    else
                    {
                        return _lvis;
                    }
                }
            }

            /// <summary>
            /// �w�背�R�[�h�ɑ΂���B�ꂽ�s�����ׂĕԂ��i�x���j
            /// </summary>
            /// <param name="lvi"></param>
            /// <returns></returns>
            public ICollection<ListViewItem> GetHiddenRows(ListViewItem ilvi)
            {
                var ret = new List<ListViewItem>();
                if (_column_no_of_distinctedRow >= 0)
                {
                    foreach (var lvi in GetAllItems())
                    {
                        if (ilvi.SubItems[_column_no_of_distinctedRow].Text == lvi.SubItems[_column_no_of_distinctedRow].Text)
                        {
                            ret.Add(lvi);
                        }
                    }
                }
                else
                {
                    ret.Add(ilvi);
                }
                return ret;
            }

            /// <summary>
            /// �t�B���^��Ԃ̂��̂��܂߂āA���ׂẴA�C�e��
            /// </summary>
            public ICollection<ListViewItem> GetAllItems()
            {
                var ret = new List<ListViewItem>();
                foreach (ListViewItem lvi in _lvis)
                {
                    ret.Add(lvi);
                }
                foreach (var kv in _status)
                {
                    if (kv.Value.Visible == false)
                    {
                        ret.Add(kv.Key);
                    }
                }
                return ret;
            }


            /// <summary>
            /// �w�b�_�̃R���N�V����
            /// </summary>
            public ColumnHeaderCollection Columns => _lvhs;

            /// <summary>
            /// �t�B���^�[�̃R���N�V����
            /// </summary>
            public List<string> Filters
            {
                get
                {
                    for (var i = _filters.Count; i < _lvhs.Count; i++)  // �J�����̗ʂ����t�B���^������悤�ɂ���
                    {
                        _filters.Add("");
                    }
                    return _filters;
                }
            }

            /// <summary>
            /// �O���[�v�����Ă����j�[�N�ł��邩�ǂ����𒲂ׂ�
            /// </summary>
            /// <param name="lvi"></param>
            /// <param name="column"></param>
            /// <param name="summaryValue">�W�v����</param>
            /// <param name="summaryMode">�W�v���@</param>
            /// <returns>1=���j�[�N�ȂP��ނŃT�}���W�v���Ă��Ȃ��B�Q�ȏ�=���̎�ސ��i�s���ł͂Ȃ��j�ŏW�v���Ă���</returns>
            public int GetSummaryInfo(ListViewItem lvi, int column, out SummaryMode summaryMode, out object summaryValue)
            {
                if (_status.TryGetValue(lvi, out var status))
                {
                    summaryMode = status.GetSummaryMode(column);
                    summaryValue = status.GetSummaryValue(column);
                    return status.CheckNoOfKindOfTheInfo(column);
                }
                else
                {
                    summaryMode = SummaryMode.OneOfRecord;
                    summaryValue = null;
                    return 1;
                }
            }

            /// <summary>
            /// �O���[�v�����ꂽ���̃��R�[�h���𒲂ׂ�
            /// </summary>
            /// <param name="lvi"></param>
            /// <returns></returns>
            public int CheckNoOfRecordOfTheInfo(ListViewItem lvi)
            {
                if (_status.TryGetValue(lvi, out var status))
                {
                    return status.nGroupedRecord;
                }
                else
                {
                    return 1;
                }
            }
        }

        /// <summary>
        /// Tono.GuiWinForm�p�[�c�i�\���p�ꎞ�f�[�^���Ǘ��j
        /// </summary>
        public class FreeViewPartsCollection : PartsCollection
        {
            /// <summary>
            /// �Z���A�e�L�X�g�}�[�W���i�Y�[���O�j
            /// </summary>
            public float ptCellMarginX = 2;

            /// <summary>
            /// �Z���A�e�L�X�g�}�[�W���i�Y�[���O�j
            /// </summary>
            public float ptCellMarginY = 2;

            /// <summary>
            /// �t�B���^�{�b�N�X�̍����i�w�b�_�����ɕt���A�Y�[���O�j
            /// </summary>
            public float ptFilterHeight = 18;

            /// <summary>
            /// �w�b�_�̕W�������i�Y�[���O�j
            /// </summary>
            public float ptHeaderLabelHeight = 24;

            /// <summary>
            /// �w�b�_�̕W�������i�Y�[���O�j�t�B���^�̈�̃T�C�Y���܂ނ̂ŁA�擪�s�̈ʒu���擾���邽�߂ɗp����
            /// </summary>
            public float ptHDy
            {
                get => ptHeaderLabelHeight + ptFilterHeight;
                set => ptHeaderLabelHeight = value - ptFilterHeight;
            }

            /// <summary>
            /// �s�̕W�������i�Y�[���O�j
            /// </summary>
            public float ptROWy = 20;

            /// <summary>
            /// �w�b�_�̍����i�X�N���[�����W�j
            /// </summary>
            public float scHeaderHeight;

            /// <summary>
            /// �t�B���^�̍����i�X�N���[�����W�j
            /// </summary>
            public float scFilterHeight;

            /// <summary>
            /// �s�̍����i�X�N���[�����W�j
            /// </summary>
            public float scRowHeight;

            /// <summary>
            /// �J�����̕\���J�n�ʒu�iX���W�j
            /// </summary>
            public List<float> scColStartX = new List<float>();

            /// <summary>
            /// �}�E�X�ʒu�̃J����
            /// </summary>
            public int columnAtMouse = -1;

            /// <summary>
            /// �}�E�X�ʒu�̍s
            /// </summary>
            public int rowAtMouse = -1;

            /// <summary>
            /// �}�E�X�ʒu�̒l
            /// </summary>
            public string valueAtMouse = "";

            /// <summary>
            /// �C���[�W���X�g
            /// </summary>
            public ImageList ImageList = null;
        }

        /// <summary>
        /// Tono.GuiWinForm�����N�@�\
        /// </summary>
        public class DataLink : DataLinkBase
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

        #endregion
        #region Tono.GuiWinForm �t�B�[�`���[
        /// <summary>
        /// �X�N���[����Y�[���̒l��\���͈͂ɐ�������
        /// </summary>
        public class FeatureZoomScrollNormalizer : FeatureBase, IScrollListener, IZoomListener, IMouseListener
        {
            private IRichPane[] _rpTarget;
            private XyBase _preZoom = XyBase.FromInt(-1, -1);
            /// <summary>
            /// �Y�[�����E�Ȃ̂Ŏ������_�ړ����Ȃ�
            /// </summary>
            private DataSharingManager.Boolean _noscrollmove = null;

            public override void OnInitInstance()
            {
                base.OnInitInstance();
                _rpTarget = new IRichPane[] { Pane.GetPane("Resource") };
                _noscrollmove = (DataSharingManager.Boolean)Share.Get("NoScrollMoveFlag", typeof(DataSharingManager.Boolean));
            }

            #region IScrollListener �����o

            public IRichPane[] ScrollEventTargets => _rpTarget;

            public void ScrollChanged(IRichPane rp)
            {
                if (Pane.Zoom.Equals(_preZoom) == false)
                {
                    _preZoom = (XyBase)Pane.Zoom.Clone();
                    return;
                }
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;
                var pr = rp.GetPaneRect();
                var nRow = (pr.Height * 0.75f - ps.scHeaderHeight) / ps.scRowHeight;
                var maxStart = dat.Items.Count - nRow;
                if (maxStart < 0)
                {
                    maxStart = 0;
                }

                var maxScrollY = maxStart * ps.scRowHeight * -1;

                var scroll = (ScreenPos)Pane.Scroll.Clone();

                if (Pane.Scroll.Y < maxScrollY)
                {
                    scroll.Y = (int)maxScrollY;
                }

                if (Pane.Scroll.Y > 0)
                {
                    scroll.Y = 0;
                }
                if (scroll.Equals(Pane.Scroll) == false)
                {
                    Pane.Scroll = scroll;
                    _noscrollmove.value = true;
                }
            }

            #endregion

            #region IZoomListener �����o

            public IRichPane[] ZoomEventTargets => _rpTarget;

            public void ZoomChanged(IRichPane rp)
            {
                var zoom = (XyBase)Pane.Zoom.Clone();
                if (Pane.Zoom.Y < 240)
                {
                    zoom.Y = 240;
                }
                if (Pane.Zoom.X < 480)
                {
                    zoom.X = 480;
                }
                if (zoom.Equals(Pane.Zoom) == false)
                {
                    Pane.Zoom = zoom;
                    _noscrollmove.value = true;
                }
            }

            #endregion

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
                Pane.Invalidate(null);
            }

            public void OnMouseDown(MouseState e)
            {
            }

            public void OnMouseUp(MouseState e)
            {
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion
        }

        /// <summary>
        /// �J���������X���[�Y�ɒ���
        /// </summary>
        public class FeatureDragColumnZoom : FeatureBase, IMouseListener
        {
            /// <summary>
            /// �ŏ���
            /// </summary>
            public static readonly int MinimumWidth = 4;

            /// <summary>
            /// �ő啝
            /// </summary>
            public static readonly int MaximumWidth = 800;

            private int _column = -1;
            private readonly ScreenPos _orgPos = ScreenPos.FromInt(0, 0);
            private int _orgWidth = -1;

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
                if (_column > 0)
                {
                    var dist = e.Pos.X - _orgPos.X;
                    var nowwidth = _orgWidth + dist;
                    if (nowwidth > MaximumWidth)
                    {
                        nowwidth = MaximumWidth;
                    }

                    if (nowwidth < MinimumWidth)
                    {
                        nowwidth = MinimumWidth;
                    } ((HotData)Data).Columns[_column].Width = nowwidth;
                    Pane.Invalidate(null);
                }
            }

            public void OnMouseDown(MouseState e)
            {
                if (e.Attr.IsCtrl && e.Attr.IsShift == false)
                {
                    var ps = (FreeViewPartsCollection)Parts;
                    _column = ps.columnAtMouse;
                    _orgPos.X = e.Pos.X;
                    _orgPos.Y = e.Pos.Y;
                    _orgWidth = ((HotData)Data).Columns[_column].Width;
                }
            }

            public void OnMouseUp(MouseState e)
            {
                _column = -1;
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion
        }

        /// <summary>
        /// ���������̃X���[�Y�Y�[��
        /// </summary>
        public class FeatureDragHeightZoom : FeatureBase, IMouseListener
        {
            #region		����(�V���A���C�Y����)
            /** <summary>�Y�[�����J�n����g���K</summary> */
            private readonly MouseState.Buttons _trigger;
            private readonly bool _isCenterLock;
            #endregion
            #region		����(�V���A���C�Y���Ȃ�)
            /// <summary>�}�E�X���N���b�N�������_�ł̃}�E�X���W</summary>
            private ScreenPos _posDown = null;
            /// <summary>�}�E�X���N���b�N�������_�ł̃X�N���[����</summary>
            private ScreenPos _scrollDown;
            /// <summary>�}�E�X���N���b�N�������̃Y�[���l</summary>
            private XyBase _zoomDown;
            /// <summary>�}�E�X���N���b�N�����Ƃ��̃y�[��</summary>
            private IRichPane _paneDown;
            /// <summary>�C�x���g�ɂ���ĕύX����J�[�\���̃��X�g</summary>
            private readonly Hashtable _CursorList = new Hashtable();
            /// <summary>���O�̃}�E�X�J�[�\���̏�</summary>
            private readonly MouseState.Buttons _prev = new MouseState.Buttons();
            /// <summary>�J�[�\����</summary>
            protected NamedId _tokenListenID = NamedId.FromName("CursorSetJob");
            /// <summary>
            /// �Y�[�����E�Ȃ̂Ŏ������_�ړ����Ȃ�
            /// </summary>
            private DataSharingManager.Boolean _noscrollmove = null;
            #endregion

            /// <summary>
            /// �R���X�g���N�^
            /// </summary>
            public FeatureDragHeightZoom()
            {
                // �f�t�H���g�Ńh���b�O�X�N���[�����邽�߂̃L�[��ݒ肷��
                _trigger = new MouseState.Buttons
                {
                    IsButton = true,
                    IsButtonMiddle = false,
                    IsCtrl = true,
                    IsShift = false
                };
                _isCenterLock = false;
            }

            /// <summary>
            /// ������
            /// </summary>
            public override void OnInitInstance()
            {
                base.OnInitInstance();
                _noscrollmove = (DataSharingManager.Boolean)Share.Get("NoScrollMoveFlag", typeof(DataSharingManager.Boolean));
            }

            #region IMouseListener �����o
            /// <summary>
            /// �{�^��Down�C�x���g
            /// </summary>
            /// <param name="e"></param>
            public void OnMouseDown(MouseState e)
            {
                if (e.Attr.Equals(_trigger))
                {
                    _posDown = (ScreenPos)e.Pos.Clone();
                    _paneDown = Pane;
                    _zoomDown = (XyBase)Pane.Zoom.Clone();
                    _scrollDown = (ScreenPos)Pane.Scroll.Clone();
                }
            }

            /// <summary>
            /// �}�E�XMove�C�x���g
            /// </summary>
            /// <param name="e"></param>
            public void OnMouseMove(MouseState e)
            {
                if (_posDown == null || _zoomDown == null || _scrollDown == null || e.Pane == null)
                {
                    return;
                }

                if (e.Attr.Equals(_trigger))
                {
                    // ��ʂ̊g��/�k��
                    var movePos = e.Pos - _posDown;          // �J�[�\���̈ړ��ʂ̌v�Z
                    movePos.X = movePos.Y;

                    var pdBak = (ScreenPos)_posDown.Clone();
                    if (_isCenterLock)
                    {
                        _posDown.X = e.Pane.GetPaneRect().LT.X + (e.Pane.GetPaneRect().RB.X - e.Pane.GetPaneRect().LT.X) / 2;
                        _posDown.Y = e.Pane.GetPaneRect().LT.Y + (e.Pane.GetPaneRect().RB.Y - e.Pane.GetPaneRect().LT.Y) / 2;
                    }

                    var zoomNow = _zoomDown + movePos * 2;      // �Y�[���l�̎Z�o

                    // �Y�[���l���K��͈͓��Ɏ��߂�
                    if (zoomNow.X > 4000)
                    {
                        zoomNow.X = 4000;
                    }

                    if (zoomNow.Y > 4000)
                    {
                        zoomNow.Y = 4000;
                    }

                    if (zoomNow.X < 5)
                    {
                        zoomNow.X = 5;
                    }

                    if (zoomNow.Y < 5)
                    {
                        zoomNow.Y = 5;
                    }

                    Pane.Zoom = (XyBase)zoomNow.Clone();           // �Y�[���l�̔��f

                    // �N���b�N�����ʒu����ɂ��ăY�[������悤�ɉ�ʂ��X�N���[������B
                    var ZoomRatioX = (double)zoomNow.X / _zoomDown.X;    // X�����̃Y�[�����̎Z�o
                    var ZoomRatioY = (double)zoomNow.Y / _zoomDown.Y;    // Y�����̃Y�[�����̎Z�o

                    var beforeDownPos = _posDown - _scrollDown - e.Pane.GetPaneRect().LT;    // 
                    var afterDownPos = ScreenPos.FromInt((int)(ZoomRatioX * beforeDownPos.X), (int)(ZoomRatioY * beforeDownPos.Y));

                    if (_noscrollmove.value == false)
                    {
                        Pane.Scroll = _scrollDown - (afterDownPos - beforeDownPos);
                    }
                    else
                    {
                        _noscrollmove.value = false;
                    }
                    Pane.Invalidate(null);
                    _posDown = pdBak;
                }
                else
                {
                    OnMouseUp(e);
                }
            }

            /// <summary>
            /// �{�^��Up�C�x���g
            /// </summary>
            /// <param name="e"></param>
            public void OnMouseUp(MouseState e)
            {
                _posDown = null;
                _zoomDown = null;
                _scrollDown = null;
            }

            /// <summary>
            /// �}�E�X�z�C�[���C�x���g
            /// </summary>
            /// <param name="e"></param>
            public void OnMouseWheel(MouseState e)
            {
                // ���g�p
            }

            #endregion
        }

        /// <summary>
        /// �J�����N���b�N�C�x���g�𔭍s����
        /// </summary>
        public class FeatureColumnClickHandler : FeatureBase, IMouseListener
        {
            /// <summary>
            /// �J�����N���b�N�C�x���g�i�\�[�g�Ȃǁj
            /// </summary>
            public event ColumnClickEventHandler ColumnClick;

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
            }

            public void OnMouseDown(MouseState e)
            {
            }

            public void OnMouseUp(MouseState e)
            {
                if (e.Attr.IsButton && e.Attr.IsCtrl == false && e.Attr.IsShift == false)
                {
                    var ps = (FreeViewPartsCollection)Parts;
                    if (ps.columnAtMouse >= 0)
                    {
                        if (e.Pos.Y < ps.scHeaderHeight - ps.scFilterHeight)
                        {
                            if (ColumnClick != null)
                            {
                                var ea = new ColumnClickEventArgs(ps.columnAtMouse);
                                ColumnClick(this, ea);
                            }
                        }
                    }
                }
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion
        }

        /// <summary>
        /// �t�B���^�����̕ҏW���T�|�[�g
        /// </summary>
        public class FeatureFilterEdit : FeatureBase, IMouseListener, ITokenListener, IScrollListener, IZoomListener
        {
            private IRichPane[] _tarPanes = null;

            public override void OnInitInstance()
            {
                base.OnInitInstance();
                _tarPanes = new IRichPane[] { Pane.GetPane("Resource") };
            }

            /// <summary>
            /// �t�B���^�[���s�v����������
            /// </summary>
            public event ColumnClickEventHandler FilterRequested;

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
            }

            public void OnMouseDown(MouseState e)
            {
            }

            public void OnMouseUp(MouseState e)
            {
                if (e.Attr.IsButton && e.Attr.IsCtrl == false && e.Attr.IsShift == false)
                {
                    // �Â��R���g���[��������������A����
                    deleteTextControl();

                    var ps = (FreeViewPartsCollection)Parts;
                    if (ps.columnAtMouse >= 0)
                    {
                        if (e.Pos.Y > ps.scHeaderHeight - ps.scFilterHeight && e.Pos.Y < ps.scHeaderHeight)
                        {
                            // �e�L�X�g�{�b�N�X�R���g���[�����쐬����
                            var zx = (float)Pane.Zoom.X / 1000;
                            var zy = (float)Pane.Zoom.Y / 1000;
                            var tb = new TextBox
                            {
                                Font = new Font("Tahoma", 9f * zy),
                                Location = new Point((int)ps.scColStartX[ps.columnAtMouse] + 1, (int)(ps.scHeaderHeight - ps.scFilterHeight) - 1),
                                Size = new Size((int)(ps.scColStartX[ps.columnAtMouse + 1] - ps.scColStartX[ps.columnAtMouse]), (int)ps.scFilterHeight - 4),
                                BackColor = Color.FromArgb(64, 64, 80),
                                ForeColor = Color.Yellow
                            };
                            tb.TextChanged += new EventHandler(tb_TextChanged);
                            tb.Tag = ps.columnAtMouse;
                            tb.Leave += new EventHandler(tb_Leave);
                            tb.KeyDown += new KeyEventHandler(tb_KeyDown);
                            tb.Text = ((HotData)Data).Filters[ps.columnAtMouse];
                            tb.Parent = Pane.Control;

                            Pane.Control.Controls.Add(tb);
                            tb.Select();
                            tb.SelectAll();
                        }
                    }
                }
            }

            private void deleteTextControl()
            {
                var dels = new List<Control>();
                foreach (Control c in Pane.Control.Controls)
                {
                    if (c is TextBox)
                    {
                        dels.Add(c);
                    }
                }
                foreach (var c in dels)
                {
                    Pane.Control.Controls.Remove(c);
                    c.Dispose();
                }
            }

            /// <summary>
            /// �g�[�N�����󂯂āA�t�B���^�[���s�v���̃C�x���g�𔭍s����
            /// </summary>
            /// <param name="who"></param>
            public override void Start(NamedId who)
            {
                base.Start(who);
                if (_token.Equals(who))
                {
                    FilterRequested?.Invoke(this, new ColumnClickEventArgs(-1));    // -1�͑S�t�B���^�[�̈Ӗ�
                }
            }

            private void tb_KeyDown(object sender, KeyEventArgs e)
            {
                var tb = sender as TextBox;
                if (e.KeyCode == Keys.Enter)
                {
                    var col = (int)tb.Tag;  //	�t�B���^�l�𔽉f
                    ((HotData)Data).Filters[col] = tb.Text;

                    tb_Leave(sender, EventArgs.Empty);

                    FilterRequested?.Invoke(this, new ColumnClickEventArgs((int)tb.Tag));
                }
                if (e.KeyCode == Keys.Escape)
                {
                    tb_Leave(sender, EventArgs.Empty);
                }
                if (e.KeyCode == Keys.A && e.Control)
                {
                    tb.SelectAll();
                }
            }

            private void tb_Leave(object sender, EventArgs e)
            {
                var tb = sender as TextBox;
                Pane.Control.Controls.Remove(tb);
                tb.Dispose();
                Pane.Invalidate(null);
            }

            private void tb_TextChanged(object sender, EventArgs e)
            {
                //var tb = sender as TextBox;
                //int col = (int)tb.Tag;	// ���A���^�C���X�V���Ȃ�
                //((daData)Data).Filters[col] = tb.Text;
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion

            #region ITokenListener �����o
            private static readonly NamedId _token = NamedId.FromName("RequestFilter");
            public NamedId TokenTriggerID => _token;

            #endregion

            #region IScrollListener �����o

            public IRichPane[] ScrollEventTargets => _tarPanes;

            public void ScrollChanged(IRichPane rp)
            {
                ZoomChanged(rp);
            }

            #endregion

            #region IZoomListener �����o

            public IRichPane[] ZoomEventTargets => _tarPanes;

            public void ZoomChanged(IRichPane rp)
            {
                deleteTextControl();
            }

            #endregion
        }

        /// <summary>
        /// �R���e�L�X�g���j���[����
        /// </summary>
        public class FeatureContextMenu : FeatureBase, IMouseListener
        {
            /// <summary>
            /// �t�B���^�[���s�v��
            /// </summary>
            public event ColumnClickEventHandler FilterRequested;

            /// <summary>
            /// �t�@�W�[������쐬�⏕�̗v���C�x���g
            /// </summary>
            public event EventHandler<StringAugmentEventArgs> RequestFuzzyString;

            /// <summary>
            /// �f�U�C�i�ō�����R���e�L�X�g���j���[�̎Q��
            /// </summary>
            private ContextMenuStrip _menu = null;

            /// <summary>
            /// �R���e�L�X�g���j���[��o�^����
            /// </summary>
            /// <param name="cms"></param>
            public void SetContextMenu(ContextMenuStrip cms)
            {
                _menu = cms;
                Pane.Control.ContextMenuStrip = _menu;
                _menu.Opening += new CancelEventHandler(_menu_Opening);
                _menu.Items["allClearFilterToolStripMenuItem"].Click += new EventHandler(allClearFilterToolStripMenuItem_Click);
                _menu.Items["clearFilterToolStripMenuItem"].Click += new EventHandler(clearFilterToolStripMenuItem_Click);
                _menu.Items["setGroupKeyToolStripMenuItem"].Click += new EventHandler(setGroupKeyToolStripMenuItem_Click);
                _menu.Items["showAllRecordsToolStripMenuItem"].Click += new EventHandler(showAllRecordsToolStripMenuItem_Click);
                _menu.Items["addFilterWithThisToolStripMenuItem"].Click += new EventHandler(addFilterWithThisToolStripMenuItem_Click);
                _menu.Items["addNegativeFilterToolStripMenuItem"].Click += new EventHandler(addNegativeFilterToolStripMenuItem_Click);
                _menu.Items["showTheGroupedRecordToolStripMenuItem"].Click += new EventHandler(showTheGroupedRecordToolStripMenuItem_Click);
                _menu.Items["fazzyoutFilterToolStripMenuItem"].Click += new EventHandler(fazzyoutFilterToolStripMenuItem_Click);


            }

            /// <summary>
            /// ���j���[�J�����̏���
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void _menu_Opening(object sender, CancelEventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;

                // �񖼂�ǉ�����
                foreach (ToolStripItem tsi in _menu.Items)
                {
                    if (tsi is ToolStripMenuItem tsmi)
                    {
                        if (tsmi.Tag is string txt)
                        {
                            var col = "?";
                            var val = "?";
                            var key = "?";

                            var colKey = dat.GetColumnNoOfDistinctedRow();
                            var groupedNoOfRec = 1;
                            if (colKey >= 0 && ps.rowAtMouse >= 0)
                            {
                                key = dat.Items[ps.rowAtMouse].SubItems[colKey].Text;
                                if (key.Length > 10)
                                {
                                    key = key.Substring(0, 10) + "...";
                                }

                                groupedNoOfRec = dat.CheckNoOfRecordOfTheInfo(dat.Items[ps.rowAtMouse]);

                            }

                            if (ps.columnAtMouse >= 0)
                            {
                                col = dat.Columns[ps.columnAtMouse].Text;
                                col = col.Replace("\n", "");
                                col = col.Replace("\r", "");
                                if (col.Length > 10)
                                {
                                    col = col.Substring(0, 10) + "...";
                                }
                            }
                            if (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0)
                            {
                                val = ps.valueAtMouse;
                                val = val.Replace("\n", "");
                                val = val.Replace("\r", "");
                                if (val.Length > 10)
                                {
                                    val = val.Substring(0, 10) + "...";
                                }
                            }

                            // Enable����
                            if (txt.IndexOf("@COL@") >= 0 || txt.IndexOf("@VAL@") >= 0)
                            {
                                tsmi.Enabled = true;
                            }
                            if (txt.IndexOf("@COL@") >= 0)
                            {
                                tsmi.Enabled &= ps.columnAtMouse >= 0;
                            }
                            if (txt.IndexOf("@VAL@") >= 0)
                            {
                                tsmi.Enabled &= (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0);
                            }
                            if (txt.IndexOf("@KEY@") >= 0)
                            {
                                tsmi.Enabled = key != "?" && groupedNoOfRec > 1;
                            }


                            // �e�L�X�g���f
                            txt = txt.Replace("@COL@", col);
                            txt = txt.Replace("@VAL@", val);
                            txt = txt.Replace("@KEY@", key);
                            tsmi.Text = txt;
                        }
                    }
                }

                // ���j���[�̗L������
                var isNoFilter = true;
                for (var i = 0; i < dat.Columns.Count; i++)
                {
                    if (string.IsNullOrEmpty(dat.Filters[i]) == false)
                    {
                        isNoFilter = false;
                        break;
                    }
                }
                _menu.Items["allClearFilterToolStripMenuItem"].Enabled = !isNoFilter;
                _menu.Items["clearFilterToolStripMenuItem"].Enabled = ps.columnAtMouse >= 0 && string.IsNullOrEmpty(dat.Filters[ps.columnAtMouse]) == false;
                _menu.Items["showAllRecordsToolStripMenuItem"].Enabled = dat.IsDistinctedRow;
                _menu.Items["setGroupKeyToolStripMenuItem"].Enabled = (dat.GetColumnNoOfDistinctedRow() != ps.columnAtMouse) & _menu.Items["setGroupKeyToolStripMenuItem"].Enabled;

                var ffsw = false;
                if (ps.columnAtMouse >= 0)
                {
                    var f2 = StringMatch.MakeFilterFazzy(dat.Filters[ps.columnAtMouse]);
                    if (RequestFuzzyString != null)
                    {
                        var sae = new StringAugmentEventArgs(f2);
                        RequestFuzzyString(this, sae);
                        f2 = sae.Value;
                    }
                    f2 = StringMatch.MakeFilterFazzy(f2);
                    ffsw = (f2 != dat.Filters[ps.columnAtMouse]);
                }
                _menu.Items["fazzyoutFilterToolStripMenuItem"].Enabled = ffsw;
            }

            /// <summary>
            /// �t�B���^�[�̐������� ^ $ �����͂���
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void fazzyoutFilterToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;

                var s = StringMatch.MakeFilterFazzy(dat.Filters[ps.columnAtMouse]);
                if (RequestFuzzyString != null)
                {
                    var sae = new StringAugmentEventArgs(s);
                    RequestFuzzyString(this, sae);
                    s = sae.Value;
                }
                s = StringMatch.MakeFilterFazzy(s);

                dat.Filters[ps.columnAtMouse] = s;

                FilterRequested?.Invoke(this, new ColumnClickEventArgs(ps.columnAtMouse));
                Pane.Invalidate(null);
            }

            /// <summary>
            /// �O���[�v���ꂽ���R�[�h�����\������
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void showTheGroupedRecordToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;

                var colKey = dat.GetColumnNoOfDistinctedRow();
                var key = dat.Items[ps.rowAtMouse].SubItems[colKey].Text;

                dat.Filters[colKey] = string.Format("^\"{0}\"$", key);
                dat.SetDistinctRow(-1);

                FilterRequested?.Invoke(this, new ColumnClickEventArgs(ps.columnAtMouse));

                Pane.Invalidate(null);
            }

            /// <summary>
            /// �}�E�X�ʒu�̃e�L�X�g�őI���t�B���^�[������
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void addFilterWithThisToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;

                if (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0)
                {
                    if (MouseState.Now.Attr.IsShift)
                    {
                        for (var i = 0; i < dat.Columns.Count; i++)
                        {
                            dat.Filters[i] = "";
                        }
                    }
                    dat.Filters[ps.columnAtMouse] = string.Format("{0}^{2}${1}", "{", "}", StringMatch.ReplaceMetaStr(ps.valueAtMouse));
                    FilterRequested?.Invoke(this, new ColumnClickEventArgs(ps.columnAtMouse));
                }
            }

            /// <summary>
            /// �}�E�X�ʒu�̃e�L�X�g�Ŕ�I���t�B���^�[������
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void addNegativeFilterToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;

                if (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0)
                {
                    if (MouseState.Now.Attr.IsShift)
                    {
                        for (var i = 0; i < dat.Columns.Count; i++)
                        {
                            dat.Filters[i] = "";
                        }
                    }

                    var s0 = StringMatch.ReplaceMetaStr(ps.valueAtMouse);
                    var s = string.Format("-{0}^{2}${1} {3}", "{", "}", s0, dat.Filters[ps.columnAtMouse]).Trim();
                    dat.Filters[ps.columnAtMouse] = s;
                    FilterRequested?.Invoke(this, new ColumnClickEventArgs(ps.columnAtMouse));
                }
            }



            /// <summary>
            /// �O���[�v��̉����i�S���R�[�h�\���j
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void showAllRecordsToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                dat.SetDistinctRow(-1);
                Pane.Invalidate(null);
            }

            /// <summary>
            /// �O���[�v��ݒ菈��
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void setGroupKeyToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                var ps = (FreeViewPartsCollection)Parts;
                if (ps.columnAtMouse >= 0)
                {
                    dat.SetDistinctRow(ps.columnAtMouse);
                    Pane.Invalidate(null);
                }
            }


            /// <summary>
            /// �t�B���^�[�P�N���A
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void clearFilterToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var ps = (FreeViewPartsCollection)Parts;
                if (ps.columnAtMouse >= 0)
                {
                    ((HotData)Data).Filters[ps.columnAtMouse] = "";
                    Token.Add(NamedId.FromName("RequestFilter"), this);
                }
            }

            /// <summary>
            /// �t�B���^�[�I�[���N���A
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void allClearFilterToolStripMenuItem_Click(object sender, EventArgs e)
            {
                var dat = (HotData)Data;
                for (var i = 0; i < dat.Filters.Count; i++)
                {
                    dat.Filters[i] = "";
                }
                Token.Add(NamedId.FromName("RequestFilter"), this);
            }

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
            }

            public void OnMouseDown(MouseState e)
            {
            }

            public void OnMouseUp(MouseState e)
            {
                if (e.Attr.IsButton == false && e.Attr.IsButtonMiddle == false && e.Attr.IsCtrl == false && e.Attr.IsShift == false)
                {
                }
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion
        }

        /// <summary>
        /// CTRL+C�ŃR�s�[���T�|�[�g
        /// </summary>
        public class FeatureCopyKey : FeatureBase, IKeyListener
        {
            private Synonym _synonim = null;

            /// <summary>
            /// �V�m�j���I�u�W�F�N�g��݂�
            /// </summary>
            /// <param name="synonym"></param>
            public void SetSynonym(Synonym synonym)
            {
                _synonim = synonym;
            }

            #region IKeyListener �����o

            public void OnKeyDown(KeyState e)
            {
            }

            public void OnKeyUp(KeyState e)
            {
                var ps = (FreeViewPartsCollection)Parts;

                if (e.IsControl && e.Key == Keys.C)
                {
                    if (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0)
                    {
                        if (e.IsShift && _synonim != null)
                        {
                            Clipboard.SetText(_synonim.GetMultiWordsOf(ps.valueAtMouse));
                        }
                        else
                        {
                            Clipboard.SetText(ps.valueAtMouse);
                        }
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// �c�[���`�b�v�T�|�[�g
        /// </summary>
        public class FeatureListTooltip : FeatureToolTip, IMouseListener, IScrollListener, IZoomListener
        {
            /// <summary>
            /// �c�[���`�b�v�ɏ��e�L�X�g�����v���C�x���g
            /// </summary>
            public event EventHandler<TooltipTextRequestArgs> TooltipTextRequest;

            private IRichPane[] _tars = null;
            private GuiTimer.Handle _th = null;
            private ScreenPos _prePos = ScreenPos.FromInt(1, -11);

            public override void OnInitInstance()
            {
                base.OnInitInstance();

                base.Text = string.Empty;
                _tars = new IRichPane[] { Pane.GetPane("Resource") };
            }

            /// <summary>
            /// �x������
            /// </summary>
            /// <param name="e"></param>
            private void delayProc(object ie)
            {
                var ps = (FreeViewPartsCollection)Parts;
                var e = (MouseState)ie;

                if (e.Attr.Equals(_trigger))
                {
                    var ts = new ThreadUtil();
                    var mousepos = ts.PointToClient(Pane.Control, MouseState.Now.Pos);
                    if (mousepos.Y >= 0)
                    {

                        if (TooltipTextRequest != null)
                        {
                            var ea = new TooltipTextRequestArgs
                            {
                                Text = ps.valueAtMouse,
                                Column = ps.columnAtMouse,
                                Row = ps.rowAtMouse
                            };
                            if (ps.rowAtMouse >= 0)
                            {
                                ea.Item = ((HotData)Data).Items[ps.rowAtMouse];
                            }
                            TooltipTextRequest(this, ea);
                            Text = ea.Text;
                        }
                        else
                        {
                            if (ps.columnAtMouse >= 0 && ps.rowAtMouse >= 0)
                            {
                                Text = ps.valueAtMouse;
                            }
                        }
                        if (string.IsNullOrEmpty(base.Text) == false)
                        {
                            _prePos = (ScreenPos)e.Pos.Clone();
                        }
                    }
                    else
                    {
                        base.Text = string.Empty;
                    }
                }
                else
                {
                    base.Text = string.Empty;
                }
            }

            #region IMouseListener �����o

            public void OnMouseMove(MouseState e)
            {
                Timer.Stop(_th);
                if (_prePos.Equals(e.Pos) == false)
                {
                    Text = string.Empty;
                    if (e.Pos.Y > 0)
                    {
                        _th = Timer.AddTrigger(e, 450, new GuiTimer.Proc1(delayProc));
                    }
                }
            }

            public void OnMouseDown(MouseState e)
            {
                base.Text = string.Empty;
                _prePos = ScreenPos.FromInt(1, -11);
                Timer.Stop(_th);
            }

            public void OnMouseUp(MouseState e)
            {
            }

            public void OnMouseWheel(MouseState e)
            {
            }

            #endregion

            #region IScrollListener �����o

            public IRichPane[] ScrollEventTargets => _tars;

            public void ScrollChanged(IRichPane rp)
            {
                base.Text = string.Empty;
                _prePos = ScreenPos.FromInt(1, -11);
            }

            #endregion

            #region IZoomListener �����o

            public IRichPane[] ZoomEventTargets => _tars;

            public void ZoomChanged(IRichPane rp)
            {
                base.Text = string.Empty;
                _prePos = ScreenPos.FromInt(1, -11);
            }

            #endregion
        }

        #endregion
        #region Tono.GuiWinForm �p�[�c�A�[�L�e�N�`��

        /// <summary>
        /// �`��S�̂�S��
        /// </summary>
        public class PartsViewFree : PartsBase
        {
            private ImageList _imageList = null;
            private Font _font = new Font("Tahoma", 9f);
            private Font _fontSummary = new Font("Tahoma", 9f, FontStyle.Italic);
            private Font _fontNoKind = new Font("Coureir New", 9f * 0.75f);
            private Font _fontB = new Font("Tahoma", 9f, FontStyle.Bold | FontStyle.Underline);
            private HotData _data = null;
            private FreeViewPartsCollection _partsset = null;
            private readonly Brush _headerBrush = new SolidBrush(Color.FromArgb(96, 96, 128));
            private readonly Brush _filterBoxBrush = new SolidBrush(Color.FromArgb(128, 128, 160));
            private readonly Brush _dataBrush = new SolidBrush(Color.FromArgb(64, 64, 64));
            private readonly Brush _rowHeaderTextBrush = new SolidBrush(Color.FromArgb(192, 192, 128));
            private readonly Brush _summaryFontBrush = new SolidBrush(Color.Blue);
            private readonly Brush _hiBrushRow = new SolidBrush(Color.FromArgb(48, 0, 255, 200));
            private readonly Brush _hiBrushCol = new SolidBrush(Color.FromArgb(16, 0, 255, 200));
            private readonly Pen _glidlinePen = new Pen(Color.FromArgb(236, 236, 236));
            private readonly Brush _notUniqueMaskBrush = new SolidBrush(Color.FromArgb(32, Color.DarkRed));
            private readonly Brush _summaryBackBrush = new SolidBrush(Color.FromArgb(128, Color.Yellow));
            private readonly Brush _notUniqueNoOfKindTextBrush = new SolidBrush(Color.FromArgb(64, 96, 96));
            private readonly Brush _notUniqueNoOfRecordTextBrush1 = new SolidBrush(Color.FromArgb(128, 96, 96, 160));
            private readonly Brush _notUniqueNoOfRecordTextBrushN = new SolidBrush(Color.FromArgb(96, 96, 160));


            /// <summary>
            /// �X�^�e�B�b�N������
            /// </summary>
            static PartsViewFree()
            {
            }

            /// <summary>
            /// �f�[�^�����蓖�Ă�
            /// </summary>
            /// <param name="dat"></param>
            public void SetData(HotData dat, FreeViewPartsCollection partsset)
            {
                _data = dat;
                _partsset = partsset;
            }

            /// <summary>
            /// �C���[�W���X�g���w�肷��
            /// </summary>
            /// <param name="il"></param>
            public void SetImageList(ImageList il)
            {
                _imageList = il;
            }

            /// <summary>
            /// �`��
            /// </summary>
            /// <param name="rp"></param>
            /// <returns></returns>
            public override bool Draw(IRichPane rp)
            {
                var zx = (float)rp.Zoom.X / 1000;
                var zy = (float)rp.Zoom.Y / 1000;
                float sx = rp.Scroll.X;
                float sy = rp.Scroll.Y;

                var fontsize = zy * 9;
                if (fontsize < 4)
                {
                    fontsize = 4;
                }

                if (Math.Abs(_font.SizeInPoints - fontsize) > 0.5)
                {
                    _font = new Font(_font.FontFamily, fontsize);
                    _fontB = new Font(_font, FontStyle.Bold | FontStyle.Underline);
                    _fontNoKind = new Font(_fontNoKind.FontFamily, fontsize * 0.75f);
                    _fontSummary = new Font(_fontSummary.FontFamily, fontsize);
                }

                var mx = _partsset.ptCellMarginX * zx;    // �}�[�W���X�y�[�X�iX�j
                var my = _partsset.ptCellMarginY * zy;    // �}�[�W���X�y�[�X�iY�j

                // �w�b�_�̍ő卂���𒲂ׂ�
                var maxHeaderHeight = zy * _partsset.ptHDy;
                var headerMargin = rp.Graphics.MeasureString("��", _font); // ���
                headerMargin.Height = maxHeaderHeight - headerMargin.Height;
                for (var i = 1; i < _data.Columns.Count; i++)
                {
                    var hd = _data.Columns[i];
                    var hw = zx * hd.Width;
                    SizeF size;
                    if (hd.Width < 8)   // �����Ȃ����͍����g���̑Ώۂ���O��
                    {
                        size = rp.Graphics.MeasureString(hd.Text, _font);
                    }
                    else
                    {
                        var hxtest = (int)(hw - mx * 2 + 0);
                        //						size = rp.Graphics.MeasureString(hd.Text, _font, (int)(hw - mx * 2 + 0));
                        var sf = makeStringFormat(hd);
                        var hdstr = hd.Text;
                        if (hxtest < headerMargin.Width * 1.5)
                        {
                            sf.FormatFlags |= StringFormatFlags.DirectionVertical;
                            sf.LineAlignment = StringAlignment.Near;
                            sf.Alignment = StringAlignment.Near;
                            hdstr = hdstr.Replace("\r", "");
                            hdstr = hdstr.Replace("\n", "");
                        }
                        size = rp.Graphics.MeasureString(hdstr, _font, hxtest, sf);
                    }
                    var height = size.Height + headerMargin.Height;
                    if (height > maxHeaderHeight)
                    {
                        maxHeaderHeight = height;
                    }
                }

                _partsset.scHeaderHeight = maxHeaderHeight; // �w�b�_����

                var paneRect = rp.GetPaneRect();
                var mouse = rp.Control.PointToClient(MouseState.NowPosition) + paneRect.LT;

                // �n�C���C�g�\���i�J�����j
                var hx = sx;          // �w�b�_�ʒu
                _partsset.scColStartX.Clear();
                _partsset.columnAtMouse = -1;
                for (var i = 0; i < _data.Columns.Count; i++)
                {
                    _partsset.scColStartX.Add(hx);
                    var hd = _data.Columns[i];
                    var hw = zx * hd.Width;

                    // �O���[�v�L�[�ɂȂ��Ă���J�����̐F��ς���
                    if (i == _data.GetColumnNoOfDistinctedRow())
                    {
                        rp.Graphics.FillRectangle(Brushes.White, hx, paneRect.LT.Y, hw, paneRect.Height);
                        rp.Graphics.FillRectangle(new LinearGradientBrush(new PointF(hx, 0), new PointF(hx + 48, 0), Color.FromArgb(48, Color.Red), Color.White), hx, paneRect.LT.Y, Math.Min(48, hw), paneRect.Height);
                    }

                    // �}�E�X�ʒu�̃J�����𒲍����A�F��ύX
                    if (hx <= mouse.X && mouse.X < hx + hw)
                    {
                        rp.Graphics.FillRectangle(_hiBrushCol, hx, paneRect.LT.Y, hw, paneRect.Height);
                        _partsset.columnAtMouse = i;
                    }

                    hx += hw;
                }


                // �f�[�^��`��
                _partsset.rowAtMouse = -1;
                var dy = sy + _partsset.scHeaderHeight;
                if (dy > _partsset.scHeaderHeight)
                {
                    dy = _partsset.scHeaderHeight;
                }

                _partsset.scRowHeight = zy * _partsset.ptROWy;
                var start_i = (int)(-sy / _partsset.scRowHeight);
                if (start_i < 0)
                {
                    start_i = 0;
                }

                dy += _partsset.scRowHeight * start_i;  // �`��J�n�ʒu�𒲐�
                bool isMouseRow;    // �}�E�X�s�̏�����
                _partsset.valueAtMouse = "";

                for (var i = start_i; i < _data.Items.Count; i++)
                {
                    // �n�C���C�g�\���i�s�j
                    if (dy <= mouse.Y && mouse.Y < dy + _partsset.scRowHeight)
                    {
                        rp.Graphics.FillRectangle(_hiBrushRow, paneRect.LT.X, dy, paneRect.Width, _partsset.scRowHeight);
                        _partsset.rowAtMouse = i;
                        isMouseRow = true;
                    }
                    else
                    {
                        isMouseRow = false;
                    }

                    // �f�[�^�`��
                    var lvi = _data.Items[i];
                    var dx = sx;
                    for (var col = 0; col < lvi.SubItems.Count; col++)
                    {
                        var hd = _data.Columns[col];
                        var dw = zx * hd.Width;
                        if (dx + dw + mx >= paneRect.LT.X && dx + mx <= paneRect.RB.X)  // �����Ă����̂ݕ`��
                        {
                            // �O���[�v�ɂ���ă��j�[�N�łȂ��Ȃ������i�P�Ȃ��\���ł��邱�Ɓj�𕪂�悤�ɂ���
                            var noInfoKind = _data.GetSummaryInfo(lvi, col, out var summaryMode, out var summaryResult);

                            // ListViewSubItem�̃f�[�^����
                            var si = lvi.SubItems[col];

                            // �l�`��
                            var sf = makeStringFormat(hd);
                            var rectcell = new RectangleF(dx, dy, dw, _partsset.scRowHeight);
                            var rect = new RectangleF(dx + mx, dy + my, dw - mx * 2, _partsset.scRowHeight - my * 2);

                            if (col == 0)   // �ŏ��̗�́A�s�w�b�_�Ƃ���
                            {
                                // �s�w�b�_�̔w�i�F
                                rp.Graphics.FillRectangle(_headerBrush, rectcell);
                            }
                            if (rect.Width > 2)
                            {
                                if (summaryMode == SummaryMode.OneOfRecord || col == _data.GetColumnNoOfDistinctedRow())
                                {
                                    if (col == 0)
                                    {
                                        rp.Graphics.DrawString(si.Text, _font, _rowHeaderTextBrush, rect, sf);
                                    }
                                    else
                                    {
                                        rp.Graphics.DrawString(si.Text, _font, _dataBrush, rect, sf);
                                    }
                                }
                                else
                                {
                                    switch (summaryMode)
                                    {
                                        default:
                                            rp.Graphics.FillRectangle(_summaryBackBrush, rect);
                                            string txt;
                                            if (hd is ColumnHeaderEx)
                                            {
                                                txt = ((ColumnHeaderEx)hd).ToFormatterdString(summaryResult);
                                            }
                                            else
                                            {
                                                txt = summaryResult.ToString();
                                            }
                                            rp.Graphics.DrawString(txt, _fontSummary, _summaryFontBrush, rect, sf);
                                            break;
                                    }
                                    switch (summaryMode)
                                    {
                                        case SummaryMode.Average:
                                            rp.Graphics.DrawImage(Properties.Resources.summary_ave, rect.Left, rect.Top);
                                            break;
                                        case SummaryMode.Count:
                                            rp.Graphics.DrawImage(Properties.Resources.summary_n, rect.Left, rect.Top);
                                            break;
                                        case SummaryMode.Maximum:
                                            rp.Graphics.DrawImage(Properties.Resources.summary_max, rect.Left, rect.Top);
                                            break;
                                        case SummaryMode.Minimum:
                                            rp.Graphics.DrawImage(Properties.Resources.summary_min, rect.Left, rect.Top);
                                            break;
                                        case SummaryMode.Sum:
                                            rp.Graphics.DrawImage(Properties.Resources.symmary_sum, rect.Left, rect.Top);
                                            break;
                                    }
                                }
                            }

                            // �����̂͂ݏo����Ԃ����ŕ���悤�ɂ���
                            if (col > 0)
                            {
                                var sizestr = rp.Graphics.MeasureString(si.Text, _font);
                                var diff = sizestr.Width - rect.Width;
                                if (diff > 0)
                                {
                                    if (diff < 4)
                                    {
                                        diff = 4;
                                    }

                                    if (diff > rect.Width)
                                    {
                                        diff = rect.Width;
                                    }

                                    rp.Graphics.FillRectangle(Brushes.Salmon, rect.Right - diff, rect.Bottom - 2, diff, 1);
                                }

                                var sf2 = new StringFormat();
                                if (sf.Alignment == StringAlignment.Far)
                                {
                                    sf2.Alignment = StringAlignment.Near;
                                }
                                else
                                {
                                    sf2.Alignment = StringAlignment.Far;
                                }

                                if (summaryMode == SummaryMode.OneOfRecord)
                                {
                                    if (noInfoKind > 1) // �f�[�^�̎�ސ�
                                    {
                                        rp.Graphics.FillRectangle(_notUniqueMaskBrush, rect);
                                        rp.Graphics.DrawString(
                                            noInfoKind.ToString(),
                                            _fontNoKind,
                                            _notUniqueNoOfKindTextBrush,
                                            rect,
                                            sf2
                                        );
                                    }
                                }
                                if (col == _data.GetColumnNoOfDistinctedRow())
                                {
                                    // �O���[�v���ꂽ���R�[�h���̕\��
                                    var n_of_kind = _data.CheckNoOfRecordOfTheInfo(lvi);
                                    rp.Graphics.DrawString(
                                        "n=" + n_of_kind.ToString(),
                                        _fontNoKind,
                                        n_of_kind == 1 ? _notUniqueNoOfRecordTextBrush1 : _notUniqueNoOfRecordTextBrushN,
                                        rect, sf2
                                    );

                                }
                            }
                            // �}�E�X�ʒu�̒l�𕶎���ۑ�
                            if (isMouseRow && _partsset.columnAtMouse == col)
                            {
                                _partsset.valueAtMouse = si.Text;
                            }
                        }
                        else
                        {
                            // �\���X�L�b�v�̗�
                        }
                        dx += dw;
                    }
                    dy += _partsset.scRowHeight;
                    rp.Graphics.DrawLine(_glidlinePen, Math.Max(paneRect.LB.X, sx), dy, paneRect.RB.X, dy);
                    if (dy > paneRect.RB.Y)
                    {
                        break;
                    }
                }

                // �w�b�_��`��
                hx = sx;            // �w�b�_�ʒu
                _partsset.scFilterHeight = zy * _partsset.ptFilterHeight;       // �t�B���^�{�b�N�X�̍����i�X�N���[�����W�j
                var hh = _partsset.scHeaderHeight - _partsset.scFilterHeight; // �w�b�_�̈�̂����A�t�B���^�{�b�N�X�������������i�w�b�_�\���p�j

                _partsset.scColStartX.Clear();
                for (var i = 0; i < _data.Columns.Count; i++)
                {
                    _partsset.scColStartX.Add(hx);
                    var hd = _data.Columns[i];
                    var hw = zx * hd.Width;
                    if (hx + hw > paneRect.LT.X && hx < paneRect.RB.X)  // �����Ă���w�b�_�̂ݕ`��
                    {
                        // �t�B���^�{�b�N�X�`��
                        rp.Graphics.FillRectangle(_filterBoxBrush, hx, hh, hw, _partsset.scFilterHeight);
                        rp.Graphics.DrawRectangle(Pens.DarkBlue, hx, hh, hw, _partsset.scFilterHeight);
                        var rect = new RectangleF(hx + 2, hh + 2, hw - 4, _partsset.scFilterHeight - 2);
                        if (rect.Width > 2)
                        {
                            rp.Graphics.DrawString(_data.Filters[i], _font, Brushes.Yellow, rect);
                        }

                        // �w�b�_�`��
                        rp.Graphics.FillRectangle(_headerBrush, hx, 0, hw, hh);
                        rp.Graphics.DrawRectangle(Pens.White, hx, 0, hw, hh);
                        var font = i == _data.GetColumnNoOfDistinctedRow() ? _fontB : _font;
                        var sf = makeStringFormat(hd);
                        rect = new RectangleF(hx + mx, my, hw - mx * 2, hh - my * 2);
                        if (rect.Width > 2)
                        {
                            var hdstr = hd.Text;
                            if (rect.Width < headerMargin.Width * 1.5)
                            {
                                sf.FormatFlags |= StringFormatFlags.DirectionVertical;
                                sf.LineAlignment = StringAlignment.Near;
                                sf.Alignment = StringAlignment.Near;
                                hdstr = hdstr.Replace("\r", "");
                                hdstr = hdstr.Replace("\n", "");
                            }
                            rp.Graphics.DrawString(hdstr, font, Brushes.White, rect, sf);

                            if (string.IsNullOrEmpty(hd.ImageKey) == false && hd.ImageKey != null && _imageList != null)
                            {
                                try
                                {
                                    var icon = _imageList.Images[hd.ImageKey];
                                    if (hw >= icon.Width)
                                    {
                                        rp.Graphics.DrawImage(icon, hx + hw - icon.Width, my);
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                    hx += hw;
                }
                _partsset.scColStartX.Add(hx);

                return true;
            }

            /// <summary>
            /// StringFormat���쐬����
            /// </summary>
            /// <param name="hd"></param>
            /// <returns></returns>
            private StringFormat makeStringFormat(ColumnHeader hd)
            {
                var sf = new StringFormat
                {
                    Trimming = StringTrimming.Character
                };

                switch (hd.TextAlign)
                {
                    case HorizontalAlignment.Left:
                        sf.Alignment = StringAlignment.Near;
                        break;
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }
                return sf;
            }
        }
        #endregion

        /// <summary>
        /// �O���[�v���������̏W�v���@
        /// </summary>
        public enum SummaryMode
        {
            /// <summary>
            /// �P���̂ǂꂩ
            /// </summary>
            OneOfRecord,
            /// <summary>
            /// ���v
            /// </summary>
            Sum,
            /// <summary>
            /// ����
            /// </summary>
            Average,
            /// <summary>
            /// �ŏ�
            /// </summary>
            Minimum,
            /// <summary>
            /// �ő�
            /// </summary>
            Maximum,
            /// <summary>
            /// ����
            /// </summary>
            Count,
        }

        /// <summary>
        /// ���l���ۑ��ł���T�u�A�C�e��
        /// </summary>
        public class ListViewSubItemEx : ListViewItem.ListViewSubItem
        {
            /// <summary>
            /// ���l
            /// </summary>
            public object RawData = null;
        }

        /// <summary>
        /// �t�H�[�}�b�^���ۑ��ł���J�����w�b�_
        /// </summary>
        public class ColumnHeaderEx : ColumnHeader
        {
            /// <summary>
            /// �t�H�[�}�b�^�[
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public delegate string FormatterMethod(object value);

            /// <summary>
            /// �t�H�[�}�b�^�[���\�b�h
            /// </summary>
            public FormatterMethod Formatter = null;

            /// <summary>
            /// �t�H�[�}�b�^���g������������쐬����
            /// </summary>
            /// <param name="summaryResult"></param>
            /// <returns></returns>
            public string ToFormatterdString(object value)
            {
                if (Formatter == null)
                {
                    return value.ToString();
                }
                else
                {
                    return Formatter(value);
                }
            }

            /// <summary>
            /// �R���X�g���N�^
            /// </summary>
            public ColumnHeaderEx() : base()
            {
            }

            /// <summary>
            /// �R���X�g���N�^
            /// </summary>
            /// <param name="txt"></param>
            public ColumnHeaderEx(string txt)
                : base(txt)
            {
            }
        }

        /// <summary>
        /// �A�C�e���R���N�V����
        /// </summary>
        public class ListViewItemCollection : IEnumerable, IEnumerable<ListViewItem>
        {
            private readonly List<ListViewItem> _items = new List<ListViewItem>();

            /// <summary>
            /// �S�A�C�e���N���A
            /// </summary>
            public void Clear()
            {
                _items.Clear();
            }

            /// <summary>
            /// �A�C�e�����Q��
            /// </summary>
            /// <param name="idx"></param>
            /// <returns></returns>
            public ListViewItem this[int idx] => _items[idx];

            /// <summary>
            /// ���ڂ�ǉ�
            /// </summary>
            /// <param name="lvi"></param>
            public void Add(ListViewItem lvi)
            {
                _items.Add(lvi);
            }

            /// <summary>
            /// ���������o�^
            /// </summary>
            /// <param name="lvis"></param>
            public void AddRange(ICollection<ListViewItem> lvis)
            {
                _items.AddRange(lvis);
            }

            public IEnumerator GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            /// <summary>
            /// ���ڐ�
            /// </summary>
            public int Count => _items.Count;

            #region IEnumerable<ListViewItem> �����o

            IEnumerator<ListViewItem> IEnumerable<ListViewItem>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            #endregion
        }

        /// <summary>
        /// �J�����̃R���N�V����
        /// </summary>
        public class ColumnHeaderCollection : IEnumerable<ColumnHeader>
        {
            private readonly List<ColumnHeader> _hdrs = new List<ColumnHeader>();
            private readonly List<SummaryMode> _summaryMode = new List<SummaryMode>();

            /// <summary>
            /// �J�����ǉ�
            /// </summary>
            /// <param name="hd"></param>
            public void Add(ColumnHeader hd)
            {
                _hdrs.Add(hd);
                _summaryMode.Add(TListViewFree.SummaryMode.OneOfRecord);
            }

            /// <summary>
            /// �J�����ǉ�
            /// </summary>
            /// <param name="hd"></param>
            /// <param name="summaryMode">�T�}���[���[�h</param>
            public void Add(ColumnHeader hd, TListViewFree.SummaryMode summaryMode)
            {
                _hdrs.Add(hd);
                _summaryMode.Add(summaryMode);
            }

            /// <summary>
            /// �J�������������ǉ�
            /// </summary>
            /// <param name="hds"></param>
            public void AddRange(ICollection<ColumnHeader> hds)
            {
                _hdrs.AddRange(hds);
                for (var i = 0; i < hds.Count; i++)
                {
                    _summaryMode.Add(TListViewFree.SummaryMode.OneOfRecord);
                }
            }

            /// <summary>
            /// �J�����w�b�_
            /// </summary>
            /// <param name="idx"></param>
            /// <returns></returns>
            public ColumnHeader this[int idx] => _hdrs[idx];

            /// <summary>
            /// �T�}���[���[�h
            /// </summary>
            public IList<SummaryMode> SummaryMode => _summaryMode;

            /// <summary>
            /// �w�b�_�̃J�E���g
            /// </summary>
            public int Count => _hdrs.Count;

            #region IEnumerable<ColumnHeader> �����o

            public IEnumerator<ColumnHeader> GetEnumerator()
            {
                return _hdrs.GetEnumerator();
            }

            #endregion

            #region IEnumerable �����o

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _hdrs.GetEnumerator();
            }

            #endregion
        }


        #region �C�x���g
        /// <summary>
        /// �J�����N���b�N�C�x���g�i�\�[�g�Ȃǁj
        /// </summary>
        public event ColumnClickEventHandler ColumnClick;

        /// <summary>
        /// �t�B���^�[���s�v����������
        /// </summary>
        public event ColumnClickEventHandler FilterRequested;

        /// <summary>
        /// �O���[�v���L�[���ύX���ꂽ
        /// </summary>
        public event ColumnClickEventHandler GroupKeyChanged;

        /// <summary>
        /// �t�@�W�[������쐬�⏕�̗v��
        /// </summary>
        public event EventHandler<StringAugmentEventArgs> RequestFuzzyString;

        /// <summary>
        /// �c�[���`�b�v�ɏ��e�L�X�g�����v���C�x���g
        /// </summary>
        public event EventHandler<TooltipTextRequestArgs> TooltipTextRequest;


        #endregion

        /// <summary>
        /// �B��̃f�[�^�C���X�^���X
        /// </summary>
        public HotData _data = new HotData();

        /// <summary>
        /// �B��̕\���p�e���|�����f�[�^�C���X�^���X
        /// </summary>
        public FreeViewPartsCollection _parts = new FreeViewPartsCollection();

        /// <summary>
        /// �B��A�\���p�p�[�c
        /// </summary>
        public PartsViewFree _view = new PartsViewFree();

        /// <summary>
        /// �V�m�j���n���h���[
        /// </summary>
        private Synonym _synonim = null;

        /// <summary>
        /// �\�z�q
        /// </summary>
        public TListViewFree()
        {
            InitializeComponent();
            Load += new EventHandler(coListView_Load);
        }

        /// <summary>
        /// �A�C�e���̃X�e�[�^�X���擾����
        /// </summary>
        /// <param name="lvi"></param>
        /// <returns></returns>
        public HotData.Status GetListViewItemStatus(ListViewItem lvi)
        {
            return _data.GetStatus(lvi);
        }

        /// <summary>
        /// �V�m�j����ݒ肷��
        /// </summary>
        /// <param name="synonim"></param>
        public void SetSynonim(Synonym synonim)
        {
            _synonim = synonim;
        }

        /// <summary>
        /// �R���g���[������������	
        /// </summary>
        /// <param name="e"></param>
        private void coListView_Load(object sender, EventArgs e)
        {
            // �t�B�[�`���[�̏�����
            TabStop = true;
            rpResource.TabStop = true;
            frMain.TabStop = true;

            frMain.IsDrawEmptyBackground = false;
            FeatureLoader2.SetUsingClass(GetType());
            FeatureLoader2.SetResources(Properties.Resources.ResourceManager);
            var root = frMain.GetFeatureRoot();
            root.AssignAppData(_data);
            root.AssignLink(new DataLink());
            root.AssignPartsSet(_parts);

            _data.GroupKeyChanged += new ColumnClickEventHandler(_data_GroupKeyChanged);

            // �t�B�[�`���[�o�^
            root.AddChildFeature(typeof(FeatureKeyZoom));
            root.AddChildFeature(typeof(FeatureDragColumnZoom));
            root.AddChildFeature(typeof(FeatureDragHeightZoom));
            root.AddChildFeature(typeof(FeatureDragScroll)).ParseParameter("Trigger=MIDDLE");
            root.AddChildFeature(typeof(FeatureDragScroll)).ParseParameter("Trigger=CTRL+SHIFT+BUTTON");
            root.AddChildFeature(typeof(FeatureWheelScrollXYRev)).ParseParameter("Attr=SHIFT");
            root.AddChildFeature(typeof(FeatureScrollBarHorz)).ParseParameter("Pane=Resource;Speed=0.15");
            root.AddChildFeature(typeof(FeatureScrollBarVert)).ParseParameter("Pane=Resource");
            root.AddChildFeature(typeof(FeatureZoomScrollNormalizer));

            var ltt = (FeatureListTooltip)root.AddChildFeature(typeof(FeatureListTooltip));
            ltt.TooltipTextRequest += new EventHandler<TooltipTextRequestArgs>(ltt_TooltipTextRequest);

            var ck = (FeatureCopyKey)root.AddChildFeature(typeof(FeatureCopyKey));
            ck.SetSynonym(_synonim);

            var wz = (FeatureWheelZoom)root.AddChildFeature(typeof(FeatureWheelZoom));
            wz.SetTrigger(new MouseState.Buttons(false, false, true, false, false));

            var wsc = (FeatureWheelScroll)root.AddChildFeature(typeof(FeatureWheelScroll));
            wsc.SetTrigger(new MouseState.Buttons(false, false, false, false, false));

            var fe = (FeatureFilterEdit)root.AddChildFeature(typeof(FeatureFilterEdit));
            fe.FilterRequested += new ColumnClickEventHandler(fe_FilterRequested);

            var cch = (FeatureColumnClickHandler)root.AddChildFeature(typeof(FeatureColumnClickHandler));
            cch.ColumnClick += new ColumnClickEventHandler(cch_ColumnClick);

            var cm = (FeatureContextMenu)root.AddChildFeature(typeof(FeatureContextMenu));
            cm.SetContextMenu(contextMenuStrip1);
            cm.FilterRequested += new ColumnClickEventHandler(fe_FilterRequested);
            cm.RequestFuzzyString += new EventHandler<StringAugmentEventArgs>(cm_RequestFuzzyString);

            // �p�[�c�o�^
            _view.SetData(_data, (FreeViewPartsCollection)root.GetPartsSet());
            root.GetPartsSet().Add(rpResource, _view);
        }

        /// <summary>
        /// �c�[���`�b�v�e�L�X�g�C�x���g�]��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ltt_TooltipTextRequest(object sender, TListViewFree.TooltipTextRequestArgs e)
        {
            TooltipTextRequest?.Invoke(sender, e);
        }

        /// <summary>
        /// �t�@�W�[������쐬�v��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cm_RequestFuzzyString(object sender, TListViewFree.StringAugmentEventArgs e)
        {
            RequestFuzzyString?.Invoke(sender, e);
        }

        /// <summary>
        /// �C�x���g���n���i�O���[�v�L�[�ύX�j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _data_GroupKeyChanged(object sender, ColumnClickEventArgs e)
        {
            GroupKeyChanged?.Invoke(sender, e);
        }

        /// <summary>
        /// �C�x���g���n���i�t�B���^���N�G�X�g�j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fe_FilterRequested(object sender, ColumnClickEventArgs e)
        {
            FilterRequested?.Invoke(sender, e);
        }

        /// <summary>
        /// �C�x���g���n���i�J�����N���b�N�j
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cch_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ColumnClick?.Invoke(sender, e);
        }

        /// <summary>
        /// �A�C�e���̃R���N�V����
        /// </summary>
        public ListViewItemCollection Items => _data.Items;

        /// <summary>
        /// �t�B���^��Ԃ̂��̂��܂߂āA���ׂẴA�C�e��
        /// �i���Ԃ�����j
        /// </summary>
        public ICollection<ListViewItem> GetAllItems()
        {
            return _data.GetAllItems();
        }

        /// <summary>
        /// �w�b�_�̃R���N�V����
        /// </summary>
        public ColumnHeaderCollection Columns => _data.Columns;

        /// <summary>
        /// �t�B���^�[�̃R���N�V����
        /// </summary>
        public List<string> Filters => _data.Filters;

        /// <summary>
        /// �C���[�W���X�g
        /// </summary>
        public ImageList SmallImageList
        {
            get => _parts.ImageList;
            set
            {
                _parts.ImageList = value;
                _view.SetImageList(value);
            }
        }

        /// <summary>
        /// �C���[�W���X�g
        /// </summary>
        public ImageList StateImageList
        {
            get => _parts.ImageList;
            set
            {
                _parts.ImageList = value;
                _view.SetImageList(value);
            }
        }

        /// <summary>
        /// �\�[�g�J�n
        /// </summary>
        public void Sort()
        {
            var lvis = new List<ListViewItem>();
            foreach (ListViewItem lvi in _data.Items)
            {
                lvis.Add(lvi);
            }
            lvis.Sort(listComparer);
            _data.Items.Clear();
            _data.Items.AddRange(lvis);
            Invalidate();
        }

        /// <summary>
        /// �\�[�g�p��r����
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private int listComparer(ListViewItem x, ListViewItem y)
        {
            if (_sorter != null)
            {
                return _sorter.Compare(x, y);
            }
            else
            {
                return string.Compare(x.Text, y.Text);
            }
        }

        /// <summary>
        /// �\���X�V
        /// </summary>
        public new void Invalidate()
        {
            frMain.Invalidate();
        }

        private IComparer _sorter = null;

        #region �_�~�[���\�b�h

        /// <summary>
        /// �\�[�g���\�b�h
        /// </summary>
        public IComparer ListViewItemSorter
        {
            get => _sorter;
            set => _sorter = value;
        }

        /// <summary>
        /// �\�[�g��ԁi�w���j
        /// </summary>
        public SortOrder Sorting
        {
            get => SortOrder.None;
            set
            {
            }
        }

        /// <summary>
        /// �I����Ԃ̕\�����@
        /// </summary>
        public bool FullRowSelect
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// �I����Ԃ̕\�����@
        /// </summary>
        public bool GridLines
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// �I����Ԃ̕\�����@
        /// </summary>
        public bool HideSelection
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// �I����Ԃ̕\�����@
        /// </summary>
        public bool ShowItemToolTips
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// �I����Ԃ̕\�����@
        /// </summary>
        public bool UseCompatibleStateImageBehavior
        {
            get => true;
            set
            {
            }
        }

        /// <summary>
        /// �r���[�̌`���iDetails�Œ�j
        /// </summary>
        public System.Windows.Forms.View View
        {
            get => View.Details;
            set
            {
            }
        }

        #endregion

        /// <summary>
        /// �w��s��������ʒu�܂ŃX�N���[������
        /// </summary>
        /// <param name="p"></param>
        public void EnsureVisible(int p)
        {
            frMain.Scroll = ScreenPos.FromInt(frMain.Scroll.X, 0);
            Invalidate();
        }

        /// <summary>
        /// �S�t�B���^�[����
        /// </summary>
        public void FilterOff()
        {
            _data.FilterOff();
        }

        /// <summary>
        /// �s�̕\����Ԃ�ύX
        /// �i�x���̂ŁASetVisible(ICollection...�o�[�W�������g���Ă��������j
        /// �\�[�g�͕����
        /// </summary>
        /// <param name="lvi"></param>
        /// <param name="sw"></param>
        public void SetVisible(ListViewItem lvi, bool sw)
        {
            _data.SetVisible(lvi, sw);
        }

        /// <summary>
        /// �\����Ԃ̕ύX�i�܂Ƃ߂Ă��̂ő����j
        /// �\�[�g�͕����
        /// </summary>
        /// <param name="items"></param>
        /// <param name="sw"></param>
        public void SetVisible(ICollection<ListViewItem> items, bool sw)
        {
            _data.SetVisible(items, sw);
        }

        /// <summary>
        /// �w���\���R�[�h�ɑ΂���A�S�B��s��񋓁i�x���j
        /// </summary>
        /// <param name="lvi"></param>
        /// <returns></returns>
        public ICollection<ListViewItem> GetHiddenRecord(ListViewItem lvi)
        {
            return _data.GetHiddenRows(lvi);
        }

        /// <summary>
        /// �w��J�����̃R�[�h����ԍ����擾����
        /// </summary>
        /// <param name="code"></param>
        /// <returns>-1=�G���[, ��̔ԍ� Columns[���̔ԍ�]</returns>
        public int GetColumnNo(string code)
        {
            for (var i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].Name.Equals(code))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

