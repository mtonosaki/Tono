using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// テーブル管理クラス
    /// このクラスは、テーブル管理に特化されたコレクション。
    /// Dataに所有するときにのみ、用いることが出来ます。ArrayListの便利版のように使うことができません。
    /// このクラスは、各tRecordBaseのインスタンスIDを管理します。Add/Removeはこのために遅いです。
    /// 
    /// Readonlyアーキテクチャ
    /// このオブジェクトのSetReadonly()を実行すると、以後レコードへのアクセスでエラーが出ます（Debug環境のみ）
    /// RecordCommon のメンバは必ずプロパティSetter化し、その中でcheckSettableを実行する必要があります。
    /// RecordCommon メンバが(uTimeのように）参照型オブジェクトの場合、そのクラスはIReadonlyを実装する必要があります。
    /// IReadonyを実装するオブジェクトの前メンバはプロパティSetter化し、その中でReadonly検査・Assertを実装してください。
    /// </summary>
    public class TableCollection : ICollection, IReadonlyable
    {
        #region		属性(シリアライズする)
        /** <summary>テーブルのフィールド構成</summary> */
        private System.Data.DataColumnCollection _DataColumn = null;
        private readonly IDictionary _dat = null;

        /// <summary>レコードの型</summary>
        private readonly Type _recordType;

        /// <summary>文字列からこのこのコレクションのインスタンスが取得できるためのキー</summary>
        private readonly string _collectionKey;

        /// <summary>リードオンリーフラグ</summary>
        private bool _isReadonly = false;

        #endregion
        #region		属性(シリアライズしない)
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
        /// 読み取り専用フラグを付与する
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
                        if (sw != null)	// nullの場合、必ずtCollection.setterを経由して値を入れるので、ここでは無視して差し支えない
                        {
                            sw.SetReadonly();	// uTimeのように、TotalSecondsの様なプロパティ操作で値が変わらないように、フラグをセットする
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 読み取りフラグがセットされているかどうかを調べる
        /// </summary>
        public bool IsReadonly => _isReadonly;

        /// <summary>
        /// 指定したインスタンスIDのレコードを取得する
        /// </summary>
        /// <param name="instanceID">インスタンスID</param>
        /// <returns>インスタンス</returns>
        public static RecordCommon GetRecordByInstanceID(int instanceID)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (RecordCommon)_instanceIdToRec[instanceID];
            }
        }

        /// <summary>
        /// 指定したインスタンスIDのコレクションを取得する
        /// </summary>
        /// <param name="instanceID">インスタンスID</param>
        /// <returns>インスタンス</returns>
        public static TableCollection GetCollectionByInstanceID(int instanceID)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (TableCollection)_instanceIdToCol[instanceID];
            }
        }

        /// <summary>
        /// キー文字列からコレクションを取得する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>コレクションのインスタンス</returns>
        public static TableCollection GetCollectionByKey(string key)
        {
            lock (_instanceIdToRec.SyncRoot)
            {
                return (TableCollection)_keyToInstance[key];
            }
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="recordType">レコードの型</param>
        /// <param name="collectionInstance">コレクションのインスタンス</param>
        public TableCollection(Type recordType)
        {
            // lock はしない。理由：da～で一度限り生成されるものだから。
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
        /// インスタンスを特定できる文字列を取得する
        /// </summary>
        /// <returns>キー文字列</returns>
        public string GetCollectionKey()
        {
            return _collectionKey;
        }


        /// <summary>
        /// レコードの型を調べる
        /// </summary>
        public Type RecordType => _recordType;

        /// <summary>
        /// テーブル名の取得/設定
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
        /// クラスからテーブルスキーマを取得する
        /// </summary>
        /// <returns>スキーマ名の一覧</returns>
        public IList GetFieldNames(bool isNonSchema)
        {
            var ret = new ArrayList();
            foreach (var fi in RecordType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
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
        /// フィールド情報を取得する
        /// </summary>
        /// <param name="key">DBの属性名 (DBSchemaAttributeのName)</param>
        /// <returns>.NETのフィールド情報</returns>
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
                // 属性[DBSchema]がついているものだけを選ぶ
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
            #region 旧式
            //			if( _dat.Count > 0 )
            //			{
            //				tRecordBase rec = (tRecordBase)_dat[0];
            //				return rec.GetFieldInfo(key);
            //			}
            //			return null;
            #endregion
        }

        /// <summary>
        /// フィールド構成の取得/設定（あまり使用しないで、GetFieldNamesを使用する事）
        /// </summary>
        public virtual System.Data.DataColumnCollection DataColumn
        {
            get => _DataColumn;
            set => _DataColumn = value;
        }

        #region IList メンバ

        /// <summary>
        /// データ管理を考慮しないでレコードを削除する
        /// （内部処理用なので、da****.Removeを使用してください）
        /// </summary>
        /// <param name="value"></param>
        internal void DirectRemove(object value)
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用テーブル " + GetType().Name + " からRemoveはできません");
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
        /// コレクション中のデータを削除する（インスタンス管理も適用する）
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
        /// データ管理をしないで追加
        /// da****.Addを使用してください
        /// </summary>
        /// <param name="value"></param>
        internal void DirectAdd(object value)
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用テーブル " + GetType().Name + " にAddはできません");
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
                        System.Diagnostics.Debug.WriteLine(false, "レコードが二回Addされた。又は、一時的なtCollectionを作って、daDposeにも登録したレコードをそこに追加した（この場合、tCollectionではなく、ArrayListを使って欲しい）");
                    }
#endif
                }
            }
            _dat.Add(value, this);
        }
        #endregion

        #region ICollection メンバ

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
        /// ID管理を行っているので、クラス同期となる。
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (IsReadonly)
                {
                    return new object();	// 読み取り専用では、スレッドセーフを意識しないで高速化
                }
                else
                {
                    return _dat.SyncRoot;
                }
            }
        }
        #endregion

        #region IEnumerable メンバ

        public IEnumerator GetEnumerator()
        {
            return _dat.Keys.GetEnumerator();
        }

        #endregion
    }
}
