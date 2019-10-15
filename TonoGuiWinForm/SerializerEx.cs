using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// SerializerEx �̊T�v�̐����ł��B
    /// </summary>
    public class SerializerEx
    {
        private enum Tag : byte
        {
            /*�^�O�n*/
            Null, ClassStart, SubClass, Field, ClassEnd,
            /*�N���X�n*/
            List, Dictionary,
            /*�l�n*/
            vDouble, vSingle, vInt64, vUInt64, vInt32, vUInt32, vInt16, vUInt16, vIntPtr, vSByte, vByte, vBoolean, vString,
            /*�����n*/
            vUTime, vId, Color
        }

        /// <summary>Type��eTag�̊֘A</summary>
        [NonSerialized] private static readonly IDictionary/*<Type,eTag>*/ _typeToTag;
        /// <summary>Type��eTag�̊֘A</summary>
        [NonSerialized] private static readonly IDictionary/*<eTag,Type>*/ _tagToType;

        /// <summary>uTime��Direct�n�T�|�[�g�p�i�������j</summary>
        [NonSerialized] private static readonly FieldInfo uTimeFI = typeof(DateTimeEx).GetField("_val", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


        #region �X�^�e�B�b�N�\�z�q
        /// <summary>
        /// �X�^�e�B�b�N�\�z�q
        /// </summary>
        static SerializerEx()
        {
            _typeToTag = new HybridDictionary
            {
                [typeof(double)] = Tag.vDouble,
                [typeof(float)] = Tag.vSingle,
                [typeof(long)] = Tag.vInt64,
                [typeof(ulong)] = Tag.vUInt64,
                [typeof(int)] = Tag.vInt32,
                [typeof(uint)] = Tag.vUInt32,
                [typeof(short)] = Tag.vInt16,
                [typeof(ushort)] = Tag.vUInt16,
                [typeof(IntPtr)] = Tag.vIntPtr,
                [typeof(sbyte)] = Tag.vSByte,
                [typeof(byte)] = Tag.vByte,
                [typeof(bool)] = Tag.vBoolean,
                [typeof(string)] = Tag.vString,
                [typeof(Id)] = Tag.vId,
                [typeof(DateTimeEx)] = Tag.vUTime,
                [typeof(Color)] = Tag.Color
            };
            _tagToType = new HybridDictionary();
            foreach (DictionaryEntry de in _typeToTag)
            {
                _tagToType[de.Value] = de.Key;
            }
        }
        #endregion

        /// <summary>
        /// �ǂݍ��݊�����ɁA�f�[�^��K�p���鏈���i�W�������F�A�v�������́A�ʂ̂Ƃ���ōs���Ă��������j
        /// </summary>
        /// <param name="value"></param>
        private static void receiveFinalize(object value)
        {
            if (value is PartsBase)
            {
                ((PartsBase)value).InstanciatePartsPositionCorderName();
                ((PartsBase)value).InstanciatePartsPositioner();
            }
        }

        /// <summary>
        /// �w��X�g���[������I�u�W�F�N�g���ЂƂ�M����
        /// </summary>
        /// <param name="reader">�X�g���[��</param>
        /// <returns>�I�u�W�F�N�g�̃C���X�^���X</returns>
        public static object Receive(Stream reader)
        {
            return receiveLoop(reader);
        }

        /// <summary>
        /// �w��X�g���[���̌��݈ʒu�����M����ċA���[�v
        /// </summary>
        /// <param name="reader">�X�g���[��</param>
        /// <returns>�C���X�^���X�����ꂽ�I�u�W�F�N�g</returns>
        private static object receiveLoop(Stream reader)
        {
            object ret = null;
            Type currentType = null;
            Type subClassType = null;

            for (var isLoop = true; isLoop;)
            {
                var tag = (Tag)reader.ReadByte();

                switch (tag)
                {
                    case Tag.ClassStart:
                        {
                            // �N���X����肷�� = currentType = subClassType
                            ReceiveDirect(reader, out int classid);     // �N���X����肷�� = currentType = subClassType

                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "�V���A���C�YClass ID���s���ł���");

                            // �N���X�̃C���X�^���X�𐶐����� = ret
                            currentType = Type.GetType(keys[2]);
                            if (currentType == null)
                            {
                                var hInst = Activator.CreateInstance(keys[3], keys[2]);
                                ret = hInst.Unwrap();
                                currentType = ret.GetType();
                            }
                            else
                            {
                                ret = Activator.CreateInstance(currentType, true);
                            }
                            subClassType = currentType;
                            break;
                        }
                    case Tag.SubClass:
                        {
                            // �T�u�N���X����肷�� = subClassType
                            ReceiveDirect(reader, out                         // �T�u�N���X����肷�� = subClassType
                            int classid);
                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "�V���A���C�YClass ID���s���ł���");

                            subClassType = Type.GetType(keys[2] + ", " + keys[3]);
                            break;
                        }
                    case Tag.Field:
                        {
                            // �t�B�[���h����肷�� = fi
                            ReceiveDirect(reader, out                         // �t�B�[���h����肷�� = fi
                            int fieldid);
                            var keystr = NamedId.FromIDNoName(fieldid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "�V���A���C�YField ID���s���ł���");
                            var fi = subClassType.GetField(keys[2/*field��*/], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                            // �t�B�[���h�̒l���擾����i�ċA�R�[���j
                            var fieldValue = receiveLoop(reader);

                            // �t�B�[���h�̒l���C���X�^���X�ɐݒ肷��
                            fi.SetValue(ret, fieldValue);
                            break;
                        }
                    case Tag.Null:
                        {
                            return null;
                        }
                    case Tag.List:
                        {
                            ReceiveDirect(reader, out int classid);
                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "�V���A���C�YClass ID���s���ł���");

                            // �N���X�̃C���X�^���X�𐶐����� = ret
                            currentType = Type.GetType(keys[2]);
                            if (currentType == null)
                            {
                                var hInst = Activator.CreateInstance(keys[3], keys[2]);
                                ret = hInst.Unwrap();
                                currentType = ret.GetType();
                            }
                            else
                            {
                                ret = Activator.CreateInstance(currentType, true);
                            }

                            // �z�񐔂��擾��
                            ReceiveDirect(reader, out
                            // �z�񐔂��擾��
                            int n);

                            for (var i = 0; i < n; i++)
                            {
                                // �e�v�f�̒l���擾����i�ċA�R�[���j
                                var val = receiveLoop(reader);
                                ((IList)ret).Add(val);
                            }
                            // �T�u�N���X�̂��߂ɏ�������
                            subClassType = currentType;
                            break;
                        }
                    case Tag.Dictionary:
                        {
                            ReceiveDirect(reader, out int classid);
                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "�V���A���C�YClass ID���s���ł���");

                            // �N���X�̃C���X�^���X�𐶐����� = ret
                            currentType = Type.GetType(keys[2]);
                            if (currentType == null)
                            {
                                var hInst = Activator.CreateInstance(keys[3], keys[2]);
                                ret = hInst.Unwrap();
                                currentType = ret.GetType();
                            }
                            else
                            {
                                ret = Activator.CreateInstance(currentType, true);
                            }

                            // �z�񐔂��擾��
                            ReceiveDirect(reader, out
                            // �z�񐔂��擾��
                            int n);

                            for (var i = 0; i < n; i++)
                            {
                                // �e�v�f�̒l���擾����i�ċA�R�[���j
                                var key = receiveLoop(reader);
                                var val = receiveLoop(reader);
                                ((IDictionary)ret).Add(key, val);
                            }
                            // �T�u�N���X�̂��߂ɏ�������
                            subClassType = currentType;
                            break;
                        }
                    case Tag.ClassEnd:
                        {
                            receiveFinalize(ret);
                            return ret;
                        }
                    case Tag.vString:
                        {
                            ReceiveDirect(reader, out string s);
                            return s;
                        }
                    default:
                        {
                            var valType = (Type)_tagToType[tag];
                            var valTypeR = valType.Assembly.GetType(valType.ToString() + "&");
                            ret = Activator.CreateInstance(valType);
                            var types = new Type[] { typeof(Stream), valTypeR };
                            var mi = typeof(SerializerEx).GetMethod("ReceiveDirect", types);
                            ret = mi.Invoke(null, new object[] { reader, ret });
                            receiveFinalize(ret);
                            return ret;
                        }
                }
            }
            receiveFinalize(ret);
            return ret;
        }



        /// <summary>
        /// �w��X�g���[���ɃI�u�W�F�N�g�𑗐M����
        /// </summary>
        /// <param name="writer">�X�g���[��</param>
        /// <param name="value">�I�u�W�F�N�g</param>
        public static void Send(Stream writer, object value)
        {
            sendLoop(writer, value);
        }

        /// <summary>
        /// �w��X�g���[���ɃI�u�W�F�N�g�𑗐M����A�T�C�N���b�N���[�v
        /// </summary>
        /// <param name="writer">�X�g���[��</param>
        /// <param name="value">�I�u�W�F�N�g</param>
        private static void sendLoop(Stream writer, object value)
        {
            // ���M�ł��Ȃ��^��ϊ�����
            if (value != null)
            {
                if (value is EventHandler)
                {
                    value = null;
                }
                if (value is IRichPane && value.GetType() != typeof(RichPaneBinder))
                {
                    value = RichPaneBinder.CreateCopy((IRichPane)value);
                }
                if (value is System.Windows.Forms.Form)
                {
                    value = null;
                }
                if (value is PartsBase)
                {
                    ((PartsBase)value).SetPartsPositionCorderName(true);
                    ((PartsBase)value).SetPartsPositionerName(true);
                }
            }
            // �k���l�̏���
            if (value == null)
            {
                writer.WriteByte((byte)Tag.Null);
                return;
            }

            var tarType = value.GetType();

            var ttag = _typeToTag[tarType];
            if (ttag != null)
            {
                // �^�O�̖��ߍ���
                writer.WriteByte((byte)ttag);

                // �l�̖��ߍ���
                var mi = typeof(SerializerEx).GetMethod("SendDirect", new Type[] { typeof(Stream), tarType });
                mi.Invoke(null, new object[] { writer, value });
                return;
            }


            // �N���XID
            var cid = NamedId.FromName(tarType.FullName);
            var isClassProced = false;
            IDictionary dupchk = new HybridDictionary();

            // ���X�g�̓�������
            if (value is IList)
            {
                // �^�O�̖��ߍ���
                writer.WriteByte((byte)Tag.List);

                // �N���XID
                SendDirect(writer, cid);

                SendDirect(writer, ((IList)value).Count);
                for (var i = 0; i < ((IList)value).Count; i++)
                {
                    sendLoop(writer, ((IList)value)[i]);
                }
                isClassProced = true;
            }

            // �f�B�N�V���i���̓�������
            if (value is IDictionary)
            {
                // �^�O�̖��ߍ���

                writer.WriteByte((byte)Tag.Dictionary);

                // �N���XID
                SendDirect(writer, cid.Id.Value);

                SendDirect(writer, ((IDictionary)value).Count);
                foreach (DictionaryEntry de in ((IDictionary)value))
                {
                    sendLoop(writer, de.Key);
                    sendLoop(writer, de.Value);
                }
                isClassProced = true;
            }

            // �N���X�̏���
            if (isClassProced == false)
            {
                writer.WriteByte((byte)Tag.ClassStart);
                SendDirect(writer, cid.Id.Value);
            }
            else
            {
                // ���X�g�A�f�B�N�V���i���n�ŁA�h���N���X�̃t�B�[���h�͕ۑ��s�v�ƋL���t����
                for (var type = tarType; type != typeof(object); type = type.BaseType)
                {
                    if (type.Namespace.StartsWith("System.") == false)
                    {
                        continue;
                    }
                    const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                    var fis = type.GetFields(flags);
                    foreach (var fi in fis)
                    {
                        if (fi.DeclaringType != fi.ReflectedType)
                        {
                            continue;   // �h���N���X�̕ۑ��t�F�[�Y�ōs��
                        }
                        if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                        {
                            dupchk[fi.Name] = true;
                        }
                    }
                }
            }

            // �`�N���X�t�B�[���h�̏���
            var inheritLevel = 0;
            for (var type = tarType; type != typeof(object); type = type.BaseType)
            {
                var fcnt = 0;
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var fis = type.GetFields(flags);
                foreach (var fi in fis)
                {
                    if (fi.DeclaringType != fi.ReflectedType)
                    {
                        continue;   // �h���N���X�̕ۑ��t�F�[�Y�ōs��
                    }
                    if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        if (dupchk[fi.Name] == null)
                        {
                            dupchk.Add(fi.Name, true);

                            if (fcnt == 0 && inheritLevel > 0)  // �x�[�X�N���X���߂Ẵt�B�[���h�̎��B
                            {
                                // �x�[�X�N���X�J�nTAG�𖄂ߍ���
                                writer.WriteByte((byte)Tag.SubClass);

                                // �N���X�h���掯��ID�𖄂ߍ���
                                var bid = NamedId.FromName(type.FullName);
                                SendDirect(writer, bid.Id.Value);
                            }

                            // �^�O�̖��ߍ���
                            writer.WriteByte((byte)Tag.Field);

                            // �t�B�[���h���𖄂ߍ���
                            var fid = NamedId.FromName($"{type.FullName}...{fi.Name}"); //.FromField(fi);
                            SendDirect(writer, fid.Id.Value);

                            // �t�B�[���h�̒l�𖄂ߍ���
                            sendLoop(writer, fi.GetValue(value));
                            fcnt++;
                        }
                    }
                }
                inheritLevel++;
            }
            // �N���X�I��TAG�𖄂ߍ���
            writer.WriteByte((byte)Tag.ClassEnd);

            // �V���A���C�Y�I�����̊J��������
            if (value is PartsBase)
            {
                ((PartsBase)value).SetPartsPositionCorderName(false);
                ((PartsBase)value).SetPartsPositionerName(false);
            }
        }

        /// <summary>
        /// �V���A���C�Y�Ώۂ̃����o�[��dest��src�ɃR�s�[����
        /// </summary>
        /// <param name="dest">�R�s�[��</param>
        /// <param name="src">�R�s�[��</param>
        public static void CopyObject(object dest, object src)
        {
            IDictionary dat = new HybridDictionary();
            //TODO:���̕ӂ�Null�Q�Ƃ��ł� TONO 2005.12.6
            System.Diagnostics.Debug.Assert(dest != null);
            System.Diagnostics.Debug.Assert(src != null);
            if (dest == null || src == null)
            {
                return;
            }
            for (var type = dest.GetType(); type != typeof(object); type = type.BaseType)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var fis = type.GetFields(flags);
                foreach (var fi in fis)
                {
                    if (fi.DeclaringType != fi.ReflectedType)
                    {
                        continue;   // �h���N���X�̕ۑ��t�F�[�Y�ōs��
                    }
                    if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        if (dat[fi.Name] == null)
                        {
                            fi.SetValue(dest, fi.GetValue(src));
                        }
                    }
                }
            }
        }

        #region �e�^�̃o�C�g�V���A���C�Y

        /// <summary>
        /// Id�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, Id value)
        {
            SendDirect(writer, value.Value);
        }

        /// <summary>
        /// UInt64�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out Id value)
        {
            ReceiveDirect(reader, out int id);
            value = new Id { Value = id };
            return value;
        }

        /// <summary>
        /// uTime�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, DateTimeEx value)
        {
            var val = (int)uTimeFI.GetValue(value);
            SendDirect(writer, val);
        }

        /// <summary>
        /// uTime�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out DateTimeEx value)
        {
            ReceiveDirect(reader, out int val);
            value = new DateTimeEx();
            uTimeFI.SetValue(value, val);
            return value;
        }

        /// <summary>
        /// Color�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, Color value)
        {
            var val = value.ToArgb();
            SendDirect(writer, val);
        }

        /// <summary>
        /// Color�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out Color value)
        {
            ReceiveDirect(reader, out int val);
            value = Color.FromArgb(val);
            return value;
        }

        /// <summary>
        /// UInt64�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, ulong value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[7]);
                writer.WriteByte(p[6]);
                writer.WriteByte(p[5]);
                writer.WriteByte(p[4]);
                writer.WriteByte(p[3]);
                writer.WriteByte(p[2]);
                writer.WriteByte(p[1]);
                writer.WriteByte(p[0]);
            }
        }

        /// <summary>
        /// UInt64�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out ulong value)
        {
            unsafe
            {
                fixed (ulong* pp = &value)
                {
                    var p = ((byte*)pp) + 7;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }


        /// <summary>
        /// Double�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, double value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[0]);
                writer.WriteByte(p[1]);
                writer.WriteByte(p[2]);
                writer.WriteByte(p[3]);
                writer.WriteByte(p[4]);
                writer.WriteByte(p[5]);
                writer.WriteByte(p[6]);
                writer.WriteByte(p[7]);
            }
        }

        /// <summary>
        /// Double�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out double value)
        {
            unsafe
            {
                fixed (double* pp = &value)
                {
                    var p = (byte*)pp;
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// Single�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, float value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[0]);
                writer.WriteByte(p[1]);
                writer.WriteByte(p[2]);
                writer.WriteByte(p[3]);
            }
        }

        /// <summary>
        /// Single�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out float value)
        {
            unsafe
            {
                fixed (float* pp = &value)
                {
                    var p = (byte*)pp;
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                    *p++ = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// Int64�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, long value)
        {
            unsafe
            {
                unsafe
                {
                    var p = (byte*)&value;
                    writer.WriteByte(p[7]);
                    writer.WriteByte(p[6]);
                    writer.WriteByte(p[5]);
                    writer.WriteByte(p[4]);
                    writer.WriteByte(p[3]);
                    writer.WriteByte(p[2]);
                    writer.WriteByte(p[1]);
                    writer.WriteByte(p[0]);
                }
            }
        }

        /// <summary>
        /// Int64�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out long value)
        {
            unsafe
            {
                fixed (long* pp = &value)
                {
                    var p = ((byte*)pp) + 7;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// Int32�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, int value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[3]);
                writer.WriteByte(p[2]);
                writer.WriteByte(p[1]);
                writer.WriteByte(p[0]);
            }
        }

        /// <summary>
        /// Int32�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out int value)
        {
            unsafe
            {
                fixed (int* pp = &value)
                {
                    var p = ((byte*)pp) + 3;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// UInt32�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, uint value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[3]);
                writer.WriteByte(p[2]);
                writer.WriteByte(p[1]);
                writer.WriteByte(p[0]);
            }
        }

        /// <summary>
        /// UInt32�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out uint value)
        {
            unsafe
            {
                fixed (uint* pp = &value)
                {
                    var p = ((byte*)pp) + 3;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// Int32�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, IntPtr value)
        {
            // �Ȃɂ����Ȃ�
        }

        /// <summary>
        /// IntPtr�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out IntPtr value)
        {
            // �Ȃɂ����Ȃ�
            value = IntPtr.Zero;
            return value;
        }

        /// <summary>
        /// Int16�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, short value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[1]);
                writer.WriteByte(p[0]);
            }
        }

        /// <summary>
        /// Int16�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out short value)
        {
            unsafe
            {
                fixed (short* pp = &value)
                {
                    var p = ((byte*)pp) + 1;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// Int16�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, ushort value)
        {
            unsafe
            {
                var p = (byte*)&value;
                writer.WriteByte(p[1]);
                writer.WriteByte(p[0]);
            }
        }

        /// <summary>
        /// UInt16�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out ushort value)
        {
            unsafe
            {
                fixed (ushort* pp = &value)
                {
                    var p = ((byte*)pp) + 1;
                    *p-- = (byte)reader.ReadByte();
                    *p-- = (byte)reader.ReadByte();
                }
            }
            return value;
        }

        /// <summary>
        /// SByte�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, sbyte value)
        {
            writer.WriteByte((byte)value);
        }

        /// <summary>
        /// SByte�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out sbyte value)
        {
            value = (sbyte)reader.ReadByte();
            return value;
        }

        /// <summary>
        /// Byte�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, byte value)
        {
            writer.WriteByte(value);
        }

        /// <summary>
        /// Byte�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out byte value)
        {
            value = (byte)reader.ReadByte();
            return value;
        }

        /// <summary>
        /// Boolean�^�̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, bool value)
        {
            if (value)
            {
                writer.WriteByte(1);
            }
            else
            {
                writer.WriteByte(0);
            }
        }

        /// <summary>
        /// Boolean�^�̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out bool value)
        {
            if (reader.ReadByte() == 0)
            {
                value = false;
            }
            else
            {
                value = true;
            }
            return value;
        }

        /// <summary>
        /// ������̕ۑ�
        /// </summary>
        public static void SendDirect(Stream writer, string value)
        {
            ushort scramble = 0x1914;
            SendDirect(writer, value.Length);
            for (var i = 0; i < value.Length; i++)
            {
                SendDirect(writer, (ushort)(value[i] ^ scramble));
                scramble++;
            }
        }

        /// <summary>
        /// ������̎�M
        /// </summary>
        public static object ReceiveDirect(Stream reader, out string value)
        {
            ushort scramble = 0x1914;
            value = "";
            ReceiveDirect(reader, out int len);
            if (len > 8000000)
            {
                throw new FormatException("This Binary data is illegal format.");
            }
            for (var i = 0; i < len; i++)
            {
                ReceiveDirect(reader, out ushort ci);
                value += (char)(ci ^ scramble);
                scramble++;
            }
            return value;
        }
        #endregion


        #region .NET���W���ŃT�|�[�g����V���A���C�Y���ȗ�������
        /// <summary>
        /// .NET�W���̃V���A���C�Y�������C���v�������g����
        /// </summary>
        /// <param name="topType">�V���A���C�Y����N���X�ŁA���̎w��N���X�ȍ~�̃t�B�[���h���V���A���C�Y�ΏۂƂ���</param>
        /// <param name="instance">�C���X�^���X</param>
        /// <param name="info">SerializationInfo���</param>
        /// <param name="context">StreamingContext���</param>
        /// <param name="isInherid">true = ��{�N���X�̃t�B�[���h�����o���܂߂� / false=�܂߂Ȃ�</param>
        public static void GetObjectData(Type topType, object instance, SerializationInfo info, StreamingContext context, bool isInherid)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (isInherid == false)
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            foreach (var fi in topType.GetFields(flags))
            {
                if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                {
                    info.AddValue(fi.Name, fi.GetValue(instance));
                }
            }
        }

        /// <summary>
        /// .NET�W���̋t�V���A���C�Y�������C���v�������g����
        /// </summary>
        /// <param name="topType">�V���A���C�Y����N���X�ŁA���̎w��N���X�ȍ~�̃t�B�[���h���V���A���C�Y�ΏۂƂ���</param>
        /// <param name="instance">�C���X�^���X</param>
        /// <param name="info">SerializationInfo���</param>
        /// <param name="context">StreamingContext���</param>
        /// <param name="isInherid">true = ��{�N���X�̃t�B�[���h�����o���܂߂� / false=�܂߂Ȃ�</param>
        public static void Instanciate(Type topType, object instance, SerializationInfo info, StreamingContext context, bool isInherid)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (isInherid == false)
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            foreach (var fi in topType.GetFields(flags))
            {
                if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                {
                    fi.SetValue(instance, info.GetValue(fi.Name, fi.FieldType));
                }
            }
        }
        #endregion
    }
}
