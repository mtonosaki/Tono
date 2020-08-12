using System;
using System.Linq;
using ProcessKey = System.String;


namespace Tono.Jit
{
    public static class Utils
    {
        /// <summary>
        /// Get Process Key
        /// </summary>
        /// <param name="proc"></param>
        /// <returns></returns>
        public static ProcessKey GetProcessKey(JitProcess proc)
        {
            if (proc is JitProcessDummy dummy)
            {
                return dummy.ProcessKey;
            }
            else
            {
                if (proc.Name == null)
                {
                    return proc.ID;
                }
                else
                {
                    return proc.Name;
                }
            }
        }

        /// <summary>
        /// Check equal Subset-Process pairs
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool Equals((JitSubset Subset, JitProcess Process) left, (JitSubset Subset, JitProcess Process) right)
        {
            if (left == default || right == default)
            {
                return false;
            }

            if (left.Subset == right.Subset)
            {
                return left.Process == right.Process;
            }
            return false;
        }

        /// <summary>
        /// Parse TimeSpan string (Time Units are Upper case)
        /// </summary>
        /// <param name="valuestr"></param>
        /// <returns></returns>
        public static TimeSpan ParseTimeSpan(string valuestr)
        {
            double val;
            if (valuestr.StartsWith("0") && char.IsNumber(valuestr[valuestr.Length - 1]))
            {
                val = double.Parse(valuestr);
                if (val == 0)
                {
                    return TimeSpan.Zero;
                }
            }
            string unit;
            if (valuestr.EndsWith("MS"))
            {
                unit = "MS";
                val = double.Parse(valuestr.Substring(0, valuestr.Length - 2));
            }
            else
            {
                unit = StrUtil.Right(valuestr, 1);
                val = double.Parse(valuestr.Substring(0, valuestr.Length - 1));
            }
            switch (unit)
            {
                case "MS":
                    return TimeSpan.FromMilliseconds(val);
                case "S":
                    return TimeSpan.FromSeconds(val);
                case "M":
                    return TimeSpan.FromMinutes(val);
                case "H":
                    return TimeSpan.FromHours(val);
                case "D":
                    return TimeSpan.FromDays(val);
                case "W":
                    return TimeSpan.FromDays(val * 7);
                default:
                    throw new JacException(JacException.Codes.NotSupportedUnit, $"Unit '{unit}' is not supported.");
            }
        }

        /// <summary>
        /// Make TimeSpan string
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string MakeTimeSpanString(TimeSpan ts)
        {
            var sect = new[] { 0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 0.6, 0.7, 0.75, 0.8, 0.9 };
            if (ts.TotalMilliseconds < 1)
            {
                return "0S";
            }
            if (ts.TotalSeconds < 1.0)
            {
                return $"{ts.TotalMilliseconds}MS";
            }
            if (ts.TotalSeconds < 60)
            {
                return $"{ts.TotalSeconds}S";
            }
            if (ts.TotalMinutes < 5)
            {
                if (sect.Contains(ts.TotalMinutes % 1.0))
                {
                    return $"{ts.TotalMinutes}M";
                }
                else
                {
                    return $"{ts.TotalSeconds}S";
                }
            }
            if (ts.TotalMinutes < 60)
            {
                return $"{ts.TotalMinutes}M";
            }
            if (ts.TotalHours <= 3)
            {
                if (sect.Contains(ts.TotalHours % 1.0))
                {
                    return $"{ts.TotalHours}H";
                }
                else
                {
                    return $"{ts.TotalMinutes}M";
                }
            }
            if (ts.TotalHours < 24)
            {
                return $"{ts.TotalHours}H";
            }
            return $"{ts.TotalDays}D";
        }
    }
}
