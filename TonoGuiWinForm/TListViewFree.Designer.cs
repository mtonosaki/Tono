namespace Tono.GuiWinForm
{
	partial class TListViewFree
	{
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TListViewFree));
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.addFilterWithThisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.addNegativeFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.fazzyoutFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.allClearFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.setGroupKeyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showTheGroupedRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showAllRecordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.frMain = new Tono.GuiWinForm.TGuiView(this.components);
			this.fiKeyEnabler1 = new Tono.GuiWinForm.TKeyEnabler();
			this.rpResource = new Tono.GuiWinForm.TPane();
			this.contextMenuStrip1.SuspendLayout();
			this.frMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addFilterWithThisToolStripMenuItem,
            this.addNegativeFilterToolStripMenuItem,
            this.fazzyoutFilterToolStripMenuItem,
            this.clearFilterToolStripMenuItem,
            this.allClearFilterToolStripMenuItem,
            this.toolStripSeparator1,
            this.setGroupKeyToolStripMenuItem,
            this.showTheGroupedRecordToolStripMenuItem,
            this.showAllRecordsToolStripMenuItem,
            this.toolStripSeparator2});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(291, 192);
			// 
			// addFilterWithThisToolStripMenuItem
			// 
			this.addFilterWithThisToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.filterSelect;
			this.addFilterWithThisToolStripMenuItem.Name = "addFilterWithThisToolStripMenuItem";
			this.addFilterWithThisToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.addFilterWithThisToolStripMenuItem.Tag = "Select Filter \"@VAL@\" +SHIFT: Solo";
			this.addFilterWithThisToolStripMenuItem.Text = "Select Filter \"@VAL@\" +SHIFT: Solo";
			// 
			// addNegativeFilterToolStripMenuItem
			// 
			this.addNegativeFilterToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.filterNegativeSelect;
			this.addNegativeFilterToolStripMenuItem.Name = "addNegativeFilterToolStripMenuItem";
			this.addNegativeFilterToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.addNegativeFilterToolStripMenuItem.Tag = "Negative Filter \"@VAL@\"";
			this.addNegativeFilterToolStripMenuItem.Text = "Negative Filter \"@VAL@\"";
			// 
			// fazzyoutFilterToolStripMenuItem
			// 
			this.fazzyoutFilterToolStripMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.fazzyoutFilterToolStripMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("fazzyoutFilterToolStripMenuItem.BackgroundImage")));
			this.fazzyoutFilterToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.FilterFuzzy;
			this.fazzyoutFilterToolStripMenuItem.Name = "fazzyoutFilterToolStripMenuItem";
			this.fazzyoutFilterToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.fazzyoutFilterToolStripMenuItem.Text = "Make the filter simple";
			// 
			// clearFilterToolStripMenuItem
			// 
			this.clearFilterToolStripMenuItem.BackgroundImage = global::Tono.GuiWinForm.Properties.Resources.MenuGra_Yellow;
			this.clearFilterToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.Eraser;
			this.clearFilterToolStripMenuItem.Name = "clearFilterToolStripMenuItem";
			this.clearFilterToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.clearFilterToolStripMenuItem.Tag = "&Clear the Filter = [@COL@]";
			this.clearFilterToolStripMenuItem.Text = "&Clear the Filter = [@COL@]";
			// 
			// allClearFilterToolStripMenuItem
			// 
			this.allClearFilterToolStripMenuItem.BackgroundImage = global::Tono.GuiWinForm.Properties.Resources.MenuGra_Yellow;
			this.allClearFilterToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.filterAllOFF;
			this.allClearFilterToolStripMenuItem.Name = "allClearFilterToolStripMenuItem";
			this.allClearFilterToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.allClearFilterToolStripMenuItem.Text = "Reset &All filters";
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(287, 6);
			// 
			// setGroupKeyToolStripMenuItem
			// 
			this.setGroupKeyToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.Groupkey1;
			this.setGroupKeyToolStripMenuItem.Name = "setGroupKeyToolStripMenuItem";
			this.setGroupKeyToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.setGroupKeyToolStripMenuItem.Tag = "Set Group key = [@COL@]";
			this.setGroupKeyToolStripMenuItem.Text = "Set Group key = [@COL@]";
			// 
			// showTheGroupedRecordToolStripMenuItem
			// 
			this.showTheGroupedRecordToolStripMenuItem.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("showTheGroupedRecordToolStripMenuItem.BackgroundImage")));
			this.showTheGroupedRecordToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.GroupedRecord;
			this.showTheGroupedRecordToolStripMenuItem.Name = "showTheGroupedRecordToolStripMenuItem";
			this.showTheGroupedRecordToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.showTheGroupedRecordToolStripMenuItem.Tag = "Show the grouped record of \"@KEY@\"";
			this.showTheGroupedRecordToolStripMenuItem.Text = "Show the grouped record of \"@KEY@\"";
			// 
			// showAllRecordsToolStripMenuItem
			// 
			this.showAllRecordsToolStripMenuItem.BackgroundImage = global::Tono.GuiWinForm.Properties.Resources.MenuGra_Yellow;
			this.showAllRecordsToolStripMenuItem.Image = global::Tono.GuiWinForm.Properties.Resources.Groupkey0;
			this.showAllRecordsToolStripMenuItem.Name = "showAllRecordsToolStripMenuItem";
			this.showAllRecordsToolStripMenuItem.Size = new System.Drawing.Size(290, 22);
			this.showAllRecordsToolStripMenuItem.Text = "Reset the group key";
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(287, 6);
			// 
			// frMain
			// 
			this.frMain.AllowDrop = true;
			this.frMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.frMain.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.frMain.CausesValidation = false;
			this.frMain.ContextMenuStrip = this.contextMenuStrip1;
			this.frMain.Controls.Add(this.fiKeyEnabler1);
			this.frMain.Controls.Add(this.rpResource);
			this.frMain.Font = new System.Drawing.Font("Tahoma", 8.25F);
			this.frMain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.frMain.IdText = "frMain";
			this.frMain.IsDrawEmptyBackground = true;
			this.frMain.Location = new System.Drawing.Point(0, 0);
			this.frMain.Name = "frMain";
			this.frMain.Scroll = ((Tono.GuiWinForm.ScreenPos)(resources.GetObject("frMain.Scroll")));
			this.frMain.ScrollMute = ((Tono.GuiWinForm.ScreenPos)(resources.GetObject("frMain.ScrollMute")));
			this.frMain.Size = new System.Drawing.Size(744, 506);
			this.frMain.TabIndex = 0;
			this.frMain.Text = "cFeatureRich1";
			this.frMain.Zoom = ((Tono.GuiWinForm.XyBase)(resources.GetObject("frMain.Zoom")));
			this.frMain.ZoomMute = ((Tono.GuiWinForm.XyBase)(resources.GetObject("frMain.ZoomMute")));
			// 
			// fiKeyEnabler1
			// 
			this.fiKeyEnabler1.BackColor = System.Drawing.Color.Red;
			this.fiKeyEnabler1.Location = new System.Drawing.Point(0, 0);
			this.fiKeyEnabler1.Name = "fiKeyEnabler1";
			this.fiKeyEnabler1.Size = new System.Drawing.Size(1, 1);
			this.fiKeyEnabler1.TabIndex = 3;
			// 
			// rpResource
			// 
			this.rpResource.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.rpResource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.rpResource.IdColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
			this.rpResource.IdText = "Resource";
			this.rpResource.ImeMode = System.Windows.Forms.ImeMode.Disable;
			this.rpResource.IsScrollLockX = false;
			this.rpResource.IsScrollLockY = false;
			this.rpResource.IsZoomLockX = false;
			this.rpResource.IsZoomLockY = false;
			this.rpResource.Location = new System.Drawing.Point(0, 0);
			this.rpResource.Name = "rpResource";
			this.rpResource.Scroll = ((Tono.GuiWinForm.ScreenPos)(resources.GetObject("rpResource.Scroll")));
			this.rpResource.Size = new System.Drawing.Size(744, 506);
			this.rpResource.TabIndex = 2;
			this.rpResource.Visible = false;
			this.rpResource.Zoom = ((Tono.GuiWinForm.XyBase)(resources.GetObject("rpResource.Zoom")));
			// 
			// coListView
			// 
			this.ContextMenuStrip = this.contextMenuStrip1;
			this.Controls.Add(this.frMain);
			this.Name = "coListView";
			this.Size = new System.Drawing.Size(744, 506);
			this.contextMenuStrip1.ResumeLayout(false);
			this.frMain.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Tono.GuiWinForm.TGuiView frMain;
		private Tono.GuiWinForm.TPane rpResource;
		private Tono.GuiWinForm.TKeyEnabler fiKeyEnabler1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem allClearFilterToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearFilterToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem setGroupKeyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showAllRecordsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addFilterWithThisToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showTheGroupedRecordToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem fazzyoutFilterToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem addNegativeFilterToolStripMenuItem;
	}
}
