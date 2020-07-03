using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Tono
{
    /// <summary>
    /// Bit List Utility
    /// </summary>
    /// <example>
    /// Count = 1
    /// Add(false)  → 0b0       Count = 1
    /// Add(true)   → 0b10      Count = 2
    /// Add(false)  → 0b010     Count = 3
    /// Add(true)   → 0b1010    Count = 4
    /// </example>
    public class BitList : IList<bool>
    {
        private static readonly byte[] masks = new byte[] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        private List<byte> Bytes = new List<byte>();    // Expects pading bit = 0

        public int Count { get; set; }
        public int ByteCount => Bytes.Count;

        /// <summary>
        /// default constructor to make instance as zero length
        /// </summary>
        public BitList()
        {
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="from"></param>
        public BitList(BitList from)
        {
            Bytes.AddRange(from.Bytes);
            Count = from.Count;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="from"></param>
        public BitList(BitList from, int startBitIndex, int nBits)
        {
            for( var i = startBitIndex; i < Math.Min(startBitIndex + nBits, from.Count); i++)
            {
                Add(from[i]);
            }
            SetMinimumSize(nBits);
        }


        public BitList(IEnumerable<bool> bits)
        {
            foreach (var bit in bits)
            {
                Add(bit);
            }
        }


        public BitList(IEnumerable<byte> bytes)
        {
            var c0 = Bytes.Count;
            Bytes.AddRange(bytes);
            Count += (Bytes.Count - c0) * 8;
        }

        public static BitList From(IEnumerable<bool> bits)
        {
            return new BitList(bits);
        }

        public static BitList From(IEnumerable<byte> bytes)
        {
            return new BitList(bytes);
        }

        public static BitList From(char value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(Int16 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(Int32 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(Int64 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(byte value) => new BitList(new[] { value });
        public static BitList From(UInt16 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(UInt32 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(UInt64 value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(Single value) => new BitList(BitConverter.GetBytes(value));
        public static BitList From(Double value) => new BitList(BitConverter.GetBytes(value));

        public byte ToByte() => Bytes[0];
        public char ToChar() => BitConverter.ToChar(Bytes.Take(1).ToArray(), 0);
        public Int16 ToInt16() => BitConverter.ToInt16(Bytes.Take(2).ToArray(), 0);
        public Int32 ToInt32() => BitConverter.ToInt32(Bytes.Take(4).ToArray(), 0);
        public Int64 ToInt64() => BitConverter.ToInt64(Bytes.Take(8).ToArray(), 0);
        public UInt16 ToUInt16() => BitConverter.ToUInt16(Bytes.Take(2).ToArray(), 0);
        public UInt32 ToUInt32() => BitConverter.ToUInt32(Bytes.Take(4).ToArray(), 0);
        public UInt64 ToUInt64() => BitConverter.ToUInt64(Bytes.Take(8).ToArray(), 0);
        public Single ToSingle() => BitConverter.ToSingle(Bytes.Take(4).ToArray(), 0);
        public Double ToDouble() => BitConverter.ToDouble(Bytes.Take(8).ToArray(), 0);

        /// <summary>
        /// Make instance string
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// Index   |Value
        /// --------+-----
        /// 0       |1
        /// 1       |0
        /// 2       |1
        /// 3       |1
        /// 4       |1
        /// ToString() = 11101
        /// </example>
        public override string ToString()
        {
            var ret = new StringBuilder();
            int len = Count;
            if (len > 128)
            {
                len = 128;
                ret.Append("...");
            }
            bool canSeparation = false;
            for (var i = len - 1; i >= 0; i--)
            {
                if (canSeparation && i % 8 == 7)
                {
                    ret.Append('_');
                }
                ret.Append(this[i] ? '1' : '0');
                canSeparation = true;
            }
            return ret.ToString();
        }

        public override int GetHashCode()
        {
            return MathUtil.GetFnvHash(Bytes);
        }
        public override bool Equals(object obj)
        {
            if (obj is BitList tar)
            {
                if (tar.Count == this.Count)
                {
                    for (var i = Bytes.Count - 1; i >= 0; i--)
                    {
                        if (Bytes[i] != tar.Bytes[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool this[int bitindex]
        {
            get => Check(bitindex);
            set => Set(bitindex, value);
        }

        public bool IsReadOnly => false;

        /// <summary>
        /// Add to upper bit
        /// </summary>
        /// <param name="bit"></param>
        public void Add(bool bit)
        {
            if (IsReadOnly) throw new ReadOnlyException();

            var bytepos = Count / 8;
            if (Bytes.Count <= bytepos)
            {
                Bytes.Add(0x00);
            }
            Count++;
            Set(Count - 1, bit);
        }

        public static BitList MakeVariableBits(UInt64 no)
        {
            var ret = new BitList();
            var nobits = BitList.From(no);
            if (no <= 0b1111)
            {
                ret.Add(true);
                ret.Add(nobits, 0, 4);
                return ret;
            }
            if (no <= 0b111111)
            {
                ret.Add(false);
                ret.Add(true);
                ret.Add(nobits, 0, 6);
                return ret;
            }
            if (no <= 0b111111111111)
            {
                ret.Add(false);
                ret.Add(false);
                ret.Add(true);
                ret.Add(nobits, 0, 12);
                return ret;
            }
            if (no <= 0b111111111111111111111111)
            {
                ret.Add(false);
                ret.Add(false);
                ret.Add(false);
                ret.Add(true);
                ret.Add(nobits, 0, 24);
                return ret;
            }
            ret.Add(false);
            ret.Add(false);
            ret.Add(false);
            ret.Add(false);
            ret.Add(nobits);
            return ret;
        }

        public static UInt64 GetNumberFromVariableBits(BitList bits)
        {
            return GetNumberFromVariableBits(bits, out var _ );
        }
        public static UInt64 GetNumberFromVariableBits(IEnumerable<bool> bits)
        {
            return GetNumberFromVariableBits(BitList.From(bits));
        }
        public static UInt64 GetNumberFromVariableBits(IEnumerable<bool> bits, out int nBit)
        {
            return GetNumberFromVariableBits(BitList.From(bits), out nBit);
        }

        public static UInt64 GetNumberFromVariableBits(BitList bits, out int nBit)
        {
            nBit = 1 + 4;
            if (bits.Count >= nBit && bits[0])
            {
                return BitList.ToBitListFixedSize(BitList.From(bits.Subbit(1, 4)), 64).ToUInt64();
            }
            nBit = 2 + 6;
            if (bits.Count >= nBit && bits[1])
            {
                return BitList.ToBitListFixedSize(BitList.From(bits.Subbit(2, 6)), 64).ToUInt64();
            }
            nBit = 3 + 12;
            if (bits.Count >= nBit && bits[2])
            {
                return BitList.ToBitListFixedSize(BitList.From(bits.Subbit(3, 12)), 64).ToUInt64();
            }
            nBit = 4 + 24;
            if (bits.Count >= nBit && bits[3])
            {
                return BitList.ToBitListFixedSize(BitList.From(bits.Subbit(4, 24)), 64).ToUInt64();
            }
            nBit = 4 + 64;
            if (bits.Count >= nBit && !bits[3])
            {
                return BitList.From(bits.Subbit(4, 64)).ToUInt64();
            }
            throw new Exception("Not expected format (GetNumberFromVariableBits)");
        }

        /// <summary>
        /// Add to upper bits
        /// </summary>
        /// <param name="bits"></param>
        public void Add(IEnumerable<bool> bits)
        {
            foreach (var bit in bits)
            {
                Add(bit);
            }
        }

        /// <summary>
        /// Add to upper bits
        /// </summary>
        /// <param name="bits"></param>
        /// <param name="startbitpos"></param>
        /// <param name="nBits"></param>
        public void Add(IEnumerable<bool> bits, int startbitpos, int nBits)
        {
            foreach (var bit in bits.Skip(startbitpos).Take(nBits))
            {
                Add(bit);
            }
        }

        /// <summary>
        /// Add to upper bits
        /// </summary>
        /// <param name="bytes"></param>
        public void Add(IEnumerable<byte> bytes)
        {
            if (Count % 8 == 0)
            {
                var c0 = Bytes.Count;
                Bytes.AddRange(bytes);
                Count += (Bytes.Count - c0) * 8;
            }
            else
            {
                Add(BitList.From(bytes));
            }
        }

        /// <summary>
        /// Add to upper bits
        /// </summary>
        /// <param name="bits"></param>
        public void Add(BitArray bits)
        {
            for (var i = 0; i < bits.Count; i++)
            {
                Add(bits[i]);
            }
        }
        public void Add(char dat) => Add(BitList.From(dat));
        public void Add(byte dat) => Add(BitList.From(dat));
        public void Add(Int16 dat) => Add(BitList.From(dat));
        public void Add(Int32 dat) => Add(BitList.From(dat));
        public void Add(Int64 dat) => Add(BitList.From(dat));
        public void Add(UInt16 dat) => Add(BitList.From(dat));
        public void Add(UInt32 dat) => Add(BitList.From(dat));
        public void Add(UInt64 dat) => Add(BitList.From(dat));
        public void Add(Single dat) => Add(BitList.From(dat));
        public void Add(Double dat) => Add(BitList.From(dat));

        /// <summary>
        /// Add pad value to fill byte segment
        /// </summary>
        /// <param name="padvalue"></param>
        public BitList AddPad(bool padValue = false)
        {
            var e = 8 - Count % 8;
            if (e < 8)
            {
                foreach (var _ in Collection.Rep(e))
                {
                    Add(padValue);
                }
            }
            return this;
        }

        public IEnumerable<bool> Subbit(int startbitpos, int nBits)
        {
            for (int i = startbitpos; i < Math.Min(Count, startbitpos + nBits); i++)
            {
                yield return this[i];
            }
        }
        public IEnumerable<bool> Subbit(int startbitpos)
        {
            for (int i = startbitpos; i < Count; i++)
            {
                yield return this[i];
            }
        }

        public byte GetByte(int byteindex)
        {
            if (byteindex < 0 || byteindex >= Bytes.Count)
            {
                return 0x00;
            }
            else
            {
                return Bytes[byteindex];
            }
        }

        public bool Check(int bitindex)
        {
            var B = GetByte(bitindex / 8);
            var bitpos = bitindex % 8;
            return (B & masks[bitpos]) != 0;
        }

        public void Set(int bitindex, bool value)
        {
            if (IsReadOnly) throw new ReadOnlyException();

            var byteindex = bitindex / 8;
            var bitpos = bitindex % 8;
            var B = GetByte(byteindex);
            if (value)
            {
                Bytes[byteindex] = (byte)(B | masks[bitpos]);
            }
            else
            {
                Bytes[byteindex] = (byte)(B & ~masks[bitpos]);
            }
        }

        public static BitList Join(BitList left, IEnumerable<bool> right)
        {
            var ret = new BitList(left);
            foreach (var bit in right)
            {
                ret.Add(bit);
            }
            return ret;
        }
        public static BitList ToBitListFixedSize(BitList left, int nBits, bool padValue = false)
        {
            var ret = new BitList(left);
            ret.SetMinimumSize(nBits);
            return ret;
        }

        public BitList SetMinimumSize(int nBit, bool padding = false)
        {
            for( var i = Count; i < nBit; i++)
            {
                Add(padding);
            }
            return this;
        }

        public void Clear()
        {
            if (IsReadOnly) throw new ReadOnlyException();

            Count = 0;
            Bytes.Clear();
        }

        public IEnumerable<byte> ToBytes()
        {
            if (Count > 0)
            {
                return Bytes;
            }
            else
            {
                return new byte[] { };
            }
        }

        public BitArray ToBitArray()
        {
            return new BitArray(this.ToArray());
        }

        public byte[] ToByteArray() => ToBytes().ToArray();


        public bool Contains(bool item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(bool[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
            {
                array[i] = Check(i);
            }
        }

        public IEnumerator<bool> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return Check(i);
            }
        }

        public int IndexOf(bool item)
        {
            for (var i = 0; i < Count; i++)
            {
                if (Check(i) == item)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, bool item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(bool item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return Check(i);
            }
        }
    }
}
