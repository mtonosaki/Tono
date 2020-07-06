// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Tono;

namespace UnitTestCore
{
    [TestClass]
    public class CompressionTest
    {
        [TestMethod]
        public void Test001()
        {
            var co = new CompressionHuffmanCoding();
            var intext = "FROM WIKIPEDIA: FLORENCE VAN LEER EARLE COATES (JULY 1, 1850 - APRIL 6, 1927) WAS AN AMERICAN POET. SHE BECAME WELL KNOWN, BOTH AT HOME AND ABROAD, FOR HER WORKS OF POETRY, NEARLY THREE HUNDRED OF WHICH WERE PUBLISHED IN LITERARY MAGAZINES SUCH AS THE ATLANTIC MONTHLY, SCRIBNER'S MAGAZINE, THE LITERARY DIGEST, LIPPINCOTT'S, THE CENTURY MAGAZINE, AND HARPER'S MAGAZINE. SHE WAS ENCOURAGED BY MATTHEW ARNOLD WITH WHOM SHE MAINTAINED A CORRESPONDENCE UNTIL HIS DEATH IN 1888. MANY OF HER NATURE POEMS WERE INSPIRED BY THE FLORA AND FAUNA OF THE ADIRONDACKS, WHERE THE COATES FAMILY SPENT THEIR SUMMER MONTHS AT \"CAMP ELSINORE\" BESIDE UPPER ST. REGIS LAKE; HERE THEY ENTERTAINED MANY FRIENDS SUCH AS OTIS SKINNER, VIOLET OAKLEY, HENRY MILLS ALDEN, AND AGNES REPPLIER.";
            intext = intext.ToUpper();
            var bufin = Encoding.ASCII.GetBytes(intext);
            var bufout = co.Compress(bufin);
            Assert.IsTrue(bufout.Length <= bufin.Length);   // Shuld success good compression

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.ASCII.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }
        [TestMethod]
        public void Test002()
        {
            var co = new CompressionHuffmanCoding();
            var intext = "ウィキペディアより：有茎の種。高さは15 - 20 cmになる。地下茎は太く長く発達し、垂直に伸び、長さ0.3 - 1 mに達し、肥厚して木質化する。多数の地上茎を分枝させ、しばしば大株になり、径50 cmに達する。根出葉の葉身は質が厚く光沢があり、表面が内側に巻き、心形で、花時に長さ幅ともに1 - 2.5 cm、花後に3.5 - 4 cmになり、先は鋭頭または短くとがり、基部は心形、縁には波状の鋸歯があり、両面ともに無毛。葉の表面は濃緑色で、裏面は淡緑色になる。基部にある托葉は狭卵形で、縁は羽状に浅裂-深裂する。葉柄は長さ3-5cmになり、無毛。花期は4月から5月。花柄は根出葉および茎葉の腋から伸び、葉より高く抜く出て花をつける。花は径2 - 2.5 cmと大きく、濃紫色から淡紫色。花弁は長さ13 - 15 mm、花弁の先は円みを帯び、側弁の基部は無毛。唇弁の距は太く短く、長さ約5 mmで、白色。萼片は広披針形で、先端は鋭突頭。雄蕊は5個あり、花柱は筒形になり、柱頭は下向きに突き出る。染色体数は2n=20。1株に20から30以上の花が咲く個体もある。太平洋側では花弁の幅が狭く、花色は淡紫色に、日本海側では花弁の幅が広く円く、花色が濃紫色にになる傾向がある。";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);
            Assert.IsTrue(bufout.Length <= bufin.Length);   // Shuld success good compression

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test003()
        {
            var co = new CompressionHuffmanCoding();
            var intext = @"★★★★★★★★★★★★★😊😎🎉😃★★★★★★★★★★★★★★★";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);
            var outtext = Encoding.UTF8.GetString(co.Decompress(bufout));
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test004()
        {
            var co = new CompressionHuffmanCodingASCII();
            var intext = "Hello C# World!!";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test005()
        {
            var co = new CompressionHuffmanCodingASCII();
            var intext = "Much of the South's infrastructure was destroyed, especially its railroads. The Confederacy collapsed, slavery was abolished, and four million black slaves were freed. The war is one of the most studied and written about episodes in U.S. history.";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test006()
        {
            var co = new CompressionHuffmanCodingASCII();
            var intext = "東京都特許許可局、蛙ピョコピョコ３ピョコピョコ、合わせてピョコピョコ、６ピョコピョコ。";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test007()
        {
            var co = new CompressionHuffmanCodingJapanese();
            var intext = "東京都特許許可局、蛙ピョコピョコ３ピョコピョコ、合わせてピョコピョコ、６ピョコピョコ。";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }
        [TestMethod]
        public void Test008()
        {
            var co = new CompressionHuffmanCodingBase64();
            var intext = "PQdnaZoupi9WkURNdgBlw9j2sw3jn8LsYbXoD3re+BkOyK4/OY9JfvGs18000FAb8mx/qvM/hlZSQLbpI0CUOQ==";
            var bufin = Encoding.UTF8.GetBytes(intext);
            var bufout = co.Compress(bufin);

            var bufin2 = co.Decompress(bufout);
            var outtext = Encoding.UTF8.GetString(bufin2);
            Assert.AreEqual(intext, outtext);
        }

        [TestMethod]
        public void Test009()
        {
            var co = new Compression();
            foreach (var bufin in new[]
            {
                Encoding.UTF8.GetBytes("123"),
                Encoding.UTF8.GetBytes("Much of the South's infrastructure was destroyed, especially its railroads. The Confederacy collapsed, slavery was abolished, and four million black slaves were freed. The war is one of the most studied and written about episodes in U.S. history."),
                Encoding.UTF8.GetBytes("東京都特許許可局、蛙ピョコピョコ３ピョコピョコ、合わせてピョコピョコ、６ピョコピョコ。"),
                Encoding.UTF8.GetBytes("Server=tcp:vnetpocdbserver.database.windows.net,1433;Initial Catalog=vnetpocdb;Persist Security Info=False;User ID=vnetpocadmin;Password=HELLO_V-Net2020!!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"),
                Encoding.UTF8.GetBytes("8dcf0cbc8c9c-4a3e-6d44-5ab5-014066f9"),
                Encoding.UTF8.GetBytes("PQdnaZoupi9WkURNdgBlw9j2sw3jn8LsYbXoD3re+BkOyK4/OY9JfvGs18000FAb8mx/qvM/hlZSQLbpI0CUOQ=="),
            })
            {
                var bufout = co.Compress(bufin);
                var bufin2 = co.Decompress(bufout);
                Assert.IsTrue(bufout.Length - bufin.Length < 3, $"Size is not compressed");
                Assert.AreEqual(bufin.Length, bufin2.Length);
                for (var i = 0; i < bufin.Length; i++)
                {
                    Assert.AreEqual(bufin[i], bufin2[i]);
                }
            }
        }


        [TestMethod]
        public void MakeTableASCII()
        {
            var intext = "using System; using System.Collections; using System.Collections.Generic; using System.Diagnostics; using System.Linq; using System.Text; using System.Xml; namespace Tono {   public abstract class CompressionBase   {     public abstract byte[] Compress(byte[] buf);     public abstract byte[] Decompress(byte[] buf);   }   /// <summary>   /// Compression Easy handler   /// </summary>   public class Compression : CompressionBase   {     private readonly CompressionHuffmanCoding coder = new CompressionHuffmanCoding     {     };     public override byte[] Compress(byte[] buf)     {       return coder.Compress(buf);     }     public override byte[] Decompress(byte[] buf)     {       return coder.Decompress(buf);     }   }   public class CompressionHuffmanCodingASCII : CompressionHuffmanCoding   {     private static readonly Dictionary<BitList, byte> Table = ReadTableBinary(new BitList(new byte[] { 0xc2, 0x00, 0x20, 0x42, 0x33, 0xc5, 0xd7, 0x29, 0x4e, 0xdc, 0xb1, 0x43, 0x41, 0xb6, 0x34, 0x90, 0xcd, 0x10, 0x63, 0xc1, 0x28, 0x86, 0xce, 0x10, 0x9b, 0xc2, 0x5d, 0x1c, 0x76, 0x02, 0xc4, 0x5a, 0x03, 0xb1, 0x3c, 0x51, 0xcc, 0x8b, 0x60, 0xcc, 0x3d, 0x82, 0x31, 0xd7, 0x8b, 0xc5, 0x1c, 0x23, 0x89, 0xd9, 0x15, 0xc4, 0x24, 0xe6, 0x30, 0xc2, 0x10, 0x72, 0x53, 0xf0, 0x9b, 0x82, 0xc8, 0x1c, 0x78, 0x93, 0x05, 0xfb, 0x25, 0x0c, 0x76, 0xd5, 0x30, 0xd8, 0x69, 0xc1, 0x60, 0x5b, 0x07, 0x83, 0xad, 0x15, 0x0b, 0x36, 0x38, 0x12, 0xac, 0x18, 0x08, 0x46, 0x8e, 0x81, 0xd2, 0x1c, 0xf0, 0x14, 0x05, 0x3a, 0x11, 0x06, 0xda, 0x49, 0x18, 0x68, 0xdd, 0x58, 0xa0, 0x69, 0x93, 0x40, 0xa1, 0x41, 0x20, 0x65, 0x0e, 0xf0, 0x0e, 0x03, 0x7c, 0x25, 0x0e, 0xf0, 0x3e, 0xe2, 0x00, 0x2f, 0x26, 0x0e, 0xf0, 0x44, 0xf2, 0x00, 0x8f, 0x56, 0x1e, 0xe0, 0x21, 0xcf, 0x02, 0x1c, 0x8b, 0x05, 0x58, 0x3b, 0x0b, 0x30, 0x73, 0x12, 0xa0, 0x35, 0x0e, 0xe0, 0x9f, 0xe1, 0x00, 0xbe, 0x22, 0x0e, 0xe0, 0x3d, 0xf1, 0x00, 0x9e, 0x7a, 0x22, 0xc0, 0xe3, 0x10, 0x11, 0xe0, 0x11, 0x1e, 0x08, 0xf0, 0x70, 0x86, 0x01, 0xdc, 0x96, 0x06, 0xb0, 0xbb, 0x34, 0x80, 0x55, 0xa5, 0x01, 0xcc, 0x20, 0x0d, 0x60, 0x8c, 0x59, 0x00, 0x0d, 0x14, 0x01, 0xf2, 0x0b, 0x8a, 0x00, 0xf9, 0xfa, 0x44, 0x80, 0xbc, 0x7e, 0x22, 0x40, 0x9e, 0x42, 0x11, 0x20, 0xb7, 0xa1, 0x08, 0x90, 0x6b, 0x50, 0x04, 0xc8, 0x49, 0x28, 0x02, 0xe4, 0xe0, 0x12, 0x01, 0xb2, 0x7b, 0x89, 0x00, 0xd9, 0xb2, 0x44, 0x80, 0xac, 0x5a, 0x22, 0x40, 0x96, 0x3d, 0x11, 0x20, 0x33, 0x9f, 0x08, 0x90, 0xe9, 0x4b, 0x04, 0xc8, 0x08, 0x26, 0x02, 0x64, 0x88, 0x14, 0x01, 0xd2, 0x4b, 0x8a, 0x00, 0xe9, 0x1e, 0x45, 0x80, 0xb4, 0x90, 0x22, 0x40, 0x1a, 0x4b, 0x11, 0x20, 0xf5, 0xa5, 0x08, 0x90, 0x8a, 0x52, 0x04, 0x48, 0x59, 0x29, 0x02, 0xa4, 0x48, 0x14, 0x01, 0x92, 0x2b, 0x8a, 0x00, 0xc9, 0x0e, 0x45, 0x80, 0xa4, 0x88, 0x22, 0x40, 0x92, 0x46, 0x11, 0x20, 0xb1, 0xa3, 0x08, 0x90, 0x68, 0x51, 0x04, 0x48, 0xc8, 0x28, 0x02, 0x24, 0xa0, 0xd2, 0x00, 0x62, 0x8f, 0x08, 0x10, 0xde, 0x47, 0x04, 0x08, 0xb7, 0x23, 0x02, 0x84, 0xe5, 0x11, 0x01, 0xc2, 0x44, 0x89, 0x00, 0xa1, 0xb1, 0x44, 0x80, 0x50, 0x3f, 0x22, 0x40, 0x48, 0x20, 0x11, 0x20, 0x84, 0x93, 0x07, 0x10, 0x34, 0x11, 0x01, 0x02, 0xab, 0x88, 0x00, 0x81, 0x48, 0x44, 0x80, 0x80, 0x25, 0x22, 0x40, 0x00, 0x0e, 0x12, 0xc0, 0xff, 0x0e, 0x12, 0xc0, 0x3f, 0x0d, 0x12, 0xc0, 0xdf, 0x0d, 0x12, 0xc0, 0x9f, 0x38, 0x12, 0xc0, 0xef, 0x3f, 0x12, 0xc0, 0x2f, 0x0f, 0x12, 0xc0, 0xcf, 0x0f, 0x12, 0xc0, 0x0f, 0x0a, 0x12, 0xc0, 0xf7, 0x0a, 0x12, 0xc0, 0x37, 0x09, 0x12, 0xc0, 0xd7, 0x09, 0x12, 0xc0, 0x17, 0x0c, 0x12, 0xc0, 0xe7, 0x0c, 0x12, 0xc0, 0x27, 0x0b, 0x12, 0xc0, 0xc7, 0x0b, 0x12, 0xc0, 0x07, 0x5d, 0x11, 0xc0, 0xfb, 0xae, 0x08, 0xe0, 0x0d, 0x57, 0x04, 0xf0, 0x9a, 0x2b, 0x02, 0x78, 0xf1, 0x15, 0x01, 0x3c, 0xff, 0x8a, 0x00, 0x9e, 0x78, 0x45, 0x00, 0x8f, 0xbd, 0x22, 0x80, 0x07, 0x29, 0x0e, 0xc0, 0x65, 0x20, 0x01, 0x9c, 0x6f, 0x20, 0x01, 0x9c, 0x53, 0x20, 0x01, 0x9c, 0x5d, 0x20, 0x01, 0x9c, 0x81, 0x20, 0x01, 0x9c, 0x8e, 0x20, 0x01, 0x9c, 0x72, 0x20, 0x01, 0x9c, 0x7c, 0x20, 0x01, 0x9c, 0x18, 0x20, 0x01, 0x1c, 0x27, 0x20, 0x01, 0x1c, 0x0b, 0x20, 0x01, 0x1c, 0x15, 0x20, 0x01, 0x1c, 0x39, 0x20, 0x01, 0x1c, 0x46, 0x20, 0x01, 0x1c, 0x2a, 0x20, 0x01, 0x1c, 0x34, 0x20, 0x01, 0x1c, 0x10, 0x15, 0x01, 0xec, 0x8f, 0x8a, 0x00, 0xf6, 0x40, 0x45, 0x00, 0xbb, 0xa1, 0x22, 0x80, 0x1d, 0x53, 0x11, 0xc0, 0xf6, 0xa9, 0x08, 0x60, 0x8b, 0x54, 0x04, 0xb0, 0x59, 0x2a, 0x02, 0xd8, 0xd0, 0x14, 0x01, 0xac, 0x6f, 0x8a, 0x00, 0xd6, 0x30, 0x45, 0x00, 0xab, 0x99, 0x22, 0x80, 0x15, 0x4f, 0x11, 0xc0, 0xf2, 0xa7, 0x08, 0x60, 0x89, 0x53, 0x04, 0xb0, 0xd8, 0x29, 0x02, 0x58, 0x90, 0x15, 0x01, 0xcc, 0xcf, 0x8a, 0x00, 0xe6, 0x60, 0x45, 0x00, 0xb3, 0xb1, 0x22, 0x80, 0x19, 0x5b, 0x11, 0xc0, 0xf4, 0xad, 0x08, 0x60, 0x8a, 0x56, 0x04, 0x30, 0x59, 0x2b, 0x02, 0x98, 0x50, 0x15, 0x01, 0x8c, 0xaf, 0x8a, 0x00, 0xc6, 0x50, 0x45, 0x00, 0xa3, 0xa9, 0x22, 0x80, 0x11, 0x57, 0x11, 0xc0, 0xf0, 0xab, 0x08, 0x60, 0x88, 0x55, 0x04, 0x30, 0xd8, 0x2a, 0x02, 0x18, 0x10, 0xc6, 0x00, 0x74, 0x18, 0x03, 0x90, 0x1c, 0x0d, 0x80, 0x03, 0x6a, 0x00, 0x4c, 0x46, 0x03, 0xa0, 0x36, 0x1a, 0x00, 0x81, 0xc3, 0x00, 0x70, 0x52, 0x03, 0x80, 0x28, 0x1e, 0x00, 0x38, 0xc5, 0x03, 0x00, 0x09, 0x71, 0x00, 0x00, }), out var _);     public override byte[] Compress(byte[] buf)     {       var ret = MakeTableBinary(0x8001, Table, out var table);       // ADD DATA FIELDS       ret.Add((Int32)buf.Length);        // [DA] Data Bytes (32bits)       foreach (var c in buf)       {         ret.Add(table[c]);          // [DB] Huffman code       }       ret.AddPad();               // [DC] padding       return ret.ToByteArray();     }     public override byte[] Decompress(byte[] buf)     {       var ret = new BitList();       var bits = new BitList(buf);       var nextBitIndex = 0;       var tableID = BitList.From(bits.Subbit(0, 16)).ToUInt16();  // [TA]       Debug.Assert(tableID == 0x8001);       var table = Table;       //=== READ HUFFMAN CODE ===       var len = BitList.From(bits.Subbit(nextBitIndex, 32)).ToInt32();       nextBitIndex += 32;       var outdat = new List<byte>();       for (var i = 0; i < len; i++)       {         var hb = new BitList();         for (var j = 0; j < 32768; j++)         {           hb.Add(bits[nextBitIndex++]);           if (table.TryGetValue(hb, out var value))           {             outdat.Add(value);             break;           }         }       }       return outdat.ToArray();     }   }   /// <summary>   /// Huffman coding external table small version   /// </summary>   public class CompressionHuffmanCoding : CompressionBase   {     private class Node     {       public byte? Value { get; set; }       public Node Parent { get; set; }       public Node Left { get; set; } // = 1       public Node Right { get; set; } // = 0       public uint Count { get; set; }       public bool? Bit { get; set; }       public BitList BitPattern       {         get         {           if (Parent != null)           {             if (Bit == null)             {               return Parent.BitPattern;             }             else             {               return BitList.Join(Parent.BitPattern, new[] { Bit == true });             }           }           else           {             if (Bit != null)             {               return new BitList               {                 Bit == true,               };             }           }           return new BitList();         }       }       public override string ToString()       {         if (Parent != null)         {           return $\"Total={Count}\";         }         else         {           return $\"{Value} x {Count}\";         }       }     }     public struct ValueCount     {       public byte Value { get; set; }       public uint Count { get; set; }       public override string ToString()       {         return $\"{Value} x {Count}\";       }     }     public override byte[] Compress(byte[] buf)     {       var ret = MakeTableBinary(buf, out var table);       // ADD DATA FIELDS       ret.Add((Int32)buf.Length);        // [DA] Data Bytes (32bits)       foreach (var c in buf)       {         ret.Add(table[c]);          // [DB] Huffman code       }       ret.AddPad();               // [DC] padding       return ret.ToByteArray();     }     private static void makeTableProc(Dictionary<byte, BitList> table, Node node)     {       if (node.Value != null)       {         table[(byte)node.Value] = node.BitPattern;       }       if (node.Left != null)       {         makeTableProc(table, node.Left);       }       if (node.Right != null)       {         makeTableProc(table, node.Right);       }     }     public override byte[] Decompress(byte[] buf)     {       var ret = new BitList();       var bits = new BitList(buf);       var table = ReadTableBinary(bits, out var nextBitIndex);       //=== READ HUFFMAN CODE ===       var len = BitList.From(bits.Subbit(nextBitIndex, 32)).ToInt32();       nextBitIndex += 32;       var outdat = new List<byte>();       for (var i = 0; i < len; i++)       {         var hb = new BitList();         for (var j = 0; j < 32768; j++)         {           hb.Add(bits[nextBitIndex++]);           if (table.TryGetValue(hb, out var value)           {             outdat.Add(value);             break;           }         }       }       return outdat.ToArray();     }     protected BitList MakeTableBinary(UInt16 tableID, Dictionary<BitList, byte> table, out Dictionary<byte, BitList> valbits )     {       Debug.Assert(tableID > 32768);       valbits = table.ToSwapKeyValue();       var ret = new BitList();       ret.Add(tableID);             // [TA] Table ID (>= 32768)       return ret;     }     public static BitList MakeTableBinary(byte[] buf, out Dictionary<byte, BitList> table)     {       // Make Huffman Tree       var sortedbuf = Collection.Seq(256).Select(a => new ValueCount { Value = (byte)a, Count = 0, }).ToDictionary(a => a.Value);       for (var i = 0; i < buf.Length; i++)       {         var val = buf[i];         sortedbuf[val] = new ValueCount { Value = val, Count = sortedbuf[val].Count + 1 };       }       var nodes = new LinkedList<Node>(sortedbuf                         .Where(a => a.Value.Count > 0)                         .Select(a => a.Value)                         .OrderByDescending(a => a.Count)                         .Select(a => new Node                         {                           Value = a.Value,                           Count = a.Count,                         })       );       while (nodes.Count > 1)       {         var last1 = nodes.Last;         nodes.RemoveLast();         var last2 = nodes.Last;         nodes.RemoveLast();         var joinnode = new Node         {           Left = last2.Value,           Right = last1.Value,           Count = last2.Value.Count + last1.Value.Count,         };         joinnode.Left.Parent = joinnode;         joinnode.Left.Bit = true;         joinnode.Right.Parent = joinnode;         joinnode.Right.Bit = false;         if (nodes.Count > 0)         {           LinkedListNode<Node> n = null;           for (n = nodes.Last; n != null && n.Value.Count < joinnode.Count; n = n.Previous)           {           }           nodes.AddAfter((n ?? nodes.Last), joinnode);         }         else         {           nodes.AddLast(joinnode);         }       }       // Make table binary       table = new Dictionary<byte, BitList>();       makeTableProc(table, nodes.First.Value);       var ret = new BitList();       ret.Add((UInt16)table.Count);       // [TA] TABLE COUNT (16bits) / < 32767 = Count       foreach (var kv in table)       {         ret.Add(kv.Key);           // [TB] VALUE (8bits)         if (kv.Value.Count <= 0b11111)         {           ret.Add(false);          // [TC] False = MARK OF 5-bits value           var cntbits = BitList.From((byte)kv.Value.Count);           ret.Add(cntbits, 0, 5);      // [TD] bit length         }         else         {           ret.Add(true);          // [TC] True = MARK OF 10-bits value           var cntbits = BitList.From((UInt16)kv.Value.Count);           ret.Add(cntbits, 0, 10);     // [TD'] bit length         }         ret.Add(kv.Value);          // [TE] BIT VALUE (n bits)       }       ret.AddPad();               // [TF] padding       return ret;     }      public static Dictionary<BitList, byte> ReadTableBinary(BitList bits, out int nextBitIndex)     {       // === READ TABLE ===       var table = new Dictionary<BitList, byte>();       var tableCount = BitList.From(bits.Subbit(0, 16)).ToUInt16();  // [TA]       Debug.Assert(tableCount < 32768);       nextBitIndex = 16;       for (var i = 0; i < tableCount; i++)       {         var value = BitList.From(bits.Subbit(nextBitIndex, 8)).ToByte(); // [TB]         nextBitIndex += 8;         var sizeFlag = bits[nextBitIndex++]; // [TC]         int bitLen = 0;         if (sizeFlag)         {           bitLen = BitList.From(bits.Subbit(nextBitIndex, 10)).ToUInt16();  // [TD']           nextBitIndex += 10;         }         else         {           bitLen = BitList.From(bits.Subbit(nextBitIndex, 5)).ToByte();   // [TD]           nextBitIndex += 5;         }         table[BitList.From(bits.Subbit(nextBitIndex, bitLen))] = value;    // [TE]         nextBitIndex += bitLen;       }       if (nextBitIndex % 8 != 0)       {         nextBitIndex += 8 - nextBitIndex % 8; // [TF]       }       return table;     }   } }    FROM WIKIPEDIA: FLORENCE VAN LEER EARLE COATES (JULY 1, 1850 - APRIL 6, 1927) WAS AN AMERICAN POET. SHE BECAME WELL KNOWN, BOTH AT HOME AND ABROAD, FOR HER WORKS OF POETRY, NEARLY THREE HUNDRED OF WHICH WERE PUBLISHED IN LITERARY MAGAZINES SUCH AS THE ATLANTIC MONTHLY, SCRIBNER'S MAGAZINE, THE LITERARY DIGEST, LIPPINCOTT'S, THE CENTURY MAGAZINE, AND HARPER'S MAGAZINE. SHE WAS ENCOURAGED BY MATTHEW ARNOLD WITH WHOM SHE MAINTAINED A CORRESPONDENCE UNTIL HIS DEATH IN 1888. MANY OF HER NATURE POEMS WERE INSPIRED BY THE FLORA AND FAUNA OF THE ADIRONDACKS, WHERE THE COATES FAMILY SPENT THEIR SUMMER MONTHS AT \"CAMP ELSINORE\" BESIDE UPPER ST. REGIS LAKE; HERE THEY ENTERTAINED MANY FRIENDS SUCH AS OTIS SKINNER, VIOLET OAKLEY, HENRY MILLS ALDEN, AND AGNES REPPLIER.    The Battle of Malvern Hill was fought on July 1, 1862, between the Confederate Army of Northern Virginia, led by Robert E. Lee, and the Union Army of the Potomac under George B. McClellan. It was the final battle of the Seven Days Battles during the American Civil War, taking place on the 130-foot (40 m) elevation of Malvern Hill, near the Confederate capital of Richmond. Including inactive reserves, more than 50,000 soldiers from each side took part, using more than 200 pieces of artillery. The Union's V Corps, commanded by Fitz John Porter, took up positions on the hill on June 30. The battle occurred in stages: over the course of four hours a series of blunders in planning and communication caused Lee's forces to launch three failed frontal infantry assaults across hundreds of yards of open ground, unsupported by Confederate artillery, charging toward strongly entrenched Union infantry and artillery. These errors provided Union forces with an opportunity to inflict heavy casualties. (Full article...) From Wikipedia, the free encyclopedia Jump to navigationJump to search For other uses, see Civil War (disambiguation). American Civil War CivilWarUSAColl.png Clockwise from top: Battle of Gettysburg, Union Captain John Tidball's artillery, Confederate prisoners, ironclad USS Atlanta, ruins of Richmond, Virginia, Battle of Franklin Date	April 12, 1861 – May 9, 1865 (4 years and 31 days)[a][1] Location	 Southern United States, Northeastern United States, Western United States, Atlantic Ocean Result	Union victory: Dissolution of the Confederate States U.S. territorial integrity preserved Slavery abolished Beginning of the Reconstruction era Passage and ratification of the 13th, 14th and 15th amendments to the Constitution of the United States Belligerents United States	 Confederate States Commanders and leaders Abraham Lincoln Ulysses S. Grant and others...	 Jefferson Davis Robert E. Lee and others... Strength 2,200,000[b] 698,000 (peak)[2][3]	750,000–1,000,000[b][4] 360,000 (peak)[2][5] Casualties and losses 110,000+ KIA/DOW 230,000+ accident/disease deaths[6][7] 25,000–30,000 died in Confederate prisons[2][6] 365,000+ total dead[8] 282,000+ wounded[7] 181,193 captured[2][better source needed][c] Total: 828,000+ casualties	 94,000+ KIA/DOW[6] 26,000–31,000 died in Union prisons[7] 290,000+ total dead 137,000+ wounded 436,658 captured[2][better source needed][d] Total: 864,000+ casualties 50,000 free civilians dead[9] 80,000+ slaves dead (disease)[10] Total: 616,222[11]–1,000,000+ dead[12][13] vte Theaters of the American Civil War Events leading to the American Civil War Northwest Ordinance Kentucky and Virginia Resolutions End of Atlantic slave trade Missouri Compromise Tariff of 1828 Nat Turner's slave rebellion Nullification Crisis Trial of Reuben Crandall Gag rule Martyrdom of Elijah Lovejoy Burning of Pennsylvania Hall End of slavery in British colonies American Slavery as It Is The Amistad affair Prigg v. Pennsylvania Texas annexation Mexican–American War Wilmot Proviso Nashville Convention Fugitive Slave Act of 1850 Compromise of 1850 Uncle Tom's Cabin Kansas–Nebraska Act Recapture of Anthony Burns Ostend Manifesto Bleeding Kansas Caning of Charles Sumner Dred Scott case The Impending Crisis of the South Lincoln–Douglas debates Oberlin–Wellington Rescue John Brown's raid on Harpers Ferry 1860 presidential election Crittenden Compromise Secession of Southern States Baltimore Plot President Lincoln's 75,000 volunteers Star of the West Corwin Amendment Battle of Fort Sumter vte Periods in United States history [hide] Colonial period	1607–1765 American Revolution	1765–1783 Confederation Period	1783–1788 Federalist Era	1788–1801 Jeffersonian Era	1801–1817 Era of Good Feelings	1817–1825 Jacksonian Era	1825–1849 Civil War Era	1850–1865 Reconstruction Era	1865–1877 Gilded Age	1877–1895 Progressive Era	1896–1916 World War I	1917–1919 Roaring Twenties	1920–1929 Great Depression	1929–1941 World War II	1941–1945 Post-war Era	1945–1964 Civil Rights Era	1965–1980 Reagan Era	1981–1991 Post-Cold War Era	1991–2008 Modern Day	2008–present Timeline vte The American Civil War (also known by other names) was a civil war in the United States from 1861 to 1865, fought between northern states loyal to the Union and southern states that had seceded from the Union to form the Confederate States of America.[e] The civil war began primarily as a result of the long-standing controversy over the enslavement of black people. War broke out in April 1861 when secessionist forces attacked Fort Sumter in South Carolina just over a month after Abraham Lincoln had been inaugurated as the President of the United States. The loyalists of the Union in the North, which also included some geographically western and southern states, proclaimed support for the Constitution. They faced secessionists of the Confederate States in the South, who advocated for states' rights to uphold slavery. Of the 34 U.S. states in February 1861, seven Southern slave-holding states were declared by their state governments to have seceded from the country, and the Confederate States of America was organized in rebellion against the U.S. constitutional government. The Confederacy grew to control at least a majority of territory in eleven states, and it claimed the additional states of Kentucky and Missouri by assertions from native secessionists fleeing Union authority. These states were given full representation in the Confederate Congress throughout the Civil War. The two remaining slave-holding states, Delaware and Maryland, were invited to join the Confederacy, but nothing substantial developed due to intervention by federal troops. The Confederate states were never diplomatically recognized as a joint entity by the government of the United States, nor by that of any foreign country.[f] The states that remained loyal to the U.S. were known as the Union.[g] The Union and the Confederacy quickly raised volunteer and conscription armies that fought mostly in the South for four years. Intense combat left between 620,000 and 750,000 people dead. The Civil War remains the deadliest military conflict in American history,[h] and accounted for more American military deaths than all other wars combined until the Vietnam War.[i] The war effectively ended on April 9, 1865, when Confederate General Robert E. Lee surrendered to Union General Ulysses S. Grant at the Battle of Appomattox Court House. Confederate generals throughout the Southern states followed suit, the last surrender on land occurring June 23. Much of the South's infrastructure was destroyed, especially its railroads. The Confederacy collapsed, slavery was abolished, and four million black slaves were freed. The war is one of the most studied and written about episodes in U.S. history.";
            foreach (var _ in Collection.Rep(64))
            {
                intext += "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            }
            foreach (var _ in Collection.Rep(64))
            {
                intext += "!\"#$%&'()=~|{`}*+_?><@[;:],./\\abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            }
            var bs = MakeTableBytes(intext);
            OutputTableBytes(bs);
        }
        [TestMethod]
        public void MakeTableJapanese()
        {
            var intext = "サクラ（桜、英訳：Cherry blossom、Japanese cherry、Sakura）は、バラ科サクラ亜科サクラ属（国によってはスモモ属に分類)の落葉広葉樹の総称。　　　　　　　　　　　　　　　　　　一般的に春に桜色と表現される白色や淡紅色から濃紅色の花を咲かせる。サクラはヨーロッパ・西シベリア、日本、中国、米国・カナダなど、主に北半球の温帯に広範囲に自生しているが、歴史的に日本文化に馴染みの深い植物であり、その変異しやすい特質から特に日本で花見目的に多くの栽培品種が作出されてきた（#日本における栽培品種と品種改良、#日本人とサクラ）。このうち観賞用として最も多く植えられているのがソメイヨシノである。鑑賞用としてカンザンなど日本由来の多くの栽培品種が世界各国に寄贈されて各地に根付いており（日本花の会、キューガーデン、全米桜祭りなど参照）、英語では桜の花のことを「Cherry blossom」と呼ぶのが一般的であるが、日本文化の影響から「Sakura」と呼ばれることも多くなってきている。サクラの果実はサクランボまたはチェリーと呼ばれ、世界中で広く食用とされる。日本では、塩や梅酢に漬けた花も食用とされる。サクラ全般の花言葉は「精神の美」「優美な女性」、西洋では「優れた教育」も追加される。サクラ属（狭義のサクラ属）とスモモ属（広義のサクラ属）サクラ類をサクラ属（Cerasus、ケラスス）に分類するか、スモモ属（Prunus、プルヌス）に分類するか国や時代で相違があり、現在では両方の分類が使われている。ロシア、中国、1992年以降の日本ではヤマザクラやセイヨウミザクラなどサクラのみ約100種をサクラ属（Cerasus）として分類するのが主流である（狭義のサクラ属）。一方で西欧や北米では各種サクラとスモモ、モモ、ウメ、ウワミズザクラなど約400種を一括してスモモ属（Prunus）として分類するのが主流である（広義のサクラ属）。これは比較的サクラ類の多いロシアや中国ではサクラ類を独立した属として分類していたのに対し、伝統的にサクラ類の少ない西欧と北米ではサクラ類をスモモやモモやウメなどと一括して分類していたためである。日本の科学は西欧や北米の基準に合わせる事が多かったため従来はサクラ類をスモモ属（Prunus）としていたが、1992年の東京大学の大場秀章の論文発表以降は、実態に合ったサクラ属（Cerasus）表記が主流である。なお、この「種」とは分類学上の種（species）のことで野生に自生する種のみを指し、種（species）の雑種や種（species）の下位分類の変種（variety）や、全く異なる分類体系となる野生種から開発された栽培品種（cultivar）は種（species）の数に含めないことに留意する必要がある。西欧と北米式のスモモ属（Prunus）による分類法スモモ属（Prunus）は約400の野生の種（species）からなるが、主に果実の特徴から5から7の亜属に分類される。サクラ亜属 subg. Cerasus はその一つである。サクラ亜属は節に分かれ、それらは非公式な8群に分かれる[要出典]このうちサクラ亜属には100の野生の種（species）があり、日本に古来から自生するものとしてはヤマザクラ、オオヤマザクラ、カスミザクラ、オオシマザクラ、エドヒガン、チョウジザクラ、マメザクラ、タカネザクラ、ミヤマザクラ、クマノザクラの10種、もしくはカンヒザクラも加えた11種（species）である。日本人は歴史的にこれらの野生種からサトザクラ群に代表される200品種以上（分類によっては600品種）の栽培品種を生み出して花見に利用してきたのである。日本における栽培品種と品種改良 多くの栽培品種を生み出した日本固有種のオオシマザクラ日本に自生する野生種のサクラは上記の10種、もしくは11種（species）であり、世界の野生種の全100種（species）から見るとそう多くはない。しかし日本のサクラに関して特筆できるのは、この10もしくは11種の下位分類の変種（variety）以下の分類で約100種の自生種が存在し、古来からこれらの野生種から開発してきた栽培品種（cultivar）が200種以上存在し、分類によっては最大で600種存在すると言われており、世界でも圧倒的に多種多様な栽培品種を開発してきた事である。日本人はこれら野生の種が他の種と交雑したりしながら誕生した突然変異個体と優良個体を選抜・育成・接ぎ木などで増殖してそれを繰り返すことで、多種の栽培品種を生み出してきた。エドヒガンやヤマザクラ、オオシマザクラなどは比較的に変性を起こしやすい種であり、特にオオシマザクラは成長が速く、花を大量に付け、大輪で、芳香であり、その特徴を好まれて結果として栽培品種の母親となって多くのサトザクラ群を生み出してきた。2014年に発表された森林総合研究所の215の栽培品種のDNA解析結果により、日本のサクラの栽培品種は、エドヒガンから誕生したシダレザクラのように一つの野生種から誕生した存在は稀で、多くがオオシマザクラに多様な野生種が交雑して誕生した種間雑種であることが判明した。";
            var bs = MakeTableBytes(intext);
            OutputTableBytes(bs);
        }

        [TestMethod]
        public void MakeTableBase64()
        {
            var intext = "";
            foreach (var _ in Collection.Rep(64))
            {
                intext += " +/=0123456789AabBcCdDeEFfGgHhIiJjkKlLmMnNoOPpqQrRSstTUuVvWwXxYyZz";
                intext += "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            }
            foreach (var _ in Collection.Rep(64))
            {
                intext += " +/=0123456789AabBcCdDeEFfGgHhIiJjkKlLmMnNoOPpqQrRSstTUuVvWwXxYyZz";
                intext += "!\"#$%&'()=~|{`}*+_?><@[;:],./\\abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            }
            var bs = MakeTableBytes(intext);
            OutputTableBytes(bs);
        }

        [TestMethod]
        public void MakeTableNumber()
        {
            var intext = "";
            foreach (var _ in Collection.Rep(64))
            {
                intext += " 0123456789";
            }
            foreach (var _ in Collection.Rep(64))
            {
                intext += " 0123456789";
                intext += ".-/+*[]()";
            }
            var bs = MakeTableBytes(intext);
            OutputTableBytes(bs);
        }

        [TestMethod]
        public void MakeTableHexNumber()
        {
            var intext = "";
            foreach (var _ in Collection.Rep(64))
            {
                intext += " 0123456789ABCDEFabcdef";
            }
            foreach (var _ in Collection.Rep(64))
            {
                intext += " 0123456789ABCDEFabcdef";
                intext += ".-/+*[]()";
            }
            var bs = MakeTableBytes(intext);
            OutputTableBytes(bs);
        }

        private static byte[] MakeTableBytes(string intext0)
        {
            var b = new StringBuilder();
            for (var i = 0; i < 512; i++)
            {
                b.Append(intext0);
            }
            var intext = b.ToString();
            var bufin = new List<byte>(Encoding.UTF8.GetBytes(intext));
            for (byte i = 255; i > 0; i--)
            {
                bufin.Add(i);
            }
            bufin.Add(0);
            var table = CompressionHuffmanCoding.MakeTableBinary(bufin.ToArray(), out var _).ToByteArray();
            return table;
        }

        private static void OutputTableBytes(byte[] table)
        {
            Debug.Write("new byte[]{ ");
            foreach (var c in table)
            {
                Debug.Write($"0x{c:x2}, ");
            }
            Debug.WriteLine("}");
        }
    }
}
