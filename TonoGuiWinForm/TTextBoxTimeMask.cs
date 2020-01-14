// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 時刻表示用にテキストをマスクして
    /// 特定のキーの入力を受け付けないテキストボックス
    /// </summary>
    public class TTextBoxTimeMask : TextBox
    {
        #region	属性(シリアライズする)
        #endregion
        #region	属性(シリアライズしない)
        /// <summary>表示する時間</summary>
        private DateTimeEx _time = new DateTimeEx();
        /// <summary>コピーを開始したテキスト位置</summary>
        private static int _CopyStartIndex = int.MaxValue;
        /// <summary>コピーした文字列</summary>
        private static string _CopyText = "";
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TTextBoxTimeMask() : base()
        {
        }

        /// <summary>
        /// キーダウンのイベント
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {   // BackSpaceもしくはDelete(Ctrl+BackSpace)は処理しない。
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        /// <summary>
        /// キープレスイベント(キーダウンイベントより後に発生する)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            // BackSpaceもしくはDelete(Ctrl+BackSpace)は処理しない。
            if (e.KeyChar == 0x8 || e.KeyChar == 0x7f)
            {
                e.Handled = true;
            }

            switch ((int)e.KeyChar)
            {   // char型をint型にキャストするとASCII文字コードに変換できる
                case 0x30:          // 0
                case 0x31:          // 1
                case 0x32:          // 2
                case 0x33:          // 3
                case 0x34:          // 4
                case 0x35:          // 5
                case 0x36:          // 6
                case 0x37:          // 7
                case 0x38:          // 8
                case 0x39:          // 9
                                    // もし、選択範囲に":"が含まれていたら、文字列は書き換えない。
                    if (SelectedText.IndexOf(":", 0, SelectedText.Length) >= 0)
                    {
                        e.Handled = true;
                    }

                    var Sindex = 0;
                    var s = Text.Split(new char[] { ':' });
                    var text = Text + ":";
                    var count = 0;
                    if (SelectionStart > 7)
                    {   // 全体で７桁以上の表示はありえない
                        e.Handled = true;
                    }
                    else
                    {
                        while (text.IndexOf(":", Sindex) >= 0)
                        {
                            var Eindex = text.IndexOf(":", Sindex);
                            if (Sindex <= SelectionStart && SelectionStart <= Eindex)
                            {
                                var t = s[count];
                                if (SelectionLength == 0)
                                {
                                    if (t.Length >= 2)
                                    {   // 各単位で３桁以上の表示はしない
                                        e.Handled = true;
                                    }
                                }
                                if (e.Handled == false)
                                {   // 入力値が各単位の上限(時:23 分、秒:59)を超えていた場合、入力を無効にする
                                    var after = Text.Remove(SelectionStart, SelectionLength);
                                    after = after.Insert(SelectionStart, e.KeyChar.ToString());
                                    var tmp = after.Split(new char[] { ':' });
                                    if (count == 0 && int.Parse(tmp[count]) > 23)
                                    {
                                        e.Handled = true;
                                    }

                                    if (count > 0 && int.Parse(tmp[count]) > 59)
                                    {
                                        e.Handled = true;
                                    }
                                }
                            }
                            Sindex = Eindex + 1;
                            count++;
                        }
                    }
                    break;
                case 0x3:   // Ctrl + C コピーは許可する
                    if (SelectionLength == 0)
                    {
                        e.Handled = true;
                        _CopyStartIndex = int.MaxValue;
                        _CopyText = "";
                        break;
                    }
                    _CopyStartIndex = SelectionStart;
                    _CopyText = SelectedText;
                    break;
                case 0x16:  // Ctrl + V ペーストは許可する
                    var str = (string)System.Windows.Forms.Clipboard.GetDataObject().GetData(typeof(string));
                    if (SelectionLength == 0 ||
                        (SelectionStart != _CopyStartIndex || SelectionLength != _CopyText.Length || _CopyText != str))
                    {   // 他のアプリ(メモ帳やエクセル)からのコピーは受け付けないようにする
                        e.Handled = true;
                    }
                    break;
                default: e.Handled = true; break;   // それ以外
            }

            base.OnKeyPress(e);
        }

        /// <summary>
        /// テキストが変更された時に呼び出されます
        /// </summary>
        /// <param name="e"></param>
        protected override void OnTextChanged(EventArgs e)
        {
            var s = Text.Split(new char[] { ':' });

            if (s.Length != 3)
            {
                return;
            }

            _time = DateTimeEx.FromDHMS(0, int.Parse(s[0]), int.Parse(s[1]), int.Parse(s[2]));
            base.OnTextChanged(e);
        }

        /// <summary>
        /// 時間の取得
        /// </summary>
        public DateTimeEx GetTime()
        {
            return DateTimeEx.FromDHMS(0, _time.Hour, _time.Minute, _time.Second);
        }
        /// <summary>
        /// 表示時刻の設定
        /// </summary>
        /// <param name="value">表示する時刻</param>
        public void SetTime(DateTimeEx value)
        {
            // 日は切り捨てで、時刻のみを所持する
            _time = DateTimeEx.FromDHMS(0, value.Hour, value.Minute, value.Second);
            Text = _time.Hour.ToString("00") + ":" + _time.Minute.ToString("00") + ":" + _time.Second.ToString("00");
        }
    }
}
