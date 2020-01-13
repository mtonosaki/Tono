// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Mes �̊T�v�̐����ł��B
    /// </summary>
    public class Mes
    {
        #region �t�H�[�}�b�g�N���X
        [Serializable]
        public class Format
        {
            /// <summary>�t�H�[�}�b�g��ۑ�</summary>
            private readonly ArrayList _formats = new ArrayList();

            /// <summary>
            /// �I�u�W�F�N�g����\�z
            /// </summary>
            /// <param name="value">�I�u�W�F�N�g</param>
            /// <returns>�V�����C���X�^���X</returns>
            public static Format FromObject(object value)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value);
                return ret;
            }

            /// <summary>
            /// �t�H�[�}�b�g��A������
            /// </summary>
            /// <param name="f1">�t�H�[�}�b�g�P�i���̃C���X�^���X��f2���A�������j</param>
            /// <param name="f2">�t�H�[�}�b�g�Q�i�A����A������͖��g�p�j</param>
            /// <returns></returns>
            public static Format operator +(Format f1, Format f2)
            {
                f1._formats.AddRange(f2._formats);
                return f1;
            }

            /// <summary>
            /// �����񂩂�\�z
            /// </summary>
            /// <param name="format">�t�H�[�}�b�g</param>
            /// <returns>�V�����C���X�^���X</returns>
            /// <remarks>
            /// �t�H�[�}�b�g�́A@�}�[�N�ň͂�ł�����̂����b�Z�[�W�L�[�Ƃ��Ď�舵��
            /// ��GDPOS @Editor@  �� (�p)DPOS Editor  / (��)DPOS �G�f�B�^ / (��)DPOS �ҏW�
            /// </remarks>
            public static Format FromString(string formatString)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(formatString);
                return ret;
            }

            /// <summary>
            /// ��������\�z
            /// </summary>
            /// <param name="format">�t�H�[�}�b�g</param>
            /// <returns>�V�����C���X�^���X</returns>
            public static Format FromInt(int value)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value.ToString());
                return ret;
            }

            /// <summary>
            /// ��������\�z
            /// </summary>
            /// <param name="format">�t�H�[�}�b�g</param>
            /// <returns>�V�����C���X�^���X</returns>
            public static Format FromInt(int value, string toStringFormat)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value.ToString(toStringFormat));
                return ret;
            }

            /// <summary>
            /// ������
            /// </summary>
            /// <returns>������</returns>
            public override string ToString()
            {
                var ret = "";
                for (var j = 0; j < _formats.Count; j++)
                {
                    var fmt = _formats[j];
                    if (Mes.Current == null)
                    {
                        ret += fmt.ToString();
                    }
                    else if (fmt is string)
                    {
                        var strs = ((string)fmt).Split(new char[] { '@' });
                        for (var i = 0; i < strs.Length; i++)
                        {
                            if (i % 2 == 0)
                            {
                                ret += strs[i];
                            }
                            else
                            {
                                var mes = Mes.Current[strs[i]];
                                ret += mes;
                            }
                        }
                    }
                    else
                    {
                        ret += Mes.Current[fmt];
                    }
                }
                return ret;
            }
        }
        #endregion

        /** <summary>���b�Z�[�W�f�[�^	</summary> */
        private readonly Hashtable _dat = new Hashtable();

        /// <summary>�J�����g���b�Z�[�W�C���X�^���X</summary>
        private static Mes _current = null;

        /// <summary>���j���[�A�C�e����Name�i�f�U�C�����j���L�����鎫��</summary>
        private static readonly IDictionary _objNameSave = new Hashtable();
        private static readonly IDictionary _fonts = new Hashtable();

        #region �C�x���g
        public delegate void CodeChangedEventHandler(object sender, CodeChangedEventArgs e);
        public class CodeChangedEventArgs : EventArgs
        {
            private readonly string _old;
            private readonly string _new;

            public CodeChangedEventArgs(string oldCode, string newCode)
            {
                _old = oldCode;
                _new = newCode;
            }
            public string NewCode => _new;
            public string OldCode => _old;
        }
        /// <summary>
        /// �R�[�h���ς�����Ƃ��ɃR�[�������
        /// </summary>
        public event CodeChangedEventHandler CodeChanged;
        #endregion

        /// <summary>����R�[�h</summary>
        private string _code = "jp";

        /// <summary>
        /// ���{�ꂩ�ǂ����𒲂ׂ�
        /// </summary>
        public bool IsJp => _code.Equals("jp", StringComparison.CurrentCultureIgnoreCase);

        public override string ToString()
        {
            return string.Format("{0} Code = {1}", GetType().Name, _code);
        }

        #region Data tips for debugging
