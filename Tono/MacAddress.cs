// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace Tono
{

    /// <summary>
    /// MAC Address type & utility
    /// </summary>
    public class MacAddress
    {
        public static readonly MacAddress Empty = new MacAddress();

        public byte[] Value { get; set; } = new byte[] { 0, 0, 0, 0, 0, 0 };

        public override bool Equals(object obj)
        {
            if (obj is MacAddress tar)
            {
                return Value[0] == tar.Value[0]
                    && Value[5] == tar.Value[5]
                    && Value[1] == tar.Value[1]
                    && Value[2] == tar.Value[2]
                    && Value[3] == tar.Value[3]
                    && Value[4] == tar.Value[4]
                    ;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (Value == null)
                {
                    return 0;
                }
                var hash = 17;
                foreach (var element in Value)
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
            return $"{MacString}";
        }


        /// <summary>
        /// make string using specific separator character
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return $"{Value[0]:X2}{separator}{Value[1]:X2}{separator}{Value[2]:X2}{separator}{Value[3]:X2}{separator}{Value[4]:X2}{separator}{Value[5]:X2}";
        }

        /// <summary>
        /// make string using specific separator character
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ToString(char separator)
        {
            return ToString(separator.ToString());
        }

        /// <summary>
        /// string property (get/set)
        /// </summary>
        public string MacString
        {
            get => BitConverter.ToString(Value);
            set
            {
                var cs = value.Trim().ToLower().Split(':', '-', ' ', '.');
                if (cs.Length == 6)
                {
                    for (var i = 0; i < 6; i++)
                    {
                        Value[i] = Convert.ToByte(cs[i], 16);
                    }
                    return;
                }
                throw new FormatException("use aa:bb:cc:00:12:34 format");
            }
        }

        /// <summary>
        /// pick up OUI part from MAC address
        /// </summary>
        /// <returns></returns>
        public MacOui ToOui()
        {
            return MacOui.From(this);
        }
    }
}
