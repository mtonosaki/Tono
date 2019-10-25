// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �N���b�N�C�x���g���g�[�N���ɕϊ�����
    /// </summary>
    /// <remarks>
    /// ���j���[��{�^����TAG�ɕ����񂪓����Ă���ꍇ�A������g�[�N���Œʒm����
    /// Tag�����̗�1�F�@Token:AAA   Click�C�x���g�������AAAA�Ƃ������̂�NamedId���g�[�N���Ƃ��ē�������
    /// Tag�����̗�2�F�@Event=Disposed;Token:BBB   Dispose�C�x���g�������ABBB�Ƃ������̂�NamedId���g�[�N���Ƃ��ē�������
    /// </remarks>
    public class FeatureClickEventBridge : FeatureControlBridgeBase
    {
        private readonly Dictionary<object, object> _dupCheck = new Dictionary<object, object>();

        /// <summary>
        /// TAG�Ɋ�Â��C�x���g�ǉ�
        /// </summary>
        /// <param name="tar"></param>
        private void addEvent(object tar)
        {
            string tag = null;
            var pi = tar.GetType().GetProperty("Tag");
            if (pi != null)
            {
                var tarval = pi.GetValue(tar, null);
                if (tarval != null)
                {
                    tag = tarval.ToString();
                }
            }
            if (tag != null)
            {
                var eventName = "Click";
                foreach (var tagstr in StrUtil.SplitTrim(tag, ";"))
                {
                    if (tagstr.ToUpper().StartsWith("EVENT="))
                    {
                        eventName = tagstr.Substring(6);
                    }
                    else
                    {
                        tag = tagstr;
                    }
                }

                var ei = tar.GetType().GetEvent(eventName);
                if (ei == null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        string.Format("������ ���� ������ �R���g���[�� {0} �ɂ� Tag�����񂪎w�肳��A����̓C�x���g���g�[�N���ɕϊ����悤�Ƃ��Ă��܂��B�������A���̃R���g���[���ɂ�{1}�Ƃ����C�x���g�͑��݂��܂���B"
                            , (tar is Control ? ((Control)tar).Name : tar.GetType().Name)
                            , eventName
                    ));
                }
                if (ei != null && tag.ToUpper().StartsWith("TOKEN:"))
                {
                    if (_dupCheck.ContainsKey(tar) == false)
                    {
                        ei.AddEventHandler(tar, new EventHandler(target_Raise));
                        _dupCheck[tar] = tar;
                    }
                }
            }
        }

        /// <summary>
        /// �S�R���g���[����ΏۂƂ���
        /// </summary>
        /// <param name="c"></param>
        private void registerLoop(Control c)
        {
            addEvent(c);
            if (c.ContextMenu != null && c.ContextMenu.MenuItems != null)
            {
                foreach (MenuItem mi in c.ContextMenu.MenuItems)
                {
                    registerLoop(mi);
                }
            }
            if (c.ContextMenuStrip != null && c.ContextMenuStrip.Items != null)
            {
                foreach (ToolStripItem ti in c.ContextMenuStrip.Items)
                {
                    registerLoop(ti);
                }
            }
            foreach (Control cc in c.Controls)
            {
                registerLoop(cc);
            }
            if (c is ToolStrip ts)
            {
                foreach (ToolStripItem ti in ts.Items)
                {
                    registerLoop(ti);
                }
            }
        }

        /// <summary>
        /// �SMenuItem�ΏۂƂ���
        /// </summary>
        /// <param name="c"></param>
        private void registerLoop(MenuItem c)
        {
            addEvent(c);
            if (c.MenuItems != null)
            {
                foreach (MenuItem cc in c.MenuItems)
                {
                    registerLoop(cc);
                }
            }
        }

        /// <summary>
        /// �SToolStripItem�ΏۂƂ���
        /// </summary>
        /// <param name="c"></param>
        private void registerLoop(ToolStripItem c)
        {
            addEvent(c);
            if (c is ToolStripMenuItem && ((ToolStripMenuItem)c).DropDown != null)
            {
                foreach (ToolStripItem ti in ((ToolStripMenuItem)c).DropDown.Items)
                {
                    registerLoop(ti);
                }
            }
            if (c is ToolStripDropDownItem && c is ToolStripMenuItem == false && ((ToolStripDropDownItem)c) != null)
            {
                foreach (ToolStripItem ti in ((ToolStripDropDownItem)c).DropDown.Items)
                {
                    registerLoop(ti);
                }
            }
        }

        /// <summary>
        /// �R���g���[���C�x���g�擾
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void target_Raise(object sender, EventArgs e)
        {
            var pi = sender.GetType().GetProperty("Tag");
            var tag = (string)pi.GetValue(sender, null);
            string token;
            var id = tag.ToUpper().IndexOf("TOKEN:");
            if (id >= 0)
            {
                token = tag.Substring(id + 6);
                if (token.IndexOf(';') >= 0)
                {
                    token = token.Substring(0, token.IndexOf(';'));
                }
            }
            else
            {
                token = tag;
            }
            Token.Add(NamedId.FromName(token), this);
            GetRoot().FlushFeatureTriggers();
        }

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            base.OnInitInstance();

            Control topc;
            for (topc = Pane.Control; topc is Form == false && topc.Parent != null; topc = topc.Parent)
            {
                ;
            }

            registerLoop(topc);

            if (topc is Form)
            {
                var top = (Form)topc;
                if (top.Menu != null && top.Menu.MenuItems != null)
                {
                    foreach (MenuItem mi in top.Menu.MenuItems)
                    {
                        registerLoop(mi);
                    }
                }
            }
            if (topc is Form)
            {
                var top = (Form)topc;
                if (top.MainMenuStrip == null)
                {
                    return;
                }

                foreach (ToolStripItem ti in top.MainMenuStrip.Items)
                {
                    registerLoop(ti);
                }
            }
        }
    }
}
