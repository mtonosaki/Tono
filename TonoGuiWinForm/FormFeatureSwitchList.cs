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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFeatureSwitchList));
            this.lvFeatureSwitchList = new System.Windows.Forms.ListView();
            this.lvhFeatureSwitchListF = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhFeatureSwitchListID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhFeatureSwitchListParam = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhFeatureSwitchListDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.foFeatureSWTab = new System.Windows.Forms.TabControl();
            this.foFeatureSWEnableFeatures = new System.Windows.Forms.TabPage();
            this.foFeatureSWDisableFeatures = new System.Windows.Forms.TabPage();
            this.lvDisableFeatures = new System.Windows.Forms.ListView();
            this.lvhDisableFeatureListF = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvhDisableFeatureListDesc = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.foFeatureSWTab.SuspendLayout();
            this.foFeatureSWEnableFeatures.SuspendLayout();
            this.foFeatureSWDisableFeatures.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvFeatureSwitchList
            // 
            this.lvFeatureSwitchList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvFeatureSwitchList.CheckBoxes = true;
            this.lvFeatureSwitchList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvhFeatureSwitchListF,
            this.lvhFeatureSwitchListID,
            this.lvhFeatureSwitchListParam,
            this.lvhFeatureSwitchListDesc});
            this.lvFeatureSwitchList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvFeatureSwitchList.FullRowSelect = true;
            this.lvFeatureSwitchList.GridLines = true;
            this.lvFeatureSwitchList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvFeatureSwitchList.HideSelection = false;
            this.lvFeatureSwitchList.Location = new System.Drawing.Point(0, 0);
            this.lvFeatureSwitchList.Name = "lvFeatureSwitchList";
            this.lvFeatureSwitchList.Size = new System.Drawing.Size(672, 246);
            this.lvFeatureSwitchList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvFeatureSwitchList.TabIndex = 0;
            this.lvFeatureSwitchList.UseCompatibleStateImageBehavior = false;
            this.lvFeatureSwitchList.View = System.Windows.Forms.View.Details;
            this.lvFeatureSwitchList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvFeatureSwitchList_ItemCheck);
            this.lvFeatureSwitchList.KeyUp += new System.Windows.Forms.KeyEventHandler(this.foFeatureSwitchList_KeyUp);
            // 
            // lvhFeatureSwitchListF
            // 
            this.lvhFeatureSwitchListF.Text = "lvhFeatureSwitchListF";
            this.lvhFeatureSwitchListF.Width = 180;
            // 
            // lvhFeatureSwitchListID
            // 
            this.lvhFeatureSwitchListID.Text = "lvhFeatureSwitchListID";
            this.lvhFeatureSwitchListID.Width = 40;
            // 
            // lvhFeatureSwitchListParam
            // 
            this.lvhFeatureSwitchListParam.Text = "lvhFeatureSwitchListParam";
            this.lvhFeatureSwitchListParam.Width = 102;
            // 
            // lvhFeatureSwitchListDesc
            // 
            this.lvhFeatureSwitchListDesc.Text = "lvhFeatureSwitchListDesc";
            this.lvhFeatureSwitchListDesc.Width = 322;
            // 
            // foFeatureSWTab
            // 
            this.foFeatureSWTab.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.foFeatureSWTab.Controls.Add(this.foFeatureSWEnableFeatures);
            this.foFeatureSWTab.Controls.Add(this.foFeatureSWDisableFeatures);
            this.foFeatureSWTab.Dock = System.Windows.Forms.DockStyle.Fill;
            this.foFeatureSWTab.ImageList = this.imageList1;
            this.foFeatureSWTab.Location = new System.Drawing.Point(0, 0);
            this.foFeatureSWTab.Name = "foFeatureSWTab";
            this.foFeatureSWTab.SelectedIndex = 0;
            this.foFeatureSWTab.Size = new System.Drawing.Size(680, 273);
            this.foFeatureSWTab.TabIndex = 1;
            this.foFeatureSWTab.KeyUp += new System.Windows.Forms.KeyEventHandler(this.foFeatureSwitchList_KeyUp);
            // 
            // foFeatureSWEnableFeatures
            // 
            this.foFeatureSWEnableFeatures.Controls.Add(this.lvFeatureSwitchList);
            this.foFeatureSWEnableFeatures.ImageIndex = 1;
            this.foFeatureSWEnableFeatures.Location = new System.Drawing.Point(4, 4);
            this.foFeatureSWEnableFeatures.Name = "foFeatureSWEnableFeatures";
            this.foFeatureSWEnableFeatures.Size = new System.Drawing.Size(672, 246);
            this.foFeatureSWEnableFeatures.TabIndex = 0;
            this.foFeatureSWEnableFeatures.Text = "Using Features";
            this.foFeatureSWEnableFeatures.UseVisualStyleBackColor = true;
            // 
            // foFeatureSWDisableFeatures
            // 
            this.foFeatureSWDisableFeatures.Controls.Add(this.lvDisableFeatures);
            this.foFeatureSWDisableFeatures.ImageIndex = 0;
            this.foFeatureSWDisableFeatures.Location = new System.Drawing.Point(4, 4);
            this.foFeatureSWDisableFeatures.Name = "foFeatureSWDisableFeatures";
            this.foFeatureSWDisableFeatures.Size = new System.Drawing.Size(672, 246);
            this.foFeatureSWDisableFeatures.TabIndex = 1;
            this.foFeatureSWDisableFeatures.Text = "Unused";
            this.foFeatureSWDisableFeatures.UseVisualStyleBackColor = true;
            // 
            // lvDisableFeatures
            // 
            this.lvDisableFeatures.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvDisableFeatures.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.lvhDisableFeatureListF,
            this.lvhDisableFeatureListDesc});
            this.lvDisableFeatures.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDisableFeatures.FullRowSelect = true;
            this.lvDisableFeatures.GridLines = true;
            this.lvDisableFeatures.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvDisableFeatures.HideSelection = false;
            this.lvDisableFeatures.Location = new System.Drawing.Point(0, 0);
            this.lvDisableFeatures.Name = "lvDisableFeatures";
            this.lvDisableFeatures.Size = new System.Drawing.Size(672, 246);
            this.lvDisableFeatures.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvDisableFeatures.TabIndex = 1;
            this.lvDisableFeatures.UseCompatibleStateImageBehavior = false;
            this.lvDisableFeatures.View = System.Windows.Forms.View.Details;
            this.lvDisableFeatures.KeyUp += new System.Windows.Forms.KeyEventHandler(this.foFeatureSwitchList_KeyUp);
            // 
            // lvhDisableFeatureListF
            // 
            this.lvhDisableFeatureListF.Text = "lvhDisableFeatureListF";
            this.lvhDisableFeatureListF.Width = 180;
            // 
            // lvhDisableFeatureListDesc
            // 
            this.lvhDisableFeatureListDesc.Text = "lvhDisableFeatureListDesc";
            this.lvhDisableFeatureListDesc.Width = 471;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            // 
            // foFeatureSwitchList
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.ClientSize = new System.Drawing.Size(680, 273);
            this.Controls.Add(this.foFeatureSWTab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "foFeatureSwitchList";
            this.Text = "Feature Switch";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.foFeatureSwitchList_KeyUp);
            this.foFeatureSWTab.ResumeLayout(false);
            this.foFeatureSWEnableFeatures.ResumeLayout(false);
            this.foFeatureSWDisableFeatures.ResumeLayout(false);
            this.ResumeLayout(false);

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
