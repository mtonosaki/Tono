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
    /// Mes の概要の説明です。
    /// </summary>
    public class Mes
    {
        #region フォーマットクラス
        [Serializable]
        public class Format
        {
            /// <summary>フォーマットを保存</summary>
            private readonly ArrayList _formats = new ArrayList();

            /// <summary>
            /// オブジェクトから構築
            /// </summary>
            /// <param name="value">オブジェクト</param>
            /// <returns>新しいインスタンス</returns>
            public static Format FromObject(object value)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value);
                return ret;
            }

            /// <summary>
            /// フォーマットを連結する
            /// </summary>
            /// <param name="f1">フォーマット１（このインスタンスにf2が連結される）</param>
            /// <param name="f2">フォーマット２（連結後、こちらは未使用）</param>
            /// <returns></returns>
            public static Format operator +(Format f1, Format f2)
            {
                f1._formats.AddRange(f2._formats);
                return f1;
            }

            /// <summary>
            /// 文字列から構築
            /// </summary>
            /// <param name="format">フォーマット</param>
            /// <returns>新しいインスタンス</returns>
            /// <remarks>
            /// フォーマットは、@マークで囲んであるものをメッセージキーとして取り扱う
            /// 例；DPOS @Editor@  → (英)DPOS Editor  / (日)DPOS エディタ / (中)DPOS 編集軟件
            /// </remarks>
            public static Format FromString(string formatString)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(formatString);
                return ret;
            }

            /// <summary>
            /// 整数から構築
            /// </summary>
            /// <param name="format">フォーマット</param>
            /// <returns>新しいインスタンス</returns>
            public static Format FromInt(int value)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value.ToString());
                return ret;
            }

            /// <summary>
            /// 整数から構築
            /// </summary>
            /// <param name="format">フォーマット</param>
            /// <returns>新しいインスタンス</returns>
            public static Format FromInt(int value, string toStringFormat)
            {
                var ret = new Format();
                ret._formats.Clear();
                ret._formats.Add(value.ToString(toStringFormat));
                return ret;
            }

            /// <summary>
            /// 文字列化
            /// </summary>
            /// <returns>文字列</returns>
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

        /** <summary>メッセージデータ	</summary> */
        private readonly Hashtable _dat = new Hashtable();

        /// <summary>カレントメッセージインスタンス</summary>
        private static Mes _current = null;

        /// <summary>メニューアイテムのName（デザイン時）を記憶する辞書</summary>
        private static readonly IDictionary _objNameSave = new Hashtable();
        private static readonly IDictionary _fonts = new Hashtable();

        #region イベント
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
        /// コードが変わったときにコールされる
        /// </summary>
        public event CodeChangedEventHandler CodeChanged;
        #endregion

        /// <summary>言語コード</summary>
        private string _code = "jp";

        /// <summary>
        /// 日本語かどうかを調べる
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
        /// アプリケーション唯一のインスタンス
        /// </summary>
        public static Mes Current => _current;

        /// <summary>
        /// 現在の言語コードを取得する
        /// </summary>
        public string GetCode()
        {
            return _code;
        }

        /// <summary>
        /// デフォルトの値をロードする
        /// </summary>
        public static void SetDefault()
        {
            _current = fromAuto();
        }

        /// <summary>
        /// DateTimeのフォーマット（年月日時分秒）
        /// </summary>
        public string FormatYMDHMS => provideDefault(this["FormatYMDHMS"], "yyyy/MM/dd HH:mm:ss");
        /// <summary>
        /// DateTimeのフォーマット（時分秒）
        /// </summary>
        public string FormatHMS => provideDefault(this["FormatHMS"], "HH:mm:ss");
        /// <summary>
        /// DateTimeのフォーマット（時分）
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
        /// ローカライズされたフォントを取得する。取得したら責任もってDisposeすること
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>フォント：見つからない場合は、デフォルトフォントが返る</returns>
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
        /// uMes用のファイル名を作成する
        /// </summary>
        /// <param name="langCode"></param>
        /// <returns></returns>
        private static string makeMesFilename(string langCode)
        {
            return FileUtil.MakeMesFilename(@"uMes." + langCode + ".xml");
        }

        /// <summary>
        /// 言語コードを指定してデフォルト言語をセットする
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
        /// 自動初期化コンストラクタ
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
        /// 空のインスタンスを作成する
        /// </summary>
        /// <returns></returns>
        public static Mes FromNull()
        {
            var ret = new Mes();
            return ret;
        }

        /// <summary>
        /// フルパスを指定してインスタンスを構築する
        /// </summary>
        /// <param name="fullPath">フルパス</param>
        /// <returns>新しいインスタンス</returns>
        public static Mes FromFile(string fullPath)
        {
            var ret = new Mes();
            ret._init(fullPath, new Hashtable());
            return ret;
        }

        /// <summary>
        /// 値を更新する
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            _dat[key] = value;
        }

        /// <summary>
        /// 指定ファイルのXMLからメッセージを読み込む
        /// </summary>
        /// <param name="filename">ファイル名（フルパス）</param>
        /// <param name="recCheck">循環参照防止用 ファイル名記憶辞書</param>
        private void _init(string filename, IDictionary recCheck)
        {
            // 循環参照防止
            if (recCheck.Contains(filename))
            {
                return;
            }
            else
            {
                recCheck[filename] = this;
            }

            // 読み込み処理
            var xd = new XmlDocument();
            xd.Load(filename);

            var root = xd.DocumentElement;
            var week = root.GetElementsByTagName("mes");

            foreach (XmlNode node in week)
            {
                // load 属性の処理
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

                // 値の処理
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

                // 下位メッセージの処理
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

            // フォントの読み込み
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
        /// 言語のフォントを更新する
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
        /// キーとバージョンから値を取得する
        /// </summary>
        public string this[string key, string ver]
        {
            [NoTest]
            get => this[key + "@" + ver];
        }

        /// <summary>
        ///　文字列を取得する
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
        /// 指定文字列キーが含まれているか調査する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="ver">バージョン</param>
        /// <returns>true = 含まれている / false = いない</returns>
        [NoTest]
        public bool Contains(string key, string ver)
        {
            return _dat.Contains(key + "@" + ver);
        }

        /// <summary>
        /// 指定文字列キーが含まれているか調査する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>true = 含まれている / false = いない</returns>
        [NoTest]
        public bool Contains(string key)
        {
            return _dat.Contains(key);
        }

        /// <summary>
        /// メニューアイテムをセットした場合のメッセージ適用
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
        /// オブジェクトから値を取得する
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
        /// オブジェクトから値を取得する
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
        /// 指定コントロールに属する言語テキストをすべて適用する
        /// </summary>
        /// <param name="rootForm">フォーム</param>
        public void ResetText(Control rootControl)
        {
            resetControlText(rootControl);
        }

        /// <summary>
        /// 指定フォームに属する言語テキストをすべて適用する
        /// </summary>
        /// <param name="rootForm">フォーム</param>
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
        /// 指定コントロールとそれが所有する配列形式のオブジェクトに言語を設定する
        /// </summary>
        /// <param name="c">ローカライズしたいコントロール</param>
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
            // リストビューの様に、配列を持っているオブジェクトに対して、メッセージ処理
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
                        // ao = 配列内の各値
                        var ptext = ao.GetType().GetProperty("Text");
                        var pname = ao.GetType().GetProperty("Name");
                        if (ptext != null)
                        {
                            PropertyInfo sp;
                            if (pname == null || string.IsNullOrEmpty((string)pname.GetValue(ao, null)))
                            {
                                sp = ptext;

                                // Text属性をオリジナルに戻す
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
        /// コントロールの表示言語の変更
        /// </summary>
        /// <param name="c">Controlオブジェクト</param>
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
        /// メニューの表示言語の変更
        /// </summary>
        /// <param name="c">MenuItemオブジェクト</param>
        private void resetMenuText(Menu menu)
        {
            //メニューアイテムのテキストを変更
            foreach (MenuItem mi in menu.MenuItems)
            {
                menuItemLoop(mi);
            }
        }

        /// <summary>
        /// メニューアイテムを再帰コールしながら適用する
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
        /// メニューアイテムを再帰コールしながら適用する
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
