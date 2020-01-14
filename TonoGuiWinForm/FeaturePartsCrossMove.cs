// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeaturePartsSelect の概要の説明です。
    /// </summary>
    public class FeaturePartsCrossMove : FeatureBase, IMouseListener
    {
        #region 属性（シリアライズしない）

        /// <summary>パーツ位置管理オブジェクト</summary>
        protected PartsPositionManager _pos;
        private ScreenPos _clickPos = ScreenPos.FromInt(0, 0);
        private bool _isNoKey = false;  // Shiftキーを押さなくても機能させたい場合はTrue

        #endregion

        public FeaturePartsCrossMove()
        {
        }

        /// <summary>
        /// パラメータ読み込み
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            base.ParseParameter(param);
            if (param.Equals("KEYREVERSE", StringComparison.CurrentCultureIgnoreCase))
            {
                _isNoKey = true;
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _pos = (PartsPositionManager)Share.Get("MovingParts", typeof(PartsPositionManager));    // 移動中のパーツ一覧
        }

        /// <summary>
        /// パーツ選択処理をマウスダウンイベントで行う
        /// </summary>
        public void OnMouseDown(MouseState e)
        {
            _clickPos = (ScreenPos)e.Pos.Clone();
        }

        /// <summary>
        /// マウス移動イベントは不要
        /// </summary>
        public void OnMouseMove(MouseState e)
        {
            if (e.Attr.IsShift && _isNoKey == false || e.Attr.IsShift == false && _isNoKey)
            {
                int dx = 0, dy = 0;
                if (e != null)
                {
                    dx = Math.Abs(e.Pos.X - _clickPos.X);
                    dy = Math.Abs(e.Pos.Y - _clickPos.Y);
                }

                foreach (DictionaryEntry de in _pos)
                {
                    var parts = (PartsBase)de.Key;
                    var p3 = (PartsPositionManager.Pos3)de.Value;
                    if (dx > dy)
                    {
                        p3.Now.LT.Y = p3.Org.LT.Y;
                        p3.Now.RB.Y = p3.Org.RB.Y;
                    }
                    else
                    {
                        p3.Now.LT.X = p3.Org.LT.X;
                        p3.Now.RB.X = p3.Org.RB.X;
                    }
                }
            }
        }

        /// <summary>
        /// マウスアップイベントは不要
        /// </summary>
        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }
    }
}
