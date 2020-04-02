// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Feature Group Base の概要の説明です。
    /// fg : Feature Group
    /// 複数のフィーチャーをひとつのパッケージとして管理するための基本クラス
    /// </summary>
    public abstract class FeatureGroupBase : DataLinkManager, IMouseListener, IKeyListener, IDisposable
    {
        #region	属性(シリアライズする)
        /// <summary>子フィーチャーグループを管理する配列</summary>
        private readonly IList /*<fgBase または FeatureBase>*/ _children = new ArrayList();
        #endregion
        #region 属性(シリアライズしない)
        /// <summary>マウスのクリック位置を保存する共有変数</summary>
        private MouseState _clickPos = null;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected FeatureGroupBase()
        {
        }
        #region IDisposable メンバ

        public virtual void Dispose()
        {
            // 注意：ここでffDataLinkBaseのDisposeは実施しない。すれば、TimerObjectなどが使用できなくなる
            //        そのDisposeはルートグループから実行されます

            // 子供のDisposeをコールする
            foreach (var lb in _children)
            {
                if (lb is IDisposable)
                {
                    ((IDisposable)lb).Dispose();
                }
            }
            _children.Clear();
        }

        #endregion

        /// <summary>
        /// 自分以下のフィーチャーを全て取得する（子グループも含む）
        /// </summary>
        /// <returns></returns>
        public ICollection GetChildFeatureInstance()
        {
            var ret = new ArrayList();
            _getChildFeatureInstances(ret, this);
            return ret;
        }

        /// <summary>
        /// GetChildFeatureInstanceで使用する再帰関数
        /// </summary>
        /// <param name="buf">結果を収める配列</param>
        /// <param name="fg">検索開始するフィーチャーグループ</param>
        private void _getChildFeatureInstances(IList buf, FeatureGroupBase fg)
        {
            //lock(fg._children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var cg in fg._children)
                {
                    if (cg is FeatureGroupBase)
                    {
                        _getChildFeatureInstances(buf, (FeatureGroupBase)cg);
                    }
                    if (cg is FeatureBase)
                    {
                        buf.Add(cg);
                    }
                }
            }
        }

        /// <summary>
        /// 指定フィーチャーにトークンをセットする
        /// </summary>
        /// <param name="feature">フィーチャー</param>
        /// <param name="tokenID">トークンID</param>
        protected void requestStartup(FeatureGroupBase group, Type feature, NamedId tokenID)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var cg in _children)
                {
                    if (cg is FeatureGroupBase)
                    {
                        requestStartup((FeatureGroupBase)cg, feature, tokenID);
                    }
                    if (cg is FeatureBase)
                    {
                        if (((FeatureBase)cg).Enabled == false)
                        {
                            continue;
                        }

                        if (cg.GetType().Equals(feature))
                        {
                            ((FeatureBase)cg).RequestStartup(tokenID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 子フィーチャーグループを追加する
        /// </summary>
        public FeatureGroupBase AddChildGroup()
        {
            FeatureGroupBase ret = new FeatureGroupChild();
            linkDataTo(ret);
            //lock(_children.SyncRoot)	// トークン実行中いAddChildしたいので、デッドロックしないようにLockしない。
            {
                _children.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// 子フィーチャーを追加する（名前をつける）
        /// </summary>
        /// <param name="child"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FeatureBase AddChildFeature(Type child, string name)
        {
            var ret = AddChildFeature(child);
            ret.Name = name;
            return ret;
        }

        /// <summary>
        /// 子フィーチャークラスを追加する
        /// </summary>
        /// <param name="child">フィーチャークラスの型</param>
        public FeatureBase AddChildFeature(Type child)
        {
            System.Diagnostics.Debug.Assert(child.IsSubclassOf(typeof(FeatureBase)), "AddChildFeatureに登録できるのは、FeatureBaseを継承したクラスのみです");
            try
            {
                var ret = (FeatureBase)Activator.CreateInstance(child, true);
                linkDataTo(ret);
                //lock(_children.SyncRoot)	// トークン実行中にAddChildしたいので、デッドロック防止でlockしない。
                {
                    _children.Add(ret);
                }
                try
                {
                    ret.OnInitInstance();
                    var dummy = ret.CanStart;
                }
                catch (Exception ee)
                {
                    if (ret is IAutoRemovable)
                    {
                        Remove(ret);
                        ret = null;
                        System.Diagnostics.Debug.WriteLine("Feature " + child.ToString() + " is Auto Removed because " + ee.Message + " (" + ee.GetType().Name + ")");
                    }
                    else
                    {
                        if (ee.InnerException == null)
                        {
                            throw ee;
                        }
                        else
                        {
                            throw ee.InnerException;
                        }
                    }
                }
                return ret;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// 指定したフィーチャーインスタンスを除去する
        /// </summary>
        /// <param name="value">除去するフィーチャー</param>
        public void Remove(DataLinkManager value)
        {
            //lock(_children.SyncRoot)	// トークン実行中にRemoveしたいので、デッドロック防止でlockしない
            {
                _children.Remove(value);
            }
            if (value is FeatureBase)
            {
                ((FeatureBase)value).Enabled = false;
            }
            if (value is IDisposable)
            {
                ((IDisposable)value).Dispose();
            }
        }

        /// <summary>
        /// 全フィーチャーを削除する
        /// </summary>
        public void RemoveAllFeatures()
        {
            foreach (var obj in _children)
            {
                if (obj is IDisposable)
                {
                    ((IDisposable)obj).Dispose();
                }
            }
            _children.Clear();
        }

        private static readonly FieldInfo _fiTokenInvokedChecker = typeof(TokenTray).GetField("TokenInvokedChecker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// トークンが無くなるまでループして、各フィーチャーのStartを実行する
        /// </summary>
        /// <returns>true=処理された / false=処理は無かった</returns>
        private bool invokeStartupToken()
        {
            var root = (FeatureGroupRoot)GetRoot();
            var ret = false;


            var dupChecker = (IDictionary)_fiTokenInvokedChecker.GetValue(Token);

            dupChecker.Clear();

            var retryMax = 1000;
            while (Token.Count > 0)
            {
                System.Diagnostics.Debug.Assert(retryMax-- >= 0, "InvokeStartupTokenで無限ループを検知しました");
                var cnt = 0;
                root.startupLoop(dupChecker, ref cnt);
                if (cnt == 0)
                {
                    break;
                }
                else
                {
                    ret = true;
                }
            }
            Token._clear();
            dupChecker.Clear();

            // アプリ終了要求があればクローズする
            if (root.IsApplicationQuitting)
            {
                // フォームを取得する
                System.Windows.Forms.Control c;
                for (c = root.GetFeatureRich(); c is System.Windows.Forms.Form == false; c = c.Parent)
                {
                    ;
                } ((System.Windows.Forms.Form)c).Close();
                root.IsApplicationQuitting = false;
            }
            return ret;
        }

        private static readonly MethodInfo _mi_control_bredge_event = typeof(FeatureControlBridgeBase).GetMethod("_event", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// 指定コントロールにイベントを発行する
        /// </summary>
        /// <param name="featureID"></param>
        /// <param name="target"></param>
        /// <param name="eventName"></param>
        public void FireEvent(Id featureID, System.Windows.Forms.Control target, string eventName)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).FireEvent(featureID, target, eventName);
                    }
                    else if (c is FeatureControlBridgeBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (((FeatureBase)c).ID == featureID)
                        {
                            _mi_control_bredge_event.Invoke(c, new object[] { target, null, eventName });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 緊急トークン処理再帰ループ
        /// </summary>
        private void invokeUrgentTokenLoop(NamedId id, NamedId who, object param, FeatureGroupBase tar)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).invokeUrgentTokenLoop(id, who, param, tar);
                    }
                    else if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (TokenTray.ContainsTokenID((FeatureBase)c, id))
                        {
                            if (((FeatureBase)c).CanStart)
                            {
                                ((FeatureBase)c).Start(who);
                                ((FeatureBase)c).Start(who, param);
                            }
                        }
                    }
                }
            }
        }


        private void paramloop(FeatureGroupBase tar, List<FeatureBase> fcs)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).paramloop(tar, fcs);
                    }
                    else if (c is FeatureBase fc)
                    {
                        if (string.IsNullOrEmpty(fc.CommandParameter) == false)
                        {
                            fcs.Add(fc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// コマンドラインを解析して実行する
        /// </summary>
        /// <param name="v"></param>
        public void ParseCommandLineParameter(string[] args)
        {
            var fcs = new List<FeatureBase>();
            paramloop(this, fcs);
            fcs.Sort((x, y) => x.CommandParameter?.Length ?? 0 - y.CommandParameter?.Length ?? 0);  // StartWithで当てるので、長い方から処理する

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                for (var ii = 0; ii < fcs.Count; ii++)
                {
                    if (arg.StartsWith(fcs[ii].CommandParameter))
                    {
                        fcs[ii].OnCommandParameter(arg.Substring(fcs[ii].CommandParameter.Length).TrimStart());
                    }
                }
            }
        }

        /// <summary>
        /// ファイナライザーやトークンをすべて消費する
        /// </summary>
        public void FlushFeatureTriggers()
        {
            for (var cnt = 0; ; cnt++)
            {
                var finalizer = Finalizers.Flush();
                var token = invokeStartupToken();
                if ((finalizer || token) == false)
                {
                    break;
                }
                System.Diagnostics.Debug.Assert(cnt < 100, "ファイナライザとトークンで循環参照が発生しているかもしれません");
            }
            // パーツ削除イベント
            checkAndFireDataChanged();

            // 全トークン終了イベント
            checkAndFireAllTokenCompleted();
        }

        /// <summary>
        /// 名前で子フィーチャーを検索する（孫も検索する）
        /// </summary>
        /// <param name="name">検索する名前（大文字小文字の区別有り）</param>
        /// <returns>見つかったフィーチャー一覧</returns>
        public IList<FeatureBase> FindChildFeatures(string name)
        {
            var ret = new List<FeatureBase>();
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        var tmp = ((FeatureGroupBase)c).FindChildFeatures(name);
                        ret.AddRange(tmp);
                    }
                    if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Name == name)
                        {
                            ret.Add((FeatureBase)c);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 指定タイプを継承しているフィーチャーを検索する
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public IList<FeatureBase> FindChildFeatures(Type interfaceType)
        {
            var ret = new List<FeatureBase>();
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        var tmp = ((FeatureGroupBase)c).FindChildFeatures(interfaceType);
                        ret.AddRange(tmp);
                    }
                    if (c is FeatureBase)
                    {
                        if (interfaceType.IsInstanceOfType(c))
                        {
                            ret.Add((FeatureBase)c);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 全トークン終了イベント処理
        /// </summary>
        private void checkAndFireAllTokenCompleted()
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).checkAndFireAllTokenCompleted();
                    }
                    if (c is IAllTokenCompletedListener)
                    {
                        ((IAllTokenCompletedListener)c).OnAllTokenCompleted();
                    }
                }
            }
        }

        private static readonly FieldInfo _fiRemovedPartsOfData = typeof(PartsCollectionBase).GetField("_removedParts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo _fiAddedPartsOfData = typeof(PartsCollectionBase).GetField("_addedParts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// パーツ削除を検出したら、イベントを飛ばす
        /// </summary>
        private void checkAndFireDataChanged()
        {
            if (Parts == null)
            {
                return;
            }

            // 削除データを取得する
            var removed = (IList)_fiRemovedPartsOfData.GetValue(Parts);
            var added = (IList)_fiAddedPartsOfData.GetValue(Parts);
            if (removed.Count > 0 || added.Count > 0)
            {
                _dataChangedEventLoop(removed, added);
                removed.Clear();
                added.Clear();
            }
        }

        /// <summary>
        /// 削除イベントのインターフェースを実装しているフィーチャーにイベントを飛ばす
        /// </summary>
        /// <param name="removed"></param>
        private void _dataChangedEventLoop(ICollection removed, ICollection added)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c)._dataChangedEventLoop(removed, added);
                    }
                    if (c is IPartsRemoveListener)
                    {
                        ((IPartsRemoveListener)c).OnPartsRemoved(removed);
                    }
                    if (c is IPartsAddListener)
                    {
                        ((IPartsAddListener)c).OnPartsAdded(added);
                    }
                }
            }
        }

        /// <summary>
        /// 緊急トークンを発行する（すべてのトークンより先に実行する）
        /// </summary>
        /// <param name="id">TokenListenerに登録されているID</param>
        /// <param name="who">FeatureBase.Start(who)に伝えるID</param>
        /// <param name="param">パラメータ付きStartに渡すパラメータ</param>
        public void SetUrgentToken(NamedId id, NamedId who, object param)
        {
            invokeUrgentTokenLoop(id, who, param, GetRoot());
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// イベント起動
        /// </summary>
        private void startupLoop(IDictionary dupChecker, ref int cnt)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildするので、デッドロック防止のためスレッドセーフ
            {
                foreach (var c in new ArrayList(_children))  // 途中でAddChildするので、_childrenが変更される事に対応
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).startupLoop(dupChecker, ref cnt);
                    }
                    else if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (dupChecker.Contains(c) == false)
                        {
#if DEBUG
                            TimeKeeper.SetStart(TimeKeeper.RecordType.Start, ((FeatureBase)c).ID);
#endif
                            ((FeatureBase)c).invokeToken();
                            cnt++;
                            dupChecker[c] = this;
#if DEBUG
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.Start, ((FeatureBase)c).ID);
#endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ズーム変更イベント転送
        /// </summary>
        /// <param name="target"></param>
        public virtual void ZoomChanged(IRichPane target)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IZoomListener)
                    {
                        foreach (var t in ((IZoomListener)c).ZoomEventTargets)
                        {
                            if (t == target)
                            {
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetStart(TimeKeeper.RecordType.ZoomChanged, ((FeatureBase)c).ID);
                                }
#endif
#if DEBUG == false
                                try
#endif
                                {
                                    ((IZoomListener)c).ZoomChanged(target);
                                }
#if DEBUG == false
                                catch (Exception ex)
                                {
                                    LOG.WriteLineException(ex);
                                }
#endif
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetEnd(TimeKeeper.RecordType.ZoomChanged, ((FeatureBase)c).ID);
                                }
#endif
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// スクロール変更イベント転送
        /// </summary>
        /// <param name="target"></param>
        public virtual void ScrollChanged(IRichPane target)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IScrollListener)
                    {
                        foreach (var t in ((IScrollListener)c).ScrollEventTargets)
                        {
                            if (target == null || t == target)
                            {
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetStart(TimeKeeper.RecordType.ScrollChanged, ((FeatureBase)c).ID);
                                }
#endif
#if DEBUG == false
                                try
#endif
                                {
                                    ((IScrollListener)c).ScrollChanged(target);
                                }
#if DEBUG == false
                                catch (Exception ex)
                                {
                                    LOG.WriteLineException(ex);
                                }
#endif
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetEnd(TimeKeeper.RecordType.ScrollChanged, ((FeatureBase)c).ID);
                                }
#endif
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }


        /// <summary>
        /// マウス移動イベント転送
        /// </summary>
        public virtual void OnMouseMove(Tono.GuiWinForm.MouseState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseMove, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseMove(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseMove, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }


        /// <summary>
        /// テキストコマンドを各フィーチャーに転送する
        /// </summary>
        /// <param name="tc"></param>
        public void PlayTextCommand(CommandBase tc)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).PlayTextCommand(tc);
                    }
                    else
                    {
                        if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (c is ITextCommand)
                        {
                            var targets = ((ITextCommand)c).tcTarget();
                            for (var i = 0; i < targets.Length; i++)
                            {
                                if (tc.Command == targets[i])
                                {
                                    ((ITextCommand)c).tcPlay(tc);
                                }
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// マウスダウンイベント転送
        /// </summary>
        public virtual void OnMouseDown(Tono.GuiWinForm.MouseState e)
        {
            if (Parts == null)
            {
                return;
            }

            if (_clickPos == null)
            {
                _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));   // 移動中のパーツ一覧
            }
            SerializerEx.CopyObject(_clickPos, e);

            // パーツを取得する
            ClickParts = Parts.GetPartsAt(e.Pos, true, out var clickPane);
            ClickPane = clickPane;

            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseDown, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseDown(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseDown, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// マウスアップイベント転送
        /// </summary>
        public virtual void OnMouseUp(Tono.GuiWinForm.MouseState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseUp, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseUp(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseUp, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// マウスホイールイベントの転送
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseWheel(MouseState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseWheel, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseWheel(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseWheel, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// アイテムのドロップイベントの転送
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDragDrop(Tono.GuiWinForm.DragState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IDragDropListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.DragDrop, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IDragDropListener)c).OnDragDrop(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.DragDrop, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// キーダウンイベント転送
        /// </summary>
        public virtual void OnKeyDown(KeyState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IKeyListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.KeyDown, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IKeyListener)c).OnKeyDown(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.KeyDown, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }

        /// <summary>
        /// キーアップイベント転送
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnKeyUp(KeyState e)
        {
            //lock(_children.SyncRoot)	// 途中でAddChildしないので、スレッドセーフ。途中でAddChildしてしまうと下のループでAssertされる
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IKeyListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.KeyUp, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IKeyListener)c).OnKeyUp(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.KeyUp, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // パーツ削除イベント
        }
    }
}
