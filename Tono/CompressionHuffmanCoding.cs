// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tono
{
    /// <summary>
    /// Huffman coding external table small version
    /// </summary>
    public class CompressionHuffmanCoding : CompressionBase
    {
        public class ValueCount : HuffmanTree.INode
        {
            public byte Value { get; set; }
            public uint Count { get; set; }
            public double Cost => Count;

            public override string ToString()
            {
                return $"{Value} x {Count}";
            }
        }

        public override byte[] Compress(byte[] buf)
        {
            var ret = MakeTableBinary(buf, out var table);

            // ADD DATA FIELDS
            ret.Add(BitList.MakeVariableBits((UInt64)buf.Length));  // [DA] Data Bytes

            foreach (var c in buf)
            {
                ret.Add(table[c]);  // [DB] Huffman code
                if (ret.ByteCount > LimitSize)
                {
                    return null;
                }
            }
            ret.AddPad();   // [DC] padding

            return ret.ToByteArray();
        }
        public override byte[] Decompress(byte[] buf)
        {
            var ret = new BitList();
            var bits = new BitList(buf);
            var table = ReadTableBinary(bits, out var nextBitIndex);

            //=== READ HUFFMAN CODE ===
            var len = (int)BitList.GetNumberFromVariableBits(bits.Subbit(nextBitIndex), out var nobits);    // [DA] Data Bytes
            nextBitIndex += nobits;
            var outdat = new List<byte>();
            for (var i = 0; i < len; i++)
            {
                var hb = new BitList();
                for (var j = 0; j < 32768; j++)
                {
                    hb.Add(bits[nextBitIndex++]);
                    if (table.TryGetValue(hb, out var value))
                    {
                        outdat.Add(value);
                        break;
                    }
                }
            }
            return outdat.ToArray();
        }

        protected BitList MakeTableBinary(UInt16 tableID, Dictionary<BitList, byte> table, out Dictionary<byte, BitList> valbits)
        {
            Debug.Assert(tableID > 32768);

            valbits = table.ToSwapKeyValue();
            var ret = new BitList();
            ret.Add(tableID);   // [TA] Table ID (>= 32768)
            return ret;
        }

        public static BitList MakeTableBinary(byte[] buf, out Dictionary<byte, BitList> table)
        {
            // Make Huffman Tree
            var nodemap = Collection.Seq(256).Select(a => new ValueCount { Value = (byte)a, Count = 0, }).ToDictionary(a => a.Value);
            for (var i = 0; i < buf.Length; i++)
            {
                nodemap[buf[i]].Count++;
            }
            var htree = new HuffmanTree(nodemap.Values.Where(a => a.Count > 0))
                        .Build();

            table = new Dictionary<byte, BitList>();
            foreach (ValueCount vc in htree)
            {
                table[vc.Value] = htree.GetBitResult(vc);
            }

            var ret = new BitList();
            ret.Add((UInt16)table.Count);   // [TA] TABLE COUNT (16bits) / < 32767 = Count
            foreach (var kv in table)
            {
                ret.Add(kv.Key);    // [TB] VALUE (8bits)
                var lenbits = BitList.MakeVariableBits((UInt64)kv.Value.Count);
                ret.Add(lenbits);   // [TC] LENGTH OF BITPATTERN
                ret.Add(kv.Value);  // [TE] BIT Pattern (n bits)
            }
            ret.AddPad();   // [TF] padding

            return ret;
        }


        public static Dictionary<BitList, byte> ReadTableBinary(BitList bits, out int nextBitIndex)
        {
            // === READ TABLE ===
            var table = new Dictionary<BitList, byte>();
            var tableCount = BitList.From(bits.Subbit(0, 16)).SetMinimumSize(16).ToUInt16();   // [TA]
            Debug.Assert(tableCount < 32768);

            nextBitIndex = 16;
            for (var i = 0; i < tableCount; i++)
            {
                var value = BitList.From(bits.Subbit(nextBitIndex, 8)).ToByte(); // [TB]
                nextBitIndex += 8;
                var bitLen = (int)BitList.GetNumberFromVariableBits(bits.Subbit(nextBitIndex), out var numbits);
                nextBitIndex += numbits;
                table[BitList.From(bits.Subbit(nextBitIndex, bitLen))] = value; // [TE]
                nextBitIndex += bitLen;
            }
            if (nextBitIndex % 8 != 0)
            {
                nextBitIndex += 8 - nextBitIndex % 8; // [TF]
            }
            return table;
        }
    }
}
