using System;
using System.Diagnostics;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 変更時にイベントが発行できる文字列
    /// </summary>
    [Serializable]
    public class StringListener : ISerializable
    {
        /// <summary>
        /// テキスト変更直前のイベント
        /// </summary>
        public event EventHandler TextChanging;
        /// <summary>
        /// テキスト変更直後のイベント
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// シリアライズする対象の文字列
        /// </summary>
        private string _str = "";

        public override string ToString()
        {
            return _str;
        }

        public override int GetHashCode()
        {
            return _str.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _str.Equals(obj);
        }

        public static bool operator ==(StringListener s0, StringListener s1)
        {
            if (s0 == null && s1 == null)
            {
                return true;
            }
            if (s0 != null && s1 != null)
            {
                return s0.Equals(s1);
            }
            return false;
        }

        public static bool operator !=(StringListener s0, StringListener s1)
        {
            return !(s0 == s1);
        }

        /// <summary>
        /// string型への自動変換
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static implicit operator string(StringListener from)
        {
            return from._str;
        }

        /// <summary>
        /// 文字列からキャストすると、イベントが失われるので許可しない
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static explicit operator StringListener(string from)
        {
            Debug.Assert(false, "Program error : 'StrUtilinge = string' is not accepted. use StrUtilinge.Value property");
            return new StringListener(from);
        }

        /// <summary>
        /// イベントを考慮した値
        /// </summary>
        public string Text
        {
            get => _str;
            set
            {
                if (_str != value)
                {
                    var e = new EventArgs();
                    TextChanging?.Invoke(this, e);
                    _str = value;
                    TextChanged?.Invoke(this, e);
                }
            }
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public StringListener()
        {
        }

        /// <summary>
        /// 代入コンストラクタ
        /// </summary>
        /// <param name="val"></param>
        public StringListener(string val)
        {
            _str = val;
        }

        /// <summary>
        /// コピーコンストラクタ
        /// </summary>
        /// <param name="val"></param>
        public StringListener(StringListener val)
        {
            _str = val._str;
            TextChanged = val.TextChanged;
            TextChanging = val.TextChanging;
        }

        #region ISerializable メンバ

        /// <summary>
        /// シリアライズ構築（イベントは復帰しない）
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected StringListener(SerializationInfo info, StreamingContext context)
        {
            _str = info.GetString("_str");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_str", _str);
        }

        #endregion
    }
}
