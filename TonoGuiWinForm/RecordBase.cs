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
    /// ���R�[�h��{���ۃN���X�i�p�[�c�ɑ΂��郊���N�̑Ώہj
    /// </summary>
    [Serializable]
    public abstract class RecordBase : ICloneable
    {
        #region ICloneable �����o

        public abstract object Clone();

        #endregion
    }

    /// <summary>
    /// �e�[�u���̊�{�N���X
    /// </summary>
    [Serializable]
    public abstract class RecordCommon : RecordBase
    {
        #region �����i�V���A���C�Y����)
        private static int _instanceIDCounter = 0;

        /// <summary>���R�[�h�����ʂ��邽�߂̃��j�[�N��ID</summary>
        private readonly int _instanceID = _instanceIDCounter++;

        #endregion

        /// <summary>
        /// �f�o�b�O�p�̒l
        /// </summary>
        public string _ => ToString();

        /// <summary>
        /// �f�[�^�i�[��ɕK�v�ȏ����������������s����i�Q��Ă΂�邱�Ƃ�����j
        /// </summary>
        public virtual void Construct()
        {
        }

        #region ICloneable �����o
        public override object Clone()
        {
            var ret = Activator.CreateInstance(GetType());
            return ret;
        }
        #endregion

        /// <summary>
        /// �l�𐳋K������
        /// </summary>
        /// <returns></returns>
        public virtual RecordCommon Normalize(LogUtil logger)
        {
            return this;
        }

        /// <summary>
        /// �l�Z�b�g��������邩�𒲍�����
        /// </summary>
        protected void checkSettable()
        {
#if DEBUG
            var col = TableCollection.GetCollectionByInstanceID(InstanceID);
            if (col == null)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(col.IsReadonly == false, col.TableName + " �e�[�u���͓ǂݎ���p�ɃZ�b�g����Ă��܂�");
#endif
        }

        /// <summary>
        /// ���R�[�h�̕�������쐬����
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
        /// ���R�[�h�f�[�^�ŗL�̃C���X�^���XID
        /// </summary>
        public int InstanceID => _instanceID;

        /// <summary>
        /// �t�B�[���h�𕶎���w��Ŏ擾����
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
                System.Diagnostics.Debug.Assert(fi != null, "tRecordBase[FieldInfo] = ?? ���g�p���܂������A�Y�����R�[�h�͈ꌏ������܂���ł���");
                fi.SetValue(this, value);
            }
        }

        internal static IDictionary/*<Type,IDictionary<string key,FieldInfo>>*/ fInfos = new HybridDictionary();

        /// <summary>
        /// �t�B�[���h�����擾����
        /// </summary>
        /// <param name="key">DB�̑����� (DBSchemaAttribute��Name)</param>
        /// <returns>.NET�̃t�B�[���h���</returns>
        public FieldInfo GetFieldInfo(string key)
        {
            return GetFieldInfo(key, GetType());
        }

        /// <summary>
        /// �t�B�[���h�����擾����
        /// </summary>
        /// <param name="key">DB�̑����� (DBSchemaAttribute��Name)</param>
        /// <param name="type">tRecordBase�̃^�C�v</param>
        /// <returns>.NET�̃t�B�[���h���</returns>
        public static FieldInfo GetFieldInfo(string key, Type type)
        {
            // �L���b�V��
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

            // �����o�ϐ����X�L��������
            foreach (var fi in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
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
        /// �t�B�[���h�𕶎���w��Ŏ擾����
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
                // �����o�ϐ����X�L��������
                //foreach (FieldInfo fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                //{
                //    // ����[DBSchema]�����Ă�����̂�����I��
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
        /// �����f�[�^�̑S�擾
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetStrings()
        {
            IList buf = new ArrayList();

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields())
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
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

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields())
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
                    buf.Add(fi.GetValue(this));
                }
            }
            //			string[] ret = new string[buf.Count];
            //			buf.CopyTo(ret, 0);
            //			return ret;
            return buf;
        }


        /// <summary>
        /// �����f�[�^�̑S�擾�i���я����w�肷��j
        /// </summary>
        /// <param name="order">���я����w��ł���</param>
        /// <returns>�f�[�^</returns>
        public virtual string[] GetStrings(IList order)
        {
            IList buf = new ArrayList();    // �f�[�^���ꎞ�i�[����o�b�t�@
            IList odr = new ArrayList();    // �\�������l����o�b�t�@

            // �\�������l����o�b�t�@���\�z����
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeader�Ȃ�AText�v���p�e�B�𔽉f
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // �����łȂ���΁AToString()�𔽉f
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    var pos = buf.Count;    // ������}���ꏊ�i�b��j
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr �ɂ��ƁA���Ԃ́H
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // ���Ԃ𐮂���
                        }
                    }
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
                    buf.Insert(pos, fi.GetValue(this).ToString());
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }

            // �߂�l�p�Ƀo�b�t�@�����
            var ret = new string[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// �����f�[�^�̑S�擾(Hashtable�Ŏ擾)
        /// </summary>
        /// <returns>�f�[�^</returns>
        public virtual ICollection GetObjects()
        {
            IDictionary buf = new Hashtable();  // �f�[�^���ꎞ�i�[����o�b�t�@

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
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
        /// �����f�[�^�̑S�擾�i���я����w�肷��j
        /// </summary>
        /// <param name="order">���я����w��ł���</param>
        /// <returns>�f�[�^</returns>
        public virtual DBRelationAttribute[] GetRelations(IList order)
        {
            IList buf = new ArrayList();    // �f�[�^���ꎞ�i�[����o�b�t�@
            IList odr = new ArrayList();    // �\�������l����o�b�t�@

            // �\�������l����o�b�t�@���\�z����
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeader�Ȃ�AText�v���p�e�B�𔽉f
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // �����łȂ���΁AToString()�𔽉f
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                if (fi.GetCustomAttributes(typeof(DBSchemaAttribute), true).Length > 0)
                {
                    var pos = buf.Count;    // ������}���ꏊ�i�b��j
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr �ɂ��ƁA���Ԃ́H
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // ���Ԃ𐮂���
                        }
                    }
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
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

            // �߂�l�p�Ƀo�b�t�@�����
            var ret = new DBRelationAttribute[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// �����f�[�^�̑S�擾
        /// </summary>
        /// <returns>�f�[�^</returns>
        public virtual ICollection GetRelations()
        {
            IDictionary buf = new Hashtable();  // �f�[�^���ꎞ�i�[����o�b�t�@

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
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
        /// �����f�[�^�̑S�擾�i���я����w�肷��j
        /// </summary>
        /// <param name="order">���я����w��ł���</param>
        /// <returns>�f�[�^</returns>
        public virtual Type[] GetTypes(IList order)
        {
            IList buf = new ArrayList();    // �f�[�^���ꎞ�i�[����o�b�t�@
            IList odr = new ArrayList();    // �\�������l����o�b�t�@

            // �\�������l����o�b�t�@���\�z����
            for (var i = 0; i < order.Count; i++)
            {
                var o = order[i];
                if (o is System.Windows.Forms.ColumnHeader) // ColumnHeader�Ȃ�AText�v���p�e�B�𔽉f
                {
                    odr.Add(((System.Windows.Forms.ColumnHeader)o).Text.ToLower());
                }
                else                                            // �����łȂ���΁AToString()�𔽉f
                {
                    odr.Add(o.ToString().ToLower());
                }
            }

            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    var pos = buf.Count;    // ������}���ꏊ�i�b��j
                    var ii = odr.IndexOf(fi.Name.ToLower());    // odr �ɂ��ƁA���Ԃ́H
                    if (ii >= 0)
                    {
                        if (ii < pos)
                        {
                            pos = ii;   // ���Ԃ𐮂���
                        }
                    }
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
                    buf.Insert(pos, fi.FieldType);
                }
            }
            if (buf.Count == 0)
            {
                return null;
            }

            // �߂�l�p�Ƀo�b�t�@�����
            var ret = new Type[buf.Count];
            buf.CopyTo(ret, 0);
            return ret;
        }

        /// <summary>
        /// �����f�[�^�̑S�擾
        /// </summary>
        /// <returns>�f�[�^</returns>
        public virtual ICollection GetTypes()
        {
            // �����o�ϐ����X�L��������
            ICollection buf = new Hashtable();
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
                var DBSchemaAttr = (DBSchemaAttribute[])fi.GetCustomAttributes(typeof(DBSchemaAttribute), true);
                if (DBSchemaAttr.Length > 0)
                {
                    // �C���X�^���X�̒l�� ToString() ���R�[������ �߂�l�Ɋ܂߂�
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
        /// �����f�[�^��ۑ�����
        /// </summary>
        /// <param name="data">�ۑ�����f�[�^</param>
        /// <returns></returns>
        public virtual void SetObjects(IDictionary data)
        {
            // �����o�ϐ����X�L��������
            foreach (var fi in GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                // ����[DBSchema]�����Ă�����̂�����I��
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
