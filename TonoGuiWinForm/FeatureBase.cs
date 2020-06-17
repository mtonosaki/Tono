// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureBase の概要の説明です。
    /// フィーチャークラスの基本クラス
    /// </summary>
    public abstract class FeatureBase : DataLinkManager
    {
        #region ファイルセレクタ　フィーチャー版
        /// <summary>
        /// ファイルセレクタ　フィーチャー版
        /// </summary>
        public class FileSelectorAdapter
        {
            private string _filename = "";
            private string _filter = "*.*";
            private bool _isSave = false;
            private readonly IList _hlList = new ArrayList();
            private readonly PersistManager.RecorderBridge IR;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public FileSelectorAdapter(PersistManager.RecorderBridge irPersister)
            {
                IR = irPersister;
            }

            public bool IsSave
            {
                get => _isSave;
                set => _isSave = value;
            }


            /// <summary>
            /// 実行する
            /// </summary>
            /// <returns></returns>
            public DialogResult ShowDialog()
            {
                object ret;

                {
#if false  // fFileSelector使用の場合
				// インスタンス生成
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				ConstructorInfo ci = typeof(fFileSelector).GetConstructor(flags, null, new Type[]{}, null);				
				fFileSelector fs = (fFileSelector)ci.Invoke(new object[]{});
				foreach( string s in _hlList )
				{
					fs.AddHighlightExt(s);
				}
				fs.FileName = this._filename;
				fs.Filter = this._filter;

				// ShowDialog実行
				MethodInfo mi = typeof(fFileSelector).GetMethod("showDialog", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				object ret = mi.Invoke(fs, new object[]{});
#else      // OpenFileDialog使用の場合
                    IR.Save(new DeviceRecord.TagSkipStart(), NamedId.FromName("InputRecorderSaveSerializeID"));

                    FileDialog fs;
                    if (_isSave)
                    {
                        fs = new SaveFileDialog
                        {
                            CheckFileExists = false
                        };
                    }
                    else
                    {
                        fs = new OpenFileDialog
                        {
                            CheckFileExists = true
                        };
                    }
                    fs.FileName = _filename;
                    //string f = "";
                    //string[] fis = _filter.Split(new char[] { '|' });
                    //for (int i = 0; i < fis.Length; i++)
                    //{
                    //    string ff = fis[i];
                    //    if (ff == "*.*")
                    //    {
                    //        f += "All files|*.*|";
                    //        continue;
                    //    }
                    //    if (ff.StartsWith("*."))
                    //    {
                    //        f += ff.Substring(2).ToUpper() + " files|" + ff + "|";
                    //    }
                    //}
                    //if (f.EndsWith("|"))
                    //{
                    //    f = f.Substring(0, f.Length - 1);
                    //}
                    //fs.Filter = f;
                    fs.Filter = _filter;
#endif
                    ret = fs.ShowDialog();

                    // 操作記録
                    IR.Save(new DeviceRecord.TagFileSelect((DialogResult)ret, fs.FileName), NamedId.FromName("InputRecorderSaveSerializeID"));

                    _filename = fs.FileName;
                    fs.Dispose();

                    IR.Save(new DeviceRecord.TagSkipEnd(), NamedId.FromName("InputRecorderSaveSerializeID"));
                }
                // ファイル名以外は、すべてリセットする
                _hlList.Clear();
                _filter = "*.*";

                return (DialogResult)ret;
            }

            /// <summary>
            /// ハイライトの拡張子を指定する
            /// </summary>
            /// <param name="ext">拡張子</param>
            public void AddHighlightExt(string ext)
            {
                _hlList.Add(ext);
            }

            /// <summary>
            /// ファイル名
            /// </summary>
            public string FileName
            {
                get => _filename;
                set => _filename = value;
            }

            /// <summary>
            /// フィルタ 例：  *.doc|*.txt|*.rtf
            /// </summary>
            public string Filter
            {
                set => _filter = value;
            }
        }

        #endregion

        internal class forMultiTokenDummy : FeatureBase
        {
        }

        /// <summary>
        /// 必ず到着するトークンを発行できるフィーチャーオブジェクト
        /// </summary>
        /// <returns></returns>
        public static FeatureBase ForMultiTokenDummy()
        {
            return new forMultiTokenDummy();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
        }

        #region 属性（シリアライズする）
        /// <summary>インスタンスを特定するためのID</summary>
        private Id _instanceID;
        /// <summary>最終表示カーソル</summary>
        public static System.Windows.Forms.Cursor Cursor = Cursors.Default;
        /// <summary>自イベント時の表示カーソル</summary>
        protected System.Windows.Forms.Cursor _EventCursor = Cursors.Default;
        /// <summary>パラメータ文字列の保存</summary>
        private readonly string _paramString = "";

        /// <summary>フィーチャーの名前</summary>
        private string _featureName = null;

        /// <summary>フィーチャーを使用するかどうかのフラグ</summary>
        private bool _isEnable = true;
        #endregion
        #region 属性（シリアライズしない）

        /// <summary>トークン実行をRecordするタイミング</summary>
        protected NamedId startToken;
        private static int _instanceCounter = 1;
        /// <summary>
        /// Enableプロパティー変更の知らせ
        /// </summary>
        protected EventHandler<EventArgs> EnableChanged;

        /// <summary>一時的なトークンID（メニューやボタンからの起動に使用）</summary>
        private NamedId _temporaryTokenListenerID = null;
        /// <summary>
        /// REDO用のID
        /// </summary>
        public static readonly NamedId REDO = NamedId.FromName("PersisterForRedo");
        /// <summary>
        /// UNDO用のID
        /// </summary>
        public static readonly NamedId UNDO = NamedId.FromName("PersisterForUndo");

        private FileSelectorAdapter _fileSelector = null;

        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected FeatureBase()
        {
            _instanceID = new Id { Value = _instanceCounter++ };
            startToken = NamedId.FromName("FeatureGotToken");
            _featureName = GetType().Name + "#" + _instanceID.Value.ToString();
        }

        /// <summary>
        /// インスタンス名
        /// </summary>
        public string Name
        {
            get => _featureName;
            set => _featureName = value;
        }

        /// <summary>
        /// 自動起動させるコマンドパラメータ
        /// </summary>
        public string CommandParameter { get; set; } = null;

        /// <summary>
        /// コマンドパラメータが指定されている時、起動時に自動的にコールされる
        /// </summary>
        /// <param name="arg">コマンドライン  /s=aaa  なら、aaa が格納される</param>
        public virtual void OnCommandParameter(string arg)
        {
        }

        /// <summary>
        /// コマンドパラメータ例外
        /// </summary>
        public class CommandParameterException : Exception
        {
            public CommandParameterException() : base()
            {
            }
            public CommandParameterException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// ファイルセレクタ
        /// </summary>
        public FileSelectorAdapter FileSelector
        {
            get
            {
                if (_fileSelector == null)
                {
                    _fileSelector = new FileSelectorAdapter(Persister[NamedId.FromName("RecorderDeviceInput")]);
                }
                return _fileSelector;
            }
        }

        /// <summary>
        /// パラメータ文字列を返す
        /// </summary>
        public string GetParamString()
        {
            return _paramString;
        }

        /// <summary>
        /// フィーチャーを次回から使用するかどうかのフラグ
        /// </summary>
        public virtual bool Enabled
        {
            get => _isEnable;
            set
            {
                var pre = _isEnable;
                _isEnable = value;
                if (_isEnable != pre)
                {
                    EnableChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// フィーチャーのデフォルトの文字列を作成する
        /// </summary>
        /// <returns>インスタンスの文字列</returns>
        public override string ToString()
        {
            return GetType().Name + "[" + _instanceID.ToString() + "]";
        }


        /// <summary>
        /// パラメータ文字列を解析する
        /// </summary>
        /// <param name="param"></param>
        public virtual void ParseParameter(string param)
        {
        }

        /// <summary>
        /// フィーチャーが開始できる状態かどうかを外部に通知する手段
        /// </summary>
        public virtual bool CanStart => Enabled;

        /// <summary>
        /// メニューのチェックに連動させる
        /// </summary>
        public virtual bool Checked => false;

        /// <summary>
        /// メッセージを取得する
        /// </summary>
        protected Mes Mes => Mes.Current;

        /// <summary>
        /// 自フィーチャーにズーム変更イベントを送出する
        /// </summary>
        protected void SendZoomChanged()
        {
            if (this is IZoomListener)
            {
                foreach (var rp in ((IZoomListener)this).ZoomEventTargets)
                {
                    ((IZoomListener)this).ZoomChanged(rp);
                }
            }
        }

        /// <summary>
        /// 自フィーチャーにスクロール変更イベントを送出する
        /// </summary>
        protected void SendScrollChanged()
        {
            if (this is IScrollListener)
            {
                foreach (var rp in ((IScrollListener)this).ScrollEventTargets)
                {
                    ((IScrollListener)this).ScrollChanged(rp);
                }
            }
        }

        /// <summary>
        /// 自己呼び出しの予約
        /// （仕組み：仮トークンが自動的に発行されて動作する）
        /// </summary>
        public void RequestStartup()
        {
            RequestStartup(Id.Nothing);
        }

        /// <summary>
        /// 自己呼び出しの予約
        /// （仕組み：一時トークンが自動的に発行されて動作する）
        /// </summary>
        /// <param name="id">IDを明示することができる</param>
        public void RequestStartup(Id id)
        {
            if (id.IsNothing())
            {
                var t = GetType();
                _temporaryTokenListenerID = NamedId.FromName("request@@" + t.AssemblyQualifiedName + "@@" + ID.ToString());
            }
            else
            {
                _temporaryTokenListenerID = NamedId.FromIDNoName(new Id { Value = id.Value });
            }
            Token.Add(_temporaryTokenListenerID, this);
            GetRoot().FlushFeatureTriggers();
            //GetRoot().InvokeStartupToken(); // TONO
        }

        /// <summary>
        /// 自分宛てのトークン実行して、自分宛のトークンを消去する
        /// </summary>
        /// <returns>
        /// 実行したトークンID
        /// </returns>
        internal NamedId invokeToken()
        {
            var isNeedUrgentToken = false;


            // 実行確認
            NamedId need = null;


            if (this is ITokenListener)
            {
                var id = ((ITokenListener)this).TokenTriggerID;
                if (Token.Contains(id))
                {
                    need = id;
                }
            }
            if (need == null && this is IMultiTokenListener)
            {
                foreach (var id in ((IMultiTokenListener)this).MultiTokenTriggerID)
                {
                    if (Token.Contains(id))
                    {
                        need = id;
                        break;
                    }
                }
            }

            if (_temporaryTokenListenerID != null) // 臨時トークンの確認（内部トークンと両方あった場合、こちらが使用される）
            {
                if (Token.Contains(_temporaryTokenListenerID))
                {
                    need = _temporaryTokenListenerID;
                    isNeedUrgentToken = true;
                }
            }
            if (need != null)
            {
                if (CanStart)
                {
                    var sw = new StopWatch();

                    // トークン実行を通知する緊急臨時トークン
                    if (isNeedUrgentToken)
                    {
                        GetRoot().SetUrgentToken(startToken, need, this);
                    }

                    // スタート
                    Start(need);

                    var sec = sw.Stop();
                    if (sec >= 1.0)
                    {
                        var s = GetType().Name + (this is FeatureThreadBase ? " (Thread)" : "");
                        System.Diagnostics.Debug.WriteLine(s + " " + sec.ToString("0.000") + "[sec]");
                    }
                }
            }
            return need;
        }

        /// <summary>
        /// データを使用した初期化が必要ならここに実装する
        /// </summary>
        public virtual void OnInitInstance()
        {
        }

        /// <summary>
        /// イベント起動
        /// </summary>
        public virtual void Start(NamedId who)
        {
        }

        /// <summary>
        /// イベント起動
        /// </summary>
        public virtual void Start(NamedId who, object arg)
        {
        }

        /// <summary>
        /// カーソルをセットする（ファイナライズ）
        /// </summary>
        protected void onCursorSeFinalizert()
        {
            if (Cursor != null)
            {
                //((IControlUI)Pane).Cursor = Cursor;
                var ts = new ThreadUtil();
                ts.SetCursorControl(Pane.Control, Cursor);
            }
        }

        #region IID メンバ

        /// <summary>
        /// 
        /// </summary>
        public Id ID => _instanceID;

        #endregion
    }
}
