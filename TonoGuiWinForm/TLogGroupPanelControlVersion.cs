// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
	public partial class TLogGroupPanelControlVersion : UserControl
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TLogGroupPanelControlVersion()
        {
            InitializeComponent();
            LOG.LogClearRequested += new EventHandler<EventArgs>(LOG_LogClearRequested);
        }

        /// <summary>
        /// ログクリア後の処理要求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LOG_LogClearRequested(object sender, EventArgs e)
        {
            listViewLogView.Items.Clear();
        }

        /// <summary>
        /// ログ表示更新タイマー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerLog_Tick(object sender, EventArgs e)
        {
            if (LOG.CheckAndClearRequestFlag())
            {
                var inspos = listViewLogView.Items.Count;
                var lastseq = inspos == 0 ? -1 : (int)listViewLogView.Items[listViewLogView.Items.Count - 1].Tag;
                LOG.GetCurrent();
                var lu = LOG.GetCurrentLast();

                while (lu != null && lu.Value.Seq > lastseq)
                {
                    string type;
                    switch (lu.Value.Level)
                    {
                        case LLV.WAR: type = "w"; break;
                        case LLV.ERR: type = "e"; break;
                        case LLV.DEV: type = "d"; break;
                        case LLV.INF: type = "i"; break;
                        default: type = "?"; break;
                    }
                    //var type = lu.Value.Level switch
                    //{
                    //    LLV.WAR => "w",
                    //    LLV.ERR => "e",
                    //    LLV.DEV => "d",
                    //    LLV.INF => "i",
                    //    _ => "?",
                    //};
                    var lvi = new ListViewItem(type);
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, DateTime.Now.ToString())).Tag = DateTime.Now;
                    lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, lu.Value.Mes));
                    listViewLogView.Items.Insert(inspos, lvi);
                    lvi.Tag = lu.Value.Seq;
                    if (lu.Value.Level == LLV.ERR)
                    {
                        lvi.BackColor = Color.FromArgb(255, 224, 192);
                    }
                    if (lu.Value.Level == LLV.WAR)
                    {
                        lvi.BackColor = Color.FromArgb(255, 255, 224);
                    }

                    lu = lu.Previous;

                    if (listViewLogView.Items.Count > 10000)    // 多すぎるログは、過去から消して行く
                    {
                        listViewLogView.Items.RemoveAt(0);
                    }
                }
                if (checkBoxAutoScroll.Checked && listViewLogView.Items.Count > 0)
                {
                    listViewLogView.EnsureVisible(listViewLogView.Items.Count - 1);
                }
            }
        }
        private void checkBoxLogInfo_CheckedChanged(object sender, EventArgs e)
        {
            LOG.ChangeDispLevel(LLV.INF, ((CheckBox)sender).Checked);
            listViewLogView.Items.Clear();
        }

        private void checkBoxLogWar_CheckedChanged(object sender, EventArgs e)
        {
            LOG.ChangeDispLevel(LLV.WAR, ((CheckBox)sender).Checked);
            listViewLogView.Items.Clear();
        }

        private void checkBoxLogErr_CheckedChanged(object sender, EventArgs e)
        {
            LOG.ChangeDispLevel(LLV.ERR, ((CheckBox)sender).Checked);
            listViewLogView.Items.Clear();
        }

        private void checkBoxLogDev_CheckedChanged(object sender, EventArgs e)
        {
            LOG.ChangeDispLevel(LLV.DEV, ((CheckBox)sender).Checked);
            listViewLogView.Items.Clear();
        }

        private void buttonNotepad_Click(object sender, EventArgs e)
        {
            var fname = Path.GetTempFileName();
            File.WriteAllText(fname, LOG.GetCurrent());
            Process.Start("notepad.exe", fname);
        }
    }
}
