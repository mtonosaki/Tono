// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 選択されているパーツを移動させる
    /// </summary>
    public class FeatureMoveSelectedParts : FeatureBase, IMouseListener, IMultiTokenListener
    {
        #region	属性(シリアライズする)
        #endregion
        #region 属性（シリアライズしない）

        /// <summary>パーツ移動のトリガ（マウス左ボタン）</summary>
        protected MouseState.Buttons _trigger;
        /// <summary>パーツの連携移動のトリガ（マウス左ボタン+Shiftキー）</summary>
        protected MouseState.Buttons _FollowTrigger;

        /// <summary>選択中のパーツ（共有変数）</summary>
        protected PartsCollectionBase _selectedParts;

        /// <summary>選択中のパーツという意味でシリアライズするID</summary>
        protected NamedId _meansSelectedParts = NamedId.FromName("FeatureDataSerializeID");

        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;

        /// <summary>マウスダウン時の位置を記憶する。null = まだドラッグ開始していない事を示す</summary>
        protected ScreenPos _mouseDownOriginal = null;

        /// <summary>パーツに対する操作のモード</summary>
        protected PartsPositionManager.DevelopType _developmentMode = PartsPositionManager.DevelopType.Move;

        /// <summary>
        /// クリックした場所の記憶
        /// </summary>
        protected MouseState _clickPos;

        /// <summary>マウスボタンを押した際に必要な処理を開始するトークン</summary>
        protected NamedId[] _tokens = new NamedId[] { NamedId.FromName("MouseDownJob"), NamedId.FromName("MouseMoveJob") };

        /// <summary>マウスアップを押した際に必要な処理を開始するトークン</summary>
        protected NamedId _tokenMouseUpJob = NamedId.FromName("MoveMouseUpJob");

        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FeatureMoveSelectedParts()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons(true, false, false, false, false);
            _FollowTrigger = new MouseState.Buttons(true, false, false, true, false);
        }

        /// <summary>
        /// 初期化（共有変数の割当など）
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));   // 選択済みのパーツ一覧
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
            _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));       // 移動中のパーツ一覧
        }

        /// <summary>
        /// パーツ移動
        /// </summary>
        private void onFinalizeMoveParts()
        {
            _pos.SetNowPositionsToParts(Parts);
        }

        /// <summary>
        /// マウスダウン処理の最終決定
        /// </summary>
        protected virtual void _finalMouseDownPart()
        {
            var e = _clickPos;

            if (_selectedParts.Count > 0)
            {
                var tarPane = ClickPane;
                var parts = ClickParts; // Parts.GetPartsAt(e.Pos, true, out tarPane);
                if (parts != null)
                {
                    if (parts.IsOn(e.Pos, tarPane) == PartsBase.PointType.Inside)
                    {
                        _pos.SetDevelop(_developmentMode = PartsPositionManager.DevelopType.Move);
                        _mouseDownOriginal = e.Pos;
                        _pos.Initialize(_selectedParts);
                        Token.Add(TokenGeneral.TokenMouseDownNormalize, this);
                    }
                }
            }
        }

        /// <summary>
        /// マウスダウン時の初期化処理
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            // MouseDownトークン
            if (who.Equals(_tokens[0]))
            {
                Finalizers.Add(new FinalizeManager.Finalize(_finalMouseDownPart));  // SezeSelected・・・も継承しているので、ID不指定
                                                                                    //			Finalizers.Add(_tokenListenID, new ffFinalizer.Finalize(_finalMouseDownPart)); by Tono 調査中 2006.2.2
            }
            // MouseMoveトークン
            if (who.Equals(_tokens[1]))
            {
                OnMouseMove(null);
            }
        }


        #region IMouseListener メンバ
        /// <summary>
        /// マウスダウンイベントをドラッグ開始トリガとして実装する
        /// </summary>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                Start(_tokens[0]);
            }
        }

        /// <summary>
        /// マウス移動をドラッグ中として実装する
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_mouseDownOriginal != null)
            {
                // UNDO用のデータを記録する
                if (Persister[UNDO].IsStartedChunk == false)
                {
                    Persister[REDO].StartChunk(GetType().Name + ".OnMouseMove");
                    Persister[UNDO].StartChunk(GetType().Name + ".OnMouseMove");

                    Persister[UNDO].Save(_selectedParts, _meansSelectedParts);
                    foreach (DictionaryEntry de in _pos)
                    {
                        var parts = (PartsBase)de.Key;

                        // パーツのUNDO保存
                        Persister[UNDO].Save(parts, _meansSelectedParts);   // partsのフレンド保存は、FeatureDposeCheckOverlappedで行う

                        // レコードのUNDO保存
                        foreach (RecordBase rec in Link.GetRecordset(parts))
                        {
                            Persister[UNDO].Save(rec, _meansSelectedParts);
                        }
                    }
                }

                // すべての選択パーツの座標変更予約を入れる
                if (e != null)
                {
                    _pos.Develop(_mouseDownOriginal, e.Pos, _developmentMode);
                }

                // すべてのフィーチャー実行後に移動させる
                Finalizers.Add(new FinalizeManager.Finalize(onFinalizeMoveParts));
            }
        }

        /// <summary>
        /// マウスアップイベントをドラッグ終了トリガとして実装する
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
            Finalizers.Add(new FinalizeManager.Finalize(OnFinalizeMouseUpJob));
        }

        /// <summary>
        /// マウスアップの最終処理
        /// </summary>
        protected virtual void OnFinalizeMouseUpJob()
        {
            if (_mouseDownOriginal != null)
            {
                var movedCount = 0;

                // 移動パーツをデータに反映させる
                foreach (DictionaryEntry de in _pos)
                {
                    var parts = (PartsBase)de.Key;
                    var pos = (PartsPositionManager.Pos3)de.Value;
                    if (pos.Now.LT.Equals(pos.Org.LT) == false || pos.Now.RB.X != pos.Org.RB.X)
                    {
                        if (movedCount == 0)
                        {
                            Persister[REDO].Save(_selectedParts, _meansSelectedParts);
                        }
                        // データ更新
                        Link.Equalization(parts, false);    // Data～Partsのデータ連動

                        // REDO永続化（パーツ）
                        //savePartsWithFriend(Persister[REDO], parts, new Hashtable());

                        // REDO永続化（レコード）
                        foreach (RecordBase rb in Link.GetRecordset(parts))
                        {
                            Persister[REDO].Save(rb, _meansSelectedParts);
                        }
                        movedCount++;
                    }
                }

                // 移動終了
                _mouseDownOriginal = null;
                _pos.Clear();

                Persister[REDO].EndChunk(); // REDOを先に行うことは重要
                Persister[UNDO].EndChunk();
                Pane.Invalidate(Pane.GetPaneRect());
            }
        }

        public void OnMouseWheel(MouseState e)
        {
        }
        #endregion
        #region IMultiTokenListener メンバ

        public NamedId[] MultiTokenTriggerID => _tokens;

        #endregion
    }
}
