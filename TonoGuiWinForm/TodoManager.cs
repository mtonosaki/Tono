// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// TODO�\���p�e�L�X�g
    /// </summary>
    public class TodoCaption
    {
        /// <summary>
        /// ���j���[�\���p������
        /// </summary>
        public string Caption;

        /// <summary>
        /// �����p������
        /// </summary>
        public string Remarks;

        /// <summary>
        /// �������\�z�q
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
    /// TODO�������������
    /// </summary>
    public interface ITodoIntent
    {
        /// <summary>
        /// ���j���[�\���p�������ݒ肷��
        /// </summary>
        /// <param name="key">���ʎq</param>
        /// <returns>������</returns>
        TodoCaption GetTodoCaption(string key);

        /// <summary>
        /// �⏕�@�\�����s����
        /// </summary>
        /// <param name="key">���ʎq</param>
        /// <returns>true=�Ӑ}�ǂ��芮�� / false=���s</returns>
        bool DoAssist(string key);
    }

    /// <summary>
    /// TODO�A�C�e�����Ǘ�����
    /// </summary>
    public static class TODO
    {
        /// <summary>
        /// �L�[�ƃC���e���g�̃y�A
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
        /// TODO��\���E���삷��R���g���[����ۑ�
        /// </summary>
        private static ToolStripDropDownButton _tar = null;

        /// <summary>
        /// TODO�Ǘ�����h���b�v�_�E���{�^���̐e��o�^����
        /// </summary>
        /// <param name="btn"></param>
        public static void SetTodoButton(ToolStripDropDownButton btn)
        {
            _tar = btn;
            _tar.DropDownOpening += new EventHandler(_tar_DropDownOpening);
        }

        /// <summary>
        /// ���j���[���J�����Ƃ��ɁATODO�L���v�V��������������
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
        /// TODO�ɂЂƂǉ�����
        /// </summary>
        /// <param name="key"></param>
        /// <param name="intent"></param>
        /// <returns></returns>
        public static void Add(string key, ITodoIntent intent)
        {
            // �������ʎq�������o�^���ꂽ��A��̂ق����L�����Z������
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
        /// TODO�A�V�X�g�{�^�����N���b�N���ꂽ
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
        /// �I������TODO���X�g�������
        /// </summary>
        /// <param name="key">���ʎq</param>
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
