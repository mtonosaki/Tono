// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// カーソルセットを担当する
    /// キー押下によるマウスカーソルの変更
    /// </summary>
    public class FeatureCursorProviderKey : Tono.GuiWinForm.FeatureBase, IKeyListener, IMouseListener
    {
        /// <summary>
        /// カーソルを変更する条件、カーソル変更予約オブジェクト
        /// </summary>
        public class Reserve
        {
            #region 属性（シリアライズする）

            /// <summary>カーソル変更する条件</summary>
            public MouseState.Buttons Buttons;

            /// <summary>条件に達した際に使用するカーソル</summary>
            public Cursor Cursor;

            /// <summary>条件に対するメッセージ</summary>
            public string Message = string.Empty;

            /// <summary>
            /// 
            /// </summary>
            public MouseState.Buttons PreviousButtons = null;

            #endregion

            /// <summary>
            /// 初期化コンストラクタ
            /// </summary>
            /// <param name="buttons">表示条件となるボタンの状態</param>
            /// <param name="cursor">その条件で表示したいカーソル</param>
            /// <param name="mes">カーソルへと導くメッセージ</param>
            public Reserve(MouseState.Buttons buttons, Cursor cursor, string mes)
            {
                Buttons = buttons;
                Cursor = cursor;
                Message = mes;
            }

            /// <summary>
            /// 初期化コンストラクタ
            /// </summary>
            /// <param name="previous">カーソルを有効とする直前の状態を特定する。それ以外のボタンからこの状態になってもカーソルを変更しない</param>
            /// <param name="buttons">表示条件となるボタンの状態</param>
            /// <param name="cursor">その条件で表示したいカーソル</param>
            /// <param name="mes">カーソルへと導くメッセージ</param>
            public Reserve(MouseState.Buttons previous, MouseState.Buttons buttons, Cursor cursor, string mes)
            {
                Buttons = buttons;
                Cursor = cursor;
                Message = mes;
                PreviousButtons = previous;

            }
        }


        #region 属性（シリアライズする）

        /// <summary>通常カーソルを記憶する</summary>
        private readonly Hashtable /*<uMouseState.Buttons, Reserve>*/ _resData = new Hashtable();

        #endregion
        #region 属性（シリアライズしない）

        /// <summary>通常カーソルを記憶する</summary>
        private Cursor _normalCursor;

        /// <summary>現在のキーの状態を記憶</summary>
        private MouseState _ms = new MouseState();

        private MouseState _prev = new MouseState();

        /// <summary>要求するカーソル（後で設定するもの）</summary>
        private Cursor _requestedCursor = null;

        /// <summary>カーソル表示状態（共有変数）</summary>
        private DataSharingManager.Int _state;

        /// <summary>カーソルセット状況を保存するためのID</summary>
        private NamedId _cursorSet;

        #endregion


        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _cursorSet = NamedId.FromName("CursorSet");

            if (Pane is IControlUI)
            {
                _normalCursor = ((IControlUI)Pane).Cursor;
            }
            else
            {
                throw new NotSupportedException("FeatureCursorProviderは、IControlUIを実装しているPaneにのみ使用できます");
            }

            // ステータス同期
            _state = (DataSharingManager.Int)Share.Get("CursorProviderStatus", typeof(DataSharingManager.Int));
        }

        /// <summary>
        /// カーソルリザーブを登録する（表示条件とそのカーソルのセット＝リザーブ）
        /// </summary>
        /// <param name="value">登録するリザーブのインスタンス</param>
        public void Add(Reserve value)
        {
            _resData.Add(value.Buttons, value);
        }

        /// <summary>
        /// 条件判断してカーソルを変更する
        /// </summary>
        /// <param name="ms">現在のキー状態</param>
        private void proc(MouseState ms)
        {
            // そのまま評価する
            for (var de = _resData.GetEnumerator(); de.MoveNext();)
            {
                if (((MouseState.Buttons)de.Key).Equals(ms.Attr))
                {
                    var res = ((Reserve)de.Value);

                    if (res.PreviousButtons != null)
                    {
                        if (res.PreviousButtons.Equals(_prev.Attr) == false)
                        {
                            continue;
                        }
                    }

                    _requestedCursor = res.Cursor;
                    Finalizers.Add(_cursorSet, new FinalizeManager.Finalize(onCursorSet));
                    _state.value = 1;
                    _prev = _ms;
                    return;
                }
            }
            // カーソルを戻す時の処理
            if (Finalizers.Contains(_cursorSet) == false)
            {
                _requestedCursor = _normalCursor;
                Finalizers.Add(_cursorSet, new FinalizeManager.Finalize(onCursorSet));
                _state.value = 0;
            }
            _prev = _ms;
        }

        /// <summary>
        /// カーソルをセットする（ファイナライズ）
        /// </summary>
        private void onCursorSet()
        {
            if (_requestedCursor != null)
            {
                ((IControlUI)Pane).Cursor = _requestedCursor;
            }
        }

        //int aa = 0;

        #region IKeyListener メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnKeyDown(KeyState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnKeyDown " + aa++);
            _ms.Attr.SetKeyFrags(e);
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnKeyUp(KeyState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnKeyUp " + aa++);
            _ms.Attr.SetKeyFrags(e);
            proc(_ms);
        }

        #endregion

        #region IMouseListener メンバ

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseMove(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseMove " + aa++);
            _ms = e;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseDown(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseDown " + aa++);
            _ms = e;
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseUp(MouseState e)
        {
            //System.Diagnostics.Debug.WriteLine("OnMouseUp " + aa++);
            _ms.Attr.ResetKeyFlags(e.Attr);
            _ms.Attr.SetKeyFrags(e.Attr);
            proc(_ms);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
    }
}
