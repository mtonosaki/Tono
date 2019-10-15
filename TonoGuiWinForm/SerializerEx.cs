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
    /// SerializerEx の概要の説明です。
    /// </summary>
    public class SerializerEx
    {
        private enum Tag : byte
        {
            /*タグ系*/
            Null, ClassStart, SubClass, Field, ClassEnd,
            /*クラス系*/
            List, Dictionary,
            /*値系*/
            vDouble, vSingle, vInt64, vUInt64, vInt32, vUInt32, vInt16, vUInt16, vIntPtr, vSByte, vByte, vBoolean, vString,
            /*高速系*/
            vUTime, vId, Color
        }

        /// <summary>TypeとeTagの関連</summary>
        [NonSerialized] private static readonly IDictionary/*<Type,eTag>*/ _typeToTag;
        /// <summary>TypeとeTagの関連</summary>
        [NonSerialized] private static readonly IDictionary/*<eTag,Type>*/ _tagToType;

        /// <summary>uTimeのDirect系サポート用（高速化）</summary>
        [NonSerialized] private static readonly FieldInfo uTimeFI = typeof(DateTimeEx).GetField("_val", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);


        #region スタティック構築子
        /// <summary>
        /// スタティック構築子
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
        /// 読み込み完了後に、データを適用する処理（標準処理：アプリ特化は、別のところで行ってください）
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
        /// 指定ストリームからオブジェクトをひとつ受信する
        /// </summary>
        /// <param name="reader">ストリーム</param>
        /// <returns>オブジェクトのインスタンス</returns>
        public static object Receive(Stream reader)
        {
            return receiveLoop(reader);
        }

        /// <summary>
        /// 指定ストリームの現在位置から受信する再帰ループ
        /// </summary>
        /// <param name="reader">ストリーム</param>
        /// <returns>インスタンス化されたオブジェクト</returns>
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
                            // クラスを特定する = currentType = subClassType
                            ReceiveDirect(reader, out int classid);     // クラスを特定する = currentType = subClassType

                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "シリアライズClass IDが不正でした");

                            // クラスのインスタンスを生成する = ret
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
                            // サブクラスを特定する = subClassType
                            ReceiveDirect(reader, out                         // サブクラスを特定する = subClassType
                            int classid);
                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "シリアライズClass IDが不正でした");

                            subClassType = Type.GetType(keys[2] + ", " + keys[3]);
                            break;
                        }
                    case Tag.Field:
                        {
                            // フィールドを特定する = fi
                            ReceiveDirect(reader, out                         // フィールドを特定する = fi
                            int fieldid);
                            var keystr = NamedId.FromIDNoName(fieldid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "シリアライズField IDが不正でした");
                            var fi = subClassType.GetField(keys[2/*field名*/], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                            // フィールドの値を取得する（再帰コール）
                            var fieldValue = receiveLoop(reader);

                            // フィールドの値をインスタンスに設定する
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
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "シリアライズClass IDが不正でした");

                            // クラスのインスタンスを生成する = ret
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

                            // 配列数を取得す
                            ReceiveDirect(reader, out
                            // 配列数を取得す
                            int n);

                            for (var i = 0; i < n; i++)
                            {
                                // 各要素の値を取得する（再帰コール）
                                var val = receiveLoop(reader);
                                ((IList)ret).Add(val);
                            }
                            // サブクラスのために準備する
                            subClassType = currentType;
                            break;
                        }
                    case Tag.Dictionary:
                        {
                            ReceiveDirect(reader, out int classid);
                            var keystr = NamedId.FromIDNoName(classid).Name;
                            var keys = keystr.Split(new char[] { '/' });
                            System.Diagnostics.Debug.Assert(keys.Length == 4, "シリアライズClass IDが不正でした");

                            // クラスのインスタンスを生成する = ret
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

                            // 配列数を取得す
                            ReceiveDirect(reader, out
                            // 配列数を取得す
                            int n);

                            for (var i = 0; i < n; i++)
                            {
                                // 各要素の値を取得する（再帰コール）
                                var key = receiveLoop(reader);
                                var val = receiveLoop(reader);
                                ((IDictionary)ret).Add(key, val);
                            }
                            // サブクラスのために準備する
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
        /// 指定ストリームにオブジェクトを送信する
        /// </summary>
        /// <param name="writer">ストリーム</param>
        /// <param name="value">オブジェクト</param>
        public static void Send(Stream writer, object value)
        {
            sendLoop(writer, value);
        }

        /// <summary>
        /// 指定ストリームにオブジェクトを送信する、サイクリックループ
        /// </summary>
        /// <param name="writer">ストリーム</param>
        /// <param name="value">オブジェクト</param>
        private static void sendLoop(Stream writer, object value)
        {
            // 送信できない型を変換する
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
            // ヌル値の処理
            if (value == null)
            {
                writer.WriteByte((byte)Tag.Null);
                return;
            }

            var tarType = value.GetType();

            var ttag = _typeToTag[tarType];
            if (ttag != null)
            {
                // タグの埋め込み
                writer.WriteByte((byte)ttag);

                // 値の埋め込み
                var mi = typeof(SerializerEx).GetMethod("SendDirect", new Type[] { typeof(Stream), tarType });
                mi.Invoke(null, new object[] { writer, value });
                return;
            }


            // クラスID
            var cid = NamedId.FromName(tarType.FullName);
            var isClassProced = false;
            IDictionary dupchk = new HybridDictionary();

            // リストの特化処理
            if (value is IList)
            {
                // タグの埋め込み
                writer.WriteByte((byte)Tag.List);

                // クラスID
                SendDirect(writer, cid);

                SendDirect(writer, ((IList)value).Count);
                for (var i = 0; i < ((IList)value).Count; i++)
                {
                    sendLoop(writer, ((IList)value)[i]);
                }
                isClassProced = true;
            }

            // ディクショナリの特化処理
            if (value is IDictionary)
            {
                // タグの埋め込み

                writer.WriteByte((byte)Tag.Dictionary);

                // クラスID
                SendDirect(writer, cid.Id.Value);

                SendDirect(writer, ((IDictionary)value).Count);
                foreach (DictionaryEntry de in ((IDictionary)value))
                {
                    sendLoop(writer, de.Key);
                    sendLoop(writer, de.Value);
                }
                isClassProced = true;
            }

            // クラスの処理
            if (isClassProced == false)
            {
                writer.WriteByte((byte)Tag.ClassStart);
                SendDirect(writer, cid.Id.Value);
            }
            else
            {
                // リスト、ディクショナリ系で、派生クラスのフィールドは保存不要と記し付ける
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
                            continue;   // 派生クラスの保存フェーズで行う
                        }
                        if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                        {
                            dupchk[fi.Name] = true;
                        }
                    }
                }
            }

            // ～クラスフィールドの処理
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
                        continue;   // 派生クラスの保存フェーズで行う
                    }
                    if ((fi.Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        if (dupchk[fi.Name] == null)
                        {
                            dupchk.Add(fi.Name, true);

                            if (fcnt == 0 && inheritLevel > 0)  // ベースクラス初めてのフィールドの時。
                            {
                                // ベースクラス開始TAGを埋め込む
                                writer.WriteByte((byte)Tag.SubClass);

                                // クラス派生先識別IDを埋め込む
                                var bid = NamedId.FromName(type.FullName);
                                SendDirect(writer, bid.Id.Value);
                            }

                            // タグの埋め込み
                            writer.WriteByte((byte)Tag.Field);

                            // フィールド名を埋め込む
                            var fid = NamedId.FromName($"{type.FullName}...{fi.Name}"); //.FromField(fi);
                            SendDirect(writer, fid.Id.Value);

                            // フィールドの値を埋め込む
                            sendLoop(writer, fi.GetValue(value));
                            fcnt++;
                        }
                    }
                }
                inheritLevel++;
            }
            // クラス終了TAGを埋め込む
            writer.WriteByte((byte)Tag.ClassEnd);

            // シリアライズ終了時の開放処理等
            if (value is PartsBase)
            {
                ((PartsBase)value).SetPartsPositionCorderName(false);
                ((PartsBase)value).SetPartsPositionerName(false);
            }
        }

        /// <summary>
        /// シリアライズ対象のメンバーをdest←srcにコピーする
        /// </summary>
        /// <param name="dest">コピー先</param>
        /// <param name="src">コピー元</param>
        public static void CopyObject(object dest, object src)
        {
            IDictionary dat = new HybridDictionary();
            //TODO:この辺でNull参照がでる TONO 2005.12.6
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
                        continue;   // 派生クラスの保存フェーズで行う
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

        #region 各型のバイトシリアライズ

        /// <summary>
        /// Id型の保存
        /// </summary>
        public static void SendDirect(Stream writer, Id value)
        {
            SendDirect(writer, value.Value);
        }

        /// <summary>
        /// UInt64型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out Id value)
        {
            ReceiveDirect(reader, out int id);
            value = new Id { Value = id };
            return value;
        }

        /// <summary>
        /// uTime型の保存
        /// </summary>
        public static void SendDirect(Stream writer, DateTimeEx value)
        {
            var val = (int)uTimeFI.GetValue(value);
            SendDirect(writer, val);
        }

        /// <summary>
        /// uTime型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out DateTimeEx value)
        {
            ReceiveDirect(reader, out int val);
            value = new DateTimeEx();
            uTimeFI.SetValue(value, val);
            return value;
        }

        /// <summary>
        /// Color型の保存
        /// </summary>
        public static void SendDirect(Stream writer, Color value)
        {
            var val = value.ToArgb();
            SendDirect(writer, val);
        }

        /// <summary>
        /// Color型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out Color value)
        {
            ReceiveDirect(reader, out int val);
            value = Color.FromArgb(val);
            return value;
        }

        /// <summary>
        /// UInt64型の保存
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
        /// UInt64型の受信
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
        /// Double型の保存
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
        /// Double型の受信
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
        /// Single型の保存
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
        /// Single型の受信
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
        /// Int64型の保存
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
        /// Int64型の受信
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
        /// Int32型の保存
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
        /// Int32型の受信
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
        /// UInt32型の保存
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
        /// UInt32型の受信
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
        /// Int32型の保存
        /// </summary>
        public static void SendDirect(Stream writer, IntPtr value)
        {
            // なにもしない
        }

        /// <summary>
        /// IntPtr型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out IntPtr value)
        {
            // なにもしない
            value = IntPtr.Zero;
            return value;
        }

        /// <summary>
        /// Int16型の保存
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
        /// Int16型の受信
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
        /// Int16型の保存
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
        /// UInt16型の受信
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
        /// SByte型の保存
        /// </summary>
        public static void SendDirect(Stream writer, sbyte value)
        {
            writer.WriteByte((byte)value);
        }

        /// <summary>
        /// SByte型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out sbyte value)
        {
            value = (sbyte)reader.ReadByte();
            return value;
        }

        /// <summary>
        /// Byte型の保存
        /// </summary>
        public static void SendDirect(Stream writer, byte value)
        {
            writer.WriteByte(value);
        }

        /// <summary>
        /// Byte型の受信
        /// </summary>
        public static object ReceiveDirect(Stream reader, out byte value)
        {
            value = (byte)reader.ReadByte();
            return value;
        }

        /// <summary>
        /// Boolean型の保存
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
        /// Boolean型の受信
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
        /// 文字列の保存
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
        /// 文字列の受信
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


        #region .NETが標準でサポートするシリアライズを簡略化する
        /// <summary>
        /// .NET標準のシリアライズを自動インプリメントする
        /// </summary>
        /// <param name="topType">シリアライズするクラスで、この指定クラス以降のフィールドをシリアライズ対象とする</param>
        /// <param name="instance">インスタンス</param>
        /// <param name="info">SerializationInfo情報</param>
        /// <param name="context">StreamingContext情報</param>
        /// <param name="isInherid">true = 基本クラスのフィールドメンバを含める / false=含めない</param>
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
        /// .NET標準の逆シリアライズを自動インプリメントする
        /// </summary>
        /// <param name="topType">シリアライズするクラスで、この指定クラス以降のフィールドをシリアライズ対象とする</param>
        /// <param name="instance">インスタンス</param>
        /// <param name="info">SerializationInfo情報</param>
        /// <param name="context">StreamingContext情報</param>
        /// <param name="isInherid">true = 基本クラスのフィールドメンバを含める / false=含めない</param>
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
