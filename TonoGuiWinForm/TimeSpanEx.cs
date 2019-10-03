using System;
using System.Collections;
using System.Diagnostics;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// vPeriod �̊T�v�̐����ł��B
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
        /// �R���X�g���N�^
        /// </summary>
        
        public TimeSpanEx()
        {
        }

        /// <summary>
        /// ���S�_�̎���
        /// </summary>
        public DateTimeEx Middle => (_start + _end) / 2;

        /// <summary>
        /// ���S�_�̎���
        /// </summary>
        /// <param name="ts">����</param>
        /// <returns>���S�_�̎���</returns>
        public static DateTimeEx GetMiddle(ITimeSpan ts)
        {
            return (ts.Start + ts.End) / 2;
        }

        /// <summary>
        /// ��̎������w�肵�ăC���X�^���X�𐶐�����
        /// </summary>
        /// <param name="start">�J�n����</param>
        /// <param name="end">�I������</param>
        /// <returns>�V�����C���X�^���X</returns>
        
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

        #region ISpace �����o

        /// <summary>
        /// �w�莞�������Ԕ͈͂ɂ��邩�ǂ�����������
        /// </summary>
        /// <param name="value">uTime�^�̎���</param>
        /// <returns>true = �͈͓� / false = �͈͊O</returns>
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
        /// �w�莞�����A���Ԃ��X���C�h����
        /// </summary>
        /// <param name="value">uTime�^�̃X���C�h</param>
        
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
        /// �w�莞�����A���Ԃ��g�傷��B
        /// </summary>
        /// <param name="value">uTime�^�̊g��</param>
        
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
        /// �w�莞�����A���Ԃ��k������B
        /// </summary>
        /// <param name="value">uTime�^�̏k��</param>
        
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
        #region ITimeSpan �����o

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
        /// ITimeSpan�^�����R���N�V��������A�ŏ��l�����C���X�^���X���擾����
        /// </summary>
        /// <param name="recs">�R���N�V����</param>
        /// <returns>�ŏ��l�����C���X�^���X</returns>
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
        /// ITimeSpan�^�����R���N�V��������A�ŏ��l�����C���X�^���X���擾����
        /// </summary>
        /// <param name="s1">�l�P</param>
        /// <param name="s2">�l�Q</param>
        /// <returns>�ŏ��l�����C���X�^���X</returns>
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
        /// ITimeSpan�^�����R���N�V��������A�ő�l�����C���X�^���X���擾����
        /// </summary>
        /// <param name="recs">�R���N�V����</param>
        /// <returns>�ő�l�����C���X�^���X</returns>
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
        /// ITimeSpan�^�����R���N�V��������A�ő�l�����C���X�^���X���擾����
        /// </summary>
        /// <param name="s1">�l�P</param>
        /// <param name="s2">�l�Q</param>
        /// <returns>�ŏ��l�����C���X�^���X</returns>
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
