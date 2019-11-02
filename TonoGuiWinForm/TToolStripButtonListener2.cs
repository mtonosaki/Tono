// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fiToolStripButton2 �̊T�v�̐����ł��B
    /// ���j���[�̎��s�ŋN������t�B�[�`���[�N���^�C�~���O�Ď��N���X
    /// </summary>
    public class TToolStripButton2 : System.Windows.Forms.ToolStripButton, IFeatureEventListener
    {
        #region �����i�V���A���C�Y����j

        /// <summary>�C�x���g�]����t�B�[�`���[</summary>
        private FeatureBase _target = null;

        /// <summary>UI����C�x���g���s���ɖ����ł���ID�i�������Ȃ��Ă�OK�j</summary>
        private NamedId _triggerTokenID = null;

        #endregion

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public TToolStripButton2()
        {
            if (DesignMode == false)
            {
                base.Visible = false;
            }
        }

        [Browsable(false)]
        public new bool Visible
        {
            get => base.Visible;
            set
            {
                if (DesignMode)
                {
                    base.Visible = value;
                }
            }
        }


        public string _
        {
            get
            {
                var s = "";
                if (_triggerTokenID != null)
                {
                    s = "Trigger Token ID = " + _triggerTokenID.ToString();
                }
                if (_target != null)
                {
                    s += " to " + _target.GetType().Name;
                }
                else
                {
                    s += " to NULL target";
                }
                return s;
            }
        }

        /// <summary>
        /// �g���K�[�g�[�N��ID����Ɏw�肷�鎖���ł���
        /// </summary>
        public NamedId ID
        {
            set => _triggerTokenID = value;
        }

        /// <summary>
        /// �C�x���g�̓]����ƂȂ�t�B�[�`���[�N���X�̃C���X�^���X���w�肷��
        /// </summary>
        /// <param name="target">�t�B�[�`���[�N���X�̃C���X�^���X</param>
        public void LinkFeature(FeatureBase target)
        {
            _target = target;
            base.Visible = true;
            //			this.Owner.Click += new EventHandler(onOpenMenuItem);	// .NET2.0��
            //if( OwnerItem.GetType().IsDefined(typeof(ToolStripDropDown), true))
            //{
            //    ((ToolStripDropDown)OwnerItem).DropDownOpening += new EventHandler(onOpenMenuItem);
            //}
        }

        /// <summary>
        /// ���j���[�̃|�b�v�A�b�v�C�x���g
        /// </summary>
        private void onOpenMenuItem(object sender, EventArgs e)
        {
            Enabled = _target.CanStart;
            Checked = _target.Checked;
        }

        /// <summary>
        /// �N���b�N�C�x���g����
        /// </summary>
        protected override void OnClick(System.EventArgs e)
        {
            if (_target != null)
            {
                _target.RequestStartup(_triggerTokenID);

                // �V���[�g�J�b�g�L�[�Ńt�H�[�J�X���D��ꂽ�ꍇ�A�Ƃ肠�����SUp�C�x���g���΂�
                //_keyEventReset(GetMainMenu().GetForm());
            }
        }
        /// <summary>
        /// ���ׂĂ�cFeatureRich�ɃL�[�C�x���g�č\�z��v������
        /// </summary>
        /// <param name="cnt"></param>
        private void _keyEventReset(Control cnt)
        {
            if (cnt == null)    // �t�H�[���͏I������Ă���
            {
                return;
            }
            foreach (Control c in cnt.Controls)
            {
                if (c is TGuiView)
                {
                    ((TGuiView)c).ResetKeyEvents();
                }
                _keyEventReset(c);
            }
        }
    }
}
