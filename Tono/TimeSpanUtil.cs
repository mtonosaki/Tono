// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// Time span utility
    /// </summary>
    public struct TimeSpanUtil
    {
        /// <summary>
        /// Parse TimeSpan considering unit.(support unit: second, seconds, sec, s)
        /// </summary>
        /// <param name="s"></param>
        /// <returns>TimeSpan.MaxValue=could not convert(such as no unit string)</returns>
        public static TimeSpan Parse(string s)
        {
            string reg = @"-?[0-9]+\.?[0-9]*";
            string num = StrUtil.LeftOn(s, reg);
            string unit = StrUtil.MidSkip(s, reg).Trim();
            double d = double.Parse(num);
            switch (unit.ToLower())
            {
                case "second":
                case "seconds":
                case "sec":
                case "s":
                    return TimeSpan.FromSeconds(d);
                default:
                    return TimeSpan.MaxValue;
            }
        }
    }
}
