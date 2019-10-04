using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// foFeatureSwitchList の概要の説明です。
    /// </summary>
    public class FormFeatureSwitchList : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ListView lvFeatureSwitchList;
        private System.Windows.Forms.ColumnHeader lvhFeatureSwitchListF;
        private System.Windows.Forms.ColumnHeader lvhFeatureSwitchListID;
        private System.Windows.Forms.ColumnHeader lvhFeatureSwitchListDesc;
        private FeatureGroupBase _root = null;
        private bool _isInit = true;
        private System.Windows.Forms.ColumnHeader lvhFeatureSwitchListParam;
        private System.Windows.Forms.TabControl foFeatureSWTab;
        private System.Windows.Forms.TabPage foFeatureSWEnableFeatures;
        private System.Windows.Forms.TabPage foFeatureSWDisableFeatures;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ListView lvDisableFeatures;
        private System.Windows.Forms.ColumnHeader lvhDisableFeatureListF;
        private System.Windows.Forms.ColumnHeader lvhDisableFeatureListDesc;
        private System.ComponentModel.IContainer components;

        public FormFeatureSwitchList()
        {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        /// <summary>
        /// ルートグループを指定する
        /// </summary>
        public void SetFeatureRootGroup(FeatureGroupBase root)
        {
            _root = root;
        }

        /// <summary>
        /// 使用されているリソースに後処理を実行します。
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // フォームにフィーチャーの情報を表示させる
            System.Diagnostics.Debug.Assert(_root != null, "予めSetFeatureRootGroupを実行してからShowする必要があるフォームです");
            IDictionary regfc = new Hashtable();

            var fcs = _root.GetChildFeatureInstance();
            var lvs = new ListViewItem[fcs.Count];
            var i = 0;
            foreach (FeatureBase fc in fcs)
            {
                for (var tt = fc.GetType(); tt != typeof(FeatureBase); tt = tt.BaseType)
                {
                    regfc[tt] = true;
                }
                var li = lvs[i++] = new ListViewItem();
                li.Text = fc.GetType().Name;
                li.Checked = fc.Enabled;
                li.SubItems.Add(fc.ID.ToString());
                li.SubItems.Add(fc.GetParamString());
                li.Tag = fc;
                var s = Mes.Current["FeatureDescription", fc.GetType().Name];
                if (s == null)
                {
                    s = "";
                }
                li.SubItems.Add(s);
            }
            lvFeatureSwitchList.Items.AddRange(lvs);

            // 未使用フィーチャーのリストを作成する
            // 関係するアセンブリを列挙する
            var eaa = Assembly.GetEntryAssembly();
            var eac = Assembly.GetCallingAssembly();
            var eab = Assembly.GetExecutingAssembly();
            Assembly[] ass = { eaa, eab, eac };

            // クラスを記憶する
            var classes = new Hashtable();
            foreach (var ea in ass)
            {
                var types = ea.GetTypes();
                foreach (var t in types)
                {
                    if (regfc[t] != null)
                    {
                        continue;
                    }
                    if (t.IsSubclassOf(typeof(FeatureBase)))
                    {
                        classes[t] = true;
                    }
                }
            }
            var dlvs = new ListViewItem[classes.Count];
            i = 0;
            foreach (Type t in classes.Keys)
            {
                var li = dlvs[i++] = new ListViewItem();
                li.Text = t.Name;
                li.Tag = t;
                var s = Mes.Current["FeatureDescription", t.Name];
                if (s == null)
                {
                    s = "";
                }
                li.SubItems.Add(s);
            }
            lvDisableFeatures.Items.AddRange(dlvs);

            Mes.Current.ResetText(this);


            _isInit = false;
        }


        #region Windows フォーム デザイナで生成されたコード 
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFeatureSwitchList));
            lvFeatureSwitchList = new System.Windows.Forms.ListView();
            lvhFeatureSwitchListF = new System.Windows.Forms.ColumnHeader();
            lvhFeatureSwitchListID = new System.Windows.Forms.ColumnHeader();
            lvhFeatureSwitchListParam = new System.Windows.Forms.ColumnHeader();
            lvhFeatureSwitchListDesc = new System.Windows.Forms.ColumnHeader();
            foFeatureSWTab = new System.Windows.Forms.TabControl();
            foFeatureSWEnableFeatures = new System.Windows.Forms.TabPage();
            foFeatureSWDisableFeatures = new System.Windows.Forms.TabPage();
            lvDisableFeatures = new System.Windows.Forms.ListView();
            lvhDisableFeatureListF = new System.Windows.Forms.ColumnHeader();
            lvhDisableFeatureListDesc = new System.Windows.Forms.ColumnHeader();
            imageList1 = new System.Windows.Forms.ImageList(components);
            foFeatureSWTab.SuspendLayout();
            foFeatureSWEnableFeatures.SuspendLayout();
            foFeatureSWDisableFeatures.SuspendLayout();
            SuspendLayout();
            // 
            // lvFeatureSwitchList
            // 
            lvFeatureSwitchList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lvFeatureSwitchList.CheckBoxes = true;
            lvFeatureSwitchList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            lvhFeatureSwitchListF,
            lvhFeatureSwitchListID,
            lvhFeatureSwitchListParam,
            lvhFeatureSwitchListDesc});
            lvFeatureSwitchList.Dock = System.Windows.Forms.DockStyle.Fill;
            lvFeatureSwitchList.FullRowSelect = true;
            lvFeatureSwitchList.GridLines = true;
            lvFeatureSwitchList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lvFeatureSwitchList.HideSelection = false;
            lvFeatureSwitchList.Location = new System.Drawing.Point(0, 0);
            lvFeatureSwitchList.Name = "lvFeatureSwitchList";
            lvFeatureSwitchList.Size = new System.Drawing.Size(672, 246);
            lvFeatureSwitchList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lvFeatureSwitchList.TabIndex = 0;
            lvFeatureSwitchList.UseCompatibleStateImageBehavior = false;
            lvFeatureSwitchList.View = System.Windows.Forms.View.Details;
            lvFeatureSwitchList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(lvFeatureSwitchList_ItemCheck);
            lvFeatureSwitchList.KeyUp += new System.Windows.Forms.KeyEventHandler(foFeatureSwitchList_KeyUp);
            // 
            // lvhFeatureSwitchListF
            // 
            lvhFeatureSwitchListF.Text = "lvhFeatureSwitchListF";
            lvhFeatureSwitchListF.Width = 180;
            // 
            // lvhFeatureSwitchListID
            // 
            lvhFeatureSwitchListID.Text = "lvhFeatureSwitchListID";
            lvhFeatureSwitchListID.Width = 40;
            // 
            // lvhFeatureSwitchListParam
            // 
            lvhFeatureSwitchListParam.Text = "lvhFeatureSwitchListParam";
            lvhFeatureSwitchListParam.Width = 102;
            // 
            // lvhFeatureSwitchListDesc
            // 
            lvhFeatureSwitchListDesc.Text = "lvhFeatureSwitchListDesc";
            lvhFeatureSwitchListDesc.Width = 322;
            // 
            // foFeatureSWTab
            // 
            foFeatureSWTab.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            foFeatureSWTab.Controls.Add(foFeatureSWEnableFeatures);
            foFeatureSWTab.Controls.Add(foFeatureSWDisableFeatures);
            foFeatureSWTab.Dock = System.Windows.Forms.DockStyle.Fill;
            foFeatureSWTab.ImageList = imageList1;
            foFeatureSWTab.Location = new System.Drawing.Point(0, 0);
            foFeatureSWTab.Name = "foFeatureSWTab";
            foFeatureSWTab.SelectedIndex = 0;
            foFeatureSWTab.Size = new System.Drawing.Size(680, 273);
            foFeatureSWTab.TabIndex = 1;
            foFeatureSWTab.KeyUp += new System.Windows.Forms.KeyEventHandler(foFeatureSwitchList_KeyUp);
            // 
            // foFeatureSWEnableFeatures
            // 
            foFeatureSWEnableFeatures.Controls.Add(lvFeatureSwitchList);
            foFeatureSWEnableFeatures.ImageIndex = 1;
            foFeatureSWEnableFeatures.Location = new System.Drawing.Point(4, 4);
            foFeatureSWEnableFeatures.Name = "foFeatureSWEnableFeatures";
            foFeatureSWEnableFeatures.Size = new System.Drawing.Size(672, 246);
            foFeatureSWEnableFeatures.TabIndex = 0;
            foFeatureSWEnableFeatures.Text = "Using Features";
            foFeatureSWEnableFeatures.UseVisualStyleBackColor = true;
            // 
            // foFeatureSWDisableFeatures
            // 
            foFeatureSWDisableFeatures.Controls.Add(lvDisableFeatures);
            foFeatureSWDisableFeatures.ImageIndex = 0;
            foFeatureSWDisableFeatures.Location = new System.Drawing.Point(4, 4);
            foFeatureSWDisableFeatures.Name = "foFeatureSWDisableFeatures";
            foFeatureSWDisableFeatures.Size = new System.Drawing.Size(672, 246);
            foFeatureSWDisableFeatures.TabIndex = 1;
            foFeatureSWDisableFeatures.Text = "Unused";
            foFeatureSWDisableFeatures.UseVisualStyleBackColor = true;
            // 
            // lvDisableFeatures
            // 
            lvDisableFeatures.BorderStyle = System.Windows.Forms.BorderStyle.None;
            lvDisableFeatures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            lvhDisableFeatureListF,
            lvhDisableFeatureListDesc});
            lvDisableFeatures.Dock = System.Windows.Forms.DockStyle.Fill;
            lvDisableFeatures.FullRowSelect = true;
            lvDisableFeatures.GridLines = true;
            lvDisableFeatures.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lvDisableFeatures.HideSelection = false;
            lvDisableFeatures.Location = new System.Drawing.Point(0, 0);
            lvDisableFeatures.Name = "lvDisableFeatures";
            lvDisableFeatures.Size = new System.Drawing.Size(672, 246);
            lvDisableFeatures.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lvDisableFeatures.TabIndex = 1;
            lvDisableFeatures.UseCompatibleStateImageBehavior = false;
            lvDisableFeatures.View = System.Windows.Forms.View.Details;
            lvDisableFeatures.KeyUp += new System.Windows.Forms.KeyEventHandler(foFeatureSwitchList_KeyUp);
            // 
            // lvhDisableFeatureListF
            // 
            lvhDisableFeatureListF.Text = "lvhDisableFeatureListF";
            lvhDisableFeatureListF.Width = 180;
            // 
            // lvhDisableFeatureListDesc
            // 
            lvhDisableFeatureListDesc.Text = "lvhDisableFeatureListDesc";
            lvhDisableFeatureListDesc.Width = 471;
            // 
            // imageList1
            // 
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "");
            imageList1.Images.SetKeyName(1, "");
            // 
            // foFeatureSwitchList
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            ClientSize = new System.Drawing.Size(680, 273);
            Controls.Add(foFeatureSWTab);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "foFeatureSwitchList";
            Text = "Feature Switch";
            KeyUp += new System.Windows.Forms.KeyEventHandler(foFeatureSwitchList_KeyUp);
            foFeatureSWTab.ResumeLayout(false);
            foFeatureSWEnableFeatures.ResumeLayout(false);
            foFeatureSWDisableFeatures.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void lvFeatureSwitchList_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
        {
            if (_isInit)
            {
                return;
            }

            var li = ((ListView)sender).Items[e.Index];
            if (li.Tag == null)
            {
                return;
            }

            var fc = (FeatureBase)li.Tag;
            fc.Enabled = (e.NewValue == CheckState.Checked ? true : false);
            var dummy = fc.CanStart;

            // FeatureFeatureSwitchListが一つは必要
            var id = -1;
            foreach (ListViewItem l in ((ListView)sender).Items)
            {
                if (l.Tag is FeatureSwitchList)
                {
                    if (((FeatureBase)l.Tag).Enabled)
                    {
                        return;
                    }
                    else
                    {
                        id = l.Index;
                    }
                }
            }
            e.NewValue = CheckState.Checked;
            ((ListView)sender).Items[id].Checked = true;
            ((FeatureBase)((ListView)sender).Items[id].Tag).Enabled = true;
        }

        private void foFeatureSwitchList_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.OK;
            }
        }
    }
}
