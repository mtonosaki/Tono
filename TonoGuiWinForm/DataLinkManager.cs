// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Data linkage Manager
    /// </summary>
    public abstract class DataLinkManager
    {
        #region 属性（シリアライズする）---------------------------------------------

        /// <summary>データの参照</summary>
        private PartsCollectionBase _partsData = null;

        /// <summary>アプリケーション固有のデータ</summary>
        private DataHotBase _appData = null;

        /// <summary>共有変数管理オブジェクト</summary>
        private DataSharingManager _share = null;

        /// <summary>パーツとアプリデータを結びつけるオブジェクト</summary>
        private DataLinkBase _link = null;

        /// <summary>親フィーチャーグループ</summary>
        private DataLinkManager _parent = null;

        /// <summary>ファイナライザ（すべてのフィーチャ実行後に動作する予約オブジェクト）</summary>
        private FinalizeManageBuffer _finalizers = null;

        /// <summary>永続化管理</summary>
        private PersistManager _persister = null;

        /// <summary>
        /// トークンを入れておくための箱
        /// </summary>
        private TokenTray _tokenTray = null;

        #endregion
        #region 属性（シリアライズしない）------------------------------------------

        /// <summary>リンクするリッチペーン</summary>
        private TGuiView _pane = null;

        /// <summary>タイマー制御</summary>
        private GuiTimer _timer = null;

        /// <summary>最後にクリック（MouseDown）した所のパーツ</summary>
        private DataSharingManager.Object _clickParts = null;

        /// <summary>
        /// _clickPartsを検索したときのペーン（イリュージョンペーンであれば、Binderペーンオブジェクトになる）
        /// </summary>
        private DataSharingManager.Object _clickPane = null;

        #endregion

        /// <summary>
        /// 初期化が必要であれば初期化する処理
        /// </summary>
        private void init()
        {
            _clickParts = _clickParts == null ? new DataSharingManager.Object() : _clickParts;
            _clickPane = _clickPane == null ?  new DataSharingManager.Object() : _clickPane;
            _tokenTray = _tokenTray== null ?  new TokenTray() : _tokenTray;
            _finalizers = _finalizers == null ? new FinalizeManageBuffer() : _finalizers;
            _persister = _persister== null ? new PersistManager() : _persister;
            _timer = _timer == null ? new GuiTimer() : _timer;
        }

        /// <summary>
        /// ルートグループがDisposeする
        /// </summary>
        private void disposeByRootGroup()
        {
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// 関連付けられるペーンを指定する
        /// </summary>
        /// <param name="pane">ペーン</param>
        protected void setPane(TGuiView pane)
        {
            _pane = pane;
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void setAppData(DataHotBase value)
        {
            System.Diagnostics.Debug.Assert(_appData == null, "setAppDataはルートグループフィーチャーに一度だけ実行する特殊なメソッドです");
            _appData = value;
            init();
        }

        /// <summary>
        /// 関連付けられるリンクオブジェクトを指定する
        /// </summary>
        /// <param name="value">リンクオブジェクト</param>
        protected void setLink(DataLinkBase value)
        {
            System.Diagnostics.Debug.Assert(_link == null, "setLinkはルートグループフィーチャーに一度だけ実行する特殊なメソッドです");
            _link = value;
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void setPartsData(PartsCollectionBase value)
        {
            System.Diagnostics.Debug.Assert(_partsData == null, "setPartsDataはルートグループフィーチャーに一度だけ実行する特殊なメソッドです");
            _partsData = value;
            init();
        }

        /// <summary>
        /// 共有変数管理インスタンスを指定する
        /// </summary>
        /// <param name="value">共有変数管理インスタンス</param>
        protected void setShare(DataSharingManager value)
        {
            if (_share != null)
            {
                System.Diagnostics.Debug.Assert(_share.Count == 0, "既にShareを利用した後、LinkShareで他のffShareと結びつくのは不可能です");
            }
            _share = value;
            init();
        }

        /// <summary>
        /// タイマー制御オブジェクト
        /// </summary>
        protected GuiTimer Timer
        {
            get
            {
                System.Diagnostics.Debug.Assert(_timer != null, "Disposeされたフィーチャーです。Timerは使用できません");
                return _timer;
            }
        }

        /// <summary>
        /// リンク
        /// </summary>
        protected DataLinkBase Link => _link;

        /// <summary>
        /// トークン
        /// </summary>
        protected TokenTray Token => _tokenTray;

        /// <summary>
        /// ファイナライザー管理オブジェクト
        /// </summary>
        protected FinalizeManageBuffer Finalizers => _finalizers;

        /// <summary>
        /// 永続化管理オブジェクト
        /// </summary>
        protected PersistManager Persister => _persister;

        /// <summary>
        /// アプリケーションデータ
        /// </summary>
        protected DataHotBase Data => _appData;

        /// <summary>
        /// パーツデータ
        /// </summary>
        protected PartsCollectionBase Parts => _partsData;

        /// <summary>
        /// 共有変数
        /// </summary>
        protected DataSharingManager Share => _share;

        /// <summary>
        /// マザーペーン（基本になるcFeatureRich)
        /// </summary>
        protected IRichPane Pane => _pane;

        /// <summary>
        /// 最後にクリックした所のパーツ
        /// </summary>
        protected PartsBase ClickParts
        {
            get => (PartsBase)_clickParts.value;
            set => _clickParts.value = value;
        }

        /// <summary>
        /// 最後にクリックしたところのペーン（イリュージョンならBinderペーンが返る）
        /// </summary>
        protected IRichPane ClickPane
        {
            get => (IRichPane)_clickPane.value;
            set => _clickPane.value = value;
        }

        /// <summary>
        /// インスタンスを生成する唯一の方法。ユーザーは自分でインスタンス生成禁止
        /// </summary>
        protected DataLinkManager()
        {
        }

        /// <summary>
        /// フィーチャーで通して使用するデータを関連する
        /// </summary>
        /// <param name="theInstanceWhoHasEmptyData">まだデータ所有していないfgBase/FeatureBase</param>
        protected void linkDataTo(DataLinkManager theInstanceWhoHasEmptyData)
        {
            theInstanceWhoHasEmptyData._appData = _appData;
            theInstanceWhoHasEmptyData._partsData = _partsData;
            theInstanceWhoHasEmptyData._pane = _pane;
            theInstanceWhoHasEmptyData._share = _share;
            theInstanceWhoHasEmptyData._finalizers = _finalizers;
            theInstanceWhoHasEmptyData._persister = _persister;
            theInstanceWhoHasEmptyData._tokenTray = _tokenTray;
            theInstanceWhoHasEmptyData._link = _link;
            theInstanceWhoHasEmptyData._timer = _timer;
            theInstanceWhoHasEmptyData._parent = this;
            theInstanceWhoHasEmptyData._clickParts = _clickParts;
            theInstanceWhoHasEmptyData._clickPane = _clickPane;
        }

        /// <summary>
        /// ルートフィーチャーグループを返す
        /// </summary>
        /// <returns>ルートフィーチャーグループ</returns>
        public FeatureGroupBase GetRoot()
        {
            DataLinkManager ret;
            for (ret = this; ret._parent != null; ret = ret._parent)
            {
                ;
            }

            if (ret is FeatureGroupBase)
            {
                return (FeatureGroupBase)ret;
            }
            System.Diagnostics.Debug.Assert(ret != null, "このフィーチャーセットは、ルートにfgBase派生クラスを使用していません");
            return null;
        }
    }
}
