// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// マウスをドラッグして画面スクロールする機能
    /// </summary>
    public class FeatureDragScroll : Tono.GuiWinForm.FeatureBase, IMouseListener
    {
        #region 属性（シリアライズする）
        /// <summary>イベントを実行するキーとなるマウスの状態</summary>
        protected MouseState.Buttons _trigger;
        #endregion
        #region 属性（シリアライズしない）
        /// <summary>マウスをクリックした時点でのマウス座標</summary>
        protected ScreenPos _posDown = null;
        /// <summary>マウスをクリックした時点でのスクロール量</summary>
        protected ScreenPos _scrollDown;
        /// <summary>直前とキーの状態を記憶</summary>
        protected MouseState _prev = new MouseState();
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FeatureDragScroll()
        {
            // デフォルトでドラッグスクロールするためのキーを設定する
            _trigger = new MouseState.Buttons
            {
                IsButton = false,
                IsButtonMiddle = true,
                IsDoubleClick = false,
                IsCtrl = false,
                IsShift = false
            };
        }

        /// <summary>
        /// パラメーターの初期化
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            var coms = param.Split(new char[] { ';' });
            foreach (var com in coms)
            {
                var od = com.Split(new char[] { '=' });
                if (od.Length == 2)
                {
                    if (od[0].ToLower() == "trigger")
                    {
                        _trigger = new MouseState.Buttons();

                        var ts = od[1].Split(new char[] { '+' });
                        foreach (var t in ts)
                        {
                            if (t.ToLower() == "middle")
                            {
                                _trigger.IsButtonMiddle = true;
                            }
                            if (t.ToLower() == "button" || t.ToLower() == "left")
                            {
                                _trigger.IsButton = true;
                            }
                            if (t.ToLower() == "ctrl")
                            {
                                _trigger.IsCtrl = true;
                            }
                            if (t.ToLower() == "shift")
                            {
                                _trigger.IsShift = true;
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// トリガ（実行識別キー）を変更する
        /// </summary>
        /// <param name="value">新しいトリガー</param>
        public void SetTrigger(MouseState.Buttons value)
        {
            _trigger = value;
        }

        #region IMouseListener メンバ

        /// <summary>
        /// マウス移動イベント処理
        /// </summary>
        public virtual void OnMouseMove(MouseState e)
        {
            if (_posDown != null)
            {
                if (e.Attr.Equals(_trigger))
                {
                    var spos = _scrollDown + (e.Pos - _posDown);
                    Pane.Scroll = spos;
                    Pane.Invalidate(null);
                }
                else
                {
                    OnMouseUp(e);
                }
            }
        }

        /// <summary>
        /// マウスダウンイベント処理
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseDown(MouseState e)
        {
            if (e.Attr.Equals(_trigger))
            {
                _posDown = e.Pos;
                _scrollDown = Pane.Scroll;

            }
        }

        /// <summary>
        /// マウスアップイベント処理
        /// </summary>
        public virtual void OnMouseUp(MouseState e)
        {
            _posDown = null;
            //System.Diagnostics.Debug.WriteLine("Scroll = " + Pane.Scroll.ToString());
        }

        public virtual void OnMouseWheel(MouseState e)
        {
            // 未使用
        }
        #endregion
    }
}
