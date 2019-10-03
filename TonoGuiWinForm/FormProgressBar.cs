using System;
using System.Collections;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// foProgressBar の概要の説明です。
    /// </summary>
    public class FormProgressBar : Form
    {
        private ProgressBar progressBar1;
        private readonly Control _parent = null;
        private long _preTick = 0;
        private int _prePos = 0;
        private int _progCount = 0;
        private readonly IList _leftVal = new ArrayList();
        private string _leftForm;
        private long _firstTick;

        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private readonly System.ComponentModel.Container components = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parent"></param>
        public FormProgressBar(Control parent)
        {
            InitializeComponent();
            _parent = parent;
            Show();
        }

        /// <summary>
        /// ロード
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (_parent != null)
            {
#if DEBUG
                Left = 0;
                Top = 0;
#else
                System.Drawing.Point sp = _parent.PointToScreen(_parent.Location);
				this.Left = sp.X + _parent.Width / 2 - this.Width / 2;
				this.Top  = sp.Y + _parent.Height / 2 - this.Height / 2;
#endif
            }
            if (Mes.Current != null)
            {
                Mes.Current.ResetText(this);
            }
        }


        /// <summary>
        /// バーの位置をセットする
        /// </summary>
        public int Pos
        {
            get => progressBar1.Value;
            set
            {
                if (_preTick == 0)
                {
                    _firstTick = _preTick = DateTime.Now.Ticks;
                    _prePos = value;
                    _leftVal.Clear();
                    if (Mes.Current != null)
                    {
                        _leftForm = Mes.Current["WaitingLeftText"];
                    }
                    else
                    {
                        _leftForm = "Left @left@ Sec.  (@past@ Sec. elapsed.)";
                    }
                }
                else
                {
                    var now = DateTime.Now.Ticks;
                    var d = now - _preTick;
                    if (d > 9000000)
                    {
                        _progCount++;
                        // N/秒
                        var p = (value - _prePos) / ((double)d / 10000000);
                        _leftVal.Add(p);
                        if (_leftVal.Count > 24)
                        {
                            _leftVal.RemoveAt(0);
                        }
                        if (_progCount > 10)
                        {
                            p = 0;
                            foreach (double pp in _leftVal)
                            {
                                p += pp;
                            }
                            p /= _leftVal.Count;
                            // 残り何N
                            double leftN = Max - value;
                            var dd = leftN / p;
                            var s = _leftForm.Replace("@left@", ((int)Math.Floor(dd)).ToString());
                            s = s.Replace("@past@", ((now - _firstTick) / 10000000).ToString());
                            Text = s;
                        }

                        _prePos = value;
                        _preTick = now;
                    }
                }
                progressBar1.Value = value;
            }
        }

        /// <summary>
        /// バーの位置をセットする
        /// </summary>
        public int Max
        {
            get => progressBar1.Maximum;
            set
            {
                progressBar1.Maximum = value;
                progressBar1.Step = (progressBar1.Maximum - progressBar1.Minimum) / 50;
            }
        }

        /// <summary>
        /// バーの位置をセットする
        /// </summary>
        public int Min
        {
            get => progressBar1.Minimum;
            set
            {
                progressBar1.Minimum = value;
                progressBar1.Step = (progressBar1.Maximum - progressBar1.Minimum) / 50;
            }
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

        #region Windows フォーム デザイナで生成されたコード 
        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgressBar));
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(8, 5);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(320, 24);
            this.progressBar1.TabIndex = 0;
            // 
            // foProgressBar
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(338, 34);
            this.ControlBox = false;
            this.Controls.Add(this.progressBar1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "foProgressBar";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Wait a moment...";
            this.TopMost = true;
            this.ResumeLayout(false);

        }
        #endregion
    }
}
