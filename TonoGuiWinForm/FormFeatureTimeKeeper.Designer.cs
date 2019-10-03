namespace Tono.GuiWinForm
{
	partial class FormFeatureTimeKeeper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFeatureTimeKeeper));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeaderFeatureName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRamda = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderN = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonRecalc = new System.Windows.Forms.Button();
            this.comboBoxUnit = new System.Windows.Forms.ComboBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFeatureName,
            this.columnHeaderID,
            this.columnHeaderType,
            this.columnHeaderRamda,
            this.columnHeaderMu,
            this.columnHeaderN});
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(0, 31);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(421, 288);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFeatureName
            // 
            this.columnHeaderFeatureName.Text = "feature";
            this.columnHeaderFeatureName.Width = 90;
            // 
            // columnHeaderID
            // 
            this.columnHeaderID.Text = "ID";
            this.columnHeaderID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderID.Width = 45;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "type";
            // 
            // columnHeaderRamda
            // 
            this.columnHeaderRamda.Text = "λ";
            this.columnHeaderRamda.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeaderMu
            // 
            this.columnHeaderMu.Text = "μ";
            this.columnHeaderMu.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // columnHeaderN
            // 
            this.columnHeaderN.Text = "n";
            this.columnHeaderN.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderN.Width = 55;
            // 
            // buttonRecalc
            // 
            this.buttonRecalc.Location = new System.Drawing.Point(12, 4);
            this.buttonRecalc.Name = "buttonRecalc";
            this.buttonRecalc.Size = new System.Drawing.Size(75, 23);
            this.buttonRecalc.TabIndex = 1;
            this.buttonRecalc.Text = "Recalc";
            this.buttonRecalc.UseVisualStyleBackColor = true;
            this.buttonRecalc.Click += new System.EventHandler(this.buttonRecalc_Click);
            // 
            // comboBoxUnit
            // 
            this.comboBoxUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUnit.FormattingEnabled = true;
            this.comboBoxUnit.Location = new System.Drawing.Point(93, 5);
            this.comboBoxUnit.Name = "comboBoxUnit";
            this.comboBoxUnit.Size = new System.Drawing.Size(121, 22);
            this.comboBoxUnit.TabIndex = 2;
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(346, 4);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(61, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Visible = false;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Location = new System.Drawing.Point(254, 4);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(75, 23);
            this.buttonReset.TabIndex = 4;
            this.buttonReset.Text = "reset";
            this.buttonReset.UseVisualStyleBackColor = true;
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // foFeatureTimeKeeper
            // 
            this.AcceptButton = this.buttonRecalc;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(419, 318);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.comboBoxUnit);
            this.Controls.Add(this.buttonRecalc);
            this.Controls.Add(this.listView1);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "foFeatureTimeKeeper";
            this.Text = "Feature Time Keeper";
            this.Load += new System.EventHandler(this.foFeatureTimeKeeper_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button buttonRecalc;
		private System.Windows.Forms.ColumnHeader columnHeaderFeatureName;
		private System.Windows.Forms.ColumnHeader columnHeaderID;
		private System.Windows.Forms.ColumnHeader columnHeaderRamda;
		private System.Windows.Forms.ColumnHeader columnHeaderMu;
		private System.Windows.Forms.ColumnHeader columnHeaderN;
		private System.Windows.Forms.ComboBox comboBoxUnit;
		private System.Windows.Forms.Button buttonClose;
		private System.Windows.Forms.ColumnHeader columnHeaderType;
		private System.Windows.Forms.Button buttonReset;
	}
}