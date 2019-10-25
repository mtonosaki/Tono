// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ドラッグ＆ドロップイベントの属性を表現するクラス
    /// </summary>
    public class DragState : Tono.GuiWinForm.MouseState
    {
        #region	属性(シリアライズする)
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>ドロップされたファイルパス</summary>
        public string[] filepath;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DragState()
        {
        }

        /// <summary>
        /// マウスイベント属性からインスタンスを生成する
        /// </summary>
        /// <param name="e">マウスイベント属性</param>
        /// <returns>新しいインスタンス</returns>
        public static DragState FromDragEventArgs(System.Windows.Forms.DragEventArgs e, IRichPane posPane)
        {
            var ret = new DragState();
            ret.Pos.X = e.X;
            ret.Pos.Y = e.Y;
            ret.Delta.X = 0;
            ret.Delta.Y = 0;
            ret._paneAtPos = posPane;
            ret.Attr.IsButton = (e.KeyState == 1);
            ret.Attr.IsButtonMiddle = (e.KeyState == 16);
            ret.Attr.IsCtrl = (e.KeyState == 8);
            ret.Attr.IsShift = (e.KeyState == 4);
            ret.filepath = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            return ret;
        }

    }
}
