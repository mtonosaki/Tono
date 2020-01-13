// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// TODO表示用テキスト
    /// </summary>
    public class TodoCaption
    {
        /// <summary>
        /// メニュー表示用文字列
        /// </summary>
        public string Caption;

        /// <summary>
        /// 説明用文字列
        /// </summary>
        public string Remarks;

        /// <summary>
        /// 初期化構築子
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="remarks"></param>
        public TodoCaption(string caption, string remarks)
        {
            Caption = caption;
            Remarks = remarks;
        }
    }

    /// <summary>
    /// TODOを実装するもの
    /// </summary>
    public interface ITodoIntent
    {
        /// <summary>
        /// メニュー表示用文字列を設定する
        /// </summary>
        /// <param name="key">識別子</param>
        /// <returns>文字列</returns>
        TodoCaption GetTodoCaption(string key);

        /// <summary>
        /// 補助機能を実行する
        /// </summary>
        /// <param name="key">識別子</param>
        /// <returns>true=意図どおり完了 / false=失敗</returns>
        bool DoAssist(string key);
    }

    /// <summary>
    /// TODOアイテムを管理する
    /// </summary>
    public static class TODO
    {
        /// <summary>
        /// キーとインテントのペア
        /// </summary>
        private class KeyIntentPair
        {
            public string Key;
            public ITodoIntent Intent;

            public KeyIntentPair(string key, ITodoIntent intent)
            {
                Key = key;
                Intent = intent;
            }
        }

        /// <summary>
        /// TODOを表示・操作するコントロールを保存
        /// </summary>
        private static ToolStripDropDownButton _tar = null;

        /// <summary>
        /// TODO管理するドロップダウンボタンの親を登録する
        /// </summary>
        /// <param name="btn"></param>
        public static void SetTodoButton(ToolStripDropDownButton btn)
        {
            _tar = btn;
            _tar.DropDownOpening += new EventHandler(_tar_DropDownOpening);
        }

        /// <summary>
        /// メニューを開いたときに、TODOキャプションを交換する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void _tar_DropDownOpening(object sender, EventArgs e)
        {
            foreach (ToolStripItem tsi in _tar.DropDownItems)
            {
                if (tsi.Tag is KeyIntentPair ki)
                {
                    var tc = ki.Intent.GetTodoCaption(ki.Key);
                    tsi.Text = tc.Caption;
                    tsi.ToolTipText = tc.Remarks;
                    tsi.Width = tsi.Text.Length * 12;
                }
            }
        }

        /// <summary>
        /// TODOにひとつ追加する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="intent"></param>
        /// <returns></returns>
        public static void Add(string key, ITodoIntent intent)
        {
            // 同じ識別子が複数登録されたら、後のほうをキャンセルする
            foreach (ToolStripItem tsi in _tar.DropDownItems)
            {
                if (tsi.Tag is KeyIntentPair ki)
                {
                    if (key == ki.Key)
                    {
                        return;
                    }
                }
            }
            var db = new ToolStripDropDownButton(key)
            {
                Tag = new KeyIntentPair(key, intent),
                AutoSize = true,
                AutoToolTip = true,
                Alignment = ToolStripItemAlignment.Left,
                AllowDrop = false,
                Available = true,
                DisplayStyle = ToolStripItemDisplayStyle.Text,
                DoubleClickEnabled = false,
                Enabled = true,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                TextDirection = ToolStripTextDirection.Inherit,
                TextImageRelation = TextImageRelation.ImageBeforeText
            };
            db.Click += new EventHandler(todo_assist_click);

            _tar.DropDownItems.Add(db);
            _tar.Enabled = true;
            _tar.BackColor = Color.Yellow;
        }

        /// <summary>
        /// TODOアシストボタンがクリックされた
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void todo_assist_click(object sender, EventArgs e)
        {
            var db = (ToolStripDropDownButton)sender;
            if (db.Tag is KeyIntentPair ki)
            {
                ki.Intent.DoAssist(ki.Key);
            }
        }

        /// <summary>
        /// 終了してTODOリストから消す
        /// </summary>
        /// <param name="key">識別子</param>
        /// <returns></returns>
        public static void Finish(string key)
        {
            foreach (ToolStripItem tsi in _tar.DropDownItems)
            {
                if (tsi.Tag is KeyIntentPair ki)
                {
                    if (ki.Key == key)
                    {
                        _tar.DropDownItems.Remove(tsi);
                        break;
                    }
                }
            }
            if (_tar.DropDownItems.Count == 0)
            {
                _tar.BackColor = SystemColors.Control;
                _tar.Enabled = false;
            }
        }
    }
}
