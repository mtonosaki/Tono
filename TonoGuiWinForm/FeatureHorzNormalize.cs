using System.Collections;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// オブジェクトの横移動の制限をサポートします。
    /// </summary>
    public class FeatureHorzNormalize : Tono.GuiWinForm.FeatureBase, IMouseListener, ITokenListener
    {
        #region	属性(シリアライズする)
        /// <summary>移動制限を監視するトリガ</summary>
        protected MouseState _trigger = new MouseState();
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>選択中のパーツ</summary>
        protected PartsBase _SelectedParts = null;
        /// <summary>P3オブジェクト⇒パーツリストのリンク</summary>
        protected Hashtable _P3toPartsList = new Hashtable();
        /// <summary>パーツ⇒P3オブジェクトのリンク</summary>
        protected IDictionary<PartsBase, PartsPositionManager.Pos3> _PartsToP3 = new Dictionary<PartsBase, PartsPositionManager.Pos3>();
        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;
        /// <summary>連携移動モードのON(1)/OFF(0)</summary>
        protected DataSharingManager.Int _FollowMoveMode = null;
        /// <summary>マウスの状態を共有変数と同期</summary>
        protected MouseState _clickPos;
        /// <summary>マウスボタンを押した際に必要な処理を開始するトークン</summary>
        protected NamedId _tokenMouseDownJob = NamedId.FromName("MouseDownJob");
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureHorzNormalize()
        {
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            // ステータス同期
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
            _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));  // 移動中のパーツ一覧
        }

        /// <summary>
        ///  トークンによる起動イベント
        /// </summary>
        /// <param name="who"></param>
        public override void Start(NamedId who)
        {
            Finalizers.Add(new FinalizeManager.Finalize(FinalMouseDownJob));
        }

        /// <summary>
        /// マウスダウン処理の最終決定
        /// </summary>
        protected virtual void FinalMouseDownJob()
        {
        }

        /// <summary>
        /// マウスダウン処理の最終決定
        /// </summary>
        protected virtual void FinalMouseMoveJob()
        {
        }

        #region IMouseListener メンバ
        /// <summary>
        /// マウスムーブ
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
        }
        /// <summary>
        /// マウスダウン
        /// </summary>
        public virtual void OnMouseDown(MouseState e)
        {
        }
        /// <summary>
        /// マウスアップ
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
        }
        /// <summary>
        /// マウスホイール
        /// </summary>
        public virtual void OnMouseWheel(MouseState e)
        {
        }
        #endregion
        #region ITokenListener メンバ
        public virtual NamedId TokenTriggerID => TokenGeneral.TokenMouseDownNormalize;
        #endregion
    }
}