#if DEBUG
        public string _ => "uMes N=" + _dat.Count.ToString();
#endif
        #endregion

        /// <summary>
        /// �A�v���P�[�V�����B��̃C���X�^���X
        /// </summary>
        public static Mes Current => _current;

        /// <summary>
        /// ���݂̌���R�[�h���擾����
        /// </summary>
        public string GetCode()
        {
            return _code;
        }

        /// <summary>
        /// �f�t�H���g�̒l�����[�h����
        /// </summary>
        public static void SetDefault()
        {
            _current = fromAuto();
        }

        /// <summary>
        /// DateTime�̃t�H�[�}�b�g�i�N���������b�j
        /// </summary>
        public string FormatYMDHMS => provideDefault(this["FormatYMDHMS"], "yyyy/MM/dd HH:mm:ss");
        /// <summary>
        /// DateTime�̃t�H�[�}�b�g�i�����b�j
        /// </summary>
        public string FormatHMS => provideDefault(this["FormatHMS"], "HH:mm:ss");
        /// <summary>
        /// DateTime�̃t�H�[�}�b�g�i�����j
        /// </summary>
        public string FormatHM => provideDefault(this["FormatHM"], "HH:mm");

        private string provideDefault(string str, string def)
        {
            if (string.IsNullOrEmpty(str))
            {
                return def;
            }
            else
            {
                return str;
            }
        }

        /// <summary>
        /// ���[�J���C�Y���ꂽ�t�H���g���擾����B�擾������ӔC������Dispose���邱��
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <returns>�t�H���g�F������Ȃ��ꍇ�́A�f�t�H���g�t�H���g���Ԃ�</returns>
        public System.Drawing.Font GetFont(string key)
        {
            var ret = (System.Drawing.Font)_fonts[key];
            if (ret == null)
            {
                ret = new System.Drawing.Font("MS UI Gothic", 9);
            }
            return ret;
        }

        /// <summary>
        /// uMes�p�̃t�@�C�������쐬����
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        private static string makeMesFilename(string langCode)
        {
            return FileUtil.MakeMesFilename(@"uMes." + langCode + ".xml");
        }

        /// <summary>
        /// ����R�[�h���w�肵�ăf�t�H���g������Z�b�g����
        /// </summary>
        /// <param name="langCode"></param>
        public static void SetCurrentLanguage(string langCode)
        {
            var oldCode = _current._code;
            var old = _current;
            _current = Mes.FromFile(makeMesFilename(langCode));
            _current._code = langCode;
            _current.CodeChanged = old.CodeChanged;
            _current.CodeChanged?.Invoke(_current, new CodeChangedEventArgs(oldCode, _current.GetCode()));
        }

        /// <summary>
        /// �����������R���X�g���N�^
        /// </summary>
        private static Mes fromAuto()
        {
            var fname = FileUtil.MakeMesFilename("uMesDefault.xml");
            if ((System.IO.File.Exists(fname)))
            {
                return Mes.FromFile(fname);
            }
            else
            {
                return Mes.FromNull();
            }
        }

        /// <summary>
        /// ��̃C���X�^���X���쐬����
        /// </summary>
        /// <returns></returns>
        public static Mes FromNull()
        {
            var ret = new Mes();
            return ret;
        }

        /// <summary>
        /// �t���p�X���w�肵�ăC���X�^���X���\�z����
        /// </summary>
        /// <param name="fullPath">�t���p�X</param>
        /// <returns>�V�����C���X�^���X</returns>
        public static Mes FromFile(string fullPath)
        {
            var ret = new Mes();
            ret._init(fullPath, new Hashtable());
            return ret;
        }

        /// <summary>
        /// �l���X�V����
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            _dat[key] = value;
        }

        /// <summary>
        /// �w��t�@�C����XML���烁�b�Z�[�W��ǂݍ���
        /// </summary>
        /// <param name="filename">�t�@�C�����i�t���p�X�j</param>
        /// <param name="recCheck">�z�Q�Ɩh�~�p �t�@�C�����L������</param>
        private void _init(string filename, IDictionary recCheck)
        {
            // �z�Q�Ɩh�~
            if (recCheck.Contains(filename))
            {
                return;
            }
            else
            {
                recCheck[filename] = this;
            }

            // �ǂݍ��ݏ���
            var xd = new XmlDocument();
            xd.Load(filename);

            var root = xd.DocumentElement;
            var week = root.GetElementsByTagName("mes");

            foreach (XmlNode node in week)
            {
                // load �����̏���
                if (node.Attributes != null)
                {
                    XmlNode n2 = node.Attributes["load"];
                    if (n2 != null)
                    {
                        object load = n2.Value;
                        _code = load.ToString();
                        _init(makeMesFilename(load.ToString()), recCheck);
                    }
                }

                // �l�̏���
                string key;
                if (node.Attributes != null && node.Attributes["key"] != null)
                {
                    key = node.Attributes["key"].Value;
                }
                else
                {
                    continue;
                }
                if (node.Attributes != null && node.Attributes["ver"] != null)
                {
                    object ver = node.Attributes["ver"].Value;
                    key = key + "@" + ver.ToString();

                }

                // ���ʃ��b�Z�[�W�̏���
                var innnerN = 0;
                foreach (XmlNode cnode in node.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        if (cnode.Attributes != null && cnode.Attributes["key"] != null)
                        {
                            var ckey = cnode.Attributes["key"].Value;
                            _dat[key + "@" + ckey] = cnode.InnerText;
                            innnerN++;
                        }
                    }
                }
                if (innnerN == 0)
                {
                    _dat[key] = node.InnerText;
                }
            }

            // �t�H���g�̓ǂݍ���
            foreach (XmlNode fnode in root.GetElementsByTagName("font"))
            {
                if (fnode.Attributes != null)
                {
                    if (fnode.Attributes["key"] == null)
                    {
                        continue;
                    }

                    if (fnode.Attributes["face"] == null)
                    {
                        continue;
                    }

                    if (fnode.Attributes["size"] == null)
                    {
                        continue;
                    }

                    var ckey = fnode.Attributes["key"].Value;
                    var face = fnode.Attributes["face"].Value;
                    var size = float.Parse(fnode.Attributes["size"].Value);


                    string b = "0", u = "0", s = "0", i = "0";
                    if (fnode.Attributes["bold"] != null)
                    {
                        b = fnode.Attributes["bold"].Value;
                    }

                    if (fnode.Attributes["underline"] != null)
                    {
                        u = fnode.Attributes["underline"].Value;
                    }

                    if (fnode.Attributes["stlikeout"] != null)
                    {
                        s = fnode.Attributes["stlikeout"].Value;
                    }

                    if (fnode.Attributes["italic"] != null)
                    {
                        i = fnode.Attributes["italic"].Value;
                    }

                    SetFont(ckey, face, size, Const.IsTrue(b), Const.IsTrue(u), Const.IsTrue(i), Const.IsTrue(s));
                }
            }
        }

        /// <summary>
        /// ����̃t�H���g���X�V����
        /// </summary>
        /// <param name="key"></param>
        /// <param name="face"></param>
        /// <param name="sizepoint"></param>
        /// <param name="isBold"></param>
        /// <param name="isUnderline"></param>
        /// <param name="isItalic"></param>
        /// <param name="isStrikeout"></param>
        public void SetFont(string key, string face, float sizepoint, bool isBold, bool isUnderline, bool isItalic, bool isStrikeout)
        {
            var style = System.Drawing.FontStyle.Regular;
            if (isBold)
            {
                style |= System.Drawing.FontStyle.Bold;
            }

            if (isUnderline)
            {
                style |= System.Drawing.FontStyle.Underline;
            }

            if (isStrikeout)
            {
                style |= System.Drawing.FontStyle.Strikeout;
            }

            if (isItalic)
            {
                style |= System.Drawing.FontStyle.Italic;
            }

            var f = new System.Drawing.Font(face, sizepoint, style);
            _fonts[key] = f;
        }

        /// <summary>
        /// �L�[�ƃo�[�W��������l���擾����
        /// </summary>
        public string this[string key, string ver]
        {
            [NoTest]
            get => this[key + "@" + ver];
        }

        /// <summary>
        ///�@��������擾����
        /// </summary>
        public virtual string this[string key]
        {
            [NoTest]
            get
            {
                var ret = (string)_dat[key];
                if (ret != null)
                {
                    return ret;
                }
                return "";
            }
        }

        /// <summary>
        /// �w�蕶����L�[���܂܂�Ă��邩��������
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <param name="ver">�o�[�W����</param>
        /// <returns>true = �܂܂�Ă��� / false = ���Ȃ�</returns>
        [NoTest]
        public bool Contains(string key, string ver)
        {
            return _dat.Contains(key + "@" + ver);
        }

        /// <summary>
        /// �w�蕶����L�[���܂܂�Ă��邩��������
        /// </summary>
        /// <param name="key">�L�[</param>
        /// <returns>true = �܂܂�Ă��� / false = ���Ȃ�</returns>
        [NoTest]
        public bool Contains(string key)
        {
            return _dat.Contains(key);
        }

        /// <summary>
        /// ���j���[�A�C�e�����Z�b�g�����ꍇ�̃��b�Z�[�W�K�p
        /// </summary>
        public string this[MenuItem mi]
        {
            get
            {
                if (mi.Text == "-")
                {
                    return mi.Text;
                }
                var obj = _objNameSave[mi];
                if (obj != null)
                {
                    mi.Text = (string)obj;
                }
                else
                {
                    _objNameSave[mi] = mi.Text;
                }
                object ret = this["MenuItem", mi.Text];
                if (ret == null || ret.Equals(""))
                {
                    return mi.Text;
                }
                return ret.ToString();
            }
        }

        public string this[ToolStripItem mi]
        {
            get
            {
                if (mi.Text == "-")
                {
                    return mi.Text;
                }
                var obj = _objNameSave[mi];
                if (obj != null)
                {
                    mi.Text = (string)obj;
                }
                else
                {
                    _objNameSave[mi] = mi.Text;
                }
                object ret = this["MenuItem", mi.Text];
                if (ret == null || ret.Equals(""))
                {
                    return mi.Text;
                }
                return ret.ToString();
            }
        }

        /// <summary>
        /// �I�u�W�F�N�g����l���擾����
        /// </summary>
        public string this[Type value]
        {
            get
            {
                object ret = this["Class", value.Name];
                if (ret == null || string.IsNullOrEmpty(ret.ToString()))
                {
                    return value.Name;
                }
                else
                {
                    return ret.ToString();
                }
            }
        }

        /// <summary>
        /// �I�u�W�F�N�g����l���擾����
        /// </summary>
        public string this[object value]
        {
            get
            {
                var key = value.GetType().ToString();
                var ver = value.ToString();
                object ret = this[key, ver];
                if (ret == null || string.IsNullOrEmpty(ret.ToString()))
                {
                    return ver;
                }
                else
                {
                    return ret.ToString();
                }
            }
        }

        /// <summary>
        /// �w��R���g���[���ɑ����錾��e�L�X�g�����ׂēK�p����
        /// </summary>
        /// <param name="rootForm">�t�H�[��</param>
        public void ResetText(Control rootControl)
        {
            resetControlText(rootControl);
        }

        /// <summary>
        /// �w��t�H�[���ɑ����錾��e�L�X�g�����ׂēK�p����
        /// </summary>
        /// <param name="rootForm">�t�H�[��</param>
        public void ResetText(Form rootForm)
        {
            if (rootForm.Menu != null)
            {
                resetMenuText(rootForm.Menu);
            }
            if (rootForm.MainMenuStrip != null)
            {
                menuItemStripLoop(rootForm.MainMenuStrip.Items);
            }
            if (rootForm.ContextMenu != null)
            {
                resetMenuText(rootForm.ContextMenu);
            }
            if (rootForm.ContextMenuStrip != null)
            {
                menuItemStripLoop(rootForm.ContextMenuStrip.Items);
            }
            resetControlText(rootForm);
        }

        /// <summary>
        /// �w��R���g���[���Ƃ��ꂪ���L����z��`���̃I�u�W�F�N�g�Ɍ����ݒ肷��
        /// </summary>
        /// <param name="c">���[�J���C�Y�������R���g���[��</param>
        private void setNameToControl(Control c)
        {
            Type tp;
            for (tp = c.GetType(); ; tp = tp.BaseType)
            {
                if (tp.BaseType == null)
                {
                    break;
                }

                if (tp.Namespace.StartsWith("System."))
                {
                    break;
                }

                if (tp.Assembly.FullName == GetType().Assembly.FullName)
                {
                    break;
                }
            }

            var key = tp.Name + "@" + c.Name;
            //System.Diagnostics.Debug.WriteLine(key);
            var o = _dat[key];
            if (o != null)
            {
                c.Text = o.ToString();
            }
            // ���X�g�r���[�̗l�ɁA�z��������Ă���I�u�W�F�N�g�ɑ΂��āA���b�Z�[�W����
            var pis = c.GetType().GetProperties();
            foreach (var pi in pis)
            {
                var t = pi.PropertyType.GetInterface("ICollection");
                if (t != null)
                {
                    if (pi.Name == "Controls")
                    {
                        continue;
                    }
                    foreach (var ao in (ICollection)pi.GetValue(c, null))
                    {
                        // ao = �z����̊e�l
                        var ptext = ao.GetType().GetProperty("Text");
                        var pname = ao.GetType().GetProperty("Name");
                        if (ptext != null)
                        {
                            PropertyInfo sp;
                            if (pname == null || string.IsNullOrEmpty((string)pname.GetValue(ao, null)))
                            {
                                sp = ptext;

                                // Text�������I���W�i���ɖ߂�
                                var originalText = _objNameSave[ao];
                                if (originalText != null)
                                {
                                    ptext.SetValue(ao, originalText.ToString(), null);
                                }
                                else
                                {
                                    _objNameSave[ao] = ptext.GetValue(ao, null);
                                }
                            }
                            else
                            {
                                sp = pname;
                            }
                            var spstr = sp.GetValue(ao, null);
                            if (spstr != null)
                            {
                                var str = _dat[key + "@" + spstr.ToString()];
                                if (str != null)
                                {
                                    sp.SetValue(ao, str.ToString(), null);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �R���g���[���̕\������̕ύX
        /// </summary>
        /// <param name="c">Control�I�u�W�F�N�g</param>
        [NoTest]
        private void resetControlText(Control c)
        {
            if (c.ContextMenu != null)
            {
                resetMenuText(c.ContextMenu);
            }
            if (c.ContextMenuStrip != null)
            {
                menuItemStripLoop(c.ContextMenuStrip.Items);
            }
            setNameToControl(c);
            foreach (Control com in c.Controls)
            {
                resetControlText(com);
            }
        }

        /// <summary>
        /// ���j���[�̕\������̕ύX
        /// </summary>
        /// <param name="c">MenuItem�I�u�W�F�N�g</param>
        private void resetMenuText(Menu menu)
        {
            //���j���[�A�C�e���̃e�L�X�g��ύX
            foreach (MenuItem mi in menu.MenuItems)
            {
                menuItemLoop(mi);
            }
        }

        /// <summary>
        /// ���j���[�A�C�e�����ċA�R�[�����Ȃ���K�p����
        /// </summary>
        private void menuItemLoop(MenuItem c)
        {
            c.Text = this[c];

            foreach (MenuItem com in c.MenuItems)
            {
                menuItemLoop(com);
            }
        }
        /// <summary>
        /// ���j���[�A�C�e�����ċA�R�[�����Ȃ���K�p����
        /// </summary>
        private void menuItemStripLoop(ToolStripItemCollection c)
        {
            foreach (var com in c)
            {
                if (com is ToolStripItem)
                {
                    ((ToolStripItem)com).Text = this[(ToolStripItem)com];
                }
                if (com is ToolStripDropDownItem)
                {
                    menuItemStripLoop(((ToolStripDropDownItem)com).DropDownItems);
                }
            }
        }
    }
}
