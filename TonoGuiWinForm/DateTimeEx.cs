using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uTime �̊T�v�̐����ł��B
    /// </summary>
    [Serializable]
    public class DateTimeEx : ITime, IComparable, ICloneable, IReadonlyable
    {

        #region ����(�V���A���C�Y����)
        ///<summary>�b�ϐ�</summary>
        private int _val;
        public bool _isReadonly = false;
        #endregion

        #region	����(�V���A���C�Y���Ȃ�)
        /// <summary>�T�̕�����</summary>
        [NonSerialized] private static string[] _dayStrings = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sta", "Sun" };
        #endregion

        #region IReadonlyable�C���^�[�t�F�[�X
        public bool IsReadonly => _isReadonly;

        public void SetReadonly()
        {
            _isReadonly = true;
        }

        #endregion

        #region �Œ�l
        /// <summary>�ǂݎ���p�̍ŏ��l</summary>
        public static readonly DateTimeEx MinValue = new DateTimeEx(int.MinValue, true);
        /// <summary>�ǂݎ���p�̍ő�l</summary>
        public static readonly DateTimeEx MaxValue = new DateTimeEx(int.MaxValue, true);
        public enum Days { Mon, Tue, Wed, Thu, Fri, Sat, San }
        #endregion

        /// <summary>
        /// �����񂩂�l����͂���
        /// </summary>
        /// <param name="str"></param>
        /// <returns>�C���X�^���X�Fnull=�G���[</returns>
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
        /// �ŏ��l�̃C���X�^���X�𐶐�����
        /// </summary>
        /// <returns>�ŏ��l�̐V�����C���X�^���X</returns>
        public static DateTimeEx FromMinValue()
        {
            return new DateTimeEx(int.MinValue, false);
        }

        /// <summary>
        /// �ő�l�̃C���X�^���X�𐶐�����
        /// </summary>
        /// <returns>�ő�l�̐V�����C���X�^���X</returns>
        public static DateTimeEx FromMaxValue()
        {
            return new DateTimeEx(int.MaxValue, false);
        }


        // _val�̒l�𒼐ڎw�肵�č\�z����R���X�g���N�^
        private DateTimeEx(int directValue, bool isReadonly)
        {
            _val = directValue;
            _isReadonly = isReadonly;
        }

        /// <summary>
        /// �w�蕪�����Z����
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
        /// �T��������w�肷��
        /// </summary>
        /// <param name="strs">�Y������Day�ɑΉ����镶����</param>
        public static void SetDayStrings(string[] strs)
        {
            _dayStrings = strs;
        }

        /// <summary>
        /// �T��������擾����
        /// </summary>
        /// <returns></returns>
        public static string[] GetDayStrings()
        {
            return _dayStrings;
        }

        /// <summary>
        /// �w��uMes�ɂ�錾���ݒ肷��
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
        /// ���̕�������擾����
        /// </summary>
        /// <param name="day">���̒l</param>
        /// <returns>���̕�����</returns>
        public static string GetDayString(int day)
        {
            return _dayStrings[day % _dayStrings.Length];
        }

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
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
        /// ���Ԃ̔�r�yt1��t2�����̏ꍇ�z
        /// </summary>
        /// <param name="t1">uTime�^</param>
        /// <param name="t2">uTime�^</param>
        /// <returns>true:�����������Ă���/false�F�����������Ă��Ȃ�</returns>
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
        /// ���Ԃ̔�r�yt1��t2���傫���ꍇ�z
        /// </summary>
        /// <param name="t1">uTime�^</param>
        /// <param name="t2">uTime�^</param>
        /// <returns>true:�����������Ă���/false�F�����������Ă��Ȃ�</returns>
        //
        public static bool operator >(DateTimeEx t1, DateTimeEx t2)
        {
            return t1._val > t2._val;
        }

        /// <summary>
        /// ���Ԃ̔�r�yt1��t2�ȉ��̏ꍇ�z
        /// </summary>
        /// <param name="t1">uTime�^</param>
        /// <param name="t2">uTime�^</param>
        /// <returns>true:�����������Ă���/false�F�����������Ă��Ȃ�</returns>
        //
        public static bool operator <=(DateTimeEx t1, DateTimeEx t2)
        {
            return t1._val <= t2._val;
        }

        /// <summary>
        /// �����񂩂�C���X�^���X�𐶐�����BH��24�ȏ�ł���͂ł���
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
        /// ���Ԃ̔�r�yt1��t2�ȏ�̏ꍇ�z
        /// </summary>
        /// <param name="t1">uTime�^</param>
        /// <param name="t2">uTime�^</param>
        /// <returns>true:�����������Ă���/false�F�����������Ă��Ȃ�</returns>
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
            // _val�̒l���|(�}�C�i�X)�ɂȂ�ƃG���[�ɂȂ�̂ŁA
            // �}�C�i�X�ɂȂ�����P�T�ԕ�/*�C���N�������g����*/�Ɏ��܂�悤�ɂ��� by Tono
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
        /// �t�H�[�}�b�g�Ɋ�Â��ĕ�����𐶐�����
        /// </summary>
        /// <param name="format">
        /// �t�H�[�}�b�g
        /// %h ��
        /// %m ��
        /// %s �b
        /// %d ��
        /// %w �T�̕����i��FMon�j
        /// %S �b�̒ʎZ
        /// %M ���̒ʎZ
        /// %H ���̒ʎZ
        /// %D ���̒ʎZ
        /// %DI ���̒ʎZ�i�������̂݁j
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
            format = format.Replace("%DI", ((int)TotalDays).ToString());    // ���𐮐����i�؂�̂āj
            format = format.Replace("%D", TotalDays.ToString());
            return format;
        }

        /// <summary>
        /// ���������̒l�̏ꍇ�A�P�T�Ԏ����Ő��K������
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
        /// ���P�ʂ̒l����C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="totalMinutes">�݌v��</param>
        /// <returns>�V�����C���X�^���X</returns>
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
        /// �b�P�ʂ̒l����C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="totalMinutes">�݌v��</param>
        /// <returns>�V�����C���X�^���X</returns>
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
        /// �b�P�ʂ̒l����C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="totalMinutes">�݌v��</param>
        /// <returns>�V�����C���X�^���X</returns>
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
        /// ���Z���Z�q
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>
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
        /// ���Z���Z�q
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>
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
        /// ��Z���Z�q
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>��Z��̐V�����C���X�^���X</returns>
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
        /// ���Z���Z�q
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>���Z��̐V�����C���X�^���X</returns>
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
        /// �����l����C���X�^���X�𐶐�����
        /// </summary>
        /// <param name="day">�j���F0=���j��</param>
        /// <param name="hour">�� 0-23</param>
        /// <param name="minute">�� 0-59</param>
        /// <param name="second">�b 0-59</param>
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
        /// �[���l���Z�b�g����
        /// </summary>
        //
        public void SetZero()
        {
            System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p��uTime�ɒl�Z�b�g�ł��Ȃ�");
            _val = 0;
        }

        #region ITime �����o

        /// <summary>
        ///�@���Ԃ��擾����
        /// </summary>
        /// <returns>����</returns>
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
        /// �����擾����
        /// </summary>
        /// <returns>��</returns>
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
        /// �b���擾����
        /// </summary>
        /// <returns>�b</returns>
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
        /// �������擾����
        /// </summary>
        /// <returns>����</returns>
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
        /// ���v�b�����擾����
        /// </summary>
        /// <returns></returns>
        public int TotalSeconds
        {
            // 
            get => _val;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p��uTime�ɒl�Z�b�g�ł��Ȃ�");
                _val = value;
            }
        }

        /// <summary>
        /// ���v�������擾����
        /// </summary>
        /// <returns></returns>
        public int TotalMinutes
        {
            // 
            get => _val / 60;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p��uTime�ɒl�Z�b�g�ł��Ȃ�");
                _val = value * 60;
            }
        }

        /// <summary>
        /// ���v�b�����擾����
        /// </summary>
        /// <returns></returns>
        public int TotalHours
        {
            // 
            get => _val / 3600;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p��uTime�ɒl�Z�b�g�ł��Ȃ�");
                _val = value * 3600;
            }
        }

        /// <summary>
        /// ���v�̓������擾����
        /// </summary>
        /// <returns>����</returns>
        public double TotalDays
        {
            // 
            get => _val / 86400.0;
            // 
            set
            {
                System.Diagnostics.Debug.Assert(_isReadonly == false, "�ǂݎ���p��uTime�ɒl�Z�b�g�ł��Ȃ�");
                _val = (int)(value * 86400);
            }
        }

        #endregion
        #region IComparable �����o

        public int CompareTo(object obj)
        {
            return _val - ((DateTimeEx)obj)._val;
        }

        #endregion
        #region ICloneable �����o
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
