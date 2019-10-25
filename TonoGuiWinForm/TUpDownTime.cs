// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// cTimeUpDown �̊T�v�̐����ł��B
    /// </summary>
    public class TUpDownTime : System.Windows.Forms.UserControl
    {
        #region	����(�V���A���C�Y����)
        #endregion
        #region	����(�V���A���C�Y���Ȃ�)
        #endregion

        private TTextBoxTimeMask TiemText;
        private System.Windows.Forms.VScrollBar vScrollBar;
        private System.Windows.Forms.ComboBox comboBox_Day;

        /// <summary>
        /// �K�v�ȃf�U�C�i�ϐ��ł��B
        /// </summary>
        private readonly System.ComponentModel.Container components = null;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public TUpDownTime()
        {
            // ���̌Ăяo���́AWindows.Forms �t�H�[�� �f�U�C�i�ŕK�v�ł��B
            InitializeComponent();

            for (var i = 0; i < 7; i++)
            {
                comboBox_Day.Items.Add(DateTimeEx.GetDayString(i));
            }
        }

        /// <summary>
        /// �g�p����Ă��郊�\�[�X�Ɍ㏈�������s���܂��B
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region �R���|�[�l���g �f�U�C�i�Ő������ꂽ�R�[�h 
        /// <summary>
        /// �f�U�C�i �T�|�[�g�ɕK�v�ȃ��\�b�h�ł��B���̃��\�b�h�̓��e��
        /// �R�[�h �G�f�B�^�ŕύX���Ȃ��ł��������B
        /// </summary>
        private void InitializeComponent()
        {
            TiemText = new Tono.GuiWinForm.TTextBoxTimeMask();
            vScrollBar = new System.Windows.Forms.VScrollBar();
            comboBox_Day = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // TiemText
            // 
            TiemText.Anchor = (System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right);
            TiemText.Font = new System.Drawing.Font("Arial Black", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            TiemText.Location = new System.Drawing.Point(79, 0);
            TiemText.Name = "TiemText";
            TiemText.Size = new System.Drawing.Size(144, 30);
            TiemText.TabIndex = 1;
            TiemText.Text = "00:00:00";
            TiemText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // vScrollBar
            // 
            vScrollBar.Anchor = System.Windows.Forms.AnchorStyles.Right;
            vScrollBar.Location = new System.Drawing.Point(224, 0);
            vScrollBar.Name = "vScrollBar";
            vScrollBar.Size = new System.Drawing.Size(24, 30);
            vScrollBar.TabIndex = 2;
            vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(vScrollBar_Scroll);
            // 
            // comboBox_Day
            // 
            comboBox_Day.Anchor = System.Windows.Forms.AnchorStyles.Left;
            comboBox_Day.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            comboBox_Day.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox_Day.Font = new System.Drawing.Font("HGP�n�p�p�޼��UB", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 128);
            comboBox_Day.ImeMode = System.Windows.Forms.ImeMode.Off;
            comboBox_Day.ItemHeight = 24;
            comboBox_Day.Location = new System.Drawing.Point(0, 1);
            comboBox_Day.Name = "comboBox_Day";
            comboBox_Day.Size = new System.Drawing.Size(80, 30);
            comboBox_Day.TabIndex = 0;
            comboBox_Day.DrawItem += new System.Windows.Forms.DrawItemEventHandler(comboBox_Day_DrawItem);
            // 
            // cTimeUpDown
            // 
            Controls.Add(vScrollBar);
            Controls.Add(comboBox_Day);
            Controls.Add(TiemText);
            Font = new System.Drawing.Font("Arial Black", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            Name = "cTimeUpDown";
            Size = new System.Drawing.Size(248, 30);
            ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// �e�L�X�g�̎擾/�ݒ�
        /// </summary>
        public override string Text
        {
            get => TiemText.Text;
            set => TiemText.Text = value;
        }

        /// <summary>
        /// �\�������̎擾
        /// </summary>
        public DateTimeEx GetTime()
        {
            var t = TiemText.GetTime();
            return DateTimeEx.FromDHMS(comboBox_Day.SelectedIndex, t.Hour, t.Minute, t.Second);
        }
        /// <summary>
        /// �\�������̐ݒ�
        /// </summary>
        /// <param name="value">�\�����鎞��</param>
        public void SetTime(DateTimeEx value)
        {
            comboBox_Day.SelectedIndex = value.Day;
            TiemText.SetTime(value);
        }

        /// <summary>
        /// �X�N���[���o�[�̃C�x���g
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void vScrollBar_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            if (e.Type == System.Windows.Forms.ScrollEventType.EndScroll)
            {
                return;
            }
            var Delta = 0;
            var index = TiemText.SelectionStart;
            switch (index)
            {
                case 0:
                case 1:
                case 2: Delta = 60 * 60; break; // ��
                case 3:
                case 4:
                case 5: Delta = 60; break;      // ��
                case 6:
                case 7:
                case 8: Delta = 1; break;       // �b
                default: break;
            }
            var t = TiemText.GetTime();
            if (e.Type == System.Windows.Forms.ScrollEventType.LargeDecrement ||
                e.Type == System.Windows.Forms.ScrollEventType.SmallDecrement)
            {   // ���Ԃ��{����
                t.TotalSeconds += Delta;
            }
            else
                if (e.Type == System.Windows.Forms.ScrollEventType.LargeIncrement ||
                e.Type == System.Windows.Forms.ScrollEventType.SmallIncrement)
            {   // ���Ԃ��|����
                t.TotalSeconds -= Delta;
            }
            TiemText.SetTime(t);
            TiemText.SelectionStart = index;

        }

        private void comboBox_Day_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            e.DrawBackground();

            var cmb = (ComboBox)sender;

            var text = e.Index > -1 ? cmb.Items[e.Index].ToString() : cmb.Text;
            var ym = (e.Bounds.Height - e.Graphics.MeasureString(text, cmb.Font).Height) / 2;
            using (Brush brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(text, cmb.Font, brush, e.Bounds.X, e.Bounds.Y + ym);
            }
            e.DrawFocusRectangle();
        }
    }
}
