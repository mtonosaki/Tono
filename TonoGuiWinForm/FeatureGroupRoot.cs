// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fgRoot の概要の説明です。
    /// ルートのフィーチャーグループ
    /// 他のすべてのフィーチャーグループは、この子フィーチャーグループとなる
    /// </summary>
    public sealed class FeatureGroupRoot : FeatureGroupBase, IDisposable
    {
        #region 属性（シリアライズする）

        #endregion
        #region 属性（シリアライズしない）
        private FeatureLoaderBase _loader;
        private readonly TGuiView _motherPane;
        private readonly DataSharingManager.Int _isApplicationQuitting;

        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <param name="motherPane">このフィーチャールートで使用する主催ペーン</param>
        public FeatureGroupRoot(TGuiView motherPane)
        {
            _motherPane = motherPane;
            setPane(motherPane);
            setShare(new DataSharingManager());
            _isApplicationQuitting = (DataSharingManager.Int)Share.Get("ApplicationQuitFlag", typeof(DataSharingManager.Int));
        }

        /// <summary>
        /// 指定フィーチャーにトークンをセットする
        /// </summary>
        /// <param name="feature">フィーチャー</param>
        /// <param name="tokenID">トークンID</param>
        public void RequestStartup(Type feature, NamedId tokenID)
        {
            requestStartup(this, feature, tokenID);
        }

        /// <summary>
        /// アプリケーション終了要求がたっているかどうかを調べる
        /// </summary>
        public bool IsApplicationQuitting
        {
            get => _isApplicationQuitting.value == 0 ? false : true;
            set => _isApplicationQuitting.value = value ? 1 : 0;
        }

        /// <summary>
        /// マザーペーンを返す（基本となるフィーチャーリッチコンポーネント）
        /// </summary>
        /// <returns></returns>
        public TGuiView GetFeatureRich()
        {
            return _motherPane;
        }

        /// <summary>
        /// フォームのOnLoadで実行するコマンド
        /// </summary>
        /// <param name="featureLoader">uFeatureLoaderシリーズのタイプ</param>
        public void Initialize(Type featureLoader)
        {
            if (featureLoader != null)
            {
                _loader = (FeatureLoaderBase)Activator.CreateInstance(featureLoader);
                _loader.Load(this, "Portfolio0.xml");
            }
        }

        /// <summary>
        /// ファイル名を指定してフォームのOnLoadを実行するコマンド
        /// </summary>
        /// <param name="featureLoader">uFeatureLoaderシリーズのタイプ</param>
        public void Initialize(Type featureLoader, string file)
        {
            if (featureLoader != null)
            {
                _loader = (FeatureLoaderBase)Activator.CreateInstance(featureLoader);
                _loader.Load(this, file);
            }
        }

        #region IDisposable メンバ

        public override void Dispose()
        {
            base.Dispose();
            var mi = typeof(DataLinkManager).GetMethod("disposeByRootGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            mi.Invoke(this, Array.Empty<object>());
        }

        #endregion

        /// <summary>
        /// データを割り当てる
        /// </summary>
        /// <param name="value">割り当て（参照）されるデータのインスタンス</param>
        public void AssignAppData(DataHotBase value)
        {
            setAppData(value);
        }

        /// <summary>
        /// リンクオブジェクトを割り当てる
        /// </summary>
        /// <param name="value">リンク</param>
        public void AssignLink(DataLinkBase value)
        {
            setLink(value);
        }


        /// <summary>
        /// パーツセットを割り当てる
        /// </summary>
        /// <param name="value"></param>
        public void AssignPartsSet(PartsCollectionBase value)
        {
            setPartsData(value);
        }

        /// <summary>
        /// Shareをparentから引き継ぐ
        /// </summary>
        /// <param name="parent"></param>
        public void LinkShare(DataSharingManager parent)
        {
            setShare(parent);
        }

        /// <summary>
        /// データを取得する
        /// </summary>
        /// <returns></returns>
        public PartsCollectionBase GetPartsSet()
        {
            System.Diagnostics.Debug.WriteLineIf(Parts == null, "PartsSetを使用する前に AssignDataを実行しておくようにプログラムしてください");
            return Parts;
        }

        /// <summary>
        /// データを取得する
        /// </summary>
        /// <returns></returns>
        public DataHotBase GetData()
        {
            System.Diagnostics.Debug.WriteLineIf(Data == null, "Dataを使用する前に AssignDataを実行しておくようにプログラムしてください");
            return Data;
        }

        /// <summary>
        /// データを取得する
        /// </summary>
        /// <returns></returns>
        public DataSharingManager GetShare()
        {
            //Debug.WriteLineIf(Share == null, "Shareを使用する前に AssignDataを実行しておくようにプログラムしてください");
            return Share;
        }

        public override void OnMouseDown(MouseState e)
        {
            base.OnMouseDown(e);
            FlushFeatureTriggers();
        }

        public override void OnMouseMove(MouseState e)
        {
            base.OnMouseMove(e);
            FlushFeatureTriggers();
        }

        public override void OnMouseUp(MouseState e)
        {
            base.OnMouseUp(e);
            FlushFeatureTriggers();
        }
        public override void OnMouseWheel(MouseState e)
        {
            base.OnMouseWheel(e);
            FlushFeatureTriggers();
        }

        public override void OnDragDrop(DragState e)
        {
            base.OnDragDrop(e);
            FlushFeatureTriggers();
        }

        public override void OnKeyDown(KeyState e)
        {
            base.OnKeyDown(e);
            FlushFeatureTriggers();
        }

        public override void OnKeyUp(KeyState e)
        {
            base.OnKeyUp(e);
            FlushFeatureTriggers();
        }

        public override void ZoomChanged(IRichPane target)
        {
            base.ZoomChanged(target);
            //FlushFeatureTriggers();	// TODO:ここは、コメントでもOK？ by Tono 2006.1.23
        }
    }
}
