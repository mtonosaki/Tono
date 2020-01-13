// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    internal class FeatureTextInput : FeatureBase, IMouseListener, IKeyListener, ITokenListener
    {
        #region 属性（シリアライズしない）
        /// <summary>選択中のパーツ（共有変数）</summary>
        [NonSerialized]
        protected PartsCollectionBase _selectedParts;
        [NonSerialized]
        private TextBox textBox1 = null;
        [NonSerialized]
        private static readonly NamedId _token = NamedId.FromName("TokenFontChanged");
        /// <summary>選択中のパーツという意味でシリアライズするID</summary>
        [NonSerialized]
        protected NamedId _meansSelectedParts = NamedId.FromName("FeatureDataSerializeID");
        #endregion

        public override void Start(NamedId who)
        {
            base.Start(who);
            if (_token.Equals(who))
            {
                if (textBox1 != null && _tarParts != null)
                {
                    textBox1.Font = _tarParts.LastFont;
                }
            }
        }

        public override void OnInitInstance()
        {
            base.OnInitInstance();

            // ステータス同期
            _selectedParts = (PartsCollectionBase)Share.Get("SelectedParts", typeof(PartsCollection));   // 選択済みのパーツ一覧
        }

        private PartsTextBase _tarParts = null;
        private IRichPane _tarPane;

        private void editText(PartsTextBase parts, IRichPane rp)
        {
            _tarParts = parts;
            _tarPane = rp;
            var sr = parts.GetScRect(rp);
            sr.Deflate(XyBase.FromInt(5, 2));
            sr -= XyBase.FromInt(2, 0);
            textBox1 = new TextBox
            {
                BackColor = ColorUtil.DeleteAlpha(parts.BackColor)
            };
            if (ColorUtil.IsNearColor(textBox1.BackColor, ColorUtil.DeleteAlpha(parts.FontColor)))
            {
                textBox1.ForeColor = ColorUtil.GetNegativeColor(textBox1.BackColor);
            }
            else
            {
                textBox1.ForeColor = parts.FontColor;
            }
            if (parts.Text == Mes.Current["RequestInput.Short"] || parts.Text == Mes.Current["RequestInput"])
            {
                textBox1.Text = "";
            }
            else
            {
                textBox1.Text = parts.Text;
            }
            textBox1.Multiline = true; // parts.IsMultiLine;
            textBox1.AcceptsReturn = parts.IsMultiLine;
            textBox1.ImeMode = parts.ImeMode;
            textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            textBox1.Location = sr.LT;
            textBox1.Name = "textBox1";
            textBox1.KeyDown += new KeyEventHandler(textBox1_KeyDown);
            textBox1.Font = parts.LastFont;
            textBox1.Size = new System.Drawing.Size(sr.Width, sr.Height);
            textBox1.TabIndex = 1;
            textBox1.TextAlign = HorizontalAlignment.Left;
            Pane.Control.Controls.Add(textBox1);
            textBox1.Focus();
            textBox1.Select(0, textBox1.Text.Length);
            TKeyEnabler.Enable = false;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                endInput(false);
            }
            if (e.KeyCode == Keys.Escape)
            {
                endInput(true);
            }
        }

        #region IMouseListener メンバ

        public void OnMouseMove(MouseState e)
        {
        }

        private void endInput(bool isCancel)
        {
            if (textBox1 != null)
            {
                // 適用
                var toUpdate = false;
                if (isCancel == false)
                {
                    toUpdate = true;
                    if (_tarParts.Text == Mes.Current["RequestInput.Short"] || _tarParts.Text == Mes.Current["RequestInput"])
                    {
                        if (string.IsNullOrEmpty(textBox1.Text))
                        {
                            toUpdate = false;
                        }
                    }
                }
                if (toUpdate)
                {
                    Persister[REDO].StartChunk(GetType().Name);
                    Persister[UNDO].StartChunk(GetType().Name);
                    Persister[UNDO].Save(_tarParts, _meansSelectedParts);

                    _tarParts.Text = textBox1.Text; // テキスト変更
                    Persister[REDO].Save(_tarParts, _meansSelectedParts);

                    Persister[REDO].EndChunk(); // REDOを先に行うことは重要
                    Persister[UNDO].EndChunk();

                    Data.SetModified();
                }
                // 入力解除
                Pane.Control.Controls.Remove(textBox1);
                textBox1.Dispose();
                Parts.Invalidate(_tarParts, _tarPane);
                textBox1 = null;
                TKeyEnabler.Enable = true;
            }
        }

        private void startInput()
        {
            if (_selectedParts.Count == 1)
            {
                foreach (PartsCollectionBase.PartsEntry pe in _selectedParts)
                {
                    if (pe.Parts is PartsTextBase)
                    {
                        editText((PartsTextBase)pe.Parts, pe.Pane);
                    }
                }
            }
        }

        public void OnMouseDown(MouseState e)
        {
            endInput(false);
            if (e.Attr.IsDoubleClick)
            {
                startInput();
            }
        }

        public void OnMouseUp(MouseState e)
        {
        }

        public void OnMouseWheel(MouseState e)
        {
        }

        #endregion
        #region ITokenListener メンバ

        public NamedId TokenTriggerID => _token;

        #endregion

        #region IKeyListener メンバ

        public void OnKeyDown(KeyState e)
        {
            if (e.Key == Keys.F2 && !e.IsControl && !e.IsShift)
            {
                startInput();
            }
            if (e.Key == Keys.Escape)
            {
                endInput(true);
            }
        }

        public void OnKeyUp(KeyState e)
        {
        }

        #endregion
    }
}
