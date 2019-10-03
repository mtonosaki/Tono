using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uTime の概要の説明です。
    /// </summary>
    [Serializable]
    public class DateTimeEx : ITime, IComparable, ICloneable, IReadonlyable
    {

        #region 属性(シリアライズする)
        ///<summary>秒変数</summary>
        private int _val;
        public bool _isReadonly = false;
        #endregion

        #region	属性(シリアライズしない)
        /// <summary>週の文字列</summary>
        [NonSerialized] private static string[] _dayStrings = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sta", "Sun" };
        #endregion

        #region IReadonlyableインターフェース
        public bool IsReadonly => _isReadonly;

        public void SetReadonly()
        {
            _isReadonly = true;
        }

        #endregion

        #region 固定値
        /// <summary>読み取り専用の最小値</summary>
        public static readonly DateTimeEx MinValue = new DateTimeEx(int.MinValue, true);
        /// <summary>読み取り専用の最大値</summary>
        public static readonly DateTimeEx MaxValue = new DateTimeEx(int.MaxValue, true);
        public enum Days { Mon, Tue, Wed, Thu, Fri, Sat, San }
        #endregion

        /// <summary>
        /// 文字列から値を入力する
        /// </summary>
        /// <param name="str"></param>
        /// <returns>インスタンス：null=エラー</returns>
        public static DateTimeEx FromString(string str)
        {
            var cr1 = str.IndexOf(':');
            if (cr1 > 0)
            {
                var ho = str.Substring(0, cr1);
                var cr2 = str.IndexOf(':', cr1 + 1);
                if (cr2 > 0)
                {
                    var mi = str.Substring(cr1 + 1, cr2 - cr1 - 1);
                    var se = str.Substring(cr2 + 1);
                    try
                    {
                        var ret = DateTimeEx.FromDHMS(0, int.Parse(ho), int.Parse(mi), int.Parse(se));
                        return ret;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                else
                {
                    var mi = str.Substring(cr1 + 1);
                    var ret = DateTimeEx.FromDHMS(0, int.Parse(ho), int.Parse(mi), 0);
                    return ret;
                }
            }
            var d = double.Parse(str);
            return DateTimeEx.FromDays(d);
        }


        /// <summary>
        /// 最小値のインスタンスを生成する
        /// </summary>
        /// <returns>最小値の新しいインスタンス</returns>
        public static DateTimeEx FromMinValue()
        {
            return new DateTimeEx(int.MinValue, false);
        }

        /// <summary>
        /// 最大値のインスタンスを生成する
        /// </summary>
        /// <returns>最大値の新しいインスタンス</returns>
        public static DateTimeEx FromMaxValue()
        {
            return new DateTimeEx(int.MaxValue, false);
        }


        // _valの値を直接指定して構築するコンストラクタ
        private DateTimeEx(int directValue, bool isReadonly)
        {
            _val = directValue;
            _isReadonly = isReadonly;
        }

        /// <summary>
        /// 指定分を加算する
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static DateTimeEx AddMinutes(DateTimeEx t0, double minutes)
        {
            var ret = new DateTimeEx
            {
                _val = (int)(t0._val + minutes * 60)
            };
            return ret;

        }

        /// <summary>
        /// 週文字列を指定する
        /// </summary>
        /// <param name="strs">添え字がDayに対応する文字列</param>
        public static void SetDayStrings(string[] strs)
        {
            _dayStrings = strs;
        }

        /// <summary>
        /// 週文字列を取得する
        /// </summary>
        /// <returns></returns>
        public static string[] GetDayStrings()
        {
            return _dayStrings;
        }

        /// <summary>
        /// 指定uMesによる言語を設定する
        /// </summary>
        /// <param name="mes"></param>
        public static void SetDayStrings(Mes mes)
        {
            var strs = new string[7];
            for (var i = 0; i < strs.Length; i++)
            {
                strs[i] = mes["uTimeDay", i.ToString()];
            }
            SetDayStrings(strs);
        }

        /// <summary>
        /// 日の文字列を取得する
        /// </summary>
        /// <param name="day">日の値</param>
        /// <returns>日の文字列</returns>
        public static string GetDayString(int day)
        {
            return _dayStrings[day % _dayStrings.Length];
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        //
        public DateTimeEx()
        {
            _val = 0;
        }

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString() + " Minutes = " + TotalMinutes;
#endif
        #endregion

        /// <summary>
        /// 時間の比較【t1がt2未満の場合】
        /// </summary>
        /// <param name="t1">uTime型</param>
        /// <param name="t2">uTime型</param>
        /// <returns>true:条件が合っている/false：条件が合っていない</returns>
        //
        public static bool operator <(DateTimeEx t1, DateTimeEx t2)
        {
            if (t1 == null && t2 == null)
            {
                return true;
            }

            if (t1 == null || t2 == null)
            {
                return false;
            }

            return t1._val < t2._val;
        }

        /// <summary>
        /// 時間の比較【t1がt2より大きい場合】
        /// </summary>
        /// <param name="t1">uTime型</param>
        /// <param name="t2">uTime型</param>
        /// <returns>true:条件が合っている/false：条件が合っていない</returns>
        //
        public static bool operator >(DateTimeEx t1, DateTimeEx t2)
        {
            return t1._val > t2._val;
        }

        /// <summary>
        /// 時間の比較【t1がt2以下の場合】
        /// </summary>
        /// <param name="t1">uTime型</param>
        /// <param name="t2">uTime型</param>
        /// <returns>true:条件が合っている/false：条件が合っていない</returns>
        //
        public static bool operator <=(DateTimeEx t1, DateTimeEx t2)
        {
            return t1._val <= t2._val;
        }

        /// <summary>
        /// 文字列からインスタンスを生成する。Hは24以上でも解析できる
        /// </summary>
        /// <param name="v0"></param>
        /// <returns></returns>
        public static DateTimeEx FromHMS(string v0)
        {
            if (v0 == null)
            {
                return DateTimeEx.FromDHMS(0, 0, 0, 0);
            }

            var v = "";
            if (v0.Split(' ').Length > 1)
            {
                v = v0.Split(' ')[1];
            }
            else
            {
                v = v0.Split(' ')[0];
            }
            var vv = v.Trim();
            if (vv == "")
            {
                return DateTimeEx.FromDHMS(0, 0, 0, 0);
            }

            var i1 = vv.IndexOf(':');
            if (i1 < 0)
            {
                return DateTimeEx.FromDHMS(0, int.Parse(vv), 0, 0);
            }
            var i2 = vv.IndexOf(':', i1 + 2);
            var h = int.Parse(vv.Substring(0, i1));
            int m;
            int s;
            if (i2 > i1)
            {
                m = int.Parse(vv.Substring(i1 + 1, i2 - i1 - 1));
                s = int.Parse(vv.Substring(i2 + 1));
            }
            else
            {
                m = int.Parse(vv.Substring(i1 + 1));
                s = 0;
            }
            var d = h / 24;
            h = h % 24;
            return DateTimeEx.FromDHMS(d, h, m, s);
        }

        /// <summary>
        /// 時間の比較【t1がt2以上の場合】
        /// </summary>
        /// <param name="t1">uTime型</param>
        /// <param name="t2">uTime型</param>
        /// <returns>true:条件が合っている/false：条件が合っていない</returns>
        //
        public static bool operator >=(DateTimeEx t1, DateTimeEx t2)
        {
            return t1._val >= t2._val;
        }

        //
        public override int GetHashCode()
        {
            return _val;
        }

        //
        public override bool Equals(object obj)
        {
            if (obj is DateTimeEx)
            {
                return _val == ((DateTimeEx)obj)._val;
            }
            return false;
        }

        public static bool operator ==(DateTimeEx t, object o)
        {
            if (object.ReferenceEquals(t, null))
            {
                return o == null;
            }
            else
            {
                return t.Equals(o);
            }
        }

        //
        public static bool operator !=(DateTimeEx t, object o)
        {
            if (t == null && o == null)
            {
                return false;
            }

            if (t == null)
            {
                return true;
            }

            return !t.Equals(o);
        }

        //
        public override string ToString()
        {
            //
            // 2006.03.07 ZONO
            // _valの値が－(マイナス)になるとエラーになるので、
            // マイナスになったら１週間分/*インクリメントする*/に収まるようにする by Tono
            //
            var temp = (DateTimeEx)Clone();
            if (temp._val < 0)
            {
                temp._val = 604800 - (Math.Abs(temp._val) % 604800);
            }
            return /*(temp._val < 0 ? "-" : "") +*/ DateTimeEx._dayStrings[temp.Day % DateTimeEx._dayStrings.Length] + ":" + temp.Hour.ToString("00") + ":" + temp.Minute.ToString("00") + ":" + temp.Second.ToString("00");
            //return uTime._dayStrings[Day % uTime._dayStrings.Length] + ":" + Hour.ToString("00") + ":" + Minute.ToString("00") + ":" + Second.ToString("00");
        }

        /// <summary>
        /// フォーマットに基づいて文字列を生成する
        /// </summary>
        /// <param name="format">
        /// フォーマット
        /// %h 時
        /// %m 分
        /// %s 秒
        /// %d 日
        /// %w 週の文字（例：Mon）
        /// %S 秒の通算
        /// %M 分の通算
        /// %H 時の通算
        /// %D 日の通算
        /// %DI 日の通算（整数部のみ）
        /// </param>
        /// <returns></returns>
        //
        public string ToString(string format)
        {
            format = format.Replace("%h", Hour.ToString("00"));
            format = format.Replace("%m", Minute.ToString("00"));
            format = format.Replace("%s", Second.ToString("00"));
            format = format.Replace("%d", Day.ToString());
            var day = Day;
            if (day < 0)
            {
                day = (day % DateTimeEx._dayStrings.Length) + 7;
            }
            format = format.Replace("%w", DateTimeEx._dayStrings[day % DateTimeEx._dayStrings.Length]);
            format = format.Replace("%H", TotalHours.ToString());
            format = format.Replace("%S", TotalSeconds.ToString());
            format = format.Replace("%M", TotalMinutes.ToString());
            format = format.Replace("%DI", ((int)TotalDays).ToString());    // 日を整数化（切り捨て）
            format = format.Replace("%D", TotalDays.ToString());
            return format;
        }

        /// <summary>
        /// 時刻が負の値の場合、１週間周期で正規化する
        /// </summary>
        /// <returns></returns>
        public DateTimeEx GetNormalizeWeeklyCycle()
        {
            if (TotalSeconds < 0)
            {
                return DateTimeEx.FromSeconds(TotalSeconds % 604800 + 604800);
            }
            else
            {
                return (DateTimeEx)Clone();
            }
        }

        /// <summary>
        /// 分単位の値からインスタンスを生成する
        /// </summary>
        /// <param name="totalMinutes">累計分</param>
        /// <returns>新しいインスタンス</returns>
        //
        public static DateTimeEx FromMinutes(int totalMinutes)
        {
            var t = new DateTimeEx();
            switch (totalMinutes)
            {
                case int.MaxValue: t._val = totalMinutes; break;
                case int.MinValue: t._val = totalMinutes; break;
                default: t._val = totalMinutes * 60; break;
            }
            return t;
        }

        /// <summary>
        /// 秒単位の値からインスタンスを生成する
        /// </summary>
        /// <param name="totalMinutes">累計分</param>
        /// <returns>新しいインスタンス</returns>
        //
        public static DateTimeEx FromSeconds(int totalSeconds)
        {
            var t = new DateTimeEx
            {
                _val = totalSeconds
            };
            return t;
        }

        /// <summary>
        /// 秒単位の値からインスタンスを生成する
        /// </summary>
        /// <param name="totalMinutes">累計分</param>
        /// <returns>新しいインスタンス</returns>
        //
        public static DateTimeEx FromDays(double totalDays)
        {
            var t = new DateTimeEx
            {
                _val = (int)(totalDays * 86400)
            };
            return t;
        }

        /// <summary>
        /// 加算演算子
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>加算後の新しいインスタンス</returns>
        //
        public static DateTimeEx operator +(DateTimeEx t1, DateTimeEx t2)
        {
            var ret = new DateTimeEx
            {
                _val = t1._val + t2._val
            };
            return ret;
        }

        /// <summary>
        /// 減算演算子
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>減算後の新しいインスタンス</returns>
        //
        public static DateTimeEx operator -(DateTimeEx t1, DateTimeEx t2)
        {
            var ret = new DateTimeEx
            {
                _val = t1._val - t2._val
            };
            return ret;
        }

        /// <summary>
        /// 乗算演算子
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>乗算後の新しいインスタンス</returns>
        //
        public static DateTimeEx operator *(DateTimeEx t1, int t2)
        {
            var ret = new DateTimeEx
            {
                _val = t1._val * t2
            };
            return ret;
        }

        /// <summary>
        /// 除算演算子
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>除算後の新しいインスタンス</returns>
        //
        public static DateTimeEx operator /(DateTimeEx t1, int t2)
        {
            var ret = new DateTimeEx
            {
                _val = t1._val / t2
            };
            return ret;
        }

        /// <summary>
        /// 整数値からインスタンスを生成する
        /// </summary>
        /// <param name="day">曜日：0=月曜日</param>
        /// <param name="hour">時 0-23</param>
        /// <param name="minute">分 0-59</param>
        /// <param name="second">秒 0-59</param>
        /// <returns></returns>
        //
        public static DateTimeEx FromDHMS(int day, int hour, int minute, int second)
        {
            var ret = new DateTimeEx
            {
                _val = second + minute * 60 + hour * 3600 + day * 86400
            };
            return ret;
        }

        /// <summary>
        /// ゼロ値をセットする
        /// </summary>
        //
        public void SetZero()
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用のuTimeに値セットできない");
            _val = 0;
        }

        #region ITime メンバ

        /// <summary>
        ///　時間を取得する
        /// </summary>
        /// <returns>時間</returns>
        public int Hour
        {
            get
            {
                var dayRest = _val % 86400;
                var hour = dayRest / 3600;
                return hour;
            }
        }

        /// <summary>
        /// 分を取得する
        /// </summary>
        /// <returns>分</returns>
        public int Minute
        {
            // 
            get
            {
                var dayRest = _val % 86400;
                var hourRest = dayRest % 3600;
                var minute = hourRest / 60;
                return minute;
            }
        }

        /// <summary>
        /// 秒を取得する
        /// </summary>
        /// <returns>秒</returns>
        public int Second
        {
            // 
            get
            {
                var dayRest = _val % 86400;
                var hourRest = dayRest % 3600;
                var minuteRest = hourRest % 60;
                var second = minuteRest;
                return second;
            }
        }

        /// <summary>
        /// 日数を取得する
        /// </summary>
        /// <returns>日数</returns>
        public int Day
        {
            // 
            get
            {
                var day = _val / 86400;
                return day;
            }
        }

        /// <summary>
        /// 合計秒数を取得する
        /// </summary>
        /// <returns></returns>
        public int TotalSeconds
        {
            // 
            get => _val;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用のuTimeに値セットできない");
                _val = value;
            }
        }

        /// <summary>
        /// 合計分数を取得する
        /// </summary>
        /// <returns></returns>
        public int TotalMinutes
        {
            // 
            get => _val / 60;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用のuTimeに値セットできない");
                _val = value * 60;
            }
        }

        /// <summary>
        /// 合計秒数を取得する
        /// </summary>
        /// <returns></returns>
        public int TotalHours
        {
            // 
            get => _val / 3600;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用のuTimeに値セットできない");
                _val = value * 3600;
            }
        }

        /// <summary>
        /// 合計の日数を取得する
        /// </summary>
        /// <returns>日数</returns>
        public double TotalDays
        {
            // 
            get => _val / 86400.0;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "読み取り専用のuTimeに値セットできない");
                _val = (int)(value * 86400);
            }
        }

        #endregion
        #region IComparable メンバ

        public int CompareTo(object obj)
        {
            return _val - ((DateTimeEx)obj)._val;
        }

        #endregion
        #region ICloneable メンバ
        public object Clone()
        {
            var ret = (DateTimeEx)Activator.CreateInstance(GetType());
            ret._val = _val;
            ret._isReadonly = _isReadonly;
            return ret;
        }
        #endregion
    }
}
