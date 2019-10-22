// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tono
{
    /// <summary>
    /// Databse utility
    /// </summary>
    public static class DbUtil
    {
        /// <summary>
        /// List up the field names in the obj object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetFieldNames(Type obj)
        {
            return GetFieldNames("", obj, "");
        }

        /// <summary>
        /// List up the field names(formatted prefixes) in the obj object
        /// </summary>
        /// <param name="prefix">string before the name</param>
        /// <param name="obj"></param>
        /// <param name="postfix">string after the name</param>
        /// <returns></returns>
        public static IEnumerable<string> GetFieldNames(string prefix, Type obj, string postfix)
        {
            var col =
                //from f in obj.GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.ExactBinding | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance)
                from f in obj.GetRuntimeProperties() // for .NET Standard 1.4
                select $"{prefix}{f.Name}{postfix}";
            return col;
        }

        /// <summary>
        /// check valid value (NULL is INVALID)
        /// </summary>
        /// <param name="value"></param>
        /// <returns>false = null</returns>
        private static bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }
            if (value is DBNull)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check null or empty value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true : null or empty</returns>
        public static bool IsNullOrEmpty(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is DBNull)
            {
                return true;
            }

            if (value is string str)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// get string but returns default value if null.
        /// </summary>
        /// <param name="value">レコードの値</param>
        /// <returns>文字列</returns>
        public static string ToString(object value, string def = default)
        {
            if (IsValid(value))
            {
                return value.ToString();
            }
            else
            {
                return def;
            }
        }

        /// <summary>
        /// get bool from value object. support string parse such as "true"
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// support below format
        ///     yes/no, true/false, enable/disable
        ///     0 = false
        /// </remarks>
        public static bool ToBoolean(object value, bool def = default)
        {
            try
            {
                if (value is bool)
                {
                    return (bool)value;
                }
                if (value is int)
                {
                    return ((int)value) != 0;
                }
                if (value is double)
                {
                    return ((double)value) != 0.0;
                }
                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return false;
                    }
                    if (s.IndexOf("yes", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return true;
                    }
                    if (s.IndexOf("no", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return false;
                    }
                    if (s.IndexOf("true", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return true;
                    }
                    if (s.IndexOf("false", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return false;
                    }
                    if (s.IndexOf("enable", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return true;
                    }
                    if (s.IndexOf("disable", StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        return false;
                    }
                    try
                    {
                        double r = double.Parse((string)value);
                        return r != 0.0;
                    }
                    catch (Exception)
                    {
                        return bool.Parse((string)value);
                    }
                }
                return def;
            }
            catch (Exception)
            {
                return def;
            }
        }


        /// <summary>
        /// get double from value safely.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static double ToDouble(object value, double def = default)
        {
            try
            {
                if (value is DBNull || value == null)
                {
                    return def;
                }
                if (value is string)
                {
                    return double.Parse(value.ToString());
                }
                else
                {
                    return (double)value;
                }
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// get byte from value safely.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static byte ToByte(object value, byte def = default)
        {
            try
            {
                if (value is DBNull)
                {
                    return def;
                }
                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return def;
                    }
                    else
                    {
                        return byte.Parse(value.ToString());
                    }
                }
                else if (value is double)
                {
                    return byte.Parse(((double)value).ToString());
                }
                else
                {
                    return (byte)value;
                }
            }
            catch (Exception)
            {
                return def;
            }
        }


        /// <summary>
        /// get integer from value safely.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static int ToInt(object value, int def = default)
        {
            try
            {
                if (value is DBNull || value == null)
                {
                    return def;
                }
                if (value is string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return def;
                    }
                    else
                    {
                        return int.Parse(value.ToString());
                    }
                }
                else if (value is double)
                {
                    return int.Parse(((double)value).ToString());
                }
                else
                {
                    return (int)value;
                }
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// Get TimeSpan from value safely. (value like Excel)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="org"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static TimeSpan ToTimeSpan(object value, DateTime org, TimeSpan def)
        {
            try
            {
                if (value is DBNull || value == null)
                {
                    return def;
                }
                if (value is DateTime)
                {
                    TimeSpan ret = ((DateTime)value) - org;
                    return ret;
                }
                if (value is double)
                {
                    return TimeSpan.FromDays((double)value);
                }
                else
                {
                    string s = value.ToString();
                    if (s.StartsWith("1899/12/30"))
                    {
                        return DateTime.Parse(s) - org;
                    }
                    else
                    {
                        if (s.IndexOf(".") >= 0)
                        {
                            double d = double.Parse(s);
                            return TimeSpan.FromDays(d);
                        }
                        else
                        {
                            return TimeSpan.Parse(s);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return def;
            }
        }

        /// <summary>
        /// get DateTime from value safely. Excel like.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(object value, DateTime def = default)
        {
            try
            {
                if (value is DBNull)
                {
                    return def;
                }
                if (value is DateTime)
                {
                    return ((DateTime)value);
                }
                if (value is double)
                {
                    return (new DateTime(1900, 1, 1, 0, 0, 0)) + TimeSpan.FromDays((double)value - 1.0);
                }
                if (value is int)
                {
                    return (new DateTime(1900, 1, 1, 0, 0, 0)) + TimeSpan.FromDays((double)value - 1.0);
                }
                else
                {
                    string s = value.ToString();
                    if (s.IndexOfAny(new char[] { ':', '/' }) >= 0)
                    {
                        DateTime ret = DateTime.Parse(value.ToString());
                        return ret;
                    }
                    else
                    {
                        double days = double.Parse(s);
                        return (new DateTime(1900, 1, 1, 0, 0, 0)) + TimeSpan.FromDays(days - 1.0);
                    }
                }
            }
            catch (Exception)
            {
                return def;
            }
        }
    }
}
