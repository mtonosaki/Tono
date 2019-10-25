// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uFeatureLoader の概要の説明です。
    /// .NET Framework 1.1用、MenuItemに対するローダ
    /// </summary>
    public class FeatureLoader1 : FeatureLoaderBase
    {
        private static Hashtable _assemblyList = null;
        private FeatureGroupRoot _root;
        private readonly Stack _group = new Stack();
        private readonly IDictionary _menuorder = new Hashtable();
        private Form _form = null;


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
        /// 読込開始
        /// </summary>
        public override void Load(FeatureGroupRoot root, string fname)
        {
            _root = root;
            _group.Push(_root);
            fname = makeMesFilename(fname);
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
            if (_form.Menu != null)
            {
                menuSetOrderLoopProc(_form.Menu.MenuItems);

                // オーダーにしたがってソートする
                menuSortLoop(_form.Menu.MenuItems);
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

        private void menuSortLoop(Menu.MenuItemCollection mc)
        {
            if (mc == null)
            {
                return;
            }
            for (var i = 0; i < mc.Count; i++)
            {
                menuSortLoop(mc[i].MenuItems);
            }
            var mis = new ArrayList();
            foreach (MenuItem mi in mc)
            {
                mis.Add(mi);
            }
            mis.Sort(new Sorter(_menuorder));
            mc.Clear();
            for (var i = 0; i < mis.Count; i++)
            {
                mc.Add((MenuItem)mis[i]);
            }
        }
        private void menuSetOrderLoopProc(Menu.MenuItemCollection mc)
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
                menuSetOrderLoopProc(mi.MenuItems);
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
                var ms = node.Attributes["menu"].Value;
                var mss = ms.Split(new char[] { '/' });
                var mcol = _form.Menu.MenuItems;
                MenuItem foundMenu = null;

                int i;
                for (i = 0; i < mss.Length; i++)
                {
                    var isFound = false;
                    if (mss[i] != "-")
                    {
                        foreach (MenuItem mi in mcol)
                        {
                            if (mi.Text == mss[i])
                            {
                                mcol = mi.MenuItems;
                                foundMenu = mi;
                                isFound = true;
                                break;
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
                        MenuItem mi;
                        if (feature != null)
                        {
                            mi = new TMenuListener
                            {
                                Text = mss[i]
                            };
                            mcol.Add(mi);
                            ((TMenuListener)mi).LinkFeature(feature);
                        }
                        else
                        {
                            mi = new MenuItem
                            {
                                Text = mss[i]
                            };
                            mcol.Add(mi);
                        }

                        // ショートカットを実装
                        try
                        {
                            var scs = node.Attributes["shortcut"].Value;
                            var fi = typeof(Shortcut).GetField(scs);
                            var sc = fi.GetValue(null);
                            mi.Shortcut = (Shortcut)sc;
                        }
                        catch (Exception)
                        {
                        }
                        // メニューオーダー（順番）を記録
                        try
                        {
                            var mo = node.Attributes["menuorder"].Value;
                            _menuorder[mi] = int.Parse(mo);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    else
                    {
                        // ポップアップメニューを作成
                        var mi = new MenuItem
                        {
                            Text = mss[i]
                        };
                        mcol.Add(mi);
                        mcol = mi.MenuItems;
                    }
                }
                // コントロールとリンクしているフィーチャーなどは、CanStartで、コントロールのEnableを操作するのでここで一度行っておく
                var dummy = feature.CanStart;
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
                    FeatureLoader1.SetUsingClass(GetType());
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
                var type = getType(nameAtt.Value);
                if (type != null)
                {
                    var feature = ((FeatureGroupBase)_group.Peek()).AddChildFeature(type);
                    try
                    {
                        var swAtt = node.Attributes["name"];
                        if (swAtt.Value != null && swAtt.Value.Length > 0)
                        {
                            feature.Name = swAtt.Value;
                        }
                    }
                    catch (Exception)
                    {
                    }
                    try
                    {
                        var swAtt = node.Attributes["enabled"];
                        if (Const.IsFalse(swAtt.Value))
                        {
                            feature.Enabled = false;
                        }
                    }
                    catch (Exception)
                    {
                    }
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
                    // メニュー情報
                    procMenuItem(node, feature);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Feature Loader Error : '" + nameAtt.Value + "'は実装されていないフィーチャーです");
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Feature Loader Error 例外 : '" + e.Message + "'");
            }
        }

        /// <summary>
        /// uFeatureLoader用のファイル名を作成する
        /// </summary>
        /// <param name="filename">拡張子も含めたファイル名</param>
        /// <returns>ファイル名（フルパス）</returns>
        private static string makeMesFilename(string filename)
        {
            return FileUtil.MakeMesFilename(filename);
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
