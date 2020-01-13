// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Configulation Register
    /// </summary>
    public class ConfigRegister
    {
        /// <summary>
        /// ���[�g�L�[
        /// </summary>
        private readonly string _root;
        private static ConfigRegister _current = null;

        #region Data tips for debugging
#if DEBUG
        public string _ => "Root = " + _root;
#endif
        #endregion

        /// <summary>
        /// �O���[�o���L�[���擾����
        /// </summary>
        public static ConfigRegister Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new ConfigRegister(System.Windows.Forms.Application.ProductName);
                }
                return _current;
            }
        }

        public ConfigRegister()
        {
            _root = @"Software\uConfigDefault";
        }

        public ConfigRegister(string rootkey)
        {
            if (string.IsNullOrEmpty(rootkey))
            {
                _root = @"Software\uConfigDefault";
            }
            else
            {
                _root = @"Software\TOYOTA\" + rootkey;
            }
        }

        /// <summary>
        /// �w��L�[�ȉ������ׂč폜����
        /// uConfig X = new uConfig("AAA\\BBB");
        /// X.Delete()  �Ƃ���ƁABBB �̊K�w�ȉ����폜�����
        /// </summary>
        public void Delete()
        {
            try
            {
                var r = _root;
                var r2 = r.Substring(0, r.LastIndexOf("\\"));
                var r3 = r.Substring(r2.Length + 1);
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(r2, true);
                key.DeleteSubKeyTree(r3);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// �p�����[�^�̓ǂݏ������s��
        /// OBJ[�L�[������]
        /// </summary>
        public object this[string keystr]
        {
            get
            {
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_root);
                if (key == null)
                {
                    this["FirstExec"] = DateTime.Now.ToString();
                    key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(_root);
                }
                var ks = keystr.Split('\\');
                if (ks.Length > 1)
                {
                    var sf = new string[ks.Length - 1];
                    for (var i = 0; i < sf.Length; i++)
                    {
                        sf[i] = ks[i];
                    }
                    var sfs = string.Join("\\", sf);
                    key = key.OpenSubKey(sfs);
                    keystr = ks[ks.Length - 1];
                }
                if (key == null)
                {
                    return null;
                }
                var ret = key.GetValue(keystr);
                key.Close();
                return ret;
            }
            set
            {
                var key = Microsoft.Win32.Registry.CurrentUser;
                key = key.CreateSubKey(_root);
                var ks = keystr.Split('\\');
                if (ks.Length > 1)
                {
                    var sf = new string[ks.Length - 1];
                    for (var i = 0; i < sf.Length; i++)
                    {
                        sf[i] = ks[i];
                    }
                    var sfs = string.Join("\\", sf);
                    key = key.CreateSubKey(sfs);
                    keystr = ks[ks.Length - 1];
                }
                if (value == null)
                {
                    key.SetValue(keystr, "");
                }
                else
                {
                    key.SetValue(keystr, value);
                }
                key.Close();
            }
        }

        /// <summary>
        /// �p�����[�^�̓ǂݏ������s��
        /// OBJ[�L�[������,�f�t�H���g�l]
        /// </summary>
        public object this[string keystr, object defValue]
        {
            get
            {
                var ret = this[keystr];
                if (ret == null)
                {
                    this[keystr] = defValue;    // �f�t�H���g�l��ۑ����Ă���
                    return defValue;
                }
                return ret;
            }
            set
            {
                if (value == null)
                {
                    this[keystr] = defValue;
                }
                else
                {
                    this[keystr] = value;
                }
            }
        }
    }
}
