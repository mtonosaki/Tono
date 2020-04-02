// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public static class TimeKeeper
    {
        /// <summary>
        /// フィーチャー速度計測用
        /// </summary>
        private static readonly Dictionary<uint/*feature.ID*/, List<long/*tick*/>> _startTicks = new Dictionary<uint, List<long>>();
        private static readonly Dictionary<uint/*feature.ID*/, List<long/*tick*/>> _endTicks = new Dictionary<uint, List<long>>();

        /// <summary>
        /// uTimeKeeperの保存時刻タイプ
        /// </summary>
        public enum RecordType : uint
        {
            ///*0000.0*/ NA = 0x00000000,

            /*1000.0*/
            Start = 0x80000000,
            /*1100.0*/
            MouseDown = 0xc0000000,
            /*1010.0*/
            MouseMove = 0xa0000000,
            /*1110.0*/
            MouseUp = 0xe0000000,
            /*1001.0*/
            MouseWheel = 0x90000000,
            /*1101.0*/
            ZoomChanged = 0xd0000000,
            /*1011.0*/
            ScrollChanged = 0xb0000000,
            /*1111.0*/
            DragDrop = 0xf8000000,
            /*1000.1*/
            KeyDown = 0x88000000,
            /*1100.1*/
            KeyUp = 0xc8000000,

            //*1010.1*/ z3 = 0xa8000000,
            //*1110.1*/ z4 = 0xe8000000,
            //*1001.1*/ z5 = 0x98000000,
            //*1101.1*/ z6 = 0xd8000000,
            //*1011.1*/ z7 = 0xb8000000,
            //*1111.1*/ z8 = 0xf8000000,

        }

        private static uint makeCode(RecordType tp, Id id)
        {
            return (uint)id.Value | (uint)tp;
        }

        /// <summary>
        /// 開始時刻を保存する
        /// </summary>
        /// <param name="fc"></param>
        public static void SetStart(RecordType tp, Id iid)
        {
            var id = makeCode(tp, iid);
            if (_startTicks.TryGetValue(id, out var sticks) == false)
            {
                _startTicks[id] = sticks = new List<long>();
            }
            if (_endTicks.TryGetValue(id, out var eticks) == false)
            {
                _endTicks[id] = eticks = new List<long>();
            }
            while (eticks.Count < sticks.Count) // 終了無し（プログラムバグ）の整合を整える
            {
                eticks.Add(long.MinValue);
            }
            sticks.Add(DateTime.Now.Ticks);
        }

        /// <summary>
        /// 終了時刻を保存する
        /// </summary>
        /// <param name="fc"></param>
        public static void SetEnd(RecordType tp, Id iid)
        {
            var id = makeCode(tp, iid);
            if (_endTicks.TryGetValue(id, out var eticks) == false)
            {
                _endTicks[id] = eticks = new List<long>();
            }
            eticks.Add(DateTime.Now.Ticks);
        }

        /// <summary>
        /// フィーチャー実行開始チックのリストを返す
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static IList<long> GetStartTicks(RecordType tp, Id iid)
        {
            var id = makeCode(tp, iid);
            return _startTicks[id];
        }

        /// <summary>
        /// フィーチャー実行終了チックのリストを返す
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        public static IList<long> _getEndTicks(RecordType tp, Id iid)
        {
            var id = makeCode(tp, iid);
            return _endTicks[id];
        }

        /// <summary>
        /// 登録されているIDの一覧をクエリーする
        /// </summary>
        public static ICollection<Id> GetIDs()
        {
            var ret = new Dictionary<Id, bool>();
            foreach (int idm in _startTicks.Keys)
            {
                var id = new Id { Value = (int)((uint)idm & 0x00ffffff) };
                ret[id] = true;
            }
            return ret.Keys;
        }

        /// <summary>
        /// 登録されているデータ量を調べる
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="iid"></param>
        /// <returns></returns>
        public static double GetN(RecordType tp, Id iid)
        {
            var id = makeCode(tp, iid);
            if (_startTicks.ContainsKey(id))
            {
                return _startTicks[id].Count;
            }
            else
            {
                return 0;
            }
        }

        public static double CalcRamda(RecordType tp, Id id)
        {
            return 100;
        }

        public static double CalcMu(RecordType tp, Id id)
        {
            var n = GetN(tp, id);
            if (n < 1)
            {
                return 0;
            }
            var sts = GetStartTicks(tp, id);
            var ets = GetStartTicks(tp, id);
            double totalServiceTime = 0;

            for (var i = 0; i < (int)n; i++)
            {
                double serviceTime = ets[i] - sts[i];
                serviceTime /= 10000000;    // 100n sec(tick) --> sec
                totalServiceTime += serviceTime;
            }
            return 1 / (totalServiceTime / n);
        }

        /// <summary>
        /// カウントをリセットする
        /// </summary>
        public static void Reset()
        {
            _startTicks.Clear();
            _endTicks.Clear();
        }
    }
}
