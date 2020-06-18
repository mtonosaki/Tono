using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Tono
{
    /// <summary>
    /// Encryption Base class
    /// </summary>
    public abstract class EncryptionBase
    {
        public virtual string Encode(string planeText) => planeText;
        public virtual string Decode(string secretText) => secretText;
    }

    /// <summary>
    /// Default Encrypt
    /// </summary>
    public class Encrypt : EncryptionBase
    {
        private readonly EncryptRijndael Enc = new EncryptRijndael
        {
            TEXTSET64 = "mdZAjJMe62HYSO/3u7raPCNhogzDfG1nq4FwK0cskEpvVlBUXitL+byQIT5W89xR",
            KEY = "H4PY9eBxWAF4iK4K",
            MASK = "FBAEA0F4A1062D7AED6B7763D43D5492",
        };
        public override string Encode(string planeText)
        {
            return Enc.Encode(planeText);
        }
        public override string Decode(string secretText)
        {
            return Enc.Decode(secretText);
        }
    }

    /// <summary>
    /// Encryption Rijndael model inplement
    /// </summary>
    public class EncryptRijndael : EncryptionBase
    {
        /// <summary>
        /// usable 64 character for output/input
        /// </summary>
        /// <remarks>
        /// for your original security, shuffle this strings as you like.
        /// </remarks>
        public string TEXTSET64 { private get; set; } = "wK0cskEpvVlBUXitL+byQIT5W89xRmdZAjJMe62HYSO/3u7raPCNhogzDfG1nq4F";

        /// <summary>
        /// Rijndael security key
        /// </summary>
        /// <remarks>16 characters</remarks>
        public string KEY { private get; set; } = "F4iK4KH4PY9eBxWA";

        /// <summary>
        /// Scramble text
        /// </summary>
        /// <remarks>
        /// For example, if you want make different security key by user account.
        /// </remarks>
        public string MASK { private get; set; } = "D6B7763D43D5492FBAEA0F4A1062D7AE";

        private static readonly Random RND = new Random(Guid.NewGuid().GetHashCode());
        private static readonly int[] OffsetSamples = new[] { 79, 47, 37, 2, 7, 223, 269, 229, 211, 59, 89, 263, 149, 13, 179, 83, 281, 127, 199, 227, 31, 131, 73, 157, 5, 19, 139, 197, 167, 193, 53, 151, 29, 137, 97, 107, 241, 239, 163, 113, 103, 11, 257, 109, 23, 3, 41, 61, 233, 277, };
        private const string NullDescriptor = "//://Null-Descriptor-TonoUtil.Encryption//://";

        /// <summary>
        /// Make Fusion string
        /// </summary>
        /// <param name="basestr"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private string FusionString(string basestr, string filter)
        {
            var ret = new StringBuilder(basestr);
            var nB = basestr.Length;
            var nF = filter.Length;
            var offset = 0;
            for (var i = Math.Max(nB, nF) - 1; i >= 0; i--)
            {
                ret[i % nB] = TEXTSET64[(TEXTSET64.IndexOf(ret[i % nB]) + (int)filter[i % nF] + OffsetSamples[(i + offset) % OffsetSamples.Length]) % TEXTSET64.Length];
                offset++;
            }
            return ret.ToString();
        }

        /// <summary>
        /// RIjndael Encoding
        /// </summary>
        /// <param name="planeText"></param>
        /// <returns></returns>
        public override string Encode(string planeText)
        {
            if (planeText == NullDescriptor) throw new CryptographicUnexpectedOperationException("The hacked input string is not support");
            if (planeText == null)
            {
                planeText = NullDescriptor;
            }
            var iv = new StringBuilder();
            int ivN = 0;
            for (int ivi = 0; ivi < ivN + 16; ivi++)
            {
                iv.Append(TEXTSET64[RND.Next(TEXTSET64.Length - 1)]);
            }
            using (var ri = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv.ToString()),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var enc = ri.CreateEncryptor(ri.Key, ri.IV);
                byte[] buf;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(planeText);
                        }
                        buf = ms.ToArray();
                    }
                }
                return ($"{TEXTSET64[ivN]}{iv}{Convert.ToBase64String(buf)}");
            }
        }

        /// <summary>
        /// Rijndael Decoding
        /// </summary>
        /// <param name="secretText"></param>
        /// <returns></returns>
        public override string Decode(string secretText)
        {
            int ivN = TEXTSET64.IndexOf(secretText[0]);
            string iv = secretText.Substring(1, ivN + 16);
            string sec = secretText.Substring(ivN + iv.Length + 1);

            using (var rijndael = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                IV = Encoding.ASCII.GetBytes(iv),
                Key = Encoding.ASCII.GetBytes(FusionString(KEY, MASK)),
            })
            {
                var de = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using (var ms = new MemoryStream(Convert.FromBase64String(sec)))
                {
                    using (var cs = new CryptoStream(ms, de, CryptoStreamMode.Read))
                    {
                        using (var sr = new StreamReader(cs))
                        {
                            var ret = sr.ReadToEnd();
                            if (ret == NullDescriptor)
                            {
                                return null;
                            }
                            else
                            {
                                return ret;
                            }
                        }
                    }
                }
            }
        }
    }
}
