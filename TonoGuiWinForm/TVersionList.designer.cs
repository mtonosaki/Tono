
#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
	partial class TVersionList
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
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region コンポーネント デザイナで生成されたコード


		/// <summary> 
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.listView = new System.Windows.Forms.ListView();
			this.chVersionListName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.chVersionListVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.chVersionListAssembly = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// listView
			// 
			this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chVersionListName,
            this.chVersionListVersion,
            this.chVersionListAssembly});
			this.listView.FullRowSelect = true;
			this.listView.GridLines = true;
			this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView.HideSelection = false;
			this.listView.LabelWrap = false;
			this.listView.Location = new System.Drawing.Point(0, 0);
			this.listView.Name = "listView";
			this.listView.Size = new System.Drawing.Size(446, 97);
			this.listView.TabIndex = 0;
			this.listView.UseCompatibleStateImageBehavior = false;
			this.listView.View = System.Windows.Forms.View.Details;
			// 
			// chVersionListName
			// 
			this.chVersionListName.Text = "Name";
			this.chVersionListName.Width = 180;
			// 
			// chVersionListVersion
			// 
			this.chVersionListVersion.Text = "Version";
			this.chVersionListVersion.Width = 92;
			// 
			// chVersionListAssembly
			// 
			this.chVersionListAssembly.Text = "Assembly";
			this.chVersionListAssembly.Width = 0;
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 100);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(312, 12);
			this.label1.TabIndex = 1;
			this.label1.Text = "特許公開番号：特開2008-171229  dfPartsIllusionProjector他";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 118);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(243, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "特許公開番号：特開2007-264807  FeatureDragZoom";
			// 
			// cVersionList
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.listView);
			this.Name = "cVersionList";
			this.Size = new System.Drawing.Size(446, 137);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listView;
		private System.Windows.Forms.ColumnHeader chVersionListName;
		private System.Windows.Forms.ColumnHeader chVersionListVersion;
		private System.Windows.Forms.ColumnHeader chVersionListAssembly;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
	}
}
