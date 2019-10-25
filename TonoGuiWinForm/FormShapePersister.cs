// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FormShapePersister の概要の説明です。
    /// フォームの形状をレジストリに保存する仕組み
    /// </summary>
    public class FormShapePersister : IDisposable
    {
        /// <summary>
        /// 型に応じた値保存
        /// </summary>
        /// <param name="c">コントロール</param>
        /// <param name="key">キー</param>
        /// <param name="isSave">true=保存 / false=ロード</param>
        private void persistControl(object c, string key, bool isSave)
        {
            if (c is CheckBox)
            {
                _persist(c, "CheckState", key, isSave);
            }
            if (c is TextBox)
            {
                _persist(c, "Text", key, isSave);
            }
            if (c is ColumnHeader)
            {
                _persist(c, "Width", key, isSave);
            }
            if (c is TabControl)
            {
                _persist(c, "SelectedIndex", key, isSave);
            }
        }

        #region 内部処理
        private Form _parent = null;
        private readonly EventHandler _ehl;
        private readonly CancelEventHandler _ehc;
        private readonly Hashtable _errMes = new Hashtable();
        private bool _isChild = true;

        /// <summary>
        /// 指定コントロールが属するpropertyName名を持つパブリックプロパティを永続化ｓ
        /// </summary>
        /// <param name="control">コントロール</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <param name="key">コントロールまでのレジストリキー</param>
        /// <param name="isSave">true=保存 / false=読み込み</param>
        private void _persist(object control, string propertyName, string key, bool isSave)
        {
            var pi = control.GetType().GetProperty(propertyName);
            if (pi != null)
            {
                var val = pi.GetValue(control, Array.Empty<object>());
                if (isSave)
                {
                    ConfigRegister.Current[key + "." + propertyName] = val;
                }
                else
                {
                    if (control is CheckBox)
                    {
                    }
                    try
                    {
                        var regVal = ConfigRegister.Current[key + "." + propertyName, val];
                        try
                        {
                            pi.SetValue(control, regVal, Array.Empty<object>());
                        }
                        catch (ArgumentException)
                        {
                            // Enum型でチャレンジ
                            var eval = Enum.Parse(pi.PropertyType, regVal.ToString());
                            pi.SetValue(control, eval, Array.Empty<object>());
                        }
                    }
                    catch (Exception)
                    {
                        var k = pi.PropertyType.Name + "@" + val.ToString();
                        if (_errMes.Contains(k) == false)
                        {
                            _errMes[k] = true;
                            System.Diagnostics.Debug.WriteLine("uFormShapePersisterロード値適用異常；" + pi.PropertyType.Name + "型の" + val.ToString() + "値は復元できない");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// レジストリのキー名称を生成する
        /// </summary>
        /// <param name="name">名称部</param>
        /// <returns>キー文字列</returns>
        private string makeKey(string name)
        {
            System.Diagnostics.Debug.Assert(_parent != null);
            return _parent.Name + "." + name;
        }

        /// <summary>
        /// 唯一のコンストラクタ
        /// </summary>
        /// <param name="parent">対象とするフォーム</param>
        public FormShapePersister(Form parent)
        {
            _parent = parent;
            _parent.Load += (_ehl = new EventHandler(_parent_Load));
            _parent.Closing += (_ehc = new CancelEventHandler(_parent_Closing));
        }

        /// <summary>
        /// 子コントロールを永続化するか？
        /// </summary>
        public bool Children
        {
            set => _isChild = value;
        }

        #region IDisposable メンバ

        public void Dispose()
        {
            if (_parent != null)
            {
                _parent.Load -= _ehl;
                _parent.Closing -= _ehc;
                _parent = null;
            }
        }

        #endregion

        /// <summary>
        /// スレッドでフォーム復元を行う
        /// </summary>
        private void loading()
        {
            try
            {
                _parent.SuspendLayout();
                var state = (int)ConfigRegister.Current[makeKey("WindowState"), -1];
                switch (state)
                {
                    case 1:
                        _parent.WindowState = FormWindowState.Normal;
                        break;
                    case 2:
                        _parent.WindowState = FormWindowState.Maximized;
                        break;
                    case 0:
                        _parent.WindowState = FormWindowState.Minimized;
                        break;
                }
                if (state == 1)
                {
                    _parent.Width = (int)ConfigRegister.Current[makeKey("Width"), _parent.Width];
                    _parent.Height = (int)ConfigRegister.Current[makeKey("Height"), _parent.Height];
                    _parent.Left = (int)ConfigRegister.Current[makeKey("Left"), _parent.Left];
                    _parent.Top = (int)ConfigRegister.Current[makeKey("Top"), _parent.Top];
                }
                _controlLoop(_parent, _parent.Name, false);
                _errMes.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("uFormShapePersister失敗；" + ex.Message);
            }
            finally
            {
                _parent.ResumeLayout();
            }
        }

        /// <summary>
        /// 読み込み後の、形状記憶
        /// </summary>
        private void _parent_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer t;
            if (sender is ISite c)
            {
                t = new System.Windows.Forms.Timer(c.Container);
            }
            else
            {
                t = new System.Windows.Forms.Timer();
            }
            t.Tick += new EventHandler(t_Tick);
            t.Interval = 2;
            t.Start();
        }

        /// <summary>
        /// フォームの形が変わった後にコールされる
        /// </summary>
        public event EventHandler Loaded;

        private void t_Tick(object sender, EventArgs e)
        {
            ((System.Windows.Forms.Timer)sender).Dispose();

            loading();
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 閉じるときの保存
        /// </summary>
        private void _parent_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                switch (_parent.WindowState)
                {
                    case FormWindowState.Normal:
                        ConfigRegister.Current[makeKey("WindowState")] = 1;
                        break;
                    case FormWindowState.Maximized:
                        ConfigRegister.Current[makeKey("WindowState")] = 2;
                        break;
                    case FormWindowState.Minimized:
                        ConfigRegister.Current[makeKey("WindowState")] = 0;
                        break;
                }
                if (_parent.WindowState == FormWindowState.Normal)
                {
                    ConfigRegister.Current[makeKey("Width")] = _parent.Width;
                    ConfigRegister.Current[makeKey("Height")] = _parent.Height;
                    ConfigRegister.Current[makeKey("Left")] = _parent.Left;
                    ConfigRegister.Current[makeKey("Top")] = _parent.Top;
                }
                _controlLoop(_parent, _parent.Name, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("uFormShapePersister失敗；" + ex.Message);
            }
        }

        /// <summary>
        /// インターフェースを検索する関数
        /// </summary>
        private bool _filter(Type t, object p)
        {
            if (t.Name == "ICollection")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// コントロール階層を再帰スキャンしながら処理する
        /// </summary>
        /// <param name="c"></param>
        /// <param name="builtKey"></param>
        /// <param name="isSave"></param>
        private void _controlLoop(IComponent c, string builtKey, bool isSave)
        {
            persistControl(c, builtKey, isSave);
            if (_isChild == false)
            {
                return;
            }

            // 子コントロールのループ
            var pis = c.GetType().GetProperties();
            foreach (var pi in pis)
            {
                var ifs = pi.PropertyType.FindInterfaces(new TypeFilter(_filter), null);
                if (ifs.Length > 0)
                {
                    var col = (ICollection)pi.GetValue(c, Array.Empty<object>());
                    foreach (var child in col)
                    {
                        var k = builtKey + ".";
                        var pin = child.GetType().GetProperty("Name");
                        var name = "";
                        if (pin != null)
                        {
                            name = pin.GetValue(child, Array.Empty<object>()).ToString();
                        }
                        if (pin == null || string.IsNullOrEmpty(name))
                        {
                            pin = child.GetType().GetProperty("Index");
                            if (pin != null)
                            {
                                var id = pin.GetValue(child, Array.Empty<object>()).ToString();
                                if (child is IComponent)
                                {
                                    _controlLoop((IComponent)child, builtKey + "." + child.GetType().Name + "(ID=" + id + ")", isSave);
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(name) == false)
                        {
                            if (child is IComponent)
                            {
                                _controlLoop((IComponent)child, builtKey + "." + child.GetType().Name + "(" + name + ")", isSave);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}
