// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace Tono
{
    /// <summary>
    /// "IPv4 network address" type including utility
    /// </summary>
    public class IpNetwork
    {
        /// <summary>
        /// bit number of network address space
        /// </summary>
        [DataMember]
#pragma warning disable IDE1006
        public int nBit { get; set; }
#pragma warning restore IDE1006

        /// <summary>
        /// IPv4
        /// </summary>
        [IgnoreDataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "<OK>")]
        public byte[] Data { get; } = new byte[4];

        /// <summary>
        /// Convert instance data to base64 (for json persistance)
        /// </summary>
        [DataMember]
        public string DataBase64
        {
            get => Convert.ToBase64String(Data);
            set
            {
                var tmp = Convert.FromBase64String(value);
                for (var i = Data.Length - 1; i >= 0; i--)
                {
                    Data[i] = tmp[i];
                }
            }
        }

        /// <summary>
        /// Clone instance
        /// </summary>
        /// <returns></returns>
        public IpNetwork Clone()
        {
            var ret = new IpNetwork
            {
                nBit = nBit
            };
            ret.Data[0] = Data[0];
            ret.Data[1] = Data[1];
            ret.Data[2] = Data[2];
            ret.Data[3] = Data[3];
            return ret;
        }

        /// <summary>
        /// make instance text
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Data[0]}.{Data[1]}.{Data[2]}.{Data[3]}/{nBit}";
        }

        /// <summary>
        /// make string that not use "/" character
        /// </summary>
        /// <returns></returns>
        public string ToRowKey()
        {
            return $"{Data[0]}.{Data[1]}.{Data[2]}.{Data[3]}-{nBit}";
        }

        public override bool Equals(object obj)
        {
            if (obj is IpNetwork net)
            {
                return net.nBit == nBit && net.Data[0] == Data[0] && net.Data[1] == Data[1] && net.Data[2] == Data[2] && net.Data[3] == Data[3];
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Binary.MakeInt32FromBytes(Data);
        }

        // mask table (for speed)
        private static readonly byte[][] masks = new byte[][]
        {
			/*00*/ new byte[]{ 0x00, 0x00, 0x00, 0x00 },
			/*01*/ new byte[]{ 0x80, 0x00, 0x00, 0x00 },
			/*02*/ new byte[]{ 0xc0, 0x00, 0x00, 0x00 },
			/*03*/ new byte[]{ 0xe0, 0x00, 0x00, 0x00 },
			/*04*/ new byte[]{ 0xf0, 0x00, 0x00, 0x00 },
			/*05*/ new byte[]{ 0xf8, 0x00, 0x00, 0x00 },
			/*06*/ new byte[]{ 0xfc, 0x00, 0x00, 0x00 },
			/*07*/ new byte[]{ 0xfe, 0x00, 0x00, 0x00 },
			/*08*/ new byte[]{ 0x00, 0x00, 0x00 },
			/*09*/ new byte[]{ 0x80, 0x00, 0x00 },
			/*10*/ new byte[]{ 0xc0, 0x00, 0x00 },
			/*11*/ new byte[]{ 0xe0, 0x00, 0x00 },
			/*12*/ new byte[]{ 0xf0, 0x00, 0x00 },
			/*13*/ new byte[]{ 0xf8, 0x00, 0x00 },
			/*14*/ new byte[]{ 0xfc, 0x00, 0x00 },
			/*15*/ new byte[]{ 0xfe, 0x00, 0x00 },
			/*16*/ new byte[]{ 0x00, 0x00 },
			/*17*/ new byte[]{ 0x80, 0x00 },
			/*18*/ new byte[]{ 0xc0, 0x00 },
			/*19*/ new byte[]{ 0xe0, 0x00 },
			/*20*/ new byte[]{ 0xf0, 0x00 },
			/*21*/ new byte[]{ 0xf8, 0x00 },
			/*22*/ new byte[]{ 0xfc, 0x00 },
			/*23*/ new byte[]{ 0xfe, 0x00 },
			/*24*/ new byte[]{ 0x00 },
			/*25*/ new byte[]{ 0x80 },
			/*26*/ new byte[]{ 0xc0 },
			/*27*/ new byte[]{ 0xe0 },
			/*28*/ new byte[]{ 0xf0 },
			/*29*/ new byte[]{ 0xf8 },
			/*30*/ new byte[]{ 0xfc },
			/*31*/ new byte[]{ 0xfe },
			/*32*/ Array.Empty<byte>(),
        };

        /// <summary>
        /// set zero to host address
        /// </summary>
        public void Normalize()
        {
            for (int i = 4 - masks[nBit].Length, j = 0; i < 4; i++)
            {
                Data[i] = (byte)(Data[i] & masks[nBit][j++]);
            }
        }

        /// <summary>
        /// Parse instance
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IpNetwork Parse(string str)
        {
            var ret = new IpNetwork();
            var sl = str.IndexOf('/');
            if (sl >= 0)
            {
                ret.nBit = int.Parse(str.Substring(sl + 1));
                str = str.Substring(0, sl);
            }
            else
            {
                ret.nBit = 32;
            }
            int i = 0;
            foreach (string s in str.Split('.'))
            {
                var val = byte.Parse(s);
                ret.Data[i++] = val;
                if (i >= 4)
                {
                    break;
                }
            }
            ret.Normalize();
            return ret;
        }

        /// <summary>
        /// change network address space
        /// </summary>
        /// <param name="netwrok"></param>
        /// <param name="newNBit"></param>
        /// <returns></returns>
        public static IpNetwork ToNetwork(IpNetwork netwrok, int newNBit)
        {
            var ip2 = netwrok.Clone();
            ip2.nBit = newNBit;
            ip2.Normalize();
            return ip2;
        }

        /// <summary>
        /// get host address part
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static ulong GetHostAddress(IPAddress ip, int nbit)
        {
            var rmask = new byte[] { 0x00, 0x00, 0x00, 0x00 };

            foreach (var i in Collection.Seq(masks[nbit].Length))
            {
                rmask[4 - masks[nbit].Length + i] = (byte)~masks[nbit][i];
            }

            var ab = ip.GetAddressBytes();
            foreach (var i in Collection.Seq4)
            {
                ab[i] = (byte)(ab[i] & rmask[i]);
            }

            var ret = BitConverter.ToUInt32(ab.Reverse().ToArray(), 0);
            return ret;
        }

        /// <summary>
        /// make new instance
        /// </summary>
        /// <param name="net">network part</param>
        /// <param name="hostaddress">host address part typed ulong</param>
        /// <returns></returns>
        public static IPAddress MakeIPAddress(IpNetwork net, ulong hostaddress)
        {
            unchecked
            {
                var ha = new byte[]
                {
                    (byte)((hostaddress >> 24) & 0xff),
                    (byte)((hostaddress >> 16) & 0xff),
                    (byte)((hostaddress >> 8) & 0xff),
                    (byte)(hostaddress & 0xff),
                };

                var rmask = new byte[4];
                for (var i = 0; i < masks[net.nBit].Length; i++)
                {
                    rmask[4 - masks[net.nBit].Length + i] = (byte)~(masks[net.nBit][i]);
                }
                for (var i = 0; i < 4; i++)
                {
                    ha[i] &= rmask[i];
                }
                for (var i = 0; i < 4; i++)
                {
                    ha[i] |= net.Data[i];
                }
                return new IPAddress(ha);
            }
        }
    }
}
