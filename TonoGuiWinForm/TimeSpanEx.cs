using System;
using System.Collections;
using System.Diagnostics;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// vPeriod の概要の説明です。
    /// </summary>
    [Serializable]
    public class TimeSpanEx : ISpace, ITimeSpan
    {
        /** <summary> </summary>*/
        private DateTimeEx _start;
        /** <summary> </summary>*/
        private DateTimeEx _end;


        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        
        public TimeSpanEx()
        {
        }

        /// <summary>
        /// 中心点の時刻
        /// </summary>
        public DateTimeEx Middle => (_start + _end) / 2;

        /// <summary>
        /// 中心点の時刻
        /// </summary>
        /// <param name="ts">時間</param>
        /// <returns>中心点の時刻</returns>
        public static DateTimeEx GetMiddle(ITimeSpan ts)
        {
            return (ts.Start + ts.End) / 2;
        }

        /// <summary>
        /// 二つの時刻を指定してインスタンスを生成する
        /// </summary>
        /// <param name="start">開始時刻</param>
        /// <param name="end">終了時刻</param>
        /// <returns>新しいインスタンス</returns>
        
        public static TimeSpanEx FromTime(DateTimeEx start, DateTimeEx end)
        {
            var ret = new TimeSpanEx
            {
                _start = start,
                _end = end
            };
            return ret;
        }

        
        public override bool Equals(object obj)
        {
            if (obj is TimeSpanEx)
            {
                var t = (TimeSpanEx)obj;
                return t._start == _start && t._end == _end;
            }
            return false;
        }

        
        public override int GetHashCode()
        {
            return _start.TotalSeconds * 86400 * 1000 + _end.TotalSeconds;

        }

        
        public override string ToString()
        {
            return _start.ToString() + " - " + _end.ToString();
        }

        #region ISpace メンバ

        /// <summary>
        /// 指定時刻が時間範囲にあるかどうか調査する
        /// </summary>
        /// <param name="value">uTime型の時刻</param>
        /// <returns>true = 範囲内 / false = 範囲外</returns>
        public bool IsIn(object value)
        {
            if (value is DateTimeEx)
            {
                var t = (DateTimeEx)value;

                return _start <= t && t <= _end;
            }
            if (value is TimeSpanEx)
            {
                var ts = (TimeSpanEx)value;
                if (End >= ts.Start && Start <= ts.End ||
                    Start >= ts.Start && End <= ts.End ||
                    ts.Start >= Start && ts.End <= End)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 指定時刻分、時間をスライドする
        /// </summary>
        /// <param name="value">uTime型のスライド</param>
        
        public void Transfer(object value)
        {
            if (value is DateTimeEx)
            {
                var t = (DateTimeEx)value;
                _start += t;
                _end += t;
            }
        }

        /// <summary>
        /// 指定時刻分、時間を拡大する。
        /// </summary>
        /// <param name="value">uTime型の拡大</param>
        
        public void Inflate(object value)
        {
            if (value is DateTimeEx)
            {
                var t = (DateTimeEx)value;
                _start -= t;
                _end += t;
            }
        }

        /// <summary>
        /// 指定時刻分、時間を縮小する。
        /// </summary>
        /// <param name="value">uTime型の縮小</param>
        
        public void Deflate(object value)
        {
            if (value is DateTimeEx)
            {
                var t = (DateTimeEx)value;
                _start += t;
                _end -= t;
            }
        }

        #endregion
        #region ITimeSpan メンバ

        public DateTimeEx Start
        {
            
            get => _start;
            
            set => _start = value;
        }

        public DateTimeEx End
        {
            
            get => _end;
            
            set => _end = value;
        }

        public DateTimeEx Span => _end - _start;

        #endregion

        /// <summary>
        /// ITimeSpan型を持つコレクションから、最小値を持つインスタンスを取得する
        /// </summary>
        /// <param name="recs">コレクション</param>
        /// <returns>最小値を持つインスタンス</returns>
        public static object GetTimeMin(ICollection/*<ITimeSpan>*/ recs)
        {
            var tmin = DateTimeEx.MaxValue;
            object ret = null;
            foreach (var rec in recs)
            {
                if (rec is ITimeSpan)
                {
                    if (((ITimeSpan)rec).Start < tmin)
                    {
                        tmin = ((ITimeSpan)rec).Start;
                        ret = rec;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// ITimeSpan型を持つコレクションから、最小値を持つインスタンスを取得する
        /// </summary>
        /// <param name="s1">値１</param>
        /// <param name="s2">値２</param>
        /// <returns>最小値を持つインスタンス</returns>
        public static object GetTimeMin(ITimeSpan s1, ITimeSpan s2)
        {
            if (s2.Start < s1.Start)
            {
                return s2;
            }
            else
            {
                return s1;
            }
        }

        /// <summary>
        /// ITimeSpan型を持つコレクションから、最大値を持つインスタンスを取得する
        /// </summary>
        /// <param name="recs">コレクション</param>
        /// <returns>最大値を持つインスタンス</returns>
        public static object GetTimeMax(ICollection/*<ITimeSpan>*/ recs)
        {
            var tmax = DateTimeEx.MinValue;
            object ret = null;
            foreach (var rec in recs)
            {
                if (rec is ITimeSpan)
                {
                    if (((ITimeSpan)rec).End > tmax)
                    {
                        tmax = ((ITimeSpan)rec).End;
                        ret = rec;
                    }
                }
            }
            return ret;
        }
        /// <summary>
        /// ITimeSpan型を持つコレクションから、最大値を持つインスタンスを取得する
        /// </summary>
        /// <param name="s1">値１</param>
        /// <param name="s2">値２</param>
        /// <returns>最小値を持つインスタンス</returns>
        public static object GetTimeMax(ITimeSpan s1, ITimeSpan s2)
        {
            if (s1.End > s2.End)
            {
                return s1;
            }
            else
            {
                return s2;
            }
        }
    }
}
