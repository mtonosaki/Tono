// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace Tono
{
    /// <summary>
    /// Obsoleted (queue for another process. Working with .NET Standard 2.0)
    /// </summary>
    public class QueueOverProcess
    {
        public static void testConsole()
        {
            var qu = new QueueOverProcess("tonotest", 800 - 256);
            var txt90 = "123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";

            for (int i = 1; ; i++)
            {
                Console.Write($"[+]=Enqueue  [-]=Dequeue  > ");
                var k = Console.ReadKey();
                if (k.KeyChar == '+')
                {
                    var s = $"{i:00000}-{txt90}";
                    var ret = qu.Enqueue(s);
                    Console.WriteLine($"{ret} : {s}");
                }
                if (k.KeyChar == '-')
                {
                    var ret = qu.Dequeue();
                    Console.WriteLine(ret ?? "(null)");
                }
            }
        }
        private string mutexName { get; set; }

        private const long headerSize = 256;
        private readonly MemoryMappedFile mmf = null;
        private readonly MemoryMappedViewAccessor mmaPushPoint = null;
        private readonly MemoryMappedViewAccessor mmaPullPoint = null;
        private readonly MemoryMappedViewAccessor mmaLastPoint = null;
        private readonly MemoryMappedViewAccessor mmaLength = null;
        private readonly MemoryMappedViewAccessor mmaCount = null;

        private long __pushPoint { get => mmaPushPoint.ReadInt64(0); set { mmaPushPoint.Write(0, value); mmaPushPoint.Flush(); } }
        private long __pullPoint { get => mmaPullPoint.ReadInt64(0); set { mmaPullPoint.Write(0, value); mmaPullPoint.Flush(); } }
        private long __lastPoint { get => mmaLastPoint.ReadInt64(0); set { mmaLastPoint.Write(0, value); mmaLastPoint.Flush(); } }
        private long __length => mmaLength.ReadInt64(0);
        private long __count { get => mmaCount.ReadInt64(0); set { mmaCount.Write(0, value); mmaCount.Flush(); } }

        /// <summary>
        /// Thread safe count
        /// </summary>
        public int Count
        {
            get
            {
                using (new Mutex(mutexName))
                {
                    return (int)__count;
                }
            }
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="name">Queue name to specify in global</param>
        /// <param name="memoryCapacity">Data size[bytes]</param>
        public QueueOverProcess(string name, long memoryCapacity = 1024)
        {
            mutexName = $"__uQueueOverProcess_Mutex_{name}";

            using (new Mutex(mutexName))
            {
                var limit = memoryCapacity + headerSize;
                mmf = MemoryMappedFile.CreateOrOpen(name, limit);
                mmaPushPoint = mmf.CreateViewAccessor(8, 8);
                mmaPullPoint = mmf.CreateViewAccessor(16, 8);
                mmaLastPoint = mmf.CreateViewAccessor(24, 8);
                mmaLength = mmf.CreateViewAccessor(32, 8);
                mmaCount = mmf.CreateViewAccessor(40, 8);

                // Confrim marker and initialize
                var mk = mmf.CreateViewAccessor(0, 8);
                var mkval = new char[4];
                mk.ReadArray<char>(0, mkval, 0, 4);
                if (mkval[0] != 'R' || mkval[1] != 'T' || mkval[2] != 'X' || mkval[3] != 'S')
                {
                    // Create new memory map because of no marker yet.
                    mkval = new[] { 'R', 'T', 'X', 'S' };
                    mk.WriteArray<char>(0, mkval, 0, 4);
                    mk.Flush();

                    __pushPoint = headerSize;
                    __pullPoint = 0;
                    __lastPoint = limit;
                    mmaLength.Write(0, limit);
                    mmaLength.Flush();
                }
                else
                {
                    if (limit != __lastPoint)
                    {
                        throw new ArgumentException($"uQueueOverProcess Exception : memoryCapacity should be same with created one.");
                    }

                    limit = __lastPoint;
                }
            }
        }

        /// <summary>
        /// Enqueue string into queue (thread safe)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>true=OK, false=Error such as memory overflow</returns>
        public bool Enqueue(string str)
        {
            var data = Encoding.UTF8.GetBytes(str);
            var unitsize = data.Length;

            using (new Mutex(mutexName))
            {
                var pushpt = __pushPoint;
                var pullpt = __pullPoint;
                var cnt = __count;

                if (pushpt == pullpt && cnt > 0)
                {
                    return false;   // Capacity overflow
                }

                if (pushpt + unitsize + 4 > __lastPoint)
                {
                    if (pullpt > headerSize)
                    {
                        __lastPoint = __pushPoint;
                        pushpt = headerSize;
                    }
                    else
                    {
                        return false;   // Capacity overflow
                    }
                }

                // 1.Write next point to lock
                __pushPoint = pushpt + unitsize + 4;

                // 2.Adjust pull point
                if (pullpt == 0)
                {
                    __pullPoint = headerSize;
                }

                // 3.Write data
                using (MemoryMappedViewAccessor va = mmf.CreateViewAccessor(pushpt, unitsize + 4))
                {
                    va.Write(0, unitsize);   // データユニットのサイズ
                    va.WriteArray<byte>(4, data, 0, unitsize); // データユニット
                    va.Flush();
                }

                // 4.Count up
                __count = cnt + 1;

                return true;
            }
        }

        /// <summary>
        /// Dequeue string from the queue(thread safe)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string Dequeue()
        {
            using (new Mutex(mutexName))
            {
                var pullpt = __pullPoint;
                var cnt = __count;

                if (cnt < 1)
                {
                    return null;
                }
                if (pullpt == __lastPoint)
                {
                    pullpt = headerSize;
                    __lastPoint = __length;
                }

                int unitsize;
                using (MemoryMappedViewAccessor va = mmf.CreateViewAccessor(pullpt, 4))
                {
                    unitsize = va.ReadInt32(0);
                }
                string str;
                using (MemoryMappedViewAccessor va = mmf.CreateViewAccessor(pullpt + 4, unitsize))
                {
                    byte[] buf = new byte[unitsize];
                    va.ReadArray<byte>(0, buf, 0, unitsize);
                    str = Encoding.UTF8.GetString(buf);
                }

                __pullPoint = pullpt + 4 + unitsize;
                __count = cnt - 1;

                return str;
            }
        }
    }
}
