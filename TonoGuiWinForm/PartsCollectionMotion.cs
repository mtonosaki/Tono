// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// パーツモーション、スクロールモーションのサポートできるパーツセット
    /// </summary>
    public class PartsCollectionMotion : PartsCollection
    {
        /// <summary>
        /// 移動先と残りモーションステップ数
        /// </summary>
        private class TargetPosition
        {
            public CodePos Pos = null;
            public int StepN = 20;

            /// <summary>
            /// 初期化コンストラクタ
            /// </summary>
            /// <param name="pos"></param>
            /// <param name="stepN"></param>
            public TargetPosition(CodePos pos, int stepN)
            {
                Pos = pos;
                StepN = stepN;
            }
        }

        /// <summary>
        /// スクロール指示
        /// </summary>
        private class ScrollCommand : uCouple<ScreenPos, int/*curStepN*/>
        {
            public ScrollCommand(ScreenPos pos, int step)
                : base(pos, step)
            {
            }
        }

        /// <summary>
        /// ズーム指示
        /// </summary>
        private class ZoomCommand : uCouple<XyBase, int/*curStepN*/>
        {
            public ZoomCommand(XyBase pos, int step)
                : base(pos, step)
            {
            }
        }

        /// <summary>
        /// 移動先情報
        /// </summary>
        private readonly Dictionary<PartsBase, TargetPosition> _tarPos = new Dictionary<PartsBase, TargetPosition>();

        /// <summary>
        /// モーションスクロールの位置
        /// </summary>
        private readonly Dictionary<IRichPane, ScrollCommand> _motionScroll = new Dictionary<IRichPane, ScrollCommand>();

        /// <summary>
        /// モーションスクロールの位置
        /// </summary>
        private readonly Dictionary<IRichPane, ZoomCommand> _motionZoom = new Dictionary<IRichPane, ZoomCommand>();

        /// <summary>
        /// モーション対象のペーン
        /// </summary>
        private IRichPane _motionPane = null;

        /// <summary>
        /// モーション対象のペーンを指定する
        /// </summary>
        /// <param name="tar"></param>
        public void SetMotionPane(IRichPane tar)
        {
            _motionPane = tar;
        }

        /// <summary>
        /// モーションして移動する先を指定する
        /// </summary>
        /// <param name="nStep"></param>
        /// <param name="pos"></param>
        public void SetMotionPosition(PartsBase part, CodePos pos, int nStep)
        {
            lock (_tarPos)
            {
                _tarPos[part] = new TargetPosition(pos, nStep);
            }
        }

        /// <summary>
        /// 表示処理
        /// </summary>
        public override void ProvideDrawFunction()
        {
            var isMotionNeed = scroll1step();
            isMotionNeed |= zoom1step();

            base.ProvideDrawFunction();

            isMotionNeed |= move1step();

            if (isMotionNeed)
            {
                _motionPane.Invalidate(null);
            }
        }

        /// <summary>
        /// ズーム１ステップ実行
        /// </summary>
        /// <returns></returns>
        private bool zoom1step()
        {
            var dels = new List<IRichPane>();
            var isNeedRetry = false;
            foreach (var kv in _motionZoom)
            {
                isNeedRetry = true;

                var pane = kv.Key;
                var tarZoom = kv.Value.V1;

                var nowPos = pane.Zoom;

                if (--kv.Value.V2/*curStepN*/ <= 0)
                {
                    pane.Zoom = tarZoom;
                    dels.Add(pane);
                }
                else
                {
                    var diff = tarZoom - nowPos;
                    diff /= 4;    // イーズアウトモーション（終わりがけをゆっくり）
                    pane.Zoom = nowPos + diff;
                }
            }
            foreach (var pane in dels)
            {
                _motionZoom.Remove(pane);
            }
            if (_motionZoom.Count > 0)
            {
                _isMoving |= Moving.Scroll;
            }
            return isNeedRetry;
        }

        /// <summary>
        /// スクロールを１ステップ実行
        /// </summary>
        /// <returns></returns>
        private bool scroll1step()
        {
            var dels = new List<IRichPane>();
            var isNeedRetry = false;
            lock (_motionScroll)
            {
                foreach (var kv in _motionScroll)
                {
                    isNeedRetry = true;

                    var pane = kv.Key;
                    var tarPos = kv.Value.V1;

                    var nowPos = pane.Scroll;

                    if (--kv.Value.V2/*curStepN*/ <= 0)
                    {
                        pane.Scroll = tarPos;
                        dels.Add(pane);
                    }
                    else
                    {
                        var diff = tarPos - nowPos;
                        diff /= 4;    // イーズアウトモーション（終わりがけをゆっくり）
                        pane.Scroll = nowPos + diff;
                    }
                }
                foreach (var pane in dels)
                {
                    _motionScroll.Remove(pane);
                }
                if (_motionScroll.Count > 0)
                {
                    _isMoving |= Moving.Scroll;
                }
                return isNeedRetry;
            }
        }

        /// <summary>
        /// モーション移動フラグ
        /// </summary>
        [Flags]
        public enum Moving
        {
            /// <summary>
            /// 移動していない
            /// </summary>
            None = 0,
            /// <summary>
            /// パーツ移動中
            /// </summary>
            Parts = 0x01,
            /// <summary>
            /// モーションスクロール中
            /// </summary>
            Scroll = 0x02,
        }

        /// <summary>
        /// モーション移動フラグ
        /// </summary>
        private static Moving _isMoving = Moving.None;

        /// <summary>
        /// 移動中かどうかを示すフラグ
        /// </summary>
        public static Moving IsMoving => _isMoving;

        /// <summary>
        /// モーション移動対象パーツを１モーションステップ分 進める
        /// </summary>
        /// <returns>true=もう一度自己Invalidateでモーション継続が必要</returns>
        private bool move1step()
        {
            var dels = new List<PartsBase>();
            var nDone = 0;
            var ret = false;
            lock (_tarPos)
            {
                foreach (var kv in _tarPos)
                {
                    var part = kv.Key;
                    var tar = kv.Value;

                    if (part.Rect.LT.Equals(tar.Pos))
                    {
                        dels.Add(part);
                        tar.StepN = 0;
                    }
                    else
                    {
                        tar.StepN--;
                        if (tar.StepN == 0)
                        {
                            part.Rect = CodeRect.FromLTWH(tar.Pos.X, tar.Pos.Y, part.Rect.Width, part.Rect.Height);
                            dels.Add(part);

                        }
                        else
                        {
                            float dx = tar.Pos.X - part.Rect.LT.X;
                            float dy = tar.Pos.Y - part.Rect.LT.Y;
                            var div = (float)tar.StepN * 2;
                            dx /= div;
                            dy /= div;
                            part.Rect += CodePos.FromInt((int)dx, (int)dy);
                        }
                        nDone++;
                    }
                }
            }
            if (nDone > 0)
            {
                ret = true;
            }

            foreach (var del in dels)
            {
                _tarPos.Remove(del);
            }
            _isMoving |= _tarPos.Count > 0 ? Moving.Parts : Moving.None;
            return ret;
        }

        /// <summary>
        /// モーションスクロールする位置を設定する
        /// </summary>
        /// <param name="pane">スクロールしたいペーン</param>
        /// <param name="pos">スクロール目的地</param>
        /// <param name="step">モーションステップ数 20位をセットするとちょうど良い</param>
        public void SetMotionScrollPosition(IRichPane pane, ScreenPos pos, int step)
        {
            lock (_motionScroll)
            {
                _motionScroll[pane] = new ScrollCommand(pos, step);
            }
        }

        /// <summary>
        /// モーションズームする値を設定する
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="zoom"></param>
        /// <param name="step"></param>
        public void SetMotionZoomValue(IRichPane pane, XyBase zoom, int step)
        {
            _motionZoom[pane] = new ZoomCommand(zoom, step);
        }
    }
}
