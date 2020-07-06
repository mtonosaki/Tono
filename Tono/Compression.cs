// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Tono
{
    public abstract class CompressionBase
    {
        public int LimitSize { get; set; } = int.MaxValue;

        public abstract byte[] Compress(byte[] buf);
        public abstract byte[] Decompress(byte[] buf);
    }

    public class NoCompression : CompressionBase
    {
        public override byte[] Compress(byte[] buf)
        {
            if (buf.Length + 2 > LimitSize)
            {
                return null;
            }
            var ret = BitList.From((UInt16)0xffff);
            ret.Add(buf);
            return ret.ToByteArray();
        }
        public override byte[] Decompress(byte[] buf)
        {
            var bits = new BitList(buf);
            if (bits.Count < 24)
            {
                throw new Exception("Table Format Error: Size is not enough");
            }
            var tableID = bits.SetMinimumSize(16).ToUInt16();
            if (tableID != 0xffff)
            {
                throw new Exception("Table Format Error");
            }
            return bits.ToBytes().Skip(2).ToArray();
        }
    }

    /// <summary>
    /// Compression Easy handler
    /// </summary>
    public class Compression : CompressionBase
    {
        private static readonly Dictionary<UInt16, Type> FixedTableCompressions = new Dictionary<ushort, Type>
        {
            [0xffff] = typeof(NoCompression),
            [0x8001] = typeof(CompressionHuffmanCodingASCII),
            [0x8002] = typeof(CompressionHuffmanCodingJapanese),
            [0x8003] = typeof(CompressionHuffmanCodingBase64),
            [0x8004] = typeof(CompressionHuffmanCodingNumber),
            [0x8005] = typeof(CompressionHuffmanCodingHexNumber),
            [0x0000] = typeof(CompressionHuffmanCoding),
        };

        public override byte[] Compress(byte[] buf)
        {
            var engines = FixedTableCompressions.Values.Select(a => Activator.CreateInstance(a)).ToList();
            int limit = int.MaxValue;
            var outs = new List<byte[]>();

            foreach (CompressionBase com in engines)
            {
                com.LimitSize = limit;
                var ret = com.Compress(buf);
                if (ret != null)
                {
                    if (ret.Length < limit)
                    {
                        limit = ret.Length;
                    }
                    outs.Add(ret);
                }
            }
            return outs.OrderBy(a => a.Length).First();
        }

        public override byte[] Decompress(byte[] buf)
        {
            var tableID = BitList.From(buf).SetMinimumSize(16).ToUInt16();
            if (FixedTableCompressions.TryGetValue(tableID, out var compType))
            {
                var comp = Activator.CreateInstance(compType) as CompressionBase;
                return comp.Decompress(buf);
            }
            else
            {
                return (new CompressionHuffmanCoding()).Decompress(buf);
            }
        }
    }
}
