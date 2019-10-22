// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// マウスイベントの属性を表現するクラス
    /// </summary>
    public class MouseState
    {
        private static MouseState _contextSave = new MouseState();
        internal static void saveContext(MouseEventArgs e, IRichPane pane)
        {
            _contextSave = FromMouseEventArgs(e, pane);
            _contextSave.Attr.IsShift = (Form.ModifierKeys & Keys.Shift) != 0;
            _contextSave.Attr.IsCtrl = (Form.ModifierKeys & Keys.Control) != 0;
        }
        /// <summary>
        /// コンテキストメニューを表示する直前の状態
        /// </summary>
        public static MouseState StateAtContext => _contextSave;
        #region Buttons クラス
        public class Buttons : ICloneable
        {
            /// <summary>マウスボタンの押下状態</summary>
            public bool IsButton;

            /// <summary>マウス中心ボタンの押下状態</summary>
            public bool IsButtonMiddle;

            /// <summary>ダブルクリック状態</summary>
            public bool IsDoubleClick;

            /// <summary>CTRLキーの押下状態</summary>
            public bool IsCtrl;

            /// <summary>SHIFTキーの押下状態</summary>
            public bool IsShift;

            /// <summary>
            /// デフォルトコンストラクタ
            /// </summary>
            public Buttons()
            {
                IsButton = false;
                IsButtonMiddle = false;
                IsDoubleClick = false;
                IsCtrl = false;
                IsShift = false;
            }

            /// <summary>
            /// 初期化コンストラクタ
            /// </summary>
            /// <param name="button">マウスボタンの状態「</param>
            /// <param name="middle">マウス中央ボタンの状態</param>
            /// <param name="ctrl">CTRLキーの状態</param>
            /// <param name="shift">SHIFTキーの状態</param>
            public Buttons(bool button, bool middle, bool ctrl, bool shift, bool doubleclick)
            {
                IsShift = shift;
                IsButton = button;
                IsButtonMiddle = middle;
                IsCtrl = ctrl;
                IsDoubleClick = doubleclick;
            }

            public override string ToString()
            {
                var s = "";
                if (IsButton)
                {
                    s += "<L>";
                }

                if (IsButtonMiddle)
                {
                    s += "<M>";
                }

                if (IsDoubleClick)
                {
                    s += "<D>";
                }

                if (IsCtrl)
                {
                    s += "[CTRL]";
                }

                if (IsShift)
                {
                    s += "[SHIFT]";
                }

                if (string.IsNullOrEmpty(s))
                {
                    s = "(no buttons)";
                }
                return s;
            }


            /// <summary>
            /// MouseEventArgsでは構築し切れなかったフラグを埋め合わせる
            /// </summary>
            /// <param name="value">埋め合わせに使う情報（主にキーの状態）</param>
            public void SetKeyFrags(Buttons value)
            {
                IsCtrl = value.IsCtrl;
                IsShift = value.IsShift;
            }

            /// <summary>
            /// MouseEventArgsでは構築し切れなかったフラグを埋め合わせる
            /// </summary>
            /// <param name="value">埋め合わせに使う情報（主にキーの状態）</param>
            public void SetKeyFrags(KeyState value)
            {
                IsCtrl = value.IsControl;
                IsShift = value.IsShift;
            }

            /// <summary>
            /// 指定したビットがtrueの項目を falseにする（Upイベントで対象をfalseにする時に使用する）
            /// </summary>
            /// <param name="value">falseにしたいビットをセットしておいたButtons</param>
            public void ResetKeyFlags(Buttons value)
            {
                if (value.IsButton)
                {
                    IsButton = false;
                }

                if (value.IsButtonMiddle)
                {
                    IsButtonMiddle = false;
                }

                if (value.IsDoubleClick)
                {
                    IsDoubleClick = false;
                }

                if (value.IsCtrl)
                {
                    IsCtrl = false;
                }

                if (value.IsShift)
                {
                    IsShift = false;
                }
            }

            /// <summary>
            /// 指定したボタンすべてが一致しているかどうかを調べる
            /// </summary>
            /// <param name="value">調べるボタン</param>
            /// <returns>true = 一致 / false = 不一致</returns>
            public override bool Equals(object value)
            {
                if (value is MouseState.Buttons)
                {
                    return GetHashCode() == value.GetHashCode();
                }
                return false;
            }

            /// <summary>
            /// インスタンスの状態を表すハッシュコード
            /// </summary>
            /// <returns>ハッシュコード</returns>
            public override int GetHashCode()
            {
                var ret = 0;
                if (IsButton)
                {
                    ret |= 0x01;
                }

                if (IsButtonMiddle)
                {
                    ret |= 0x02;
                }

                if (IsCtrl)
                {
                    ret |= 0x04;
                }

                if (IsShift)
                {
                    ret |= 0x08;
                }

                if (IsDoubleClick)
                {
                    ret |= 0x10;
                }

                return ret;
            }
            #region ICloneable メンバ

            public object Clone()
            {
                return new Buttons(IsButton, IsButtonMiddle, IsCtrl, IsShift, IsDoubleClick);
            }

            #endregion
        }
        #endregion

        #region 属性（シリアライズが必要）

        /// <summary>マウススクリーン座標</summary>
        public ScreenPos Pos = new ScreenPos();
        /// <summary>ホイールの移動量</summary>
        public XyBase Delta = new XyBase();
        /// <summary>ボタン状況</summary>
        public Buttons Attr = new Buttons();
        /// <summary>クリックされたペーン</summary>
        protected IRichPane _paneAtPos = null;

        #endregion

        #region Data tips for debugging
#if DEBUG
        public string _ => ToString();
#endif
        #endregion

        private static ScreenPos _nowPos = null;
        private static Buttons _nowButtons = null;

        /// <summary>
        /// インスタンスの複製
        /// </summary>
        /// <returns></returns>
        public MouseState Clone()
        {
            var ret = new MouseState
            {
                Pos = (ScreenPos)Pos.Clone(),
                Delta = Delta,
                Attr = (Buttons)Attr.Clone(),
                _paneAtPos = _paneAtPos
            };
            return ret;
        }

        /// <summary>
        /// 現在の位置とボタンを反映するインスタンス（
        /// </summary>
        public static MouseState Now
        {
            get
            {
                var ret = new MouseState
                {
                    Attr = NowButtons,
                    Pos = NowPosition,
                    Pane = null
                };
                return ret;
            }
        }

        /// <summary>
        /// 現在の状態（ボタン）
        /// </summary>
        public static Buttons NowButtons
        {
            get
            {
                if (_nowButtons == null)
                {
                    for (; ; Thread.Sleep(0))   // .NETがスレッドセーフで例外を出す時、自動的に再試行する
                    {
                        try
                        {
                            _nowButtons = new Buttons
                            {
                                IsButton = (Control.MouseButtons & MouseButtons.Left) != 0 ? true : false,
                                IsButtonMiddle = (Control.MouseButtons & MouseButtons.Middle) != 0 ? true : false,
                                IsCtrl = (Form.ModifierKeys & Keys.Control) != 0 ? true : false,
                                IsShift = (Form.ModifierKeys & Keys.Shift) != 0 ? true : false,
                                IsDoubleClick = false
                            };
                            break;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                var ret = _nowButtons;  // Tono:NowButtonsが更新するように修正（レコーダ系で正しく動作するかは未検証）
                _nowButtons = null;
                return ret;
            }
        }

        /// <summary>現在の状態</summary>
        public static ScreenPos NowPosition
        {
            get
            {
                for (; ; Thread.Sleep(0))   // .NETがスレッドセーフで例外を出す時、自動的に再試行する
                {
                    try
                    {
                        if (_nowPos == null)
                        {
                            return ScreenPos.FromInt(Control.MousePosition.X, Control.MousePosition.Y);
                        }
                        else
                        {
                            return (ScreenPos)_nowPos.Clone();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            set => _nowPos = value;
        }

        /// <summary>
        /// インスタンスの文字列化
        /// </summary>
        /// <returns>文字列</returns>
        public override string ToString()
        {
            return Pos.ToString() + " " + "D" + Delta.ToString() + " " + Attr.ToString() + (_paneAtPos != null ? " at " + _paneAtPos.IdText : "");
        }


        /// <summary>
        /// クリックしたところのペーン
        /// </summary>
        public IRichPane Pane
        {
            get => _paneAtPos;
            set => _paneAtPos = value;
        }

        /// <summary>
        /// マウス座標 0 / ボタンフリー状態のオブジェクト / Pane = null
        /// </summary>
        public static MouseState FreeZero
        {
            get
            {
                var ret = new MouseState();
                ret.Attr.IsButton = false;
                ret.Attr.IsButtonMiddle = false;
                ret.Attr.IsCtrl = false;
                ret.Attr.IsDoubleClick = false;
                ret.Attr.IsShift = false;
                ret.Delta = XyBase.FromInt(0, 0);
                ret.Pos = ScreenPos.FromInt(0, 0);
                ret._paneAtPos = null;
                return ret;
            }
        }

        /// <summary>
        /// マウスイベント属性からインスタンスを生成する
        /// </summary>
        /// <param name="e">マウスイベント属性</param>
        /// <returns>新しいインスタンス</returns>
        public static MouseState FromMouseEventArgs(System.Windows.Forms.MouseEventArgs e, IRichPane posPane)
        {
            var ret = new MouseState();
            ret.Pos.X = e.X;
            ret.Pos.Y = e.Y;
            ret.Delta.X = 0;
            ret.Delta.Y = e.Delta;
            ret._paneAtPos = posPane;
            ret.Attr.IsButton = (e.Button == System.Windows.Forms.MouseButtons.Left);
            ret.Attr.IsButtonMiddle = (e.Button == System.Windows.Forms.MouseButtons.Middle);
            if (e.Clicks > 1)
            {
                ret.Attr.IsDoubleClick = true;
            }

            ret.Attr.IsCtrl = false;
            ret.Attr.IsShift = false;
            return ret;
        }
    }
}
