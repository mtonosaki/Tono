using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ストップウォッチクラス
    /// </summary>
    [NoTestClass]
    public class StopWatch
    {
        private readonly IList _buf = new ArrayList();
        private readonly string _name;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public StopWatch()
        {
            _name = "Stop";
            Start();
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="name">ストップウォッチの名前</param>
        public StopWatch(string name)
        {
            _name = name;
            Start();
        }

        /// <summary>
        /// 初期化コンストラクタ
        /// </summary>
        /// <param name="sw">TRUE:計測を開始する/FALSE:計測を開始しない</param>
        public StopWatch(bool sw)
        {
            _name = "Stop";
            if (sw == true)
            {
                Start();
            }
        }

        /// <summary>
        /// デストラクタはいつ呼ばれるかわからない
        /// </summary>
        ~StopWatch()
        {
            _buf.Clear();
        }

        public void Start()
        {
            if ((_buf.Count % 2) == 1)  // 計測中
            {
                return;
            }
            _buf.Add(DateTime.Now.Ticks);
        }

        /// <summary>
        /// 計測開始からの総秒数を返す
        /// </summary>
        /// <returns>累積秒</returns>
        public double span()
        {
            if (_buf.Count < 2)
            {
                return 0.0;
            }
            if ((_buf.Count % 2) == 1)  // 計測中
            {
                return 0.0;
            }
            double dif = (long)_buf[_buf.Count - 1] - (long)_buf[0];
            return dif * 100 / 1000000000;
        }

        /// <summary>
        /// 計測開始から現在までの経過時間［秒］を返す
        /// </summary>
        /// <returns>累積秒数</returns>
        public double NowSpan()
        {
            if (_buf.Count == 0)
            {
                return 0.0;        // 未計測
            }

            if ((_buf.Count % 2) == 0)
            {
                return 0.0;  // 計測終了済
            }

            double ret = DateTime.Now.Ticks - (long)_buf[0];
            return ret * 100 / 1000000000;
        }

        /// <summary>
        /// ストップウォッチを停止して累積秒を返す
        /// </summary>
        /// <returns>累積秒</returns>
        public double Stop()
        {
            _buf.Add(DateTime.Now.Ticks);
            var ret = span();
            return ret;
        }

        /// <summary>
        /// ストップウォッチを停止して累積秒をデバッグ出力に表示する
        /// </summary>
        public void StopAndDiag()
        {
            var r = Stop();
            System.Diagnostics.Debug.WriteLine("> " + _name + " at " + DateTime.Now.ToString() + " Result = " + (r * 1000).ToString("0.000") + " ms");
        }
    }
}
