// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// レコード基本抽象クラス（パーツに対するリンクの対象）
    /// </summary>
    [Serializable]
    public abstract class RecordBase : ICloneable
    {
        #region ICloneable メンバ

        public abstract object Clone();

        #endregion
    }

    /// <summary>
    /// テーブルの基本クラス
    /// </summary>
    [Serializable]
    public abstract class RecordCommon : RecordBase
    {
        #region 属性（シリアライズする)
        private static int _instanceIDCounter = 0;

        /// <summary>レコードを識別するためのユニークなID</summary>
        private readonly int _instanceID = _instanceIDCounter++;

        #endregion

        /// <summary>
        /// デバッグ用の値
        /// </summary>
        public string _ => ToString();

        /// <summary>
        /// データ格納後に必要な初期化処理等を実行する（２回呼ばれることがある）
        /// </summary>
        public virtual void Construct()
        {
        }

        #region ICloneable メンバ
        public override object Clone()
        {
            var ret = Activator.CreateInstance(GetType());
            return ret;
        }
        #endregion

        /// <summary>
        /// 値を正規化する
        /// </summary>
        /// <returns></returns>
        public virtual RecordCommon Normalize(LogUtil logger)
        {
            return this;
        }

        /// <summary>
        /// 値セットが許可されるかを調査する
        /// </summary>
        protected void checkSettable()
        {
#if DEBUG
            var col = TableCollection.GetCollectionByInstanceID(InstanceID);
            if (col == null)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(col.IsReadonly == false, col.TableName + " テーブルは読み取り専用にセットされています");
#endif
        }

        /// <summary>
        /// レコードの文字列を作成する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var s = GetType().Name + " ID=" + InstanceID.ToString() + " ";
            var ss = GetStrings();
            for (var i = 0; i < ss.Length; i++)
            {
                if (i > 0)
                {
                    s += ", ";
                }
                s += ss[i];
            }
            return s;
        }

        /// <summary>
        /// レコードデータ固有のインスタンスID
        /// </summary>
        public int InstanceID => _instanceID;

        /// <summary>
        /// フィールドを文字列指定で取得する
        /// </summary>
        public object this[FieldInfo fi]
        {
            get
            {
                if (fi == null)
                {
                    return null;
                }
                return fi.GetValue(this);
            }
            set
            {
                System.Diagnostics.Debug.Assert(fi != null, "tRecordBase[FieldInfo] = ?? を使用しましたが、該当レコードは一件もありませんでした");
                fi.SetValue(this, value);
            }
        }

        internal static IDictionary/*<Type,IDictionary<string key,FieldInfo>>*/ fInfos = new HybridDictionary();

        /// <summary>
        /// フィールド情報を取得する
        /// </summary>
        /// <param name="key">DBの属性名 (DBSchemaAttributeのName)</param>
        /// <returns>.NETのフィールド情報</returns>
        public FieldInfo GetFieldInfo(string key)
        {
            return GetFieldInfo(key, GetType());
        }

        /// <summary>
        /// フィールド情報を取得する
        /// </summary>
        /// <param name="key">DBの属性名 (DBSchemaAttributeのName)</param>
        /// <param name="type">tRecordBaseのタイプ</param>
        /// <returns>.NETのフィールド情報</returns>
        public static FieldInfo GetFieldInfo(string key, Type type)
        {
            // キャッシュ
            var fs = (IDictionary)fInfos[type];
            if (fs != null)
            {
                var fi = (FieldInfo)fs[key];
                if (fi != null)
                {
                    return fi;
                }
            }
            else
            {
                fInfos[type] = (fs = new HybridDictionary());
            }

            // メンバ変数をスキャンする
            foreach (var fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var ats = fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (ats.Length > 0)
                {
                    var sc = (DBSchemaAttribute)ats[0];
                    if (sc.Name == key)
                    {
                        fs[key] = fi;
                        return fi;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// フィールドを文字列指定で取得する
        /// </summary>
        public object this[string key]
        {
            get
            {
                var fi = GetFieldInfo(key);
                if (fi != null)
                {
                    return fi.GetValue(this);
                }
                return null;
            }
            set
            {
                var fi = GetFieldInfo(key);
                if (fi != null)
                {
                    fi.SetValue(this, value);
                }
                // メンバ変数をスキャンする
                //foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                //{
                //    // 属性[DBSchema]がついているものだけを選ぶ
                //    object[] ats = fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                //    if (ats.Length > 0)
                //    {
                //        DBSchemaAttribute sc = (DBSchemaAttribute)ats[0];
                //        if (sc.Name == key)
                //        {
                //            fi.SetValue(this, value);
                //            return;
                //        }
                //    }
                //}
            }
        }

        /// <summary>
        /// 所持データの全取得
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetStrings()
        {
            IList buf = new ArrayList();

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields())
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    buf.Add(fi.GetValue(this).ToString());
                }
            }
            var ret = new string[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        public virtual IList GetList()
        {
            IList buf = new ArrayList();

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields())
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    buf.Add(fi.GetValue(this));
                }
            }
            //			string[] ret = new string[buf.Count];
            //			buf.CopyTo(ret, 0);
            //			return ret;
            return buf;
        }


        /// <summary>
        /// 所持データの全取得（並び順を指定する）
        /// </summary>
        /// <param name="order">並び順が指定できる</param>
        /// <returns>データ</returns>
        public virtual string[] GetStrings(IList order)
        {
            IList buf = new ArrayList();    // データを一時格納するバッファ
            IList odr = new ArrayList();    // 表示順を考えるバッファ

            // 表示順を考えるバッファを構築する
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeaderなら、Textプロパティを反映
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // そうでなければ、ToString()を反映
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    var pos = buf.Count;    // 文字列挿入場所（暫定）
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr によると、順番は？
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // 順番を整える
                        }
                    }
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    buf.Insert(pos, fi.GetValue(this).ToString());
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }

            // 戻り値用にバッファを作る
            var ret = new string[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// 所持データの全取得(Hashtableで取得)
        /// </summary>
        /// <returns>データ</returns>
        public virtual ICollection GetObjects()
        {
            IDictionary buf = new Hashtable();  // データを一時格納するバッファ

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    var oo = fi.GetValue(this);
                    if (oo is int && (int)oo == int.MaxValue)
                    {
                        buf.Add(DBSchemaAttr[0].Name, "");
                    }
                    else
                    {
                        buf.Add(DBSchemaAttr[0].Name, fi.GetValue(this));
                    }
                }
            }

            if (buf.Count == 0)
            {
                return null;
            }
            return buf;
        }

        /// <summary>
        /// 所持データの全取得（並び順を指定する）
        /// </summary>
        /// <param name="order">並び順が指定できる</param>
        /// <returns>データ</returns>
        public virtual DBRelationAttribute[] GetRelations(IList order)
        {
            IList buf = new ArrayList();    // データを一時格納するバッファ
            IList odr = new ArrayList();    // 表示順を考えるバッファ

            // 表示順を考えるバッファを構築する
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeaderなら、Textプロパティを反映
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // そうでなければ、ToString()を反映
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    var pos = buf.Count;    // 文字列挿入場所（暫定）
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr によると、順番は？
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // 順番を整える
                        }
                    }
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    var ats = fi.GetCustomAttributes(typeof(DBRelationAttribute), true);
                    if (ats.Length > 0)
                    {
#if DEBUG
                        var dr = (DBRelationAttribute)ats[0];
#endif
                        buf.Insert(pos, ats[0]);
                    }
                    else
                    {
                        buf.Insert(pos, new DBRelationAttribute("?", "?"));
                    }
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }

            // 戻り値用にバッファを作る
            var ret = new DBRelationAttribute[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// 所持データの全取得
        /// </summary>
        /// <returns>データ</returns>
        public virtual ICollection GetRelations()
        {
            IDictionary buf = new Hashtable();  // データを一時格納するバッファ

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    var ats = fi.GetCustomAttributes(typeof(DBRelationAttribute), true);
                    if (ats.Length > 0)
                    {
#if DEBUG
                        var dr = (DBRelationAttribute)ats[0];
#endif
                        buf.Add(DBSchemaAttr[0].Name, ats[0]);
                    }
                    else
                    {
                        buf.Add(DBSchemaAttr[0].Name, new DBRelationAttribute("?", "?"));
                    }
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }
            return buf;
        }

        /// <summary>
        /// 所持データの全取得（並び順を指定する）
        /// </summary>
        /// <param name="order">並び順が指定できる</param>
        /// <returns>データ</returns>
        public virtual Type[] GetTypes(IList order)
        {
            IList buf = new ArrayList();    // データを一時格納するバッファ
            IList odr = new ArrayList();    // 表示順を考えるバッファ

            // 表示順を考えるバッファを構築する
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeaderなら、Textプロパティを反映
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // そうでなければ、ToString()を反映
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    var pos = buf.Count;    // 文字列挿入場所（暫定）
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr によると、順番は？
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // 順番を整える
                        }
                    }
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    buf.Insert(pos, fi.FieldType);
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }

            // 戻り値用にバッファを作る
            var ret = new Type[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// 所持データの全取得
        /// </summary>
        /// <returns>データ</returns>
        public virtual ICollection GetTypes()
        {
            // メンバ変数をスキャンする
            ICollection buf = new Hashtable();
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // インスタンスの値の ToString() をコールして 戻り値に含める
                    ((Hashtable)buf).Add(DBSchemaAttr[0].Name, fi.FieldType);
                }
            }

            if (buf.Count == 0)
            {
                return null;
            }
            return buf;
        }

        /// <summary>
        /// 所持データを保存する
        /// </summary>
        /// <param name="data">保存するデータ</param>
        /// <returns></returns>
        public virtual void SetObjects(IDictionary data)
        {
            // メンバ変数をスキャンする
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // 属性[DBSchema]がついているものだけを選ぶ
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    if (data[DBSchemaAttr[0].Name] == null)
                    {
                        continue;
                    }

                    if (fi.FieldType == typeof(Id))
                    {
                        Id id = new Id { Value = (int)data[DBSchemaAttr[0].Name] };
                        fi.SetValue(this, id);
                    }
                    else if (fi.FieldType == typeof(DateTimeEx))
                    {
                        fi.SetValue(this, DateTimeEx.FromMinutes((int)data[DBSchemaAttr[0].Name]));
                    }
                    else if (fi.FieldType == typeof(bool))
                    {
                        fi.SetValue(this, (bool)data[DBSchemaAttr[0].Name]);
                    }
                    else
                    {
                        fi.SetValue(this, data[DBSchemaAttr[0].Name]);
                    }
                }
            }

        }
    }
}
