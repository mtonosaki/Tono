// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FormFileSelector の概要の説明です。
    /// </summary>
    public class FormFileSelector : System.Windows.Forms.Form
    {
        #region IconLoaderクラス
        public class IconLoader
        {
            #region DLL実装
            [DllImport("Shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern uint ExtractIconEx(
                [MarshalAs(UnmanagedType.LPTStr)] string lpszFile,
                int nIconIndex,
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] phiconLarge,
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] phiconSmall,
                uint nIcons);

            [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern bool DestroyIcon(IntPtr hIcon);
            #endregion

            #region 属性（シリアライズする）
            /// <summary>拡張子とアイコン番号の組合せ</summary>
            private readonly IDictionary _dat = new Hashtable();

            /// <summary>ラージアイコン</summary>
            private readonly ImageList _large = new ImageList();

            /// <summary>スモールアイコン</summary>
            private readonly ImageList _small = new ImageList();

            private readonly IDictionary _resid = new Hashtable();

            #endregion

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="appIcons"></param>
            public IconLoader(ImageList appIcons)
            {
                ImageList li = new ImageList
                {
                    ImageSize = SystemInformation.IconSize,
                    TransparentColor = Color.Transparent,
                    ColorDepth = ColorDepth.Depth32Bit
                }, si = new ImageList
                {
                    ImageSize = SystemInformation.SmallIconSize,
                    TransparentColor = Color.Transparent,
                    ColorDepth = ColorDepth.Depth32Bit
                };
                GetImageList(@"C:\WINNT\System32\shell32.dll", ref li, ref si);

                // 空ファイル
                _large.Images.Add(li.Images[0]);
                _small.Images.Add(si.Images[0]);
                _dat["E.."] = 0;

                // フォルダ
                _large.Images.Add(li.Images[3]);
                _small.Images.Add(si.Images[3]);
                _dat["DC.."] = 1;
                _dat["D.."] = 1;

                // フォルダ（開く）
                _large.Images.Add(li.Images[4]);
                _small.Images.Add(si.Images[4]);
                _dat["DO.."] = 2;

                // 未登録アプリ
                _large.Images.Add(li.Images[2]);
                _small.Images.Add(si.Images[2]);
                _dat[".exe"] = 3;

                // 親ディレクトリ
                _large.Images.Add(appIcons.Images[0]);
                _small.Images.Add(appIcons.Images[0]);
                _dat["DP.."] = 4;

                // Shell32.dllのアイコンリソース番号とアイコン連番の変換表
                _resid[-33] = 32;
                _resid[-137] = 57;
                _resid[-138] = 58;
                _resid[-151] = 69;
                _resid[-152] = 70;
                _resid[-153] = 71;
                _resid[-154] = 72;
                _resid[-155] = 73;
                _resid[-157] = 75;
                _resid[-173] = 86;
                _resid[-178] = 91;
            }

            private int addIcon(string filename)
            {
                ImageList li = new ImageList
                {
                    ImageSize = new Size(32, 32),
                    TransparentColor = Color.Transparent,
                    ColorDepth = ColorDepth.Depth32Bit
                }, si = new ImageList
                {
                    ImageSize = new Size(16, 16),
                    TransparentColor = Color.Transparent,
                    ColorDepth = ColorDepth.Depth32Bit
                };

                var ext = Path.GetExtension(filename).ToLower();
                if (ext == ".exe")
                {
                    // exe拡張子
                    GetImageList(filename, ref li, ref si);
                    if (li.Images.Count > 0)
                    {
                        _large.Images.Add(li.Images[0]);
                        _small.Images.Add(si.Images[0]);
                        var ret = _large.Images.Count - 1;
                        _dat[Path.GetFileName(filename)] = ret;
                        return ret;
                    }
                }
                else
                {
                    // その他の拡張子
                    try
                    {
                        var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
                        var type = key.GetValue("").ToString();
                        var keytype = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(type + @"\DefaultIcon");
                        var ipath = keytype.GetValue("").ToString();
                        var fns = ipath.Split(new char[] { ',' });
                        GetImageList(fns[0], ref li, ref si);
                        var ino = fns.Length == 0 ? 0 : (int.Parse(fns[1]));
                        if (ipath.ToLower().IndexOf("shell32.dll") >= 0 && _resid[ino] != null)
                        {
                            ino = (int)_resid[ino];
                        }
                        if (li.Images.Count < ino || ino < 0)
                        {
                            _dat[ext] = 0;
                            return 0;
                        }
                        _large.Images.Add(li.Images[ino]);
                        _small.Images.Add(si.Images[ino]);
                        var ret = _large.Images.Count - 1;
                        _dat[ext] = ret;
                        return ret;
                    }
                    catch (Exception)
                    {
                        _dat[ext] = 0;
                        return 0;
                    }
                }
                return 0;
            }

            /// <summary>
            /// 指定拡張子のイメージ番号を取得する
            /// </summary>
            /// <param name="ext">拡張子（最後の.は含める 例： .exe）</param>
            /// <returns>イメージ番号</returns>
            public int GetImageNo(string filename)
            {
                string ext;
                if (filename.EndsWith(".."))
                {
                    ext = filename;
                }
                else
                {
                    ext = Path.GetExtension(filename).ToLower();
                }
                object no;
                if (ext == ".exe")
                {
                    no = _dat[Path.GetFileName(filename).ToLower()];
                    if (no == null)
                    {
                        var r = addIcon(filename);
                        if (r < 0)
                        {
                            no = _dat[".exe"];
                        }
                        else
                        {
                            no = r;
                        }
                    }
                }
                else
                {
                    no = _dat[ext];
                    if (no == null)
                    {
                        no = addIcon(ext);
                    }
                }
                if (no == null)
                {
                    return (int)_dat["E.."];
                }
                else
                {
                    return (int)no;
                }
            }

            /// <summary>
            /// ラージアイコン
            /// </summary>
            public ImageList Large => _large;

            /// <summary>
            /// スモールアイコン
            /// </summary>
            public ImageList Small => _small;

            /// <summary>
            /// 指定ファイル名のイメージリストを取得する
            /// </summary>
            /// <param name="filename">ファイル名</param>
            /// <param name="large">ラージアイコン</param>
            /// <param name="small">スモールアイコン</param>
            public static void GetImageList(string fileName, ref ImageList large, ref ImageList small)
            {
                var uIconCount = ExtractIconEx(fileName, -1, new IntPtr[] { IntPtr.Zero }, new IntPtr[] { IntPtr.Zero }, 0);

                if (uIconCount > 0)
                {
                    var hLarge = new IntPtr[uIconCount];
                    var hSmall = new IntPtr[uIconCount];
                    var uRet = ExtractIconEx(fileName, 0, hLarge, hSmall, uIconCount);
                    large.Images.Clear();
                    large.ImageSize = SystemInformation.IconSize;
                    large.ColorDepth = ColorDepth.Depth32Bit;
                    foreach (var hIcon in hLarge)
                    {
                        if (hIcon.Equals(IntPtr.Zero) == false)
                        {
                            var ic = new Icon(Icon.FromHandle(hIcon), SystemInformation.IconSize);
                            large.Images.Add(ic);
                            DestroyIcon(hIcon);
                        }
                    }
                    small.Images.Clear();
                    small.ImageSize = SystemInformation.SmallIconSize;
                    small.ColorDepth = ColorDepth.Depth32Bit;
                    foreach (var hIcon in hSmall)
                    {
                        if (hIcon.Equals(IntPtr.Zero) == false)
                        {
                            small.Images.Add((Icon)Icon.FromHandle(hIcon).Clone());
                            DestroyIcon(hIcon);
                        }
                    }
                }
            }
        }

        #endregion

        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelSelectorFilename;
        private System.Windows.Forms.ListView listViewFileSelect;
        private System.Windows.Forms.ImageList imageListLarge;
        private System.Windows.Forms.ImageList imageListSmall;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.TextBox textBoxFileSelectorFilename;
        private System.Windows.Forms.TextBox textBoxFileSelectorPath;
        private System.Windows.Forms.Label labelSelectorPath;
        private System.Windows.Forms.CheckBox checkBoxFileSelectorLarge;
        private System.Windows.Forms.CheckBox checkBoxFileSelectorSmall;
        private System.Windows.Forms.CheckBox checkBoxFileSelectorList;
        private System.Windows.Forms.CheckBox checkBoxFileSelectorDetail;
        private System.Windows.Forms.ImageList imageListApp;
        private IconLoader _icons = null;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderLastModify;
        private System.Windows.Forms.Button buttonBack;
        private readonly IDictionary _highlightExt = new Hashtable();
        private System.Windows.Forms.Button buttonForward;
        private readonly IList _hist = new ArrayList();
        private System.Windows.Forms.ComboBox comboBoxDrive;
        private int _histpos = 0;
        private static readonly System.Threading.Mutex _mute = new System.Threading.Mutex();
        private readonly IDictionary _driveCurrentPath = new Hashtable();
        private string _filename = "";
        private string _path;
        private string _searchptn = "*.*";
        private bool _isCreateDriveList = false;
        private bool _isEnd = false;

        protected override void OnClosing(CancelEventArgs e)
        {
            _filename = textBoxFileSelectorPath.Text + "\\" + textBoxFileSelectorFilename.Text;
            base.OnClosing(e);
        }

        /// <summary>
        /// 表示ファイルフィルタ  例：  *.doc|*.rtf|*.txt
        /// </summary>
        public string Filter
        {
            set => _searchptn = value;
        }

        /// <summary>
        /// 終わったか？
        /// </summary>
        public bool IsEnd => _isEnd;

        /// <summary>
        /// ファイル名を取得する
        /// </summary>
        public string FileName
        {
            get => _filename;
            set
            {
                _filename = value;
                _path = Path.GetDirectoryName(_filename);
                _filename = Path.GetFileName(_filename);
                textBoxFileSelectorPath.Text = _path;
                textBoxFileSelectorFilename.Text = _filename;
            }
        }



        /// <summary>
        /// ハイライトにする拡張子を追加する
        /// </summary>
        /// <param name="ext"></param>
        public void AddHighlightExt(string ext)
        {
            _highlightExt[ext.ToLower()] = true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private FormFileSelector()
        {
            //
            // Windows フォーム デザイナ サポートに必要です。
            //
            InitializeComponent();
        }

        /// <summary>
        /// 使用禁止
        /// </summary>
        public new void Show()
        {
        }

        /// <summary>
        /// 使用禁止
        /// </summary>
        public new DialogResult ShowDialog()
        {
            System.Diagnostics.Debug.Assert(false, "フィーチャーのFileSelector.ShowDialog()を使用してください");
            return DialogResult.Cancel;
        }
        /// <summary>
        /// 使用禁止
        /// </summary>
        public new DialogResult ShowDialog(IWin32Window dummy)
        {
            System.Diagnostics.Debug.Assert(false, "フィーチャーのFileSelector.ShowDialog()を使用してください");
            return DialogResult.Cancel;
        }


        private DialogResult showDialog()
        {
            return base.ShowDialog();
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFileSelector));
            listViewFileSelect = new System.Windows.Forms.ListView();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            columnHeaderLastModify = new System.Windows.Forms.ColumnHeader();
            buttonOK = new System.Windows.Forms.Button();
            textBoxFileSelectorFilename = new System.Windows.Forms.TextBox();
            labelSelectorFilename = new System.Windows.Forms.Label();
            imageListLarge = new System.Windows.Forms.ImageList(components);
            imageListSmall = new System.Windows.Forms.ImageList(components);
            textBoxFileSelectorPath = new System.Windows.Forms.TextBox();
            labelSelectorPath = new System.Windows.Forms.Label();
            checkBoxFileSelectorLarge = new System.Windows.Forms.CheckBox();
            checkBoxFileSelectorSmall = new System.Windows.Forms.CheckBox();
            checkBoxFileSelectorList = new System.Windows.Forms.CheckBox();
            checkBoxFileSelectorDetail = new System.Windows.Forms.CheckBox();
            imageListApp = new System.Windows.Forms.ImageList(components);
            buttonBack = new System.Windows.Forms.Button();
            buttonForward = new System.Windows.Forms.Button();
            comboBoxDrive = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // listViewFileSelect
            // 
            listViewFileSelect.AllowColumnReorder = true;
            listViewFileSelect.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right);
            listViewFileSelect.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeaderName,
            columnHeaderSize,
            columnHeaderLastModify});
            listViewFileSelect.FullRowSelect = true;
            listViewFileSelect.HideSelection = false;
            listViewFileSelect.Location = new System.Drawing.Point(8, 72);
            listViewFileSelect.Name = "listViewFileSelect";
            listViewFileSelect.Size = new System.Drawing.Size(584, 272);
            listViewFileSelect.TabIndex = 0;
            listViewFileSelect.UseCompatibleStateImageBehavior = false;
            listViewFileSelect.View = System.Windows.Forms.View.List;
            listViewFileSelect.SelectedIndexChanged += new System.EventHandler(listViewFileSelect_SelectedIndexChanged);
            listViewFileSelect.DoubleClick += new System.EventHandler(listViewFileSelect_DoubleClick);
            listViewFileSelect.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            listViewFileSelect.MouseDown += new System.Windows.Forms.MouseEventHandler(fFileSelector_MouseDown);
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 316;
            // 
            // columnHeaderSize
            // 
            columnHeaderSize.Text = "Size";
            columnHeaderSize.Width = 97;
            // 
            // columnHeaderLastModify
            // 
            columnHeaderLastModify.Text = "LastModify";
            columnHeaderLastModify.Width = 128;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
            buttonOK.Location = new System.Drawing.Point(512, 376);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(80, 24);
            buttonOK.TabIndex = 3;
            buttonOK.Text = "OK";
            buttonOK.Click += new System.EventHandler(buttonOK_Click);
            buttonOK.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // textBoxFileSelectorFilename
            // 
            textBoxFileSelectorFilename.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right);
            textBoxFileSelectorFilename.Location = new System.Drawing.Point(96, 376);
            textBoxFileSelectorFilename.Name = "textBoxFileSelectorFilename";
            textBoxFileSelectorFilename.Size = new System.Drawing.Size(408, 19);
            textBoxFileSelectorFilename.TabIndex = 2;
            textBoxFileSelectorFilename.TextChanged += new System.EventHandler(textBoxFileSelectorFilename_TextChanged);
            textBoxFileSelectorFilename.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // labelSelectorFilename
            // 
            labelSelectorFilename.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            labelSelectorFilename.Location = new System.Drawing.Point(0, 376);
            labelSelectorFilename.Name = "labelSelectorFilename";
            labelSelectorFilename.Size = new System.Drawing.Size(88, 24);
            labelSelectorFilename.TabIndex = 1;
            labelSelectorFilename.Text = "File name";
            labelSelectorFilename.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // imageListLarge
            // 
            imageListLarge.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListLarge.ImageSize = new System.Drawing.Size(32, 32);
            imageListLarge.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // imageListSmall
            // 
            imageListSmall.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageListSmall.ImageSize = new System.Drawing.Size(16, 16);
            imageListSmall.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // textBoxFileSelectorPath
            // 
            textBoxFileSelectorPath.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right);
            textBoxFileSelectorPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            textBoxFileSelectorPath.Location = new System.Drawing.Point(96, 352);
            textBoxFileSelectorPath.Name = "textBoxFileSelectorPath";
            textBoxFileSelectorPath.ReadOnly = true;
            textBoxFileSelectorPath.Size = new System.Drawing.Size(496, 19);
            textBoxFileSelectorPath.TabIndex = 12;
            textBoxFileSelectorPath.WordWrap = false;
            textBoxFileSelectorPath.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // labelSelectorPath
            // 
            labelSelectorPath.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left);
            labelSelectorPath.Location = new System.Drawing.Point(0, 352);
            labelSelectorPath.Name = "labelSelectorPath";
            labelSelectorPath.Size = new System.Drawing.Size(88, 24);
            labelSelectorPath.TabIndex = 11;
            labelSelectorPath.Text = "Path";
            labelSelectorPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // checkBoxFileSelectorLarge
            // 
            checkBoxFileSelectorLarge.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            checkBoxFileSelectorLarge.Appearance = System.Windows.Forms.Appearance.Button;
            checkBoxFileSelectorLarge.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxFileSelectorLarge.Image")));
            checkBoxFileSelectorLarge.Location = new System.Drawing.Point(496, 4);
            checkBoxFileSelectorLarge.Name = "checkBoxFileSelectorLarge";
            checkBoxFileSelectorLarge.Size = new System.Drawing.Size(24, 24);
            checkBoxFileSelectorLarge.TabIndex = 7;
            checkBoxFileSelectorLarge.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBoxFileSelectorLarge.Click += new System.EventHandler(checkBoxFileSelectorLarge_Click);
            checkBoxFileSelectorLarge.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // checkBoxFileSelectorSmall
            // 
            checkBoxFileSelectorSmall.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            checkBoxFileSelectorSmall.Appearance = System.Windows.Forms.Appearance.Button;
            checkBoxFileSelectorSmall.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxFileSelectorSmall.Image")));
            checkBoxFileSelectorSmall.Location = new System.Drawing.Point(520, 4);
            checkBoxFileSelectorSmall.Name = "checkBoxFileSelectorSmall";
            checkBoxFileSelectorSmall.Size = new System.Drawing.Size(24, 24);
            checkBoxFileSelectorSmall.TabIndex = 8;
            checkBoxFileSelectorSmall.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBoxFileSelectorSmall.Click += new System.EventHandler(checkBoxFileSelectorSmall_Click);
            checkBoxFileSelectorSmall.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // checkBoxFileSelectorList
            // 
            checkBoxFileSelectorList.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            checkBoxFileSelectorList.Appearance = System.Windows.Forms.Appearance.Button;
            checkBoxFileSelectorList.Checked = true;
            checkBoxFileSelectorList.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxFileSelectorList.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxFileSelectorList.Image")));
            checkBoxFileSelectorList.Location = new System.Drawing.Point(544, 4);
            checkBoxFileSelectorList.Name = "checkBoxFileSelectorList";
            checkBoxFileSelectorList.Size = new System.Drawing.Size(24, 24);
            checkBoxFileSelectorList.TabIndex = 9;
            checkBoxFileSelectorList.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBoxFileSelectorList.Click += new System.EventHandler(checkBoxFileSelectorList_Click);
            checkBoxFileSelectorList.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // checkBoxFileSelectorDetail
            // 
            checkBoxFileSelectorDetail.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
            checkBoxFileSelectorDetail.Appearance = System.Windows.Forms.Appearance.Button;
            checkBoxFileSelectorDetail.Image = ((System.Drawing.Image)(resources.GetObject("checkBoxFileSelectorDetail.Image")));
            checkBoxFileSelectorDetail.Location = new System.Drawing.Point(568, 4);
            checkBoxFileSelectorDetail.Name = "checkBoxFileSelectorDetail";
            checkBoxFileSelectorDetail.Size = new System.Drawing.Size(24, 24);
            checkBoxFileSelectorDetail.TabIndex = 10;
            checkBoxFileSelectorDetail.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            checkBoxFileSelectorDetail.Click += new System.EventHandler(checkBoxFileSelectorDetail_Click);
            checkBoxFileSelectorDetail.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // imageListApp
            // 
            imageListApp.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListApp.ImageStream")));
            imageListApp.TransparentColor = System.Drawing.Color.Transparent;
            imageListApp.Images.SetKeyName(0, "");
            // 
            // buttonBack
            // 
            buttonBack.Enabled = false;
            buttonBack.Image = ((System.Drawing.Image)(resources.GetObject("buttonBack.Image")));
            buttonBack.Location = new System.Drawing.Point(10, 4);
            buttonBack.Name = "buttonBack";
            buttonBack.Size = new System.Drawing.Size(30, 24);
            buttonBack.TabIndex = 4;
            buttonBack.Click += new System.EventHandler(buttonBack_Click);
            buttonBack.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // buttonForward
            // 
            buttonForward.Enabled = false;
            buttonForward.Image = ((System.Drawing.Image)(resources.GetObject("buttonForward.Image")));
            buttonForward.Location = new System.Drawing.Point(40, 4);
            buttonForward.Name = "buttonForward";
            buttonForward.Size = new System.Drawing.Size(30, 24);
            buttonForward.TabIndex = 5;
            buttonForward.Click += new System.EventHandler(buttonForward_Click);
            buttonForward.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // comboBoxDrive
            // 
            comboBoxDrive.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBoxDrive.Location = new System.Drawing.Point(88, 8);
            comboBoxDrive.MaxDropDownItems = 16;
            comboBoxDrive.Name = "comboBoxDrive";
            comboBoxDrive.Size = new System.Drawing.Size(256, 20);
            comboBoxDrive.TabIndex = 6;
            comboBoxDrive.DropDown += new System.EventHandler(comboBoxDrive_DropDown);
            comboBoxDrive.SelectionChangeCommitted += new System.EventHandler(comboBoxDrive_SelectionChangeCommitted);
            comboBoxDrive.KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            // 
            // foFileSelector
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 12);
            ClientSize = new System.Drawing.Size(600, 405);
            Controls.Add(comboBoxDrive);
            Controls.Add(buttonBack);
            Controls.Add(checkBoxFileSelectorLarge);
            Controls.Add(labelSelectorFilename);
            Controls.Add(textBoxFileSelectorFilename);
            Controls.Add(textBoxFileSelectorPath);
            Controls.Add(buttonOK);
            Controls.Add(listViewFileSelect);
            Controls.Add(labelSelectorPath);
            Controls.Add(checkBoxFileSelectorSmall);
            Controls.Add(checkBoxFileSelectorList);
            Controls.Add(checkBoxFileSelectorDetail);
            Controls.Add(buttonForward);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "foFileSelector";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Select the file.";
            KeyUp += new System.Windows.Forms.KeyEventHandler(fFileSelector_KeyUp);
            MouseDown += new System.Windows.Forms.MouseEventHandler(fFileSelector_MouseDown);
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        /// <summary>
        /// リストビューを更新する
        /// </summary>
        private void reset(bool isHistAdd)
        {
            // 履歴管理
            if (isHistAdd)
            {
                for (var hi = 0; hi < _hist.Count - _histpos - 1; hi++)
                {
                    _hist.RemoveAt(_hist.Count - 1);
                }
                _hist.Add(_path);
                _histpos = _hist.Count - 1;
            }
            buttonBack.Enabled = _histpos > 0;
            buttonForward.Enabled = _histpos < _hist.Count - 1;
            textBoxFileSelectorPath.Text = _path;
            listViewFileSelect.Items.Clear();

            // パス変更による、OKボタンの無効化
            textBoxFileSelectorFilename_TextChanged(null, null);

            // コンボボックスの変更
            for (var coni = 0; coni < comboBoxDrive.Items.Count; coni++)
            {
                var ci = comboBoxDrive.Items[coni];
                if (ci.ToString().StartsWith(_path.ToUpper().Substring(0, 2)))
                {
                    comboBoxDrive.SelectedIndex = coni;
                    break;
                }
            }

            if (comboBoxDrive.SelectedItem == null || _path.ToUpper().StartsWith(comboBoxDrive.SelectedItem.ToString().Substring(0, 2)) == false)
            {
                comboBoxDrive.Items.Clear();

                // ドライブ一覧作成
                var drvs = Directory.GetLogicalDrives();
                foreach (var drive in drvs)
                {
                    var d = drive.Substring(0, drive.Length - 1);
                    if (_path.ToUpper().IndexOf(d) >= 0)
                    {
                        var disk = new System.Management.ManagementObject("win32_logicaldisk.deviceid=\"" + d + "\"");
                        disk.Get();
                        var vn = disk["VolumeName"];
                        var s = d + (vn != null ? " (" + vn.ToString() + ")" : "");
                        var ds = disk["Description"];
                        if (ds != null)
                        {
                            s += "   " + ds.ToString();
                        }
                        comboBoxDrive.Items.Add(s);
                        comboBoxDrive.SelectedIndex = 0;
                        break;
                    }
                }
            }

            // ドライブが見つからない場合
            if (_path.EndsWith(":"))
            {
                return;
            }

            Cursor = Cursors.WaitCursor;
            _driveCurrentPath[_path.Substring(0, 2)] = _path;

            // 特殊ディレクトリ
            if (_path.EndsWith(@":\"))
            {
                // ドライブ
            }
            else
            {
                // 親ディレクトリ
                var li = new ListViewItem(Mes.Current["fFileSelector.ParentDirectory"], _icons.GetImageNo("DP.."))
                {
                    ForeColor = Color.FromArgb(0, 64, 128),
                    Tag = ".."
                };
                listViewFileSelect.Items.Add(li);
            }

            // 各ディレクトリ
            IList seldir = new ArrayList();
            var dirs = Directory.GetDirectories(_path);
            foreach (var dir in dirs)
            {
                // 表示不必要なものを除去
                var di = new DirectoryInfo(dir);
                if ((di.Attributes & FileAttributes.Hidden) != 0)
                {
                    continue;
                }

                if ((di.Attributes & FileAttributes.System) != 0)
                {
                    continue;
                }

                if ((di.Attributes & FileAttributes.Temporary) != 0)
                {
                    continue;
                }

                if ((di.Attributes & FileAttributes.SparseFile) != 0)
                {
                    continue;
                }

                seldir.Add(dir);

            }
            var lisD = new ListViewItem[seldir.Count];
            var ino = _icons.GetImageNo("D..");
            var i = 0;
            foreach (string dir in seldir)
            {
                lisD[i] = new ListViewItem(Path.GetFileName(dir), ino)
                {
                    Tag = "...." + dir,
                    ForeColor = Color.FromArgb(0, 64, 128)
                };
                i++;
            }
            listViewFileSelect.Items.AddRange(lisD);

            // ファイル
            seldir.Clear();
            var filters = _searchptn.Split(new char[] { '|' });
            for (var fili = 0; fili < filters.Length; fili++)
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(_path, filters[fili]);
                }
                catch (Exception)
                {
                    continue;
                }
                foreach (var file in files)
                {
                    // 表示不必要なものを除去
                    var fi = new FileInfo(file);
                    if ((fi.Attributes & FileAttributes.Hidden) != 0)
                    {
                        continue;
                    }

                    if ((fi.Attributes & FileAttributes.System) != 0)
                    {
                        continue;
                    }

                    if ((fi.Attributes & FileAttributes.Temporary) != 0)
                    {
                        continue;
                    }

                    if ((fi.Attributes & FileAttributes.SparseFile) != 0)
                    {
                        continue;
                    }

                    seldir.Add(file);
                }
            }
            var filD = new ListViewItem[seldir.Count];
            i = 0;
            foreach (string file in seldir)
            {
                var fi = new FileInfo(file);
                ino = _icons.GetImageNo(file);
                filD[i] = new ListViewItem(Path.GetFileName(file), ino)
                {
                    Tag = fi
                };

                // サブアイテム
                filD[i].SubItems.Add(Const.Formatter.Size(fi.Length));
                filD[i].SubItems.Add(fi.LastWriteTime.ToString("g"));

                if (_highlightExt.Count > 0)
                {
                    if (_highlightExt.Contains(Path.GetExtension(file).ToLower()))
                    {
                        filD[i].ForeColor = Color.Red;
                    }
                    else
                    {
                        filD[i].ForeColor = Color.FromArgb(64, 64, 64);
                    }
                }
                i++;
            }
            listViewFileSelect.Items.AddRange(filD);
            Cursor = Cursors.Arrow;
        }

        // ドライブ一覧作成
        private void createDriveList()
        {
            comboBoxDrive.Items.Clear();
            var drvs = Directory.GetLogicalDrives();
            foreach (var drive in drvs)
            {
                var d = drive.Substring(0, drive.Length - 1);
                var disk = new System.Management.ManagementObject("win32_logicaldisk.deviceid=\"" + d + "\"");
                disk.Get();
                var vn = disk["VolumeName"];
                var s = d + (vn != null ? " (" + vn.ToString() + ")" : "");
                var ds = disk["Description"];
                if (ds != null)
                {
                    s += "   " + ds.ToString();
                }
                comboBoxDrive.Items.Add(s);
            }
        }

        /// <summary>
        /// フォーム初期化
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // パスの初期値を作成する
            if (Directory.Exists(_path) == false)
            {
                _path = Directory.GetCurrentDirectory();
            }

            Cursor = Cursors.WaitCursor;

            // アイコンのセット
            _icons = new IconLoader(imageListApp);
            listViewFileSelect.LargeImageList = _icons.Large;
            listViewFileSelect.SmallImageList = _icons.Small;

            // 内容のリセット
            reset(true);
            Mes.Current.ResetText(this);

            Cursor = Cursors.Arrow;
        }

        private void apply()
        {
            if (_th == null)
            {
                _th = new System.Threading.Thread(new System.Threading.ThreadStart(okProc));
                _th.Start();
            }
        }

        private System.Threading.Thread _th = null;

        private void okProc()
        {
            Opacity = 0;
            System.Threading.Thread.Sleep(100);
            _isEnd = true;
            DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// OKボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            apply();
        }

        /// <summary>
        /// キャンセルボタン
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            _isEnd = true;
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// アイテムをダブルクリック
        /// </summary>
        private void listViewFileSelect_DoubleClick(object sender, System.EventArgs e)
        {
            var res = select();
            if (res)
            {
                apply();
            }
        }

        private bool select()
        {
            return select(null);
        }

        /// <summary>
        /// ファイルセレクト処理
        /// </summary>
        private bool select(string force)
        {
            string s;
            if (force == null)
            {
                if (listViewFileSelect.SelectedItems.Count != 1)
                {
                    return false;
                }
                if (listViewFileSelect.SelectedItems[0].Tag is FileInfo fi)
                {
                    s = fi.FullName;
                }
                else
                {
                    s = listViewFileSelect.SelectedItems[0].Tag.ToString();
                }
            }
            else
            {
                s = force;
            }

            // 親ディレクトリ
            if (s == "..")
            {
                var id = _path.LastIndexOf('\\');
                if (id < 0)
                {
                    return false;
                }
                _path = _path.Substring(0, id);
                if (_path.EndsWith(":"))
                {
                    _path += "\\";
                }
                reset(true);
                return false;
            }

            // 下位フォルダ
            if (s.StartsWith("...."))
            {
                s = s.Substring(4);
                _path = s;
                reset(true);
                return false;
            }

            // ファイル
            textBoxFileSelectorFilename.Text = Path.GetFileName(s);
            return true;
        }

        // 普通に選択
        private void listViewFileSelect_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (listViewFileSelect.SelectedItems.Count != 1)
            {
                return;
            }
            string s;
            if (listViewFileSelect.SelectedItems[0].Tag is FileInfo fi)
            {
                s = fi.FullName;
            }
            else
            {
                s = listViewFileSelect.SelectedItems[0].Tag.ToString();
            }
            if (s.StartsWith("."))
            {
                return;
            }
            textBoxFileSelectorFilename.Text = Path.GetFileName(s);
        }

        /// <summary>
        /// リストビューの種類を変更する
        /// </summary>
        /// <param name="sender">クリックしたチェックボタン</param>
        private void changeMode(object sender)
        {
            foreach (Control c in Controls)
            {
                if (c is CheckBox)
                {
                    ((CheckBox)c).Checked = (c == sender);
                }
            }
        }

        /// <summary>
        /// ラージモード
        /// </summary>
        private void checkBoxFileSelectorLarge_Click(object sender, System.EventArgs e)
        {
            changeMode(sender);
            listViewFileSelect.View = View.LargeIcon;
        }

        /// <summary>
        /// スモールモード
        /// </summary>
        private void checkBoxFileSelectorSmall_Click(object sender, System.EventArgs e)
        {
            changeMode(sender);
            listViewFileSelect.View = View.SmallIcon;
        }

        /// <summary>
        /// リストモード
        /// </summary>
        private void checkBoxFileSelectorList_Click(object sender, System.EventArgs e)
        {
            changeMode(sender);
            listViewFileSelect.View = View.List;
        }

        /// <summary>
        /// 詳細モード
        /// </summary>
        private void checkBoxFileSelectorDetail_Click(object sender, System.EventArgs e)
        {
            changeMode(sender);
            listViewFileSelect.View = View.Details;
        }

        private void fFileSelector_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.XButton1) != 0)
            {
                buttonBack_Click(null, null);
            }
        }

        private void buttonBack_Click(object sender, System.EventArgs e)
        {
            if (_histpos < 1)
            {
                return;
            }
            var s = (string)_hist[--_histpos];
            _path = s;
            reset(false);
        }

        private void buttonForward_Click(object sender, System.EventArgs e)
        {
            if (_histpos >= _hist.Count - 1)
            {
                return;
            }
            _path = (string)_hist[++_histpos];
            reset(false);
        }

        private void textBoxFileSelectorFilename_TextChanged(object sender, System.EventArgs e)
        {
            var fn = textBoxFileSelectorFilename.Text;
            if (fn.IndexOf(":") >= 0 || fn.IndexOf("\\") >= 0)
            {
            }
            else
            {
                fn = textBoxFileSelectorPath.Text + "\\" + fn;
            }
            buttonOK.Enabled = File.Exists(fn);
        }

        /// <summary>
        /// コンボボックスでドライブを変更した時の処理
        /// </summary>
        private void comboBoxDrive_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            var driveF = comboBoxDrive.SelectedItem.ToString();
            var drive = driveF.Substring(0, 2);
            var scd = (string)_driveCurrentPath[drive.Substring(0, 2)];
            if (scd == null)
            {
                Cursor = Cursors.WaitCursor;
                _mute.WaitOne();
                var cd = Directory.GetCurrentDirectory();
                if (Directory.Exists(drive + "\\"))
                {
                    try
                    {
                        Directory.SetCurrentDirectory(drive);
                        scd = Directory.GetCurrentDirectory();
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        scd = drive;
                    }
                }
                else
                {
                    scd = drive;
                }
                Directory.SetCurrentDirectory(cd);
                _mute.ReleaseMutex();
            }
            _path = scd;
            reset(true);
            Cursor = Cursors.Arrow;
        }

        private void comboBoxDrive_DropDown(object sender, System.EventArgs e)
        {
            if (_isCreateDriveList == false)
            {
                Cursor = Cursors.WaitCursor;
                _isCreateDriveList = true;
                var s = comboBoxDrive.SelectedItem.ToString();
                createDriveList();
                for (var i = 0; i < comboBoxDrive.Items.Count; i++)
                {
                    if (comboBoxDrive.Items[i].ToString() == s)
                    {
                        comboBoxDrive.SelectedIndex = i;
                        break;
                    }
                }
                Cursor = Cursors.Arrow;
            }
        }

        private void fFileSelector_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                buttonBack_Click(null, null);
            }
            if (e.Control && e.KeyCode == Keys.Y)
            {
                buttonForward_Click(null, null);
            }
            if (e.KeyCode == Keys.Back)
            {
                if (sender != textBoxFileSelectorFilename)
                {
                    select("..");
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                _isEnd = true;
                DialogResult = DialogResult.Cancel;
            }
            if (e.KeyCode == Keys.Enter)
            {
                if (sender == listViewFileSelect && listViewFileSelect.SelectedItems.Count == 1)
                {
                    listViewFileSelect_DoubleClick(null, null);
                }
                else
                {
                    if (buttonOK.Enabled)
                    {
                        apply();
                    }
                    else
                    {
                        var s = textBoxFileSelectorFilename.Text;
                        var evs = s.Split(new char[] { '%' });
                        s = "";
                        for (var i = 0; i < evs.Length; i++)
                        {
                            if (i % 2 == 0)
                            {
                                s += evs[i];
                            }
                            else
                            {
                                var es = Environment.GetEnvironmentVariable(evs[i]);
                                if (es != null)
                                {
                                    s += es;
                                }
                            }
                        }
                        if (s != textBoxFileSelectorFilename.Text)
                        {
                            textBoxFileSelectorFilename.Text = s;
                            textBoxFileSelectorFilename.Select(s.Length, s.Length);
                        }
                        if (Directory.Exists(s))
                        {
                            _path = s;
                            if (_path.EndsWith(":"))
                            {
                                _path += "\\";
                            }
                            reset(true);
                        }
                    }
                }
            }
        }

    }
}
