// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fiMenuListener の概要の説明です。
    /// メニューの実行で起動するフィーチャー起動タイミング監視クラス
    /// </summary>
    public class TMenuListener : System.Windows.Forms.MenuItem, IFeatureEventListener
    {
        #region 属性（シリアライズする）

        /// <summary>イベント転送先フィーチャー</summary>
        private FeatureBase _target = null;

        /// <summary>UIからイベント発行時に明示できるID（明示しなくてもOK）</summary>
        private NamedId _triggerTokenID = null;

        #endregion

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public TMenuListener()
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
#if DEBUG
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
#endif
        /// <summary>
        /// トリガートークンIDを特に指定する事ができる
        /// </summary>
        public NamedId ID
        {
            set => _triggerTokenID = value;
        }

        /// <summary>
        /// イベントの転送先となるフィーチャークラスのインスタンスを指定する
        /// </summary>
        /// <param name="target">フィーチャークラスのインスタンス</param>
        public void LinkFeature(FeatureBase target)
        {
            _target = target;
            base.Visible = true;
            ((MenuItem)Parent).Popup += new EventHandler(fiMenuListener_Popup);
        }

        /// <summary>
        /// メニューのポップアップイベント
        /// </summary>
        private void fiMenuListener_Popup(object sender, EventArgs e)
        {
            if (_target.Enabled == false)
            {
                var dummy = _target.CanStart;
                Enabled = false;
            }
            else
            {
                Enabled = _target.CanStart;
            }
            Checked = _target.Checked;
        }

        /// <summary>
        /// クリックイベント処理
        /// </summary>
        protected override void OnClick(System.EventArgs e)
        {
            if (_target != null)
            {
                _target.RequestStartup(_triggerTokenID);

                // ショートカットキーでフォーカスが奪われた場合、とりあえず全Upイベントを飛ばす
                _keyEventReset(GetMainMenu().GetForm());
            }
        }
        /// <summary>
        /// すべてのcFeatureRichにキーイベント再構築を要請する
        /// </summary>
        /// <param name="cnt"></param>
        private void _keyEventReset(Control cnt)
        {
            if (cnt == null)    // フォームは終了されている
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
