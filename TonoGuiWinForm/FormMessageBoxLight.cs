// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public partial class FormMessageBoxLight : Form
    {
        public FormMessageBoxLight()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            textMessage.Text = _message;
        }

        private string _message = "";

        /// <summary>
        /// メッセージに表示しているテキスト
        /// </summary>
        public string Message
        {
            get => _message;
            set => _message = value;
        }

        private static readonly Dictionary<string, int> _showing = new Dictionary<string, int>();
        private static readonly ConfigRegister _conf = new ConfigRegister("Tono.GuiWinForm\\MessageBoxLight");


        /// <summary>
        /// マウスの近くにメッセージを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void Show(string message, string title)
        {
            var x = Form.MousePosition.X + 8;
            var y = Form.MousePosition.Y + 8;
            Show(message, title, x, y);
        }

        /// <summary>
        /// マウスの近くにメッセージを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void Show(string message, string title, Control pos)
        {
            var p = pos.PointToScreen(new Point(pos.Right, pos.Bottom));
            Show(message, title, p.X + 8, p.Y + 8);
        }

        /// <summary>
        /// 指定位置にメッセージを表示する
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static void Show(string message, string title, int x, int y)
        {
            var mes = new FormMessageBoxLight
            {
                Text = title,
                Message = message
            };
            var dontShow = false;
            var key = mes.getKey();

            if (_showing.ContainsKey(key))
            {
                mes.Dispose();
                return;
            }
            try
            {
                var n = (int)_conf[key, 0];
                dontShow = (n == 1);
            }
            catch (Exception)
            {
            }
            if (dontShow)
            {
                mes.Dispose();
                return;
            }
            mes.Location = new Point(x, y);

            // ウィンドウからはみ出ないようにする
            var ovh = mes.Bottom - SystemInformation.WorkingArea.Bottom;
            if (ovh > 0)
            {
                mes.Location = new Point(mes.Location.X, mes.Location.Y - ovh);
            }
            var ovw = mes.Right - SystemInformation.WorkingArea.Right;
            if (ovw > 0)
            {
                mes.Location = new Point(mes.Location.X - ovw, mes.Location.Y);
            }

            // 表示
            mes.FormClosing += new FormClosingEventHandler(mes_FormClosing);
            _showing[key] = 1;
            mes.Show(Form.ActiveForm);
        }

        private static void mes_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sender is FormMessageBoxLight mes)
            {
                var key = mes.getKey();
                _showing.Remove(key);
            }
        }

        /// <summary>
        /// キーを生成する（MessageとTextプロパティを元に作成する）
        /// </summary>
        /// <returns></returns>
        private string getKey()
        {
            var ret = Text.Replace(' ', '.') + "@" + Message.Replace(' ', '.');
            ret = ret.Replace('\r', '.');
            ret = ret.Replace("\n", "");
            return "DontShow/" + ret;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var key = getKey();
            _conf[key] = checkBoxNoNext.Checked ? 1 : 0;
            try
            {
                _showing.Remove(key);
            }
            catch (Exception)
            {
            }
            base.OnClosing(e);
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Close();
            Dispose();
        }
    }
}