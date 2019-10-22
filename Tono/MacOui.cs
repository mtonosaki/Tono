// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono
{
    /// <summary>
    /// OUI=Organizationally Unique Identifier = part of MAC address (as vendor code)
    /// </summary>
    public class MacOui
    {
        public byte[] Data { get; set; } = new byte[] { 0, 0, 0 };

        public override bool Equals(object obj)
        {
            if (obj is MacOui tar)
            {
                return Data[0] == tar.Data[0]
                    && Data[1] == tar.Data[1]
                    && Data[2] == tar.Data[2]
                    ;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (Data == null)
                {
                    return 0;
                }
                var hash = 17;
                foreach (var element in Data)
                {
                    hash = hash * 31 + element;
                }
                return hash;
            }
        }

        /// <summary>
        /// make string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString("-");
        }


        /// <summary>
        /// make string using specific separator charactere
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return $"{Data[0]:X2}{separator}{Data[1]:X2}{separator}{Data[2]:X2}";
        }

        /// <summary>
        /// Parse string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MacOui Parse(string value)
        {
            var ret = new MacOui();

            var cs = value.Trim().ToLower().Split(new[] { ':', '-', ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (cs.Length >= 3)
            {
                for (var i = 0; i < 3; i++)
                {
                    ret.Data[i] = Convert.ToByte(cs[i], 16);
                }
                return ret;
            }
            throw new FormatException("use aa:bb:cc format");
        }

        /// <summary>
        /// Pick up OUI code from MAC address
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public static MacOui From(MacAddress mac)
        {
            var ret = new MacOui();
            ret.Data[0] = mac.Value[0];
            ret.Data[1] = mac.Value[1];
            ret.Data[2] = mac.Value[2];
            return ret;
        }
    }
}
