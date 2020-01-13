// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureGanttRowSnap の概要の説明です。
    /// </summary>
    public class FeatureGanttRowSnap : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズしない）

        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;

        #endregion

        /// <summary>
        /// 初期化（共有変数の割当など）
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
        }

        /// <summary>
        /// マウス移動をドラッグ中として実装する
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            if (_pos.Count > 0)
            {
                foreach (DictionaryEntry de in _pos)            // 選択中全パーツに対して行う
                {
                    var p3 = (PartsPositionManager.Pos3)de.Value;
                    // 縦位置を16の倍数にスナップするサンプル
                    //p3.Now.LT.Y = (p3.Now.LT.Y) / 16 * 16;
                    //p3.Now.RB.Y = (p3.Now.RB.Y) / 16 * 16;

                    // とりあえず縦方向の移動はしない。
                    //p3.Now.LT.Y = p3.Pre.LT.Y;
                    //p3.Now.RB.Y = p3.Pre.LT.Y;
                }
            }
        }
        #region IMouseListener メンバ

        public void OnMouseDown(MouseState e)
        {
        }

        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }


        #endregion
    }
}
