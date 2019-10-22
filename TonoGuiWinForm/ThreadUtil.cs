// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Drawing;
//using System.Collections.Generic;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// 
    /// </summary>
    public class ThreadUtil
    {
        private delegate void DisposeControlCallback(Control c);

        private delegate void SelectTabCallback(TabControl tar, TabPage page);
        private delegate void SetEnabledCallback(Control tar, bool sw);
        private delegate void SetVisibleCallback(Control tar, bool sw);
        private delegate void SetCheckedCallback(CheckBox tar, bool sw);
        private delegate bool GetCheckedCCallback(CheckBox tar);
        private delegate bool GetVisibleCallback(Control c);
        private delegate Point PointToClientCallback(Control c, Point scpos);
        private delegate int GetWidthControlCallback(Control c);
        private delegate void SetWidthControlCallback(Control c, int w);
        private delegate void SetButtonImageCallback(Button c, Image str);
        private delegate void SetTextCallback(Control c, string str);
        private delegate void SetListViewItemTextCallback(ListViewItem li, int subitemid, string text);
        private delegate string GetTextCallback(Control c);
        private delegate IntPtr GetHandleControlCallback(Control c);
        private delegate Cursor GetCursorControlCallback(Control c);
        private delegate void SetCursorControlCallback(Control c, Cursor cursor);
        private delegate void AddControlToControlsCallback(Control parent, Control newControl);
        private delegate void RemoveControlFromControlsCallback(Control parent, Control newControl);
        private delegate void FocusControlCallback(Control c);
        private delegate bool GetListViewItemSelectedCallback(ListView lv, int itemID);
        private delegate void SetListViewItemSelectedCallback(ListView lv, int itemID, bool sw);
        private delegate void GetItemsCallback(ListView lv, IList ret);
        private delegate void GetSelectedItemsCallback(ListView lv, IList ret);
        private delegate void SortListViewCallback(ListView lv, IComparer sorter);
        private delegate void ClearListViewSelectedItemsCallback(ListView lv);
        private delegate void ClearListViewItemsCallback(ListView lv);
        private delegate void GetListViewSelectedIndicesCallback(ListView lv, IList ret);
        private delegate void AddRangeAndSortListViewItemCallback(ListView lv, ListViewItem[] items, IComparer defaultsorter);
        private delegate void AddItemToListViewCallback(ListView lv, ListViewItem li);
        private delegate void AddItemsToListViewCallback(ListView lv, ListViewItem[] lis);
        private delegate void EnsureVisibleListViewCallback(ListView lv, int li);

        private delegate void SetSelectTextBoxCallback(TextBox c, int s, int e);



        /// <summary>
        /// Control.Disposeをスレッドセーフで行う
        /// </summary>
        public void DisposeControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new DisposeControlCallback(DisposeControl);
                c.Invoke(d, new object[] { c });
            }
            else
            {
                c.Dispose();
            }
        }

        /// <summary>
        /// TextBox.SetSelectTextBoxをスレッドセーフで行う
        /// </summary>
        /// <param name="c"></param>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public void SetSelectTextBox(TextBox c, int s, int e)
        {
            if (c.InvokeRequired)
            {
                var d = new SetSelectTextBoxCallback(SetSelectTextBox);
                c.Invoke(d, new object[] { c, s, e });
            }
            else
            {
                c.Select(s, e);
            }
        }

        /// <summary>
        /// リストビューのSelectedIndicesをスレッドセーフで行う
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="ret"></param>
        public void GetListViewSelectedIndices(ListView lv, IList ret)
        {
            if (lv.InvokeRequired)
            {
                var d = new GetListViewSelectedIndicesCallback(GetListViewSelectedIndices);
                lv.Invoke(d, new object[] { lv, ret });
            }
            else
            {
                ret.Clear();
                foreach (var li in lv.SelectedIndices)
                {
                    ret.Add(li);
                }
            }
        }

        /// <summary>
        /// リストビューのSelectedItemをスレッドセーフで行う
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="ret"></param>
        public void GetListViewSelectedItems(ListView lv, IList ret)
        {
            if (lv.InvokeRequired)
            {
                var d = new GetSelectedItemsCallback(GetListViewSelectedItems);
                lv.Invoke(d, new object[] { lv, ret });
            }
            else
            {
                ret.Clear();
                foreach (var li in lv.SelectedItems)
                {
                    ret.Add(li);
                }
            }
        }


        /// <summary>
        /// リストビューのItemsをスレッドセーフで行う
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="itemID"></param>
        /// <param name="sw"></param>
        public void SetListViewItemSelected(ListView lv, int itemID, bool sw)
        {
            if (lv.InvokeRequired)
            {
                var d = new SetListViewItemSelectedCallback(SetListViewItemSelected);
                lv.Invoke(d, new object[] { lv, itemID, sw });
            }
            else
            {
                lv.Items[itemID].Selected = sw;
            }
        }

        /// <summary>
        /// リストビューのItemsをスレッドセーフで行う
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="itemID"></param>
        public bool GetListViewItemSelected(ListView lv, int itemID)
        {
            if (lv.InvokeRequired)
            {
                var d = new GetListViewItemSelectedCallback(GetListViewItemSelected);
                return (bool)lv.Invoke(d, new object[] { lv, itemID });
            }
            else
            {
                return lv.Items[itemID].Selected;
            }
        }

        /// <summary>
        /// リストビューのItemsをスレッドセーフで行う
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="ret"></param>
        public void GetListViewItems(ListView lv, IList ret)
        {
            if (lv.InvokeRequired)
            {
                var d = new GetItemsCallback(GetListViewItems);
                lv.Invoke(d, new object[] { lv, ret });
            }
            else
            {
                ret.Clear();
                foreach (var li in lv.Items)
                {
                    ret.Add(li);
                }
            }
        }

        /// <summary>
        /// コントロールのCursorをスレッドセーフで変更
        /// </summary>
        /// <param name="c">コントロール</param>
        public void FocusControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new FocusControlCallback(FocusControl);
                c.Invoke(d, new object[] { c });
            }
            else
            {
                c.Focus();
            }
        }

        /// <summary>
        /// コントロールに子コントロールをスレッドセーフで削除
        /// </summary>
        /// <param name="parent">コントロール</param>
        /// <param name="oldControl">子コントロール</param>
        public void RemoveControlFromControls(Control parent, Control oldControl)
        {
            if (parent.InvokeRequired)
            {
                var d = new RemoveControlFromControlsCallback(RemoveControlFromControls);
                parent.Invoke(d, new object[] { parent, oldControl });
            }
            else
            {
                parent.Controls.Remove(oldControl);
            }
        }

        /// <summary>
        /// コントロールに子コントロールをスレッドセーフで追加
        /// </summary>
        /// <param name="parent">コントロール</param>
        /// <param name="newControl">子コントロール</param>
        public void AddControlToControls(Control parent, Control newControl)
        {
            if (parent.InvokeRequired)
            {
                var d = new AddControlToControlsCallback(AddControlToControls);
                parent.Invoke(d, new object[] { parent, newControl });
            }
            else
            {
                parent.Controls.Add(newControl);
            }
        }


        /// <summary>
        /// コントロールのCursorをスレッドセーフで変更
        /// </summary>
        /// <param name="c">コントロール</param>
        /// <param name="cursor">Cursor</param>
        public void SetCursorControl(Control c, Cursor cursor)
        {
            if (c.InvokeRequired)
            {
                var d = new SetCursorControlCallback(SetCursorControl);
                c.Invoke(d, new object[] { c, cursor });
            }
            else
            {
                c.Cursor = cursor;
            }
        }


        /// <summary>
        /// Cursorを取得する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Cursor GetCursorControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new GetCursorControlCallback(GetCursorControl);
                return (Cursor)c.Invoke(d, new object[] { c });
            }
            else
            {
                return c.Cursor;
            }
        }

        /// <summary>
        /// Handleを取得する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public IntPtr GetHandleControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new GetHandleControlCallback(GetHandleControl);
                return (IntPtr)c.Invoke(d, new object[] { c });
            }
            else
            {
                return c.Handle;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="str"></param>
        public void SetTextControl(Control c, string str)
        {
            if (c.InvokeRequired)
            {
                var d = new SetTextCallback(SetTextControl);
                c.Invoke(d, new object[] { c, str });
            }
            else
            {
                c.Text = str;
            }
        }

        /// <summary>
        /// ボタンの画像をスレッドセーフで入れ替える
        /// </summary>
        /// <param name="c"></param>
        /// <param name="img"></param>
        public void SetButtonImage(Button c, Image img)
        {
            if (c.InvokeRequired)
            {
                var d = new SetButtonImageCallback(SetButtonImage);
                c.Invoke(d, new object[] { c, img });
            }
            else
            {
                c.Image = img;
            }
        }

        /// <summary>
        /// 指定したリストビューアイテムのテキストをスレッドセーフで更新する
        /// </summary>
        /// <param name="li"></param>
        /// <param name="subitemid"></param>
        /// <param name="text"></param>
        public void SetListViewItemText(ListViewItem li, int subitemid, string text)
        {
            if (li.ListView.InvokeRequired)
            {
                var d = new SetListViewItemTextCallback(SetListViewItemText);
                li.ListView.Invoke(d, new object[] { li, subitemid, text });
            }
            else
            {
                li.SubItems[subitemid].Text = text;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string GetTextControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new GetTextCallback(GetTextControl);
                return c.Invoke(d, new object[] { c }).ToString();
            }
            else
            {
                return c.Text;
            }
        }

        /// <summary>
        /// コントロールの幅をスレッドセーフで変更
        /// </summary>
        /// <param name="c">コントロール</param>
        /// <param name="w">幅</param>
        public void SetWidthControl(Control c, int w)
        {
            if (c.InvokeRequired)
            {
                var d = new SetWidthControlCallback(SetWidthControl);
                c.Invoke(d, new object[] { c, w });
            }
            else
            {
                c.Width = w;
            }
        }


        /// <summary>
        /// 幅を取得する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public int GetWidthControl(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new GetWidthControlCallback(GetWidthControl);
                return (int)c.Invoke(d, new object[] { c });
            }
            else
            {
                return c.Width;
            }
        }

        /// <summary>
        /// ポイントをクライアント座標に変換する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="scpos"></param>
        /// <returns></returns>
        public Point PointToClient(Control c, Point scpos)
        {
            if (c.InvokeRequired)
            {
                var d = new PointToClientCallback(PointToClient);
                return (Point)c.Invoke(d, new object[] { c, scpos });
            }
            else
            {
                return c.PointToClient(scpos);
            }
        }

        /// <summary>
        /// Visibleプロパティをスレッドセーフで取得する
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool GetVisible(Control c)
        {
            if (c.InvokeRequired)
            {
                var d = new GetVisibleCallback(GetVisible);
                return (bool)c.Invoke(d, new object[] { c });
            }
            else
            {
                return c.Visible;
            }
        }

        /// <summary>
        /// リストビューにアイテムを追加する
        /// </summary>
        /// <param name="lv">リストビュー</param>
        /// <param name="li">アイテム</param>
        public void AddItemToListView(ListView lv, ListViewItem li)
        {
            if (lv.InvokeRequired)
            {
                var d = new AddItemToListViewCallback(AddItemToListView);
                lv.Invoke(d, new object[] { lv, li });
            }
            else
            {
                lv.Items.Add(li);
            }
        }

        /// <summary>
        /// リストビューにアイテムを追加する
        /// </summary>
        /// <param name="lv">リストビュー</param>
		/// <param name="lis">アイテム</param>
        public void AddItemsToListView(ListView lv, ListViewItem[] lis)
        {
            if (lv.InvokeRequired)
            {
                var d = new AddItemsToListViewCallback(AddItemsToListView);
                lv.Invoke(d, new object[] { lv, lis });
            }
            else
            {
                lv.Items.AddRange(lis);
            }
        }

        /// <summary>
        /// リストビューにアイテムを追加する
        /// </summary>
        /// <param name="lv">リストビュー</param>
        /// <param name="li">アイテム</param>
        public void EnsureVisible(ListView lv, int li)
        {
            if (lv.InvokeRequired)
            {
                var d = new EnsureVisibleListViewCallback(EnsureVisible);
                lv.Invoke(d, new object[] { lv, li });
            }
            else
            {
                lv.EnsureVisible(li);
            }
        }

        /// <summary>
        /// アイテムを追加してソートする
        /// </summary>
        /// <param name="lv">リストビュー</param>
        /// <param name="items">追加するアイテム群</param>
        /// <param name="defaultsorter">現在のソータが無い場合に使うソータ</param>
        public void AddRangeAndSortListViewItem(ListView lv, ListViewItem[] items, IComparer defaultsorter)
        {
            if (lv.InvokeRequired)
            {
                var d = new AddRangeAndSortListViewItemCallback(AddRangeAndSortListViewItem);
                lv.Invoke(d, new object[] { lv, items, defaultsorter });
            }
            else
            {
                var sorter = lv.ListViewItemSorter;
                lv.ListViewItemSorter = null;
                lv.Items.AddRange(items);
                lv.ListViewItemSorter = (sorter ?? defaultsorter);
                lv.Sort();
            }
        }

        /// <summary>
        /// リストビューのアイテムクリア
        /// </summary>
        /// <param name="lv"></param>
        public void ClearListViewItems(ListView lv)
        {
            if (lv.InvokeRequired)
            {
                var d = new ClearListViewItemsCallback(ClearListViewItems);
                lv.Invoke(d, new object[] { lv });
            }
            else
            {
                lv.Items.Clear();
            }
        }

        /// <summary>
        /// リストビューの選択解除
        /// </summary>
        /// <param name="lv"></param>
        public void ClearListViewSelectedItem(ListView lv)
        {
            if (lv.InvokeRequired)
            {
                var d = new ClearListViewSelectedItemsCallback(ClearListViewSelectedItem);
                lv.Invoke(d, new object[] { lv });
            }
            else
            {
                lv.SelectedItems.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lv"></param>
        /// <param name="sorter"></param>
        public void SortListView(ListView lv, IComparer sorter)
        {
            if (lv.InvokeRequired)
            {
                var d = new SortListViewCallback(SortListView);
                lv.Invoke(d, new object[] { lv, sorter });
            }
            else
            {
                if (sorter != null)
                {
                    lv.ListViewItemSorter = sorter;
                }
                lv.Sort();
            }
        }


        /// <summary>
        /// Visibleスイッチのスレッドセーフ
        /// </summary>
        /// <param name="tar"></param>
        /// <param name="sw"></param>
        public void SetVisible(Control tar, bool sw)
        {
            if (tar.InvokeRequired)
            {
                var d = new SetVisibleCallback(SetVisible);
                tar.Invoke(d, new object[] { tar, sw });
            }
            else
            {
                tar.Visible = sw;
            }
        }

        /// <summary>
        /// Checkedスイッチのスレッドセーフ
        /// </summary>
        /// <param name="tar"></param>
        /// <param name="sw"></param>
        public void SetChecked(CheckBox tar, bool sw)
        {
            if (tar.InvokeRequired)
            {
                var d = new SetCheckedCallback(SetChecked);
                tar.Invoke(d, new object[] { tar, sw });
            }
            else
            {
                tar.Checked = sw;
            }
        }

        /// <summary>
        /// Checkedスイッチのスレッドセーフ
        /// </summary>
        /// <param name="tar"></param>
        /// <param name="sw"></param>
        public bool GetChecked(CheckBox tar)
        {
            if (tar.InvokeRequired)
            {
                var d = new GetCheckedCCallback(GetChecked);
                return (bool)tar.Invoke(d, new object[] { tar });
            }
            else
            {
                return tar.Checked;
            }
        }

        /// <summary>
        /// Enabledスイッチのスレッドセーフ
        /// </summary>
        /// <param name="tar"></param>
        /// <param name="sw"></param>
        public void SetEnabled(Control tar, bool sw)
        {
            if (tar.InvokeRequired)
            {
                var d = new SetEnabledCallback(SetEnabled);
                tar.Invoke(d, new object[] { tar, sw });
            }
            else
            {
                tar.Enabled = sw;
            }
        }

        /// <summary>
        /// タブを選択する
        /// </summary>
        /// <param name="tc"></param>
        /// <param name="tp"></param>
        public void SelectTab(TabControl tc, TabPage tp)
        {
            if (tc.InvokeRequired)
            {
                var d = new SelectTabCallback(SelectTab);
                tc.Invoke(d, new object[] { tc, tp });
            }
            else
            {
                tc.SelectedTab = tp;
            }
        }

    }
}
