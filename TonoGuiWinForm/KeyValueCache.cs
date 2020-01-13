// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �L�[�ƒl�̃L���b�V��
    /// </summary>
    public class KeyValueCache<TKEY, TVALUE> : IDisposable
    {
        /// <summary>
        /// �L���b�V���t�@�C��
        /// </summary>
        private readonly string _filepath = "";

        /// <summary>
        /// �o�[�W�������
        /// </summary>
        private readonly string _version = "NO_VER_INFO";

        /// <summary>
        /// �L���b�V���f�[�^
        /// </summary>
        private readonly Dictionary<TKEY, TVALUE> _dat = new Dictionary<TKEY, TVALUE>();

        /// <summary>
        /// �ǉ����ꂽ�L�[
        /// </summary>
        private readonly List<TKEY> _addKeys = new List<TKEY>();

        /// <summary>
        /// �l�X�V�Ȃ̂ŁA�ǋL�ł��Ȃ�
        /// </summary>
        private bool _isResetRequested = false;

        /// <summary>
        /// �ǉ����ꂽ�L�[
        /// </summary>
        private readonly List<TVALUE> _addValues = new List<TVALUE>();

        /// <summary>
        /// �o�[�W�����Ⴂ���H
        /// </summary>
        private bool _isNotMatchVersion = false;

        /// <summary>
        /// �\�z�q
        /// </summary>
        /// <param name="fullpath"></param>
        public KeyValueCache(string fullpath, string version)
        {
            _filepath = fullpath;
            _version = version;
            load();
        }

        /// <summary>
        /// ���[�h���Ȃ��ŃC���X�^���X�쐬
        /// </summary>
        /// <param name="fullpath"></param>
        private KeyValueCache(string fullpath, string version, bool isLoad)
        {
            _filepath = fullpath;
            _version = version;
            if (isLoad)
            {
                load();
            }
        }

        /// <summary>
        /// �ǂݍ��݂����t�@�C���o�[�W����������Ă������H
        /// </summary>
        public bool IsNotMatchVersionLoad => _isNotMatchVersion;

        #region IDisposable �����o

        public void Dispose()
        {
            Flush();
            Clear();
        }

        #endregion

        /// <summary>
        /// �N���A
        /// </summary>
        public void Clear()
        {
            _dat.Clear();
            _addKeys.Clear();
            _addValues.Clear();
        }

        /// <summary>
        /// �L���b�V������擾
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TVALUE this[TKEY key]
        {
            get => _dat[key];
            set
            {
                if (_isResetRequested == false)
                {
                    if (_dat.TryGetValue(key, out var val))
                    {
                        if (val.Equals(value) == false)
                        {
                            _isResetRequested = true;
                            _addKeys.Clear();
                            _addValues.Clear();
                        }
                    }
                    else
                    {
                        _addKeys.Add(key);
                        _addValues.Add(value);
                    }
                }
                _dat[key] = value;
            }
        }

        /// <summary>
        /// �L�[�̓o�^�L�����m�F
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKEY key)
        {
            return _dat.ContainsKey(key);
        }

        /// <summary>
        /// ���[�h����
        /// </summary>
        /// <returns>
        /// true=OK / false=�o�[�W�����Ⴂ�œǂݍ��݂Ȃ�
        /// </returns>
        private void load()
        {
            Clear();
            FileStream fst = null;
            try
            {
                fst = new FileStream(_filepath, FileMode.Open);
                var bf = new BinaryFormatter();

                string fileversion; // �t�@�C���o�[�W����
                fileversion = (string)bf.Deserialize(fst);

                if (_version.Equals(fileversion))
                {
                    while (fst.Position < fst.Length)
                    {
                        var key = (TKEY)bf.Deserialize(fst);
                        var value = (TVALUE)bf.Deserialize(fst);
                        _dat[key] = value;
                    }
                }
                else
                {
                    _isResetRequested = true;   // �o�[�W�����Ⴂ�͎����X�V
                    _isNotMatchVersion = true;
                }
            }
            finally
            {
                if (fst != null)
                {
                    fst.Close();
                }
            }
        }

        /// <summary>
        /// �L�����Ă������V�K�f�[�^���t�@�C���ɕۑ�����
        /// </summary>
        public void Flush()
        {
            FileStream fst = null;
            var bf = new BinaryFormatter();
            try
            {
                if (_isResetRequested)
                {
                    fst = new FileStream(_filepath, FileMode.Create);
                    bf.Serialize(fst, _version);
                    _isNotMatchVersion = false;

                    _addKeys.Clear();
                    _addValues.Clear();
                    foreach (var kv in _dat)
                    {
                        _addKeys.Add(kv.Key);
                        _addValues.Add(kv.Value);
                    }
                    _isResetRequested = false;
                }
                else
                {
                    fst = new FileStream(_filepath, FileMode.Append);
                    if (fst.Position < 1)
                    {
                        bf.Serialize(fst, _version);
                        _isNotMatchVersion = false;
                    }
                }

                for (var i = _addKeys.Count - 1; i >= 0; i--)
                {
                    var key = _addKeys[i];
                    var value = _addValues[i];
                    bf.Serialize(fst, key);
                    bf.Serialize(fst, value);
                }
                _addKeys.Clear();
                _addValues.Clear();
            }
            finally
            {
                if (fst != null)
                {
                    fst.Close();
                }
            }
        }

        /// <summary>
        /// �l�̎擾�����݂�
        /// </summary>
        /// <param name="strorg"></param>
        /// <param name="str"></param>
        public bool TryGetValue(TKEY strorg, out TVALUE str)
        {
            return _dat.TryGetValue(strorg, out str);
        }

        /// <summary>
        /// ��̃C���X�^���X���쐬����
        /// </summary>
        /// <param name="_synonymCacheFilename"></param>
        /// <returns></returns>
        public static KeyValueCache<TKEY, TVALUE> CreateEmptyInstance(string fullPath, string version)
        {
            var ret = new KeyValueCache<TKEY, TVALUE>(fullPath, version, false);
            return ret;
        }
    }
}
