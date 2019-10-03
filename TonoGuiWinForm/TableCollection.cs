using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �e�[�u���Ǘ��N���X
    /// ���̃N���X�́A�e�[�u���Ǘ��ɓ������ꂽ�R���N�V�����B
    /// Data�ɏ��L����Ƃ��ɂ̂݁A�p���邱�Ƃ��o���܂��BArrayList�֗̕��ł̂悤�Ɏg�����Ƃ��ł��܂���B
    /// ���̃N���X�́A�etRecordBase�̃C���X�^���XID���Ǘ����܂��BAdd/Remove�͂��̂��߂ɒx���ł��B
    /// 
    /// Readonly�A�[�L�e�N�`��
    /// ���̃I�u�W�F�N�g��SetReadonly()�����s����ƁA�Ȍヌ�R�[�h�ւ̃A�N�Z�X�ŃG���[���o�܂��iDebug���̂݁j
    /// RecordCommon �̃����o�͕K���v���p�e�BSetter�����A���̒���checkSettable�����s����K�v������܂��B
    /// RecordCommon �����o��(uTime�̂悤�Ɂj�Q�ƌ^�I�u�W�F�N�g�̏ꍇ�A���̃N���X��IReadonly����������K�v������܂��B
    /// IReadony����������I�u�W�F�N�g�̑O�����o�̓v���p�e�BSetter�����A���̒���Readonly�����EAssert���������Ă��������B
    /// </summary>
    public class TableCollection : ICollection, IReadonlyable
    {
        #region		����(�V���A���C�Y����)
        /** <summary>�e�[�u���̃t�B�[���h�\��</summary> */
        private System.Data.DataColumnCollection _DataColumn = null;
        private readonly IDictionary _dat = null;

        /// <summary>���R�[�h�̌^</summary>
        private readonly Type _recordType;

        /// <summary>�����񂩂炱�̂��̃R���N�V�����̃C���X�^���X���擾�ł��邽�߂̃L�[</summary>
        private readonly string _collectionKey;

        /// <summary>���[�h�I�����[�t���O</summary>
        private bool _isReadonly = false;

        #endregion
        #region		����(�V���A���C�Y���Ȃ�)
        private static readonly IDictionary _instanceIdToRec = new Hashtable();
        private static readonly IDictionary _instanceIdToCol = new Hashtable();
        private static readonly IDictionary _keyToInstance = new Hashtable();

        #endregion

#if DEBUG
        public string _
        {
            get
            {
                if (_recordType == null)
                {
                    return "NULL record type  N = " + Count.ToString();
                }
                return _recordType.Name + "  N = " + Count.ToString();
            }
        }
#endif
        public override string ToString()
        {
            object s = TableName;
            if (s == null)
            {
                return base.ToString();
            }
            return s.ToString();
        }

        private static bool _finder(Type typeObj, object criteriaObj)
        {
            return typeObj.Name.CompareTo(criteriaObj) == 0;
        }

        /// <summary>
        /// �ǂݎ���p�t���O��t�^����
        /// </summary>
        public void SetReadonly()
        {
            _isReadonly = true;
            foreach (string fnm in GetFieldNames(true))
            {
                var fi = GetFieldInfo(fnm);
                var res = fi.FieldType.FindInterfaces(new TypeFilter(_finder), "IReadonlyable");
                if (res.Length > 0)
                {
                    foreach (RecordCommon rec in this)
                    {
                        var sw = (IReadonlyable)rec[fi];
                        if (sw != null)	// null�̏ꍇ�A�K��tCollection.setter���o�R���Ēl������̂ŁA�����ł͖������č����x���Ȃ�
                        {
                            sw.SetReadonly();	// uTime�̂悤�ɁATotalSeconds�̗l�ȃv���p�e�B����Œl���ς��Ȃ��悤�ɁA�t���O���Z�b�g����
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ǂݎ��t���O���Z�b�g����Ă��邩�ǂ����𒲂ׂ�
        /// </summary>
        public bool IsReadonly => _isReadonly;

        /// <summary>
        /// �w�肵���C���X�^���XID�̃��R�[�h���擾����
        /// </summary>
        /// <param name="instanceID">�C���X�^���XID</param>
        /// <returns>�C���X�^���X</returns>
        public static RecordCommon GetRecordByInstanceID(int instanceID)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (RecordCommon)_instanceIdToRec[instanceID];
            }
        }

        /// <summary>
        /// �w�肵���C���X�^���XID�̃R���N�V�������擾����
        /// </summary>
        /// <param name="instanceID">�C���X�^���XID</param>
        /// <returns>�C���X�^���X</returns>
        public static TableCollection GetCollectionByInstanceID(int instanceID)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (TableCollection)_instanceIdToCol[instanceID];
            }
        }

        /// <summary>
        /// �L�[�����񂩂�R���N�V�������擾����
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <returns>�R���N�V�����̃C���X�^���X</returns>
        public static TableCollection GetCollectionByKey(string key)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (TableCollection)_keyToInstance[key];
            }
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        /// <param name="recordType">���R�[�h�̌^</param>
        /// <param name="collectionInstance">�R���N�V�����̃C���X�^���X</param>
        public TableCollection(Type recordType)
        {
            // lock �͂��Ȃ��B���R�Fda�`�ň�x���萶���������̂�����B
            //_dat = new ArrayList();
            _dat = new Hashtable();
            _recordType = recordType;
            _collectionKey = _recordType.Name;
            while (TableCollection._keyToInstance.Contains(_collectionKey))
            {
                _collectionKey += "+";
            }
            _keyToInstance[_collectionKey] = this;
        }

        /// <summary>
        /// �C���X�^���X�����ł��镶������擾����
        /// </summary>
        /// <returns>�L�[������</returns>
        public string GetCollectionKey()
        {
            return _collectionKey;
        }


        /// <summary>
        /// ���R�[�h�̌^�𒲂ׂ�
        /// </summary>
        public Type RecordType => _recordType;

        /// <summary>
        /// �e�[�u�����̎擾/�ݒ�
        /// </summary>
        public virtual string TableName
        {
            get
            {
                var atts = _recordType.GetCustomAttributes(typeof(DBTableClassAttribute), true);
                if (atts.Length > 0)
                {
                    return ((DBTableClassAttribute)atts[0]).Name;
                }
                return null;
            }
        }

        /// <summary>
        /// �N���X����e�[�u���X�L�[�}���擾����
        /// </summary>
        /// <returns>�X�L�[�}���̈ꗗ</returns>
        public IList GetFieldNames(bool isNonSchema)
        {
            var ret = new ArrayList();
            foreach (var fi in RecordType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var ats = fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (ats.Length > 0)
                {
                    var sc = (DBSchemaAttribute)ats[0];
                    ret.Add(sc.Name);
                }
                else
                {
                    if (isNonSchema)
                    {
                        var ats2 = fi.GetCustomAttributes(typeof(DBNonSchemaAttribute), true);
                        if (ats2.Length > 0)
                        {
                            ret.Add(fi.Name);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �t�B�[���h�����擾����
        /// </summary>
        /// <param name="key">DB�̑����� (DBSchemaAttribute��Name)</param>
        /// <returns>.NET�̃t�B�[���h���</returns>
        public FieldInfo GetFieldInfo(string key)
        {
            var finfo = (IDictionary)RecordCommon.fInfos[GetType()];
            if (finfo != null)
            {
                var fi = (FieldInfo)RecordCommon.fInfos[key];
                if (fi != null)
                {
                    return fi;
                }
            }
            else
            {
                RecordCommon.fInfos[GetType()] = (finfo = new HybridDictionary());
            }

            foreach (var fi in RecordType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var ats = fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (ats.Length > 0)
                {
                    var sc = (DBSchemaAttribute)ats[0];
                    if (sc.Name == key)
                    {
                        finfo[key] = fi;
                        return fi;
                    }
                }
                else
                {
                    var ats2 = fi.GetCustomAttributes(typeof(DBNonSchemaAttribute), true);
                    if (ats2.Length > 0)
                    {
                        if (fi.Name == key)
                        {
                            finfo[key] = fi;
                            return fi;
                        }
                    }
                }
            }
            return null;
            #region ����
            //			if( _dat.Count > 0 )
            //			{
            //				tRecordBase rec = (tRecordBase)_dat[0];
            //				return rec.GetFieldInfo(key);
            //			}
            //			return null;
            #endregion
        }

        /// <summary>
        /// �t�B�[���h�\���̎擾/�ݒ�i���܂�g�p���Ȃ��ŁAGetFieldNames���g�p���鎖�j
        /// </summary>
        public virtual System.Data.DataColumnCollection DataColumn
        {
            get => _DataColumn;
            set => _DataColumn = value;
        }

        #region IList �����o

        /// <summary>
        /// �f�[�^�Ǘ����l�����Ȃ��Ń��R�[�h���폜����
        /// �i���������p�Ȃ̂ŁAda****.Remove���g�p���Ă��������j
        /// </summary>
        /// <param name="value"></param>
        internal void DirectRemove(object value)
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p�e�[�u�� " + GetType().Name + " ����Remove�͂ł��܂���");
            lock (_instanceIdToRec.SyncRoot)
            {
                _instanceIdToRec.Remove(((RecordCommon)value).InstanceID);
                _instanceIdToCol.Remove(((RecordCommon)value).InstanceID);
                _dat.Remove(value);
            }
        }

        public bool Contains(object value)
        {
            return _dat.Contains(value);
        }

        /// <summary>
        /// �R���N�V�������̃f�[�^���폜����i�C���X�^���X�Ǘ����K�p����j
        /// </summary>
        public void Clear()
        {
            _isReadonly = false;
            lock (_instanceIdToRec.SyncRoot)
            {
                foreach (RecordCommon rb in _dat.Keys)
                {
                    _instanceIdToRec.Remove(rb.InstanceID);
                    _instanceIdToCol.Remove(rb.InstanceID);
                }
                _dat.Clear();
            }
        }

        /// <summary>
        /// �f�[�^�Ǘ������Ȃ��Œǉ�
        /// da****.Add���g�p���Ă�������
        /// </summary>
        /// <param name="value"></param>
        internal void DirectAdd(object value)
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p�e�[�u�� " + GetType().Name + " ��Add�͂ł��܂���");
            if (value is RecordCommon)
            {
                lock (_instanceIdToRec.SyncRoot)
                {
                    var preCount = _instanceIdToRec.Count;
                    _instanceIdToRec[((RecordCommon)value).InstanceID] = value;
                    _instanceIdToCol[((RecordCommon)value).InstanceID] = this;
#if DEBUG
                    if (preCount >= _instanceIdToRec.Count)
                    {
                        System.Diagnostics.Debug.WriteLine(false, "���R�[�h�����Add���ꂽ�B���́A�ꎞ�I��tCollection������āAdaDpose�ɂ��o�^�������R�[�h�������ɒǉ������i���̏ꍇ�AtCollection�ł͂Ȃ��AArrayList���g���ė~�����j");
                    }
#endif
                }
            }
            _dat.Add(value, this);
        }
        #endregion

        #region ICollection �����o

        public bool IsSynchronized => _dat.IsSynchronized;

        public int Count => _dat.Count;

        public void CopyTo(Array array, int index)
        {
            var i = index;
            foreach (var obj in _dat.Keys)
            {
                array.SetValue(obj, i++);
            }
        }

        /// <summary>
        /// ID�Ǘ����s���Ă���̂ŁA�N���X�����ƂȂ�B
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (IsReadonly)
                {
                    return new object();	// �ǂݎ���p�ł́A�X���b�h�Z�[�t���ӎ����Ȃ��ō�����
                }
                else
                {
                    return _dat.SyncRoot;
                }
            }
        }
        #endregion

        #region IEnumerable �����o

        public IEnumerator GetEnumerator()
        {
            return _dat.Keys.GetEnumerator();
        }

        #endregion
    }
}
