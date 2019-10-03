namespace Tono.GuiWinForm
{
	partial class TLogGroupPanelControlVersion
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

		#region コンポーネント デザイナで生成されたコード

		/// <summary> 
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を 
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ColumnHeader columnHeaderType;
			System.Windows.Forms.ColumnHeader columnHeaderTime;
			System.Windows.Forms.ColumnHeader columnHeaderMessage;
			this.checkBoxLogDev = new System.Windows.Forms.CheckBox();
			this.checkBoxLogErr = new System.Windows.Forms.CheckBox();
			this.checkBoxLogWar = new System.Windows.Forms.CheckBox();
			this.checkBoxLogInfo = new System.Windows.Forms.CheckBox();
			this.listViewLogView = new System.Windows.Forms.ListView();
			this.timerLog = new System.Windows.Forms.Timer(this.components);
			this.checkBoxAutoScroll = new System.Windows.Forms.CheckBox();
			this.buttonNotepad = new System.Windows.Forms.Button();
			columnHeaderType = new System.Windows.Forms.ColumnHeader();
			columnHeaderTime = new System.Windows.Forms.ColumnHeader();
			columnHeaderMessage = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// columnHeaderType
			// 
			columnHeaderType.Text = "T";
			columnHeaderType.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// columnHeaderTime
			// 
			columnHeaderTime.Text = "Time";
			// 
			// columnHeaderMessage
			// 
			columnHeaderMessage.Text = "Message";
			columnHeaderMessage.Width = 255;
			// 
			// checkBoxLogDev
			// 
			this.checkBoxLogDev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxLogDev.AutoSize = true;
			this.checkBoxLogDev.CausesValidation = false;
			this.checkBoxLogDev.Checked = true;
			this.checkBoxLogDev.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLogDev.Location = new System.Drawing.Point(215, 338);
			this.checkBoxLogDev.Name = "checkBoxLogDev";
			this.checkBoxLogDev.Size = new System.Drawing.Size(87, 16);
			this.checkBoxLogDev.TabIndex = 4;
			this.checkBoxLogDev.Text = "Developpers";
			this.checkBoxLogDev.UseVisualStyleBackColor = true;
			this.checkBoxLogDev.CheckedChanged += new System.EventHandler(this.checkBoxLogDev_CheckedChanged);
			// 
			// checkBoxLogErr
			// 
			this.checkBoxLogErr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxLogErr.AutoSize = true;
			this.checkBoxLogErr.CausesValidation = false;
			this.checkBoxLogErr.Checked = true;
			this.checkBoxLogErr.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLogErr.Location = new System.Drawing.Point(160, 338);
			this.checkBoxLogErr.Name = "checkBoxLogErr";
			this.checkBoxLogErr.Size = new System.Drawing.Size(49, 16);
			this.checkBoxLogErr.TabIndex = 5;
			this.checkBoxLogErr.Text = "Error";
			this.checkBoxLogErr.UseVisualStyleBackColor = true;
			this.checkBoxLogErr.CheckedChanged += new System.EventHandler(this.checkBoxLogErr_CheckedChanged);
			// 
			// checkBoxLogWar
			// 
			this.checkBoxLogWar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxLogWar.AutoSize = true;
			this.checkBoxLogWar.CausesValidation = false;
			this.checkBoxLogWar.Checked = true;
			this.checkBoxLogWar.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLogWar.Location = new System.Drawing.Point(90, 338);
			this.checkBoxLogWar.Name = "checkBoxLogWar";
			this.checkBoxLogWar.Size = new System.Drawing.Size(64, 16);
			this.checkBoxLogWar.TabIndex = 6;
			this.checkBoxLogWar.Text = "Warning";
			this.checkBoxLogWar.UseVisualStyleBackColor = true;
			this.checkBoxLogWar.CheckedChanged += new System.EventHandler(this.checkBoxLogWar_CheckedChanged);
			// 
			// checkBoxLogInfo
			// 
			this.checkBoxLogInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.checkBoxLogInfo.AutoSize = true;
			this.checkBoxLogInfo.CausesValidation = false;
			this.checkBoxLogInfo.Checked = true;
			this.checkBoxLogInfo.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLogInfo.Location = new System.Drawing.Point(3, 338);
			this.checkBoxLogInfo.Name = "checkBoxLogInfo";
			this.checkBoxLogInfo.Size = new System.Drawing.Size(81, 16);
			this.checkBoxLogInfo.TabIndex = 3;
			this.checkBoxLogInfo.Text = "Information";
			this.checkBoxLogInfo.UseVisualStyleBackColor = true;
			this.checkBoxLogInfo.CheckedChanged += new System.EventHandler(this.checkBoxLogInfo_CheckedChanged);
			// 
			// listViewLogView
			// 
			this.listViewLogView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewLogView.BackColor = System.Drawing.SystemColors.Window;
			this.listViewLogView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.listViewLogView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeaderType,
            columnHeaderTime,
            columnHeaderMessage});
			this.listViewLogView.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.listViewLogView.FullRowSelect = true;
			this.listViewLogView.GridLines = true;
			this.listViewLogView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listViewLogView.LabelWrap = false;
			this.listViewLogView.Location = new System.Drawing.Point(3, 3);
			this.listViewLogView.MultiSelect = false;
			this.listViewLogView.Name = "listViewLogView";
			this.listViewLogView.Size = new System.Drawing.Size(548, 325);
			this.listViewLogView.TabIndex = 2;
			this.listViewLogView.UseCompatibleStateImageBehavior = false;
			this.listViewLogView.View = System.Windows.Forms.View.Details;
			// 
			// timerLog
			// 
			this.timerLog.Enabled = true;
			this.timerLog.Interval = 500;
			this.timerLog.Tick += new System.EventHandler(this.timerLog_Tick);
			// 
			// checkBoxAutoScroll
			// 
			this.checkBoxAutoScroll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.checkBoxAutoScroll.AutoSize = true;
			this.checkBoxAutoScroll.Location = new System.Drawing.Point(470, 338);
			this.checkBoxAutoScroll.Name = "checkBoxAutoScroll";
			this.checkBoxAutoScroll.Size = new System.Drawing.Size(81, 16);
			this.checkBoxAutoScroll.TabIndex = 7;
			this.checkBoxAutoScroll.Text = "Auto Scroll";
			this.checkBoxAutoScroll.UseVisualStyleBackColor = true;
			// 
			// buttonNotepad
			// 
			this.buttonNotepad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonNotepad.Image = global::Tono.GuiWinForm.Properties.Resources.toNotepad;
			this.buttonNotepad.Location = new System.Drawing.Point(387, 334);
			this.buttonNotepad.Name = "buttonNotepad";
			this.buttonNotepad.Size = new System.Drawing.Size(65, 23);
			this.buttonNotepad.TabIndex = 8;
			this.buttonNotepad.Text = "Open";
			this.buttonNotepad.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.buttonNotepad.UseVisualStyleBackColor = true;
			this.buttonNotepad.Click += new System.EventHandler(this.buttonNotepad_Click);
			// 
			// cLogGroupPanelControlVersion
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.buttonNotepad);
			this.Controls.Add(this.checkBoxAutoScroll);
			this.Controls.Add(this.checkBoxLogDev);
			this.Controls.Add(this.checkBoxLogErr);
			this.Controls.Add(this.checkBoxLogWar);
			this.Controls.Add(this.checkBoxLogInfo);
			this.Controls.Add(this.listViewLogView);
			this.Name = "cLogGroupPanelControlVersion";
			this.Size = new System.Drawing.Size(554, 357);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox checkBoxLogDev;
		private System.Windows.Forms.CheckBox checkBoxLogErr;
		private System.Windows.Forms.CheckBox checkBoxLogWar;
		private System.Windows.Forms.CheckBox checkBoxLogInfo;
		private System.Windows.Forms.ListView listViewLogView;
		private System.Windows.Forms.Timer timerLog;
		private System.Windows.Forms.CheckBox checkBoxAutoScroll;
		private System.Windows.Forms.Button buttonNotepad;
	}
}
