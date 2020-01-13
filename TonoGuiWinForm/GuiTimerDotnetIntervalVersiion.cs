// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#if true

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// GuiTimer の概要の説明です。
    /// フィーチャーにタイマー機能を追加する場合に使用する。
    /// FeatureTripSelectorを参照
    /// </summary>
    public class GuiTimer : IDisposable
    {
        #region タイマーハンドラ

        /// <summary>
        /// タイマー停止用ハンドラ
        /// </summary>
        public class Handle
        {
            private long _sigTicks;
            private readonly int _setms;
            private readonly object _function;
            private readonly object[] _args;

            /// <summary>
            /// 唯一のコンストラクタ（アセンブリから直接コールされる；ユーザー実行禁止）
            /// </summary>
            /// <param name="ms">タイマーのミリ秒数</param>
            /// <param name="function">タイマー起動オブジェクト</param>
            /// <param name="args">タイマー起動時の引数</param>
            private Handle(int ms, object function, object[] args)
            {
                _setms = ms;
                _function = function;
                _args = args;
                _sigTicks = DateTime.Now.Ticks + ms * 10000 + (_counter % 100);
            }

            /// <summary>
            /// タイマーに設定されているコマンドを実行する
            /// </summary>
            public void Invoke()
            {
                if (_function is Proc0)
                {
                    ((Proc0)_function)();
                }
                if (_function is Proc1)
                {
                    ((Proc1)_function)(_args[0]);
                }
                if (_function is ProcN)
                {
                    ((ProcN)_function)(_args);
                }
            }

            /// <summary>
            /// タイマーが起動するときのDateTime.Ticks
            /// </summary>
            public long SignalTicks
            {
                get => _sigTicks;
                set => _sigTicks = value;
            }
        }
        #endregion

        #region 関数型

        /// <summary>引数なし関数型</summary>
        public delegate void Proc0();

        /// <summary>引数ひとつ関数型</summary>
        public delegate void Proc1(object arg);

        /// <summary>引数マルチ関数型</summary>
        public delegate void ProcN(object[] args);

        #endregion

        #region 属性（シリアライズしない）

        private static int _counter = 0;
        private readonly SortedList _dat = new SortedList(new ComparerUtil.ComparerLong());
        private Handle _current = null;
        private static Timer _hInterval = null;

        #endregion

        #region IDisposable メンバ

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            if (_hInterval != null)
            {
                _hInterval.Dispose();
                _hInterval = null;
                _dat.Clear();
                _current = null;
            }
        }

        #endregion

        /// <summary>
        /// タイマーオブジェクトのメイン処理
        /// </summary>
        public GuiTimer()
        {
        }

        /// <summary>
        /// スレッドでタイマーキックを監視・実行する
        /// </summary>
        private void timerThread(object args)
        {
            lock (this)
            {
                bool isLoop;
                do
                {
                    isLoop = false;
                    if (_current != null)
                    {
                        try
                        {
                            if (DateTime.Now.Ticks > _current.SignalTicks)
                            {
                                var hDo = _current;
                                Stop(_current);
                                hDo.Invoke();
                                isLoop = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
                    }
                } while (isLoop);
            }
        }

        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly ConstructorInfo ci = typeof(Handle).GetConstructor(flags, null, new Type[] { typeof(int), typeof(object), typeof(object[]) }, null);

        /// <summary>
        /// Handleのインスタンスを生成する
        /// </summary>
        /// <returns>新しいHandle</returns>
        private Handle _createHandle(int ms, object function, object[] args)
        {
            var h = (Handle)ci.Invoke(new object[] { ms, function, args });

            lock (_dat.SyncRoot)
            {
                while (_dat.Contains(h.SignalTicks))
                {
                    h.SignalTicks += 1;
                    _counter++;
                }
                _dat[h.SignalTicks] = h;
                if ((long)_dat.GetKey(0) == h.SignalTicks)
                {
                    _current = h;
                }

                // タイマーを使用する場合スレッドを起動する
                if (_hInterval == null)
                {
                    _hInterval = new Timer(new System.Threading.TimerCallback(timerThread), null, 77, 71);  // timerThreadは、このスレッドで実行される
                    System.Diagnostics.Debug.WriteLine("GuiTimer Process is started");
                }
                return h;
            }
        }

        /// <summary>
        /// タイマーを削除する
        /// </summary>
        /// <param name="h"></param>
        public void Stop(Handle h)
        {
            if (h == null)
            {
                return;
            }
            lock (_dat.SyncRoot)
            {
                try
                {
                    _dat.Remove(h.SignalTicks);
                    if (object.ReferenceEquals(h, _current))
                    {
                        if (_dat.Count > 0)
                        {
                            _current = (Handle)_dat.GetByIndex(0);
                        }
                        else
                        {
                            _current = null;
                        }
                    }
                }
                catch (Exception)
                {
                    Debug.WriteLine("Tono.GuiWinForm : Kill timer exception.");
                }
            }
        }

        /// <summary>
        /// 引数なしのタイマー関数を登録する
        /// </summary>
        /// <param name="delay_ms">起動ミリ秒</param>
        /// <param name="function">起動する関数</param>
        public Handle AddTrigger(int delay_ms, Proc0 function)
        {
            var ret = _createHandle(delay_ms, function, null);
            return ret;
        }

        /// <summary>
        /// 引数なしのタイマー関数を登録する
        /// </summary>
        /// <param name="delayTime">起動時間</param>
        /// <param name="function">起動する関数</param>
        public Handle AddTrigger(DateTimeEx delayTime, Proc0 function)
        {
            return AddTrigger(delayTime.TotalSeconds * 1000, function);
        }

        /// <summary>
        /// 引数付きのタイマー関数を登録する
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="delay_ms"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public Handle AddTrigger(object arg, int delay_ms, Proc1 function)
        {
            var ret = _createHandle(delay_ms, function, new object[] { arg });
            return ret;
        }

        /// <summary>
        /// 引数付きのタイマー関数を登録する
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="delayTime"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public Handle AddTrigger(object arg, DateTimeEx delayTime, Proc1 function)
        {
            return AddTrigger(arg, delayTime.TotalSeconds * 1000, function);
        }

        /// <summary>
        /// 引数付きのタイマー関数を登録する
        /// </summary>
        /// <param name="args"></param>
        /// <param name="delay_ms"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public Handle AddTrigger(object[] args, int delay_ms, ProcN function)
        {
            var ret = _createHandle(delay_ms, function, args);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="delayTime"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public Handle AddTrigger(object[] args, DateTimeEx delayTime, ProcN function)
        {
            return AddTrigger(args, delayTime.TotalSeconds * 1000, function);
        }
    }
}

#endif
