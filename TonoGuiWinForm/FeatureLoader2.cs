// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uFeatureLoader の概要の説明です。
    /// .NET Framework 2.0用、MenuStripに対するローダ
    /// </summary>
    public class FeatureLoader2 : FeatureLoaderBase
    {
        private static Hashtable _assemblyList = null;
        private FeatureGroupRoot _root;
        private readonly Stack _group = new Stack();
        private readonly IDictionary _menuorder = new Hashtable();
        private Form _form = null;
        private static ResourceManager _resMan = null;

        /// <summary>
        /// リソースが格納されているクラスを登録する
        /// </summary>
        /// <param name="resClass"></param>
        public static void SetResources(ResourceManager resMan)
        {
            _resMan = resMan;
        }

        /// <summary>
        /// 使用中のアセンブリが含まれるクラスを指定する
        /// </summary>
        /// <param name="t">クラス</param>
        public static void SetUsingClass(Type t)
        {
            if (_assemblyList == null)
            {
                _assemblyList = new Hashtable
                {
                    [Assembly.GetEntryAssembly().FullName] = Assembly.GetEntryAssembly(),
                    [Assembly.GetExecutingAssembly().FullName] = Assembly.GetExecutingAssembly()
                };
            }
            var a = t.Assembly;
            _assemblyList[a.FullName] = a;
        }

        /// <summary>
        /// 使用中のアセンブリリストを得る
        /// </summary>
        /// <returns></returns>
        public static Assembly[] GetUsingClasses()
        {
            var ret = new Assembly[_assemblyList.Count];
            var i = 0;
            foreach (Assembly val in _assemblyList.Values)
            {
                ret[i++] = val;
            }
            return ret;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="root"></param>
        public override void Load(FeatureGroupRoot root, string fname)
        {
            _root = root;
            _group.Push(_root);
            fname = FileUtil.MakeMesFilename(fname);
            load(fname);
        }

        /// <summary>
        /// ファイルから情報を読み込みフィーチャーを構築する
        /// </summary>
        /// <param name="fullpath">フルパス</param>
        private void load(string fullpath)
        {
            // フォームを取得する
            Control c;
            for (c = _root.GetFeatureRich(); c is Form == false; c = c.Parent)
            {
                ;
            }

            _form = (Form)c;

            // 読み込み処理
            var xd = new XmlDocument();
            xd.Load(fullpath);

            var root = xd.DocumentElement;
            foreach (XmlNode node in root.ChildNodes)
            {
                loopProc(node);
            }

            // メニューオーダーが付いていないメニューにオーダー番号をつける
            if (_form.MainMenuStrip != null)
            {
                menuSetOrderLoopProc(_form.MainMenuStrip.Items);

                // オーダーにしたがってソートする
                menuSortLoop(_form.MainMenuStrip.Items);
            }
        }

        #region メニューオーダーに従って比較する
        private class Sorter : IComparer
        {
            private readonly IDictionary _order;
            public Sorter(IDictionary orderbuf)
            {
                _order = orderbuf;
            }
            #region IComparer メンバ

            public int Compare(object x, object y)
            {
                var i1 = _order[x];
                var i2 = _order[y];
                if (i1 == null && i2 == null)
                {
                    return 0;
                }
                if (i1 == null && i2 != null)
                {
                    return 1;
                }
                if (i1 != null && i2 == null)
                {
                    return -1;
                }
                return (int)i1 - (int)i2;
            }

            #endregion
        }
        #endregion

        private void menuSortLoop(ToolStripItemCollection mc)
        {
            if (mc == null)
            {
                return;
            }

            for (var i = 0; i < mc.Count; i++)
            {
                if (mc[i] is ToolStripDropDownItem)
                {
                    menuSortLoop(((ToolStripDropDownItem)mc[i]).DropDownItems);
                }
            }
            var mis = new ArrayList();
            foreach (ToolStripItem mi in mc)
            {
                mis.Add(mi);
            }
            mis.Sort(new Sorter(_menuorder));
            mc.Clear();
            for (var i = 0; i < mis.Count; i++)
            {
                mc.Add((ToolStripItem)mis[i]);
            }
        }
        private void menuSetOrderLoopProc(ToolStripItemCollection mc)
        {
            if (mc == null)
            {
                return;
            }
            for (var i = 0; i < mc.Count; i++)
            {
                var mi = mc[i];
                if (_menuorder[mi] == null)
                {
                    _menuorder[mi] = (i + 1) * 100;
                }
                if (mi is ToolStripDropDownItem)
                {
                    menuSetOrderLoopProc(((ToolStripDropDownItem)mi).DropDownItems);
                }
            }
        }

        private void loopProc(XmlNode current)
        {
            var tag = current.Name.ToLower();
            if (tag == "group")
            {
                //_group.Push(((fgBase)_group.Peek()).AddChildGroup());	このコメントで、グループを使用しないようにしている。by Tono
                foreach (XmlNode child in current.ChildNodes)
                {
                    loopProc(child);
                }
                //				if( _group.Count > 1 )
                //				{
                //					_group.Pop();
                //				}
            }
            if (tag == "implement")
            {
                procImplement(current);
            }
            if (tag == "feature")
            {
                procFeature(current);
            }
            if (tag == "menuitem")
            {
                procMenuItem(current, null);
            }
        }
        private void procMenuItem(XmlNode node, FeatureBase feature)
        {
            try
            {
                if (node.Attributes["menu"] != null)
                {
                    var ms = node.Attributes["menu"].Value;
                    var mss = ms.Split(new char[] { '/' });
                    var mcol = _form.MainMenuStrip.Items;
                    ToolStripItem foundMenu = null;

                    int i;
                    for (i = 0; i < mss.Length; i++)
                    {
                        var isFound = false;
                        if (mss[i] != "-")
                        {
                            foreach (ToolStripItem mi in mcol)
                            {
                                if (mi.Text == mss[i])
                                {
                                    if (mi is ToolStripDropDownItem)
                                    {
                                        mcol = ((ToolStripDropDownItem)mi).DropDownItems;
                                        foundMenu = mi;
                                        isFound = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (isFound == false)
                        {
                            break;
                        }
                    }
                    for (; i < mss.Length; i++)
                    {
                        if (i == mss.Length - 1)
                        {
                            // フィーチャー起動用のメニューを作成
                            ToolStripItem mi;
                            if (feature != null)
                            {
                                mi = new TMenuListener2
                                {
                                    Text = mss[i]
                                };
                                mcol.Add(mi);
                                ((TMenuListener2)mi).LinkFeature(feature);
                            }
                            else
                            {
                                if (mss[i] == "-")
                                {
                                    mi = new ToolStripSeparator();
                                }
                                else
                                {
                                    mi = new ToolStripMenuItem
                                    {
                                        Text = mss[i]
                                    };
                                }
                                mcol.Add(mi);
                            }

                            // イメージを登録
                            try
                            {
                                if (_resMan != null)
                                {
                                    if (mi is ToolStripItem)
                                    {
                                        if (node.Attributes["image"] != null)
                                        {
                                            var image = node.Attributes["image"].Value;
                                            var tarImage = _resMan.GetObject(image) as Image;
                                            mi.Image = tarImage;
                                        }
                                        //if( node.Attributes["tooltip"] != null)
                                        //{
                                        //	string meskey = node.Attributes["tooltip"].Value;
                                        //	var ttmes = uMes.Current[meskey];
                                        //	mi.ToolTipText = ttmes == null || ttmes == "" ? meskey : ttmes;
                                        //}
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            // ショートカットを実装
                            try
                            {
                                if (mi is ToolStripMenuItem)
                                {
                                    int id;
                                    var sm = (ToolStripMenuItem)mi;
                                    var ks = Keys.None;
                                    if (node.Attributes["shortcut"] != null)
                                    {
                                        var scs = node.Attributes["shortcut"].Value;
                                        if ((id = scs.ToLower().IndexOf("ctrl")) >= 0)
                                        {
                                            ks |= Keys.Control;
                                            scs = scs.Substring(0, id) + scs.Substring(id + 4);
                                        }
                                        if ((id = scs.ToLower().IndexOf("alt")) >= 0)
                                        {
                                            ks |= Keys.Alt;
                                            scs = scs.Substring(0, id) + scs.Substring(id + 3);
                                        }
                                        if ((id = scs.ToLower().IndexOf("shift")) >= 0)
                                        {
                                            ks |= Keys.Shift;
                                            scs = scs.Substring(0, id) + scs.Substring(id + 5);
                                        }

                                        var fi = typeof(Keys).GetField(scs);

                                        var sc = fi.GetValue(null);
                                        ks |= (Keys)sc;
                                        sm.ShortcutKeys = ks;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                            // メニューオーダー（順番）を記録
                            try
                            {
                                if (node.Attributes["menuorder"] != null)
                                {
                                    var mo = node.Attributes["menuorder"].Value;
                                    _menuorder[mi] = int.Parse(mo);
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            // ポップアップメニューを作成
                            var mi = new ToolStripDropDown
                            {
                                Text = mss[i]
                            };
                            //mcol.Add(mi.Text);
                            mcol = mi.Items;
                        }
                    }
                    // コントロールとリンクしているフィーチャーなどは、CanStartで、コントロールのEnableを操作するのでここで一度行っておく
                    if (feature != null)
                    {
                        var dummy = feature.CanStart;
                    }
                }
            }
            catch (Exception)
            {
                //System.Diagnostics.Debug.WriteLine( e.Message, e.StackTrace );
            }
        }

        private void procImplement(XmlNode node)
        {
            try
            {
                var typeData = getType(node.Attributes["Data"].Value);
                var typeParts = getType(node.Attributes["Parts"].Value);
                var typeLink = getType(node.Attributes["Link"].Value);

                _root.AssignAppData((DataHotBase)Activator.CreateInstance(typeData));
                _root.AssignPartsSet((PartsCollectionBase)Activator.CreateInstance(typeParts));
                _root.AssignLink((DataLinkBase)Activator.CreateInstance(typeLink));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Feature Loader Error 例外 : '" + e.Message + "'");
            }
        }

        /// <summary>
        /// スキャンするアセンブリ
        /// </summary>
        private static IDictionary namespaces = null;

        /// <summary>
        /// アセンブリを超えて型を取得する
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private Type getType(string name)
        {
            if (name.Trim().Length < 1)
            {
                return null;
            }
            if (namespaces == null)
            {
                namespaces = new Hashtable();
                if (_assemblyList == null)
                {
                    FeatureLoader2.SetUsingClass(GetType());
                }

                foreach (Assembly a in _assemblyList.Values)
                {
                    foreach (var t in a.GetTypes())
                    {
                        var ss = a.FullName.Split(new char[] { ',' });
                        var s = t.Namespace + ".@CLASSNAME@, " + ss[0];
                        namespaces[s] = a;
                    }
                }
            }
            Type type = null;
            foreach (string a in namespaces.Keys)
            {
                type = Type.GetType(a.Replace("@CLASSNAME@", name));
                if (type != null)
                {
                    break;
                }
            }
            System.Diagnostics.Debug.Assert(type != null, string.Format("Class '{0}' registerd in a portfolio is not in this application.", name));
            return type;
        }

        private static readonly FieldInfo _fiParamString = typeof(FeatureBase).GetField("_paramString", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private void procFeature(XmlNode node)
        {
            try
            {
                var nameAtt = node.Attributes["class"];
                if (nameAtt != null)
                {
                    var type = getType(nameAtt.Value);
                    if (type != null)
                    {
                        var feature = ((FeatureGroupBase)_group.Peek()).AddChildFeature(type);
                        try
                        {
                            var swAtt = node.Attributes["name"];
                            if (swAtt != null)
                            {
                                if (swAtt.Value != null && swAtt.Value.Length > 0)
                                {
                                    feature.Name = swAtt.Value;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            var swAtt = node.Attributes["com"];
                            if (swAtt != null)
                            {
                                if (swAtt.Value != null && swAtt.Value.Length > 0)
                                {
                                    feature.CommandParameter = swAtt.Value;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            var swAtt = node.Attributes["enabled"];
                            if (swAtt != null)
                            {
                                if (Const.IsFalse(swAtt.Value))
                                {
                                    feature.Enabled = false;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }                   // メニュー情報
                                            // フィーチャーに引数を割り当てる
                        try
                        {
                            if (string.IsNullOrEmpty(node.InnerText) == false)
                            {
                                _fiParamString.SetValue(feature, node.InnerText);
                                feature.ParseParameter(node.InnerText);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        procMenuItem(node, feature);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Feature Loader Error : '" + nameAtt.Value + "'は実装されていないフィーチャーです");
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Feature Loader Error 例外 : '" + e.Message + "'");
            }
        }

        /// <summary>
        /// 指定したuFeatureLoader用ファイルが存在するか検査する
        /// </summary>
        /// <param name="file">検査するファイル名</param>
        /// <returns>検索結果 True:在り / False:無し</returns>
        public static bool FileExists(string file)
        {
            var fname = FileUtil.MakeMesFilename(file);
            return File.Exists(fname);
        }
    }
}
