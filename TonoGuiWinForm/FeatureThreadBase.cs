using System;
using System.Diagnostics;
using System.Threading;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// フィーチャーをスレッド化する基本クラス
    /// </summary>
    public abstract class FeatureThreadBase : FeatureControlBridgeBase, System.IDisposable
    {
        #region	属性(シリアライズする)
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>スレッドのハンドル</summary>
        private Thread _trd = null;
        /// <summary>スレッドの優先度(初期値は標準)</summary>
        protected System.Threading.ThreadPriority _priority = System.Threading.ThreadPriority.Normal;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureThreadBase()
        {
        }

        /// <summary>
        /// トークンによる起動イベント
        /// </summary>
        /// <param name="who"></param>
        public sealed override void Start(NamedId who)
        {
            if (_trd != null && _trd.IsAlive == false)
            {
                _trd = null;
            }
            System.Diagnostics.Debug.Assert(_trd == null, "スレッド起動中に別のTokenが投入されました。二重起動を有効にするには、_trdを配列にして管理してください");

            //base.Start (who);
            _trd = new Thread(new ThreadStart(Run))
            {
                IsBackground = true,       // これはTRUEじゃないとアプリが終了しなくなるので注意
                Name = GetType().Name,
                Priority = _priority
            };
            _trd.Start();
            if (Pane.Control != null)
            {
                Pane.Control.Disposed += new EventHandler(parentControl_Disposed);
            }
        }

        /// <summary>
        /// スレッドだけ残らないような工夫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void parentControl_Disposed(object sender, EventArgs e)
        {
            if (_trd != null)
            {
                if (_trd.Join(10000) == false)
                {
                    Debug.WriteLine("Waiting thread [" + _trd.Name + "] end normally for 60sec.");
                    if (_trd.Join(60000) == false)
                    {
                        var name = _trd.Name;
                        _trd.Abort();
                        _trd = null;
                        Debug.WriteLine("Thread [" + name + "] is aborted forcely.");
                    }
                }
            }
        }

        /// <summary>
        /// スレッド起動時に実行される処理
        /// (スレッドで行う処理はこれをオーバーライドして記述してください)
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// 起動中のスレッドを強制終了する
        /// </summary>
        protected void Stop()
        {
            if (_trd != null)
            {
                _trd.Abort();       // 実行中のスレッドを強制終了
            }
        }

        /// <summary>
        /// スレッドを指定時間(㍉秒)だけスリープさせる
        /// </summary>
        /// <param name="ms">スリープする時間(㍉秒)</param>
        protected void Sleep(int ms)
        {
            Thread.Sleep(ms);
        }

        /// <summary>
        /// スレッドの優先度の取得/設定
        /// </summary>
        /// ThreadPriority.Highest		最高
        /// ThreadPriority.AboveNormal	高
        /// ThreadPriority.Normal		中
        /// ThreadPriority.BelowNormal	低
        /// ThreadPriority.Lowest		最低
        protected ThreadPriority Priority
        {
            get => _priority;
            set => _priority = value;
        }

        #region IDisposable メンバ
        public new void Dispose()
        {
            base.Dispose(); // 終了処理
            Stop();
            if (_trd != null)
            {
                _trd.Join();    // スレッドが終了するまで待機します
                _trd = null;
            }
        }
        #endregion
    }
}
